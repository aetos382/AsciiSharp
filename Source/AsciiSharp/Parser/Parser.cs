
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
    /// <summary>
    /// セクションマーカー (<c>=</c>) の最大長。
    /// AsciiDoc ではセクションレベルは 0〜5 の 6 段階で、マーカーの <c>=</c> の数は 1〜6。
    /// </summary>
    private const int MaxSectionMarkerLength = 6;

    /// <summary>
    /// 許容される最大ネストレベル。
    /// </summary>
    /// <remarks>
    /// AsciiDoc のセクションは通常 6 レベルまでですが、安全マージンを含めて 64 に設定。
    /// この値を超えるネストは無限ループの兆候と見なされる。
    /// </remarks>
    private const int MaxNestLevel = 64;

    private readonly Lexer _lexer;
    private readonly ITreeSink _sink;
    private readonly List<Diagnostic> _diagnostics = [];
    private readonly Queue<InternalToken> _peekedTokens = new();
    private int _nestLevel;

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
        return this.Peek(0);
    }

    /// <summary>
    /// 指定オフセット先のトークンを先読みする。
    /// </summary>
    /// <param name="offset">先読みオフセット（0 = Current の次のトークン）。</param>
    private InternalToken Peek(int offset)
    {
        Debug.Assert(offset >= 0, "offset は 0 以上でなければならない");

        while (this._peekedTokens.Count <= offset)
        {
            this._peekedTokens.Enqueue(this._lexer.NextToken());
        }

        // Queue の offset 番目の要素を返す
        var index = 0;
        foreach (var token in this._peekedTokens)
        {
            if (index == offset)
            {
                return token;
            }

            index++;
        }

        throw new UnreachableException();
    }

    /// <summary>
    /// 次のトークンに進む。
    /// </summary>
    private void Advance()
    {
        if (this._peekedTokens.Count > 0)
        {
            this.Current = this._peekedTokens.Dequeue();
        }
        else
        {
            this.Current = this._lexer.NextToken();
        }
    }

    /// <summary>
    /// 行末までのテキストを単一トークンとして次のトークンに進む。
    /// 先読みバッファが空であることを前提とする。
    /// </summary>
    private void AdvanceRawLine()
    {
        Debug.Assert(this._peekedTokens.Count == 0, "先読みバッファが空であることを期待");
        this.Current = this._lexer.NextRawLineToken();
    }

    /// <summary>
    /// 文書全体を解析する。
    /// </summary>
    public void ParseDocument()
    {
        this._sink.StartNode(SyntaxKind.Document);

        // 冒頭のコメントと空行をスキップ（トリビアとして保持）
        this.SkipLeadingCommentsAndBlankLines();

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
    /// 文書冒頭の空行をスキップする。
    /// </summary>
    /// <remarks>
    /// <para>ドキュメントタイトルの前にある空行を処理する。</para>
    /// <para>コメントは Lexer により Trivia として次のトークンに付加されるため、
    /// Parser での明示的な処理は不要。</para>
    /// </remarks>
    private void SkipLeadingCommentsAndBlankLines()
    {
        this.SkipBlankLines();
    }

    /// <summary>
    /// ドキュメントヘッダーを解析する。
    /// </summary>
    private void ParseDocumentHeader()
    {
        this._sink.StartNode(SyntaxKind.DocumentHeader);

        // タイトル行を解析
        this.ParseSectionTitle();

        // 著者行があれば解析（属性エントリではない行）
        if (!this.IsAtEnd() && !this.IsBlankLine() && !this.IsAtSectionTitle() && !this.IsAtAttributeEntry())
        {
            this.ParseAuthorLine();
        }

        // 属性エントリを解析
        while (!this.IsAtEnd() && this.IsAtAttributeEntry())
        {
            this.ParseAttributeEntry();
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

        InternalToken? lastContentToken = null;
        InternalToken? pendingWhitespace = null;

        // 行末まで読み取る
        while (!this.IsAtEnd() && this.Current.Kind != SyntaxKind.NewLineToken && this.Current.Kind != SyntaxKind.EndOfFileToken)
        {
            if (this.Current.Kind == SyntaxKind.WhitespaceToken)
            {
                // 空白トークン：行末トリビアの候補として保留する
                if (pendingWhitespace != null)
                {
                    // 前の保留空白はコンテンツ内部の空白として確定
                    if (lastContentToken != null)
                    {
                        this._sink.EmitToken(lastContentToken);
                        lastContentToken = null;
                    }

                    this._sink.EmitToken(pendingWhitespace);
                }

                pendingWhitespace = this.Current;
                this.Advance();
            }
            else
            {
                // 非空白トークン：保留中の空白はコンテンツ内部の空白として確定
                if (pendingWhitespace != null)
                {
                    if (lastContentToken != null)
                    {
                        this._sink.EmitToken(lastContentToken);
                    }

                    this._sink.EmitToken(pendingWhitespace);
                    pendingWhitespace = null;
                }
                else if (lastContentToken != null)
                {
                    this._sink.EmitToken(lastContentToken);
                }

                lastContentToken = this.Current;
                this.Advance();
            }
        }

        // ループ終了後：最後のコンテンツトークンに行末トリビアを付与する。
        // ループ内で改行を消費しない場合、後続の EmitCurrentToken が改行を処理する。
        // このフラグにより同一の改行トークンの二重消費を防ぐ。
        var authorLineNewLineConsumed = false;
        if (lastContentToken != null)
        {
            List<InternalTrivia> trailingTrivia = [];

            if (pendingWhitespace != null)
            {
                trailingTrivia.Add(InternalTrivia.Whitespace(pendingWhitespace.Text));
            }

            if (this.Current.Kind == SyntaxKind.NewLineToken)
            {
                trailingTrivia.Add(InternalTrivia.EndOfLine(this.Current.Text));
                this.Advance();
                authorLineNewLineConsumed = true;
            }

            if (trailingTrivia.Count > 0)
            {
                this._sink.EmitToken(lastContentToken.WithTrivia(null, [.. trailingTrivia]));
            }
            else
            {
                this._sink.EmitToken(lastContentToken);
            }
        }
        else if (pendingWhitespace != null)
        {
            // コンテンツが空白のみの場合、空白をトークンとしてそのまま出力する（行末トリビア化の対象外）
            this._sink.EmitToken(pendingWhitespace);
        }

        // 改行を含める（著者行内でトリビアとして消費済みの場合はスキップ）
        if (!authorLineNewLineConsumed && this.Current.Kind == SyntaxKind.NewLineToken)
        {
            this.EmitCurrentToken();
        }

        this._sink.FinishNode();
    }

    /// <summary>
    /// ドキュメントボディを解析する。
    /// </summary>
    /// <remarks>
    /// コメントは Lexer により Trivia として次のトークンに付加されるため、
    /// Parser での明示的な処理は不要。
    /// </remarks>
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
        this._nestLevel++;

        try
        {
            // ネストレベル制限のチェック
            if (this._nestLevel > MaxNestLevel)
            {
                this.AddError("ADS0003", Resources.Error_NestingLevelExceeded, new TextSpan(this._lexer.Position, 0));

                // ノードを開始して即座に終了（残りのトークンを消費しない）
                this._sink.StartNode(SyntaxKind.Section);
                this._sink.FinishNode();
                return;
            }

            this._sink.StartNode(SyntaxKind.Section);

            var currentLevel = this.Current.Text.Length;

            // セクションタイトルを解析
            this.ParseSectionTitle();

            // 空行をスキップ
            this.SkipBlankLines();

            // セクションの内容を解析
            // コメントは Lexer により Trivia として次のトークンに付加される
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
        finally
        {
            this._nestLevel--;
        }
    }

    /// <summary>
    /// セクションタイトルを解析する。
    /// </summary>
    private void ParseSectionTitle()
    {
        this._sink.StartNode(SyntaxKind.SectionTitle);

        var startPosition = this._lexer.Position - this.Current.FullWidth;

        // セクションマーカー（= の並び）を読み取る
        if (this.Current.Kind == SyntaxKind.EqualsToken)
        {
            var markerToken = this.Current;
            this.Advance();

            if (this.Current.Kind == SyntaxKind.WhitespaceToken)
            {
                // マーカー後の空白を TrailingTrivia として付与
                var whitespaceTrivia = InternalTrivia.Whitespace(this.Current.Text);
                markerToken = markerToken.WithTrivia(null, [whitespaceTrivia]);
                this.Advance();
            }

            this._sink.EmitToken(markerToken);
        }

        // タイトルテキストを読み取る（InlineText ノードとしてラップ）
        var hasTitleText = false;
        var titleNewLineConsumed = false;
        if (!this.IsAtEnd() && this.Current.Kind != SyntaxKind.NewLineToken && this.Current.Kind != SyntaxKind.EndOfFileToken)
        {
            this._sink.StartNode(SyntaxKind.InlineText);

            InternalToken? lastContentToken = null;
            InternalToken? pendingWhitespace = null;

            while (!this.IsAtEnd() && this.Current.Kind != SyntaxKind.NewLineToken && this.Current.Kind != SyntaxKind.EndOfFileToken)
            {
                hasTitleText = true;

                if (this.Current.Kind == SyntaxKind.WhitespaceToken)
                {
                    // 空白トークン：行末トリビアの候補として保留する
                    if (pendingWhitespace != null)
                    {
                        // 前の保留空白はコンテンツ内部の空白として確定
                        if (lastContentToken != null)
                        {
                            this._sink.EmitToken(lastContentToken);
                            lastContentToken = null;
                        }

                        this._sink.EmitToken(pendingWhitespace);
                    }

                    pendingWhitespace = this.Current;
                    this.Advance();
                }
                else
                {
                    // 非空白トークン：保留中の空白はコンテンツ内部の空白として確定
                    if (pendingWhitespace != null)
                    {
                        if (lastContentToken != null)
                        {
                            this._sink.EmitToken(lastContentToken);
                        }

                        this._sink.EmitToken(pendingWhitespace);
                        pendingWhitespace = null;
                    }
                    else if (lastContentToken != null)
                    {
                        this._sink.EmitToken(lastContentToken);
                    }

                    lastContentToken = this.Current;
                    this.Advance();
                }
            }

            // ループ終了後：最後のコンテンツトークンに行末トリビアを付与する。
            // ここで改行を消費した場合、後続のセクションタイトルレベルの改行処理をスキップする。
            // このフラグにより同一の改行トークンの二重消費を防ぐ。
            if (lastContentToken != null)
            {
                List<InternalTrivia> trailingTrivia = [];

                if (pendingWhitespace != null)
                {
                    trailingTrivia.Add(InternalTrivia.Whitespace(pendingWhitespace.Text));
                }

                if (this.Current.Kind == SyntaxKind.NewLineToken)
                {
                    trailingTrivia.Add(InternalTrivia.EndOfLine(this.Current.Text));
                    this.Advance();
                    titleNewLineConsumed = true;
                }

                if (trailingTrivia.Count > 0)
                {
                    this._sink.EmitToken(lastContentToken.WithTrivia(null, [.. trailingTrivia]));
                }
                else
                {
                    this._sink.EmitToken(lastContentToken);
                }
            }
            else if (pendingWhitespace != null)
            {
                // コンテンツが空白のみの場合、空白をトークンとしてそのまま出力する（行末トリビア化の対象外）
                this._sink.EmitToken(pendingWhitespace);
            }

            this._sink.FinishNode();
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

        // 改行を読み取る（InlineText 内でトリビアとして消費済みの場合はスキップ）
        if (!titleNewLineConsumed && this.Current.Kind == SyntaxKind.NewLineToken)
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

        // 段落の要素を読み取る（空行またはセクションタイトルまで）
        while (!this.IsAtEnd() && !this.IsBlankLine() && !this.IsAtSectionTitle())
        {
            if (this.IsAtLink())
            {
                this.ParseLink();

                // リンク後の改行を消費する
                if (this.Current.Kind == SyntaxKind.NewLineToken)
                {
                    this.EmitCurrentToken();
                }
            }
            else
            {
                var newLineConsumed = this.ParseInlineText();

                // ParseInlineText() が改行を消費していない場合（EOF またはリンク開始で中断した場合）のみ消費する
                if (!newLineConsumed && this.Current.Kind == SyntaxKind.NewLineToken)
                {
                    this.EmitCurrentToken();
                }
            }
        }

        this._sink.FinishNode();
    }

    /// <summary>
    /// インラインテキストを解析する。
    /// 連続するプレーンテキスト行を 1 つの InlineText ノードとしてラップする。
    /// 中間行の改行はコンテンツトークンとして保持し、最終行の改行はトリビアとして付与する。
    /// </summary>
    /// <returns>
    /// 改行トークンをトリビアまたはコンテンツとして消費した場合は <see langword="true"/>。
    /// EOF またはリンク開始で行が中断した場合は <see langword="false"/>。
    /// 呼び出し側は <see langword="false"/> の場合にのみ改行の消費を担当する。
    /// </returns>
    private bool ParseInlineText()
    {
        this._sink.StartNode(SyntaxKind.InlineText);

        InternalToken? lastContentToken = null;
        InternalToken? pendingWhitespace = null;
        bool newLineConsumed = false;

        // 保留中のコンテンツトークンと空白トークンを出力して状態をリセットする。
        // pendingWhitespace がある場合は先に lastContentToken を出力し、次に pendingWhitespace を出力する。
        void FlushPendingTokens()
        {
            if (pendingWhitespace != null)
            {
                if (lastContentToken != null)
                {
                    this._sink.EmitToken(lastContentToken);
                    lastContentToken = null;
                }

                this._sink.EmitToken(pendingWhitespace);
                pendingWhitespace = null;
            }
            else if (lastContentToken != null)
            {
                this._sink.EmitToken(lastContentToken);
                lastContentToken = null;
            }
        }

        while (true)
        {
            // 現在の行のトークンを読む（改行・EOF・リンク開始まで）
            while (!this.IsAtEnd() &&
                   this.Current.Kind != SyntaxKind.NewLineToken &&
                   this.Current.Kind != SyntaxKind.EndOfFileToken &&
                   !this.IsAtLink())
            {
                if (this.Current.Kind == SyntaxKind.WhitespaceToken)
                {
                    // 空白：行末トリビアの候補として保留する
                    if (pendingWhitespace != null)
                    {
                        // 前の保留空白はコンテンツ内部の空白として確定
                        if (lastContentToken != null)
                        {
                            this._sink.EmitToken(lastContentToken);
                            lastContentToken = null;
                        }

                        this._sink.EmitToken(pendingWhitespace);
                    }

                    pendingWhitespace = this.Current;
                    this.Advance();
                }
                else
                {
                    // 非空白：保留中の空白を確定してコンテンツとして出力する
                    FlushPendingTokens();
                    lastContentToken = this.Current;
                    this.Advance();
                }
            }

            // 行末（改行・EOF・リンク開始）に達した
            if (this.Current.Kind == SyntaxKind.NewLineToken)
            {
                var newLineToken = this.Current;
                this.Advance();

                // 次の行が段落の継続かどうかを判定する
                bool isContinuation =
                    !this.IsAtEnd() &&
                    !this.IsBlankLine() &&
                    !this.IsAtSectionTitle() &&
                    !this.IsAtLink();

                if (isContinuation)
                {
                    // 中間行: 行末空白はコンテンツ内部として確定し、改行をコンテンツトークンとして出力する
                    FlushPendingTokens();
                    this._sink.EmitToken(newLineToken);
                    newLineConsumed = true;
                }
                else
                {
                    // 最終行: 改行をトリビアとして付与して終了する
                    List<InternalTrivia> trailingTrivia = [];

                    if (pendingWhitespace != null)
                    {
                        trailingTrivia.Add(InternalTrivia.Whitespace(pendingWhitespace.Text));
                    }

                    trailingTrivia.Add(InternalTrivia.EndOfLine(newLineToken.Text));

                    if (lastContentToken != null)
                    {
                        this._sink.EmitToken(lastContentToken.WithTrivia(null, [.. trailingTrivia]));
                    }
                    else if (pendingWhitespace != null)
                    {
                        // pendingWhitespace 自体がトークン（空白文字がコンテンツ）であるため、
                        // trailingTrivia の Whitespace エントリは付与しない（二重計上になる）。
                        // EndOfLine のみをトリビアとして付与する。
                        this._sink.EmitToken(pendingWhitespace.WithTrivia(null, [InternalTrivia.EndOfLine(newLineToken.Text)]));
                    }
                    else
                    {
                        // lastContentToken と pendingWhitespace が共に null になるケースは、
                        // ParseParagraph() が IsBlankLine() を確認した上で ParseInlineText() を呼ぶため、
                        // 通常の入力では到達しない。
                        throw new UnreachableException("ParseInlineText: 最終行処理で lastContentToken と pendingWhitespace が共に null です。");
                    }

                    newLineConsumed = true;
                    break;
                }
            }
            else
            {
                // 改行なしで終了（EOF またはリンク開始）
                // 最後のコンテンツトークンを出力する（行末空白はトリビア化しない）
                FlushPendingTokens();
                break;
            }
        }

        this._sink.FinishNode();
        return newLineConsumed;
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
        if (!string.Equals(text, "http", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(text, "https", StringComparison.OrdinalIgnoreCase))
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
    /// 現在位置がドキュメントタイトル（レベル 1 セクション）かどうかを判定する。
    /// <c>=</c> が 1 つで、その後に空白が続く場合に <see langword="true"/> を返す。
    /// </summary>
    private bool IsAtDocumentTitle()
    {
        return this.Current.Kind == SyntaxKind.EqualsToken
            && this.Current.Text.Length == 1
            && this.Peek().Kind == SyntaxKind.WhitespaceToken;
    }

    /// <summary>
    /// 現在位置がセクションタイトルかどうかを判定する。
    /// <c>=</c> が 1〜6 個で、その後に空白が続く場合に <see langword="true"/> を返す。
    /// <c>=</c> が 7 個以上の場合、または空白が続かない場合は <see langword="false"/> を返す。
    /// </summary>
    private bool IsAtSectionTitle()
    {
        return this.Current.Kind == SyntaxKind.EqualsToken
            && this.Current.Text.Length <= MaxSectionMarkerLength
            && this.Peek().Kind == SyntaxKind.WhitespaceToken;
    }

    /// <summary>
    /// 現在位置が指定レベル以上のセクションタイトルかどうかを判定する。
    /// <c>=</c> の数が <paramref name="level"/> 以下で、その後に空白が続く場合に <see langword="true"/> を返す。
    /// </summary>
    private bool IsAtSectionTitleOfLevelOrHigher(int level)
    {
        return this.Current.Kind == SyntaxKind.EqualsToken
            && this.Current.Text.Length <= level
            && this.Peek().Kind == SyntaxKind.WhitespaceToken;
    }

    /// <summary>
    /// 現在位置が属性エントリの開始位置かどうかを判定する。
    /// <c>:name:</c> の形式（コロン + テキスト + コロン）で始まる行を検出する。
    /// </summary>
    private bool IsAtAttributeEntry()
    {
        return this.Current.Kind == SyntaxKind.ColonToken
            && this.Peek(0).Kind == SyntaxKind.TextToken
            && this.Peek(1).Kind == SyntaxKind.ColonToken;
    }

    /// <summary>
    /// 属性エントリを解析する。
    /// </summary>
    /// <remarks>
    /// <para>属性エントリは <c>:name: value</c> または <c>:name:</c> の形式。</para>
    /// <para>閉じコロン後の空白はトリビアとして閉じコロンに付与する。</para>
    /// <para>行末の改行は最後のトークン（値トークンまたは閉じコロン）のトリビアとして付与する。</para>
    /// </remarks>
    private void ParseAttributeEntry()
    {
        this._sink.StartNode(SyntaxKind.AttributeEntry);

        // 開きコロン
        this.EmitCurrentToken();

        // 属性名
        this.EmitCurrentToken();

        // 閉じコロン
        var closingColonToken = this.Current;
        this.Advance();

        if (this.Current.Kind == SyntaxKind.WhitespaceToken)
        {
            // 閉じコロン後に空白がある場合
            var whitespaceTrivia = InternalTrivia.Whitespace(this.Current.Text);
            this.AdvanceRawLine();

            if (!this.IsAtEnd() && this.Current.Kind != SyntaxKind.NewLineToken && this.Current.Kind != SyntaxKind.EndOfFileToken)
            {
                // 値あり: 空白をトリビアとして閉じコロンに付与
                closingColonToken = closingColonToken.WithTrivia(null, [whitespaceTrivia]);
                this._sink.EmitToken(closingColonToken);

                var valueToken = this.Current;
                this.Advance();

                // 改行をトリビアとして値トークンに付与
                if (this.Current.Kind == SyntaxKind.NewLineToken)
                {
                    var newLineTrivia = InternalTrivia.EndOfLine(this.Current.Text);
                    valueToken = valueToken.WithTrivia(null, [newLineTrivia]);
                    this.Advance();
                }

                this._sink.EmitToken(valueToken);
            }
            else
            {
                // 値なし（空白の後に改行または EOF）: 空白と改行をトリビアとして閉じコロンに付与
                List<InternalTrivia> triviaList = [whitespaceTrivia];

                if (this.Current.Kind == SyntaxKind.NewLineToken)
                {
                    triviaList.Add(InternalTrivia.EndOfLine(this.Current.Text));
                    this.Advance();
                }

                closingColonToken = closingColonToken.WithTrivia(null, [.. triviaList]);
                this._sink.EmitToken(closingColonToken);
            }
        }
        else
        {
            // 値なし: 改行をトリビアとして閉じコロンに付与
            if (this.Current.Kind == SyntaxKind.NewLineToken)
            {
                var newLineTrivia = InternalTrivia.EndOfLine(this.Current.Text);
                closingColonToken = closingColonToken.WithTrivia(null, [newLineTrivia]);
                this.Advance();
            }

            this._sink.EmitToken(closingColonToken);
        }

        this._sink.FinishNode();
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
