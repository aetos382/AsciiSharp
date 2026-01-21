namespace AsciiSharp.Parser;

using System;
using System.Collections.Generic;
using AsciiSharp.Diagnostics;
using AsciiSharp.InternalSyntax;
using AsciiSharp.Text;

/// <summary>
/// AsciiDoc 文書の構文解析を行うクラス。
/// </summary>
internal sealed class AsciiDocParser
{
    private readonly Lexer _lexer;
    private readonly ITreeSink _sink;
    private readonly List<Diagnostic> _diagnostics = [];
    private InternalToken _currentToken;
    private InternalToken? _peekedToken;

    /// <summary>
    /// AsciiDocParser を作成する。
    /// </summary>
    /// <param name="lexer">字句解析器。</param>
    /// <param name="sink">構文木構築用のシンク。</param>
    public AsciiDocParser(Lexer lexer, ITreeSink sink)
    {
        _lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));
        _sink = sink ?? throw new ArgumentNullException(nameof(sink));
        _currentToken = _lexer.NextToken();
    }

    /// <summary>
    /// 診断情報のリスト。
    /// </summary>
    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

    /// <summary>
    /// 現在のトークン。
    /// </summary>
    private InternalToken Current => _currentToken;

    /// <summary>
    /// 次のトークンを先読みする。
    /// </summary>
    private InternalToken Peek()
    {
        if (_peekedToken is null)
        {
            _peekedToken = _lexer.NextToken();
        }

        return _peekedToken;
    }

    /// <summary>
    /// 次のトークンに進む。
    /// </summary>
    private void Advance()
    {
        if (_peekedToken is not null)
        {
            _currentToken = _peekedToken;
            _peekedToken = null;
        }
        else
        {
            _currentToken = _lexer.NextToken();
        }
    }

    /// <summary>
    /// 文書全体を解析する。
    /// </summary>
    public void ParseDocument()
    {
        _sink.StartNode(SyntaxKind.Document);

        // ヘッダーの解析を試みる
        if (IsAtDocumentTitle())
        {
            ParseDocumentHeader();
        }

        // ボディの解析
        ParseDocumentBody();

        // EOF トークン
        EmitToken(SyntaxKind.EndOfFileToken);

        _sink.FinishNode();
    }

    /// <summary>
    /// ドキュメントヘッダーを解析する。
    /// </summary>
    private void ParseDocumentHeader()
    {
        _sink.StartNode(SyntaxKind.DocumentHeader);

        // タイトル行を解析
        ParseSectionTitle();

        // 著者行があれば解析
        if (!IsAtEnd() && !IsBlankLine() && !IsAtSectionTitle())
        {
            ParseAuthorLine();
        }

        // 空行をスキップ
        SkipBlankLines();

        _sink.FinishNode();
    }

    /// <summary>
    /// 著者行を解析する。
    /// </summary>
    private void ParseAuthorLine()
    {
        _sink.StartNode(SyntaxKind.AuthorLine);

        // 行末まで読み取る
        while (!IsAtEnd() && Current.Kind != SyntaxKind.NewLineToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            EmitCurrentToken();
        }

        // 改行を含める
        if (Current.Kind == SyntaxKind.NewLineToken)
        {
            EmitCurrentToken();
        }

        _sink.FinishNode();
    }

    /// <summary>
    /// ドキュメントボディを解析する。
    /// </summary>
    private void ParseDocumentBody()
    {
        _sink.StartNode(SyntaxKind.DocumentBody);

        while (!IsAtEnd())
        {
            if (IsBlankLine())
            {
                SkipBlankLines();
            }
            else if (IsAtSectionTitle())
            {
                ParseSection();
            }
            else
            {
                ParseParagraph();
            }
        }

        _sink.FinishNode();
    }

    /// <summary>
    /// セクションを解析する。
    /// </summary>
    private void ParseSection()
    {
        _sink.StartNode(SyntaxKind.Section);

        var currentLevel = CountEqualsAtLineStart();

        // セクションタイトルを解析
        ParseSectionTitle();

        // 空行をスキップ
        SkipBlankLines();

        // セクションの内容を解析
        while (!IsAtEnd() && !IsAtSectionTitleOfLevelOrHigher(currentLevel))
        {
            if (IsBlankLine())
            {
                SkipBlankLines();
            }
            else if (IsAtSectionTitle())
            {
                // サブセクション
                ParseSection();
            }
            else
            {
                ParseParagraph();
            }
        }

        _sink.FinishNode();
    }

    /// <summary>
    /// セクションタイトルを解析する。
    /// </summary>
    private void ParseSectionTitle()
    {
        _sink.StartNode(SyntaxKind.SectionTitle);

        // = を読み取る
        while (Current.Kind == SyntaxKind.EqualsToken)
        {
            EmitCurrentToken();
        }

        // 空白を読み取る
        if (Current.Kind == SyntaxKind.WhitespaceToken)
        {
            EmitCurrentToken();
        }

        // タイトルテキストを読み取る
        while (!IsAtEnd() && Current.Kind != SyntaxKind.NewLineToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            EmitCurrentToken();
        }

        // 改行を読み取る
        if (Current.Kind == SyntaxKind.NewLineToken)
        {
            EmitCurrentToken();
        }

        _sink.FinishNode();
    }

    /// <summary>
    /// 段落を解析する。
    /// </summary>
    private void ParseParagraph()
    {
        _sink.StartNode(SyntaxKind.Paragraph);

        // 段落の行を読み取る（空行またはセクションタイトルまで）
        while (!IsAtEnd() && !IsBlankLine() && !IsAtSectionTitle())
        {
            // 行の内容を読み取る
            while (!IsAtEnd() && Current.Kind != SyntaxKind.NewLineToken && Current.Kind != SyntaxKind.EndOfFileToken)
            {
                EmitCurrentToken();
            }

            // 改行を読み取る
            if (Current.Kind == SyntaxKind.NewLineToken)
            {
                EmitCurrentToken();
            }
        }

        _sink.FinishNode();
    }

    /// <summary>
    /// 空行をスキップする。
    /// </summary>
    private void SkipBlankLines()
    {
        while (IsBlankLine())
        {
            // 空白をスキップ
            while (Current.Kind == SyntaxKind.WhitespaceToken)
            {
                EmitCurrentToken();
            }

            // 改行をスキップ
            if (Current.Kind == SyntaxKind.NewLineToken)
            {
                EmitCurrentToken();
            }
        }
    }

    /// <summary>
    /// 現在のトークンを出力して次へ進む。
    /// </summary>
    private void EmitCurrentToken()
    {
        _sink.Token(Current.Kind, Current.Text);

        // トリビアを出力
        foreach (var trivia in Current.LeadingTrivia)
        {
            _sink.LeadingTrivia(trivia.Kind, trivia.Text);
        }

        foreach (var trivia in Current.TrailingTrivia)
        {
            _sink.TrailingTrivia(trivia.Kind, trivia.Text);
        }

        Advance();
    }

    /// <summary>
    /// 指定された種別のトークンを期待して出力する。
    /// </summary>
    private void EmitToken(SyntaxKind expectedKind)
    {
        if (Current.Kind == expectedKind)
        {
            EmitCurrentToken();
        }
        else if (expectedKind == SyntaxKind.EndOfFileToken && IsAtEnd())
        {
            _sink.Token(SyntaxKind.EndOfFileToken, string.Empty);
        }
        else
        {
            // エラー: 期待されるトークンがない
            _sink.MissingToken(expectedKind);
            _sink.Error("ADS0001", $"期待されるトークン '{expectedKind}' がありません。実際: '{Current.Kind}'");
            _diagnostics.Add(new Diagnostic(
                "ADS0001",
                $"期待されるトークン '{expectedKind}' がありません。",
                DiagnosticSeverity.Error,
                new TextSpan(_lexer.Position, 0)));
        }
    }

    /// <summary>
    /// ファイル終端に達したかどうか。
    /// </summary>
    private bool IsAtEnd()
    {
        return Current.Kind == SyntaxKind.EndOfFileToken;
    }

    /// <summary>
    /// 現在位置がドキュメントタイトル（レベル1セクション）かどうか。
    /// </summary>
    private bool IsAtDocumentTitle()
    {
        return Current.Kind == SyntaxKind.EqualsToken && Peek().Kind != SyntaxKind.EqualsToken;
    }

    /// <summary>
    /// 現在位置がセクションタイトルかどうか。
    /// </summary>
    private bool IsAtSectionTitle()
    {
        return Current.Kind == SyntaxKind.EqualsToken;
    }

    /// <summary>
    /// 現在位置が指定レベル以上のセクションタイトルかどうか。
    /// </summary>
    private bool IsAtSectionTitleOfLevelOrHigher(int level)
    {
        if (Current.Kind != SyntaxKind.EqualsToken)
        {
            return false;
        }

        var equalsCount = CountEqualsAtLineStart();
        return equalsCount <= level;
    }

    /// <summary>
    /// 行頭の = の数を数える。
    /// </summary>
    private int CountEqualsAtLineStart()
    {
        if (Current.Kind != SyntaxKind.EqualsToken)
        {
            return 0;
        }

        var count = 1;
        var token = Peek();
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
        return Current.Kind == SyntaxKind.NewLineToken ||
               (Current.Kind == SyntaxKind.WhitespaceToken && Peek().Kind == SyntaxKind.NewLineToken);
    }
}
