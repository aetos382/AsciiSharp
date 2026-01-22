
using System;
using System.Collections.Generic;

using AsciiSharp.Diagnostics;
using AsciiSharp.InternalSyntax;
using AsciiSharp.Properties;
using AsciiSharp.Text;

namespace AsciiSharp.Parser;
/// <summary>
/// AsciiDoc 文書の構文解析を行うクラス。
/// </summary>
internal sealed class AsciiDocParser
{
    private readonly Lexer _lexer;
    private readonly ITreeSink _sink;
    private readonly List<Diagnostic> _diagnostics = [];
    private InternalToken? _peekedToken;

    /// <summary>
    /// AsciiDocParser を作成する。
    /// </summary>
    /// <param name="lexer">字句解析器。</param>
    /// <param name="sink">構文木構築用のシンク。</param>
    public AsciiDocParser(Lexer lexer, ITreeSink sink)
    {
        ArgumentNullException.ThrowIfNull(lexer);
        ArgumentNullException.ThrowIfNull(sink);

        this._lexer = lexer;
        this._sink = sink;
        this.Current = this._lexer.NextToken();
    }

    /// <summary>
    /// 診断情報のリスト。
    /// </summary>
    public IReadOnlyList<Diagnostic> Diagnostics => this._diagnostics;

    /// <summary>
    /// 現在のトークン。
    /// </summary>
    private InternalToken Current { get; set; }

    /// <summary>
    /// 次のトークンを先読みする。
    /// </summary>
    private InternalToken Peek()
    {
        this._peekedToken ??= this._lexer.NextToken();

        return this._peekedToken;
    }

    /// <summary>
    /// 次のトークンに進む。
    /// </summary>
    private void Advance()
    {
        if (this._peekedToken is not null)
        {
            this.Current = this._peekedToken;
            this._peekedToken = null;
        }
        else
        {
            this.Current = this._lexer.NextToken();
        }
    }

    /// <summary>
    /// 文書全体を解析する。
    /// </summary>
    public void ParseDocument()
    {
        this._sink.StartNode(SyntaxKind.Document);

        // ヘッダーの解析を試みる
        if (this.IsAtDocumentTitle())
        {
            this.ParseDocumentHeader();
        }

        // ボディの解析
        this.ParseDocumentBody();

        // EOF トークン
        this.EmitToken(SyntaxKind.EndOfFileToken);

        this._sink.FinishNode();
    }

    /// <summary>
    /// ドキュメントヘッダーを解析する。
    /// </summary>
    private void ParseDocumentHeader()
    {
        this._sink.StartNode(SyntaxKind.DocumentHeader);

        // タイトル行を解析
        this.ParseSectionTitle();

        // 著者行があれば解析
        if (!this.IsAtEnd() && !this.IsBlankLine() && !this.IsAtSectionTitle())
        {
            this.ParseAuthorLine();
        }

        // 空行をスキップ
        this.SkipBlankLines();

        this._sink.FinishNode();
    }

    /// <summary>
    /// 著者行を解析する。
    /// </summary>
    private void ParseAuthorLine()
    {
        this._sink.StartNode(SyntaxKind.AuthorLine);

        // 行末まで読み取る
        while (!this.IsAtEnd() && this.Current.Kind != SyntaxKind.NewLineToken && this.Current.Kind != SyntaxKind.EndOfFileToken)
        {
            this.EmitCurrentToken();
        }

        // 改行を含める
        if (this.Current.Kind == SyntaxKind.NewLineToken)
        {
            this.EmitCurrentToken();
        }

        this._sink.FinishNode();
    }

    /// <summary>
    /// ドキュメントボディを解析する。
    /// </summary>
    private void ParseDocumentBody()
    {
        this._sink.StartNode(SyntaxKind.DocumentBody);

        while (!this.IsAtEnd())
        {
            if (this.IsBlankLine())
            {
                this.SkipBlankLines();
            }
            else if (this.IsAtSectionTitle())
            {
                this.ParseSection();
            }
            else
            {
                this.ParseParagraph();
            }
        }

        this._sink.FinishNode();
    }

    /// <summary>
    /// セクションを解析する。
    /// </summary>
    private void ParseSection()
    {
        this._sink.StartNode(SyntaxKind.Section);

        var currentLevel = this.CountEqualsAtLineStart();

        // セクションタイトルを解析
        this.ParseSectionTitle();

        // 空行をスキップ
        this.SkipBlankLines();

        // セクションの内容を解析
        while (!this.IsAtEnd() && !this.IsAtSectionTitleOfLevelOrHigher(currentLevel))
        {
            if (this.IsBlankLine())
            {
                this.SkipBlankLines();
            }
            else if (this.IsAtSectionTitle())
            {
                // サブセクション
                this.ParseSection();
            }
            else
            {
                this.ParseParagraph();
            }
        }

        this._sink.FinishNode();
    }

