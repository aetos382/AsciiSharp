using System.Linq;

using AsciiSharp.Syntax;
using AsciiSharp.Text;

using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

/// <summary>
/// リンクの解析機能のテスト
/// </summary>
[TestClass]
[FeatureDescription(
    @"リンクの解析
URL リンクを含むテキストを解析し、
リンクの位置とターゲットを正確に把握できる")]
public sealed partial class LinkParsingFeature : FeatureFixture
{
    private SourceText? _sourceText;
    private SyntaxTree? _syntaxTree;

    [Scenario]
    public void URLリンクを含む段落の解析()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("これは https://example.com へのリンクです。\n"),
            when => 文書を解析する(),
            then => 構文木が生成される(),
            then => 構文木にLinkノードが含まれる(),
            then => LinkノードのターゲットURLは("https://example.com")
        );
    }

    [Scenario]
    public void 表示テキスト付きリンクの解析()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("詳細は https://example.com[公式サイト] を参照してください。\n"),
            when => 文書を解析する(),
            then => 構文木にLinkノードが含まれる(),
            then => LinkノードのターゲットURLは("https://example.com"),
            then => LinkノードのDisplayTextは("公式サイト")
        );
    }

    [Scenario]
    public void 複数のリンクを含む段落の解析()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("https://example1.com と https://example2.com の両方を参照してください。\n"),
            when => 文書を解析する(),
            then => 構文木に_N個のLinkノードが含まれる(2),
            then => N番目のLinkノードのターゲットURLは(1, "https://example1.com"),
            then => N番目のLinkノードのターゲットURLは(2, "https://example2.com")
        );
    }

    [Scenario]
    public void リンクを含む文書のラウンドトリップ()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("= ドキュメント\n\nリンク: https://example.com[Example]\n"),
            when => 文書を解析する(),
            then => 構文木から復元したテキストは元の文書と一致する()
        );
    }

    private void 以下のAsciiDoc文書がある(string text)
    {
        _sourceText = SourceText.From(text);
    }

    private void 文書を解析する()
    {
        Assert.IsNotNull(_sourceText);
        _syntaxTree = SyntaxTree.ParseText(_sourceText);
        Assert.IsNotNull(_syntaxTree);
    }

    private void 構文木が生成される()
    {
        Assert.IsNotNull(_syntaxTree);
    }

    private void 構文木にLinkノードが含まれる()
    {
        Assert.IsNotNull(_syntaxTree);
        var links = _syntaxTree.Root.DescendantNodes().OfType<LinkSyntax>();
        Assert.IsTrue(links.Any(), "Linkノードが見つかりませんでした");
    }

#pragma warning disable CA1054 // URI-like parameters should not be strings
    private void LinkノードのターゲットURLは(string expectedUrl)
#pragma warning restore CA1054 // URI-like parameters should not be strings
    {
        Assert.IsNotNull(_syntaxTree);
        var link = _syntaxTree.Root.DescendantNodes().OfType<LinkSyntax>().FirstOrDefault();
        Assert.IsNotNull(link);
        Assert.AreEqual(expectedUrl, link.Url);
    }

    private void LinkノードのDisplayTextは(string expectedText)
    {
        Assert.IsNotNull(_syntaxTree);
        var link = _syntaxTree.Root.DescendantNodes().OfType<LinkSyntax>().FirstOrDefault();
        Assert.IsNotNull(link);

        var displayText = link.DisplayText?.ToString();
        Assert.AreEqual(expectedText, displayText);
    }

    private void 構文木に_N個のLinkノードが含まれる(int expectedCount)
    {
        Assert.IsNotNull(_syntaxTree);
        var links = _syntaxTree.Root.DescendantNodes().OfType<LinkSyntax>().ToList();
        Assert.HasCount(expectedCount, links);
    }

#pragma warning disable CA1054 // URI-like parameters should not be strings
    private void N番目のLinkノードのターゲットURLは(int index, string expectedUrl)
#pragma warning restore CA1054 // URI-like parameters should not be strings
    {
        Assert.IsNotNull(_syntaxTree);
        var links = _syntaxTree.Root.DescendantNodes().OfType<LinkSyntax>().ToArray();
        Assert.IsTrue(index > 0 && index <= links.Length, $"Linkノード {index} が存在しません");

        var link = links[index - 1];
        Assert.AreEqual(expectedUrl, link.Url);
    }

    private void 構文木から復元したテキストは元の文書と一致する()
    {
        Assert.IsNotNull(_syntaxTree);
        Assert.IsNotNull(_sourceText);

        var reconstructed = _syntaxTree.Root.ToFullString();
        var original = _sourceText.ToString();

        Assert.AreEqual(original, reconstructed);
    }
}
