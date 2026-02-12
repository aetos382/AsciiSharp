using System;
using System.Linq;

using AsciiSharp.Diagnostics;
using AsciiSharp.Syntax;

using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

/// <summary>
/// コメント解析に関する BDD テストです。
/// </summary>
[TestClass]
[FeatureDescription(
    @"コメント解析
ライブラリユーザーとして、
AsciiDoc のコメントを正しく解析し、
ラウンドトリップが可能な構文木を取得したい")]
public partial class CommentParsingFeature : FeatureFixture
{
    private string? _sourceText;
    private SyntaxTree? _tree;
    private string? _reconstructedText;

    [Scenario]
    [TestMethod]
    public void 単一行コメントを含む文書の解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("= タイトル\n\n// これはコメント\n本文テキスト\n"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 解析は成功する(),
            and => 再構築されたテキストは元の文書と一致する(),
            and => Documentノードは_N個の段落を持つ(1),
            and => 段落のテキストは_を含まない("// これはコメント"),
            and => 構文木に_を含むコメントがある("これはコメント"),
            and => 構文木に_N個の単一行コメントがある(1));
    }

    [Scenario]
    [TestMethod]
    public void ブロックコメントを含む文書の解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("= タイトル\n\n////\nブロック\nコメント\n////\n本文テキスト\n"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 解析は成功する(),
            and => 再構築されたテキストは元の文書と一致する(),
            and => Documentノードは_N個の段落を持つ(1),
            and => 構文木に_を含むコメントがある("ブロック"),
            and => 構文木に_を含むコメントがある("コメント"),
            and => 構文木に_N個のブロックコメントがある(1));
    }

    [Scenario]
    [TestMethod]
    public void URL内のスラッシュはコメントとして扱わない()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("https://example.com/path\n"),
            and => 文書を解析する(),
            then => 解析は成功する(),
            and => LinkノードのターゲットURLは("https://example.com/path"));
    }

    [Scenario]
    [TestMethod]
    public void セクション内の単一行コメント()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("= タイトル\n\n== セクション 1\n// セクション内コメント\n段落テキスト\n\n== セクション 2\n別の段落\n"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 解析は成功する(),
            and => 再構築されたテキストは元の文書と一致する(),
            and => Documentノードは_N個のセクションを持つ(2),
            and => 構文木に_を含むコメントがある("セクション内コメント"),
            and => 構文木に_N個の単一行コメントがある(1));
    }

    [Scenario]
    [TestMethod]
    public void 冒頭に単一行コメントがある文書の解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("// 冒頭のコメント\n= タイトル\n\n本文テキスト\n"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 解析は成功する(),
            and => 再構築されたテキストは元の文書と一致する(),
            and => Documentノードはタイトル_を持つ("タイトル"),
            and => Documentノードは_N個の段落を持つ(1),
            and => 構文木のトリビアに_が含まれる("// 冒頭のコメント"),
            and => 構文木に_を含むコメントがある("冒頭のコメント"),
            and => 構文木に_N個の単一行コメントがある(1));
    }

    [Scenario]
    [TestMethod]
    public void 冒頭にブロックコメントがある文書の解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("////\n冒頭の\nブロックコメント\n////\n= タイトル\n\n本文テキスト\n"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 解析は成功する(),
            and => 再構築されたテキストは元の文書と一致する(),
            and => Documentノードはタイトル_を持つ("タイトル"),
            and => Documentノードは_N個の段落を持つ(1),
            and => 構文木のトリビアに_が含まれる("冒頭の"),
            and => 構文木に_を含むコメントがある("ブロックコメント"),
            and => 構文木に_N個のブロックコメントがある(1));
    }

    [Scenario]
    [TestMethod]
    public void 冒頭に複数のコメントがある文書の解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("// 最初のコメント\n// 2番目のコメント\n= タイトル\n\n本文テキスト\n"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 解析は成功する(),
            and => 再構築されたテキストは元の文書と一致する(),
            and => Documentノードはタイトル_を持つ("タイトル"),
            and => Documentノードは_N個の段落を持つ(1),
            and => 構文木のトリビアに_が含まれる("// 最初のコメント"),
            and => 構文木のトリビアに_が含まれる("// 2番目のコメント"),
            and => 構文木に_を含むコメントがある("最初のコメント"),
            and => 構文木に_を含むコメントがある("2番目のコメント"),
            and => 構文木に_N個の単一行コメントがある(2));
    }

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
        _tree = SyntaxTree.ParseText(_sourceText);
    }

    private void 構文木から完全なテキストを取得する()
    {
        Assert.IsNotNull(_tree);
        _reconstructedText = _tree.Root.ToFullString();
    }

    private void 解析は成功する()
    {
        Assert.IsNotNull(_tree);
        var errors = _tree.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
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
        Assert.IsNotNull(_tree);
        var paragraphs = _tree.Root.DescendantNodes().OfType<ParagraphSyntax>().ToList();
        Assert.AreEqual(expectedCount, paragraphs.Count);
    }

    private void Documentノードは_N個のセクションを持つ(int expectedCount)
    {
        Assert.IsNotNull(_tree);
        var document = _tree.Root as DocumentSyntax;
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Body);
        var sections = document.Body.ChildNodesAndTokens()
            .Where(c => c.IsNode && c.AsNode()?.Kind == SyntaxKind.Section)
            .ToList();
        Assert.AreEqual(expectedCount, sections.Count);
    }

    private void 段落のテキストは_を含まない(string unexpectedText)
    {
        Assert.IsNotNull(_tree);
        var paragraph = _tree.Root.DescendantNodes().OfType<ParagraphSyntax>().FirstOrDefault();
        Assert.IsNotNull(paragraph);

        var tokenTexts = paragraph.DescendantTokens().Select(t => t.Text);
        var paragraphText = string.Concat(tokenTexts);
        Assert.IsFalse(paragraphText.Contains(unexpectedText, StringComparison.Ordinal));
    }

    private void Documentノードはタイトル_を持つ(string expectedTitle)
    {
        Assert.IsNotNull(_tree);
        var document = _tree.Root as DocumentSyntax;
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Header);

        var titleContent = document.Header.Title.GetTitleContent();
        Assert.AreEqual(expectedTitle, titleContent);
    }

    private void 構文木のトリビアに_が含まれる(string expectedText)
    {
        Assert.IsNotNull(_tree);
        var allTokens = _tree.Root.DescendantTokens();
        var allTrivia = allTokens.SelectMany(t => t.LeadingTrivia.Concat(t.TrailingTrivia)).ToList();
        var found = allTrivia.Any(t => t.ToFullString().Contains(expectedText, StringComparison.Ordinal));
        Assert.IsTrue(found, $"トリビアに '{expectedText}' が含まれていません");
    }

    private void 構文木に_を含むコメントがある(string expectedText)
    {
        Assert.IsNotNull(_tree);
        var allTokens = _tree.Root.DescendantTokens();
        var commentTrivia = allTokens
            .SelectMany(t => t.LeadingTrivia.Concat(t.TrailingTrivia))
            .Where(t => t.Kind is SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia)
            .ToList();

        var found = commentTrivia.Any(t => t.ToFullString().Contains(expectedText, StringComparison.Ordinal));
        Assert.IsTrue(found, $"コメントに '{expectedText}' が含まれていません");
    }

    private void 構文木に_N個の単一行コメントがある(int expectedCount)
    {
        Assert.IsNotNull(_tree);
        var allTokens = _tree.Root.DescendantTokens();
        var singleLineComments = allTokens
            .SelectMany(t => t.LeadingTrivia.Concat(t.TrailingTrivia))
            .Where(t => t.Kind == SyntaxKind.SingleLineCommentTrivia)
            .ToList();
        Assert.AreEqual(expectedCount, singleLineComments.Count);
    }

    private void 構文木に_N個のブロックコメントがある(int expectedCount)
    {
        Assert.IsNotNull(_tree);
        var allTokens = _tree.Root.DescendantTokens();
        var blockComments = allTokens
            .SelectMany(t => t.LeadingTrivia.Concat(t.TrailingTrivia))
            .Where(t => t.Kind == SyntaxKind.MultiLineCommentTrivia)
            .ToList();
        Assert.AreEqual(expectedCount, blockComments.Count);
    }

    private void LinkノードのターゲットURLは(string expectedUrl)
    {
        Assert.IsNotNull(_tree);
        var link = _tree.Root.DescendantNodes().OfType<LinkSyntax>().FirstOrDefault();
        Assert.IsNotNull(link);
        Assert.AreEqual(expectedUrl, link.Url);
    }
}
