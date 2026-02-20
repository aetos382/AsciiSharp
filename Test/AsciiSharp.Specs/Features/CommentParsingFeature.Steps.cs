using System;
using System.Linq;

using AsciiSharp.Diagnostics;
using AsciiSharp.Syntax;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

public partial class CommentParsingFeature
{
    private string? _sourceText;
    private SyntaxTree? _syntaxTree;
    private string? _reconstructedText;

    private void パーサーが初期化されている()
    {
        // パーサーは初期化済み（特別な準備不要）
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

    private void 解析は成功する()
    {
        Assert.IsNotNull(_syntaxTree);
        var errors = _syntaxTree.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        Assert.AreEqual(0, errors.Count, $"解析エラーが発生しました: {string.Join(", ", errors.Select(e => e.Message))}");
    }

    private void 再構築されたテキストは元の文書と一致する()
    {
        Assert.IsNotNull(_sourceText);
        Assert.IsNotNull(_reconstructedText);
        Assert.AreEqual(_sourceText, _reconstructedText);
    }

    private void Documentノードは_N個の段落を持つ(int expectedCount)
    {
        Assert.IsNotNull(_syntaxTree);
        var paragraphs = _syntaxTree.Root.DescendantNodes().OfType<ParagraphSyntax>().ToList();
        Assert.AreEqual(expectedCount, paragraphs.Count);
    }

    private void Documentノードは_N個のセクションを持つ(int expectedCount)
    {
        Assert.IsNotNull(_syntaxTree);
        var document = _syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Body);
        var sections = document.Body.ChildNodesAndTokens()
            .Where(c => c.IsNode && c.AsNode()?.Kind == SyntaxKind.Section)
            .ToList();
        Assert.AreEqual(expectedCount, sections.Count);
    }

    private void 段落のテキストは_を含まない(string unexpectedText)
    {
        Assert.IsNotNull(_syntaxTree);
        var paragraph = _syntaxTree.Root.DescendantNodes().OfType<ParagraphSyntax>().FirstOrDefault();
        Assert.IsNotNull(paragraph);

        var tokenTexts = paragraph.DescendantTokens().Select(t => t.Text);
        var paragraphText = string.Concat(tokenTexts);
        Assert.IsFalse(paragraphText.Contains(unexpectedText, StringComparison.Ordinal));
    }

    private void Documentノードはタイトル_を持つ(string expectedTitle)
    {
        Assert.IsNotNull(_syntaxTree);
        var document = _syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Header);

        Assert.IsNotNull(document.Header.Title, "ドキュメントタイトルが存在するべきです。");
        var titleContent = document.Header.Title.GetTitleContent();
        Assert.AreEqual(expectedTitle, titleContent);
    }

    private void 構文木のトリビアに_が含まれる(string expectedText)
    {
        Assert.IsNotNull(_syntaxTree);
        var allTokens = _syntaxTree.Root.DescendantTokens();
        var allTrivia = allTokens.SelectMany(t => t.LeadingTrivia.Concat(t.TrailingTrivia)).ToList();
        var found = allTrivia.Any(t => t.ToFullString().Contains(expectedText, StringComparison.Ordinal));
        Assert.IsTrue(found, $"トリビアに '{expectedText}' が含まれていません");
    }

    private void 構文木に_を含むコメントがある(string expectedText)
    {
        Assert.IsNotNull(_syntaxTree);
        var allTokens = _syntaxTree.Root.DescendantTokens();
        var commentTrivia = allTokens
            .SelectMany(t => t.LeadingTrivia.Concat(t.TrailingTrivia))
            .Where(t => t.Kind is SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia)
            .ToList();

        var found = commentTrivia.Any(t => t.ToFullString().Contains(expectedText, StringComparison.Ordinal));
        Assert.IsTrue(found, $"コメントに '{expectedText}' が含まれていません");
    }

    private void 構文木に_N個の単一行コメントがある(int expectedCount)
    {
        Assert.IsNotNull(_syntaxTree);
        var allTokens = _syntaxTree.Root.DescendantTokens();
        var singleLineComments = allTokens
            .SelectMany(t => t.LeadingTrivia.Concat(t.TrailingTrivia))
            .Where(t => t.Kind == SyntaxKind.SingleLineCommentTrivia)
            .ToList();
        Assert.AreEqual(expectedCount, singleLineComments.Count);
    }

    private void 構文木に_N個のブロックコメントがある(int expectedCount)
    {
        Assert.IsNotNull(_syntaxTree);
        var allTokens = _syntaxTree.Root.DescendantTokens();
        var blockComments = allTokens
            .SelectMany(t => t.LeadingTrivia.Concat(t.TrailingTrivia))
            .Where(t => t.Kind == SyntaxKind.MultiLineCommentTrivia)
            .ToList();
        Assert.AreEqual(expectedCount, blockComments.Count);
    }

    private void LinkノードのターゲットURLは(string expectedUrl)
    {
        Assert.IsNotNull(_syntaxTree);
        var link = _syntaxTree.Root.DescendantNodes().OfType<LinkSyntax>().FirstOrDefault();
        Assert.IsNotNull(link);
        Assert.AreEqual(expectedUrl, link.Url);
    }
}
