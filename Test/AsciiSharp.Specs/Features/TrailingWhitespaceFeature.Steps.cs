using System.Linq;

using AsciiSharp.Syntax;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

public partial class TrailingWhitespaceFeature
{
    private string? _sourceText;
    private SyntaxTree? _syntaxTree;
    private string? _reconstructedText;

    private void パーサーが初期化されている()
    {
        // 特に初期化処理は不要
    }

    private void 以下のAsciiDoc文書がある(string text)
    {
        _sourceText = text;
    }

    private void 文書を解析する()
    {
        Assert.IsNotNull(_sourceText);
        _syntaxTree = SyntaxTree.ParseText(_sourceText);
    }

    private void 構文木から完全なテキストを取得する()
    {
        Assert.IsNotNull(_syntaxTree);
        _reconstructedText = _syntaxTree.Root.ToFullString();
    }

    private void 再構築されたテキストは元の文書と一致する()
    {
        Assert.IsNotNull(_sourceText);
        Assert.IsNotNull(_reconstructedText);
        Assert.AreEqual(_sourceText, _reconstructedText);
    }

    private void セクションタイトルの最終コンテンツトークンの後続トリビアにWhitespaceTriviaとEndOfLineTriviaが含まれる()
    {
        var sectionTitle = 最初のセクションタイトルを取得();
        var lastToken = sectionTitle.DescendantTokens().Last();
        var trivia = lastToken.TrailingTrivia;

        Assert.AreEqual(2, trivia.Count, "後続トリビアは WhitespaceTrivia と EndOfLineTrivia の 2 つであるべきです。");
        Assert.AreEqual(SyntaxKind.WhitespaceTrivia, trivia[0].Kind);
        Assert.AreEqual(SyntaxKind.EndOfLineTrivia, trivia[1].Kind);
    }

    private void セクションタイトルの最終コンテンツトークンの後続トリビアにEndOfLineTriviaのみが含まれる()
    {
        var sectionTitle = 最初のセクションタイトルを取得();
        var lastToken = sectionTitle.DescendantTokens().Last();
        var trivia = lastToken.TrailingTrivia;

        Assert.AreEqual(1, trivia.Count, "後続トリビアは EndOfLineTrivia の 1 つのみであるべきです。");
        Assert.AreEqual(SyntaxKind.EndOfLineTrivia, trivia[0].Kind);
    }

    private void セクションタイトルの最終コンテンツトークンの後続トリビアにCRLFを含むEndOfLineTriviaが含まれる()
    {
        var sectionTitle = 最初のセクションタイトルを取得();
        var lastToken = sectionTitle.DescendantTokens().Last();
        var trivia = lastToken.TrailingTrivia;

        var endOfLineTrivia = trivia.FirstOrDefault(t => t.Kind == SyntaxKind.EndOfLineTrivia);
        Assert.AreEqual(SyntaxKind.EndOfLineTrivia, endOfLineTrivia.Kind, "EndOfLineTrivia が存在するべきです。");
        Assert.AreEqual("\r\n", endOfLineTrivia.Text, "EndOfLineTrivia のテキストは CRLF であるべきです。");
    }

    private void 著者行の最終コンテンツトークンの後続トリビアにWhitespaceTriviaとEndOfLineTriviaが含まれる()
    {
        Assert.IsNotNull(_syntaxTree);
        var document = _syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document);

        var authorLine = document.Header?.AuthorLine;
        Assert.IsNotNull(authorLine, "著者行が存在するべきです。");

        var lastToken = authorLine.DescendantTokens().Last();
        var trivia = lastToken.TrailingTrivia;

        Assert.AreEqual(2, trivia.Count, "後続トリビアは WhitespaceTrivia と EndOfLineTrivia の 2 つであるべきです。");
        Assert.AreEqual(SyntaxKind.WhitespaceTrivia, trivia[0].Kind);
        Assert.AreEqual(SyntaxKind.EndOfLineTrivia, trivia[1].Kind);
    }

    /// <summary>
    /// 構文木から最初のセクションタイトルを取得する。
    /// ヘッダーのタイトルを優先し、なければ本文の最初のセクションのタイトルを返す。
    /// </summary>
    private SectionTitleSyntax 最初のセクションタイトルを取得()
    {
        Assert.IsNotNull(_syntaxTree);
        var document = _syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document);

        SectionTitleSyntax? sectionTitle = document.Header?.Title;
        if (sectionTitle is null)
        {
            var firstSection = document.Body?.ChildNodesAndTokens()
                .Where(c => c.IsNode && c.AsNode()?.Kind == SyntaxKind.Section)
                .Select(c => c.AsNode() as SectionSyntax)
                .FirstOrDefault();
            sectionTitle = firstSection?.Title;
        }

        Assert.IsNotNull(sectionTitle, "セクションタイトルが存在するべきです。");
        return sectionTitle;
    }
}