    /// <summary>
    /// セクションタイトルを解析する。
    /// </summary>
    private void ParseSectionTitle()
    {
        this._sink.StartNode(SyntaxKind.SectionTitle);

        var startPosition = this._lexer.Position - this.Current.FullWidth;

        // = を読み取る
        while (this.Current.Kind == SyntaxKind.EqualsToken)
        {
            this.EmitCurrentToken();
        }

        // 空白を読み取る
        if (this.Current.Kind == SyntaxKind.WhitespaceToken)
        {
            this.EmitCurrentToken();
        }

        // タイトルテキストを読み取る
        var hasTitleText = false;
        while (!this.IsAtEnd() && this.Current.Kind != SyntaxKind.NewLineToken && this.Current.Kind != SyntaxKind.EndOfFileToken)
        {
            hasTitleText = true;
            this.EmitCurrentToken();
        }

        // タイトルテキストがない場合はエラーを報告
        if (!hasTitleText)
        {
            this._sink.MissingToken(SyntaxKind.TextToken);
            var errorPosition = this._lexer.Position - (this.Current.Kind == SyntaxKind.NewLineToken ? this.Current.FullWidth : 0);
            this.AddError(
                "ADS0002",
                Resources.ADS0002_MissingSectionTitleText,
                new TextSpan(startPosition, errorPosition - startPosition));
        }

        // 改行を読み取る
        if (this.Current.Kind == SyntaxKind.NewLineToken)
        {
            this.EmitCurrentToken();
        }

        this._sink.FinishNode();
    }

    /// <summary>
    /// 段落を解析する。
    /// </summary>
    private void ParseParagraph()
    {
        this._sink.StartNode(SyntaxKind.Paragraph);

        // 段落の行を読み取る（空行またはセクションタイトルまで）
        while (!this.IsAtEnd() && !this.IsBlankLine() && !this.IsAtSectionTitle())
        {
            // 行の内容を読み取る
            while (!this.IsAtEnd() && this.Current.Kind != SyntaxKind.NewLineToken && this.Current.Kind != SyntaxKind.EndOfFileToken)
            {
                if (this.IsAtLink())
                {
                    this.ParseLink();
                }
                else
                {
                    this.EmitCurrentToken();
                }
            }

            // 改行を読み取る
            if (this.Current.Kind == SyntaxKind.NewLineToken)
            {
                this.EmitCurrentToken();
            }
        }

        this._sink.FinishNode();
    }

    /// <summary>
    /// 現在位置が URL リンクの開始位置かどうかを判定する。
    /// </summary>
    /// <returns>URL リンクの開始位置であれば true。</returns>
    private bool IsAtLink()
    {
        // URL パターン: http:// または https://
        if (this.Current.Kind != SyntaxKind.TextToken)
        {
            return false;
        }

        var text = this.Current.Text;
        if (!string.Equals(text, "http", System.StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(text, "https", System.StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // 次のトークンが :// かを先読み
        var nextToken = this.Peek();
        return nextToken.Kind == SyntaxKind.ColonToken;
    }

    /// <summary>
    /// リンクを解析する。
    /// </summary>
    private void ParseLink()
    {
        this._sink.StartNode(SyntaxKind.Link);

        // URL 部分を収集: http(s)://...
        // スキーム部分 (http または https)
        this.EmitCurrentToken();

        // コロン (:)
        if (this.Current.Kind == SyntaxKind.ColonToken)
        {
            this.EmitCurrentToken();
        }

        // スラッシュ 2 つ (//)
        while (this.Current.Kind == SyntaxKind.SlashToken)
        {
            this.EmitCurrentToken();
        }

        // URL の残りの部分（空白、改行、EOF、または [ まで）
        while (!this.IsAtEnd() &&
               this.Current.Kind != SyntaxKind.WhitespaceToken &&
               this.Current.Kind != SyntaxKind.NewLineToken &&
               this.Current.Kind != SyntaxKind.EndOfFileToken &&
               this.Current.Kind != SyntaxKind.OpenBracketToken)
        {
            this.EmitCurrentToken();
        }

        // 表示テキストがあれば解析 ([text])
        if (this.Current.Kind == SyntaxKind.OpenBracketToken)
        {
            // [
            this.EmitCurrentToken();

            // 表示テキスト（] まで）
            while (!this.IsAtEnd() &&
                   this.Current.Kind != SyntaxKind.CloseBracketToken &&
                   this.Current.Kind != SyntaxKind.NewLineToken &&
                   this.Current.Kind != SyntaxKind.EndOfFileToken)
            {
                this.EmitCurrentToken();
            }

            // ]
            if (this.Current.Kind == SyntaxKind.CloseBracketToken)
            {
                this.EmitCurrentToken();
            }
        }

        this._sink.FinishNode();
    }

    /// <summary>
    /// 空行をスキップする。
    /// </summary>
    private void SkipBlankLines()
    {
        while (this.IsBlankLine())
        {
            // 空白をスキップ
            while (this.Current.Kind == SyntaxKind.WhitespaceToken)
            {
                this.EmitCurrentToken();
            }

            // 改行をスキップ
            if (this.Current.Kind == SyntaxKind.NewLineToken)
            {
                this.EmitCurrentToken();
            }
        }
    }

    /// <summary>
    /// 現在のトークンを出力して次へ進む。
    /// </summary>
    private void EmitCurrentToken()
    {
        // トリビア付きのトークンをそのまま出力
        this._sink.EmitToken(this.Current);
        this.Advance();
    }

    /// <summary>
    /// 指定された種別のトークンを期待して出力する。
    /// </summary>
    private void EmitToken(SyntaxKind expectedKind)
    {
        if (this.Current.Kind == expectedKind)
        {
            this.EmitCurrentToken();
        }
        else if (expectedKind == SyntaxKind.EndOfFileToken && this.IsAtEnd())
        {
            this._sink.Token(SyntaxKind.EndOfFileToken, string.Empty);
        }
        else
        {
            // エラー: 期待されるトークンがない
            this._sink.MissingToken(expectedKind);

            // CA1863: エラー報告はホットパスではないため CompositeFormat キャッシュは不要
#pragma warning disable CA1863
            var errorMessage = string.Format(
                System.Globalization.CultureInfo.CurrentCulture,
                Resources.ADS0001_ExpectedToken,
                expectedKind);
#pragma warning restore CA1863

            this.AddError("ADS0001", errorMessage, new TextSpan(this._lexer.Position, 0));
        }
    }

    /// <summary>
    /// 診断情報を追加する。
    /// </summary>
    /// <param name="code">エラーコード。</param>
    /// <param name="message">エラーメッセージ。</param>
    /// <param name="span">位置情報。</param>
    private void AddError(string code, string message, TextSpan span)
    {
        this._sink.Error(code, message);
        this._diagnostics.Add(new Diagnostic(code, message, DiagnosticSeverity.Error, span));
    }

    /// <summary>
    /// ファイル終端に達したかどうか。
    /// </summary>
    private bool IsAtEnd()
    {
        return this.Current.Kind == SyntaxKind.EndOfFileToken;
    }

    /// <summary>
    /// 現在位置がドキュメントタイトル（レベル1セクション）かどうか。
    /// </summary>
    private bool IsAtDocumentTitle()
    {
        return this.Current.Kind == SyntaxKind.EqualsToken && this.Peek().Kind != SyntaxKind.EqualsToken;
    }

    /// <summary>
    /// 現在位置がセクションタイトルかどうか。
    /// </summary>
    private bool IsAtSectionTitle()
    {
        return this.Current.Kind == SyntaxKind.EqualsToken;
    }

    /// <summary>
    /// 現在位置が指定レベル以上のセクションタイトルかどうか。
    /// </summary>
    private bool IsAtSectionTitleOfLevelOrHigher(int level)
    {
        if (this.Current.Kind != SyntaxKind.EqualsToken)
        {
            return false;
        }

        var equalsCount = this.CountEqualsAtLineStart();
        return equalsCount <= level;
    }

    /// <summary>
    /// 行頭の = の数を数える。
    /// </summary>
    private int CountEqualsAtLineStart()
    {
        if (this.Current.Kind != SyntaxKind.EqualsToken)
        {
            return 0;
        }

        var count = 1;
        var token = this.Peek();
        while (token.Kind == SyntaxKind.EqualsToken)
        {
            count++;
            // 先読みをさらに進めることはできないので、1つ目の = の後をカウント
            break;
        }

        return count;
    }

    /// <summary>
    /// 現在位置が空行かどうか。
    /// </summary>
    private bool IsBlankLine()
    {
        return this.Current.Kind == SyntaxKind.NewLineToken ||
               (this.Current.Kind == SyntaxKind.WhitespaceToken && this.Peek().Kind == SyntaxKind.NewLineToken);
    }
}
