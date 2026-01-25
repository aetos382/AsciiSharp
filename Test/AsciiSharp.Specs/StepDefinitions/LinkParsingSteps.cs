
using System.Linq;

using AsciiSharp.Syntax;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reqnroll;

namespace AsciiSharp.Specs.StepDefinitions;

/// <summary>
/// リンク解析機能のステップ定義。
/// </summary>
[Binding]
public sealed class LinkParsingSteps
{
    private readonly BasicParsingSteps _basicParsingSteps;

    /// <summary>
    /// コンストラクタ。依存するステップ定義を注入。
    /// </summary>
    /// <param name="basicParsingSteps">基本パーサーのステップ定義。</param>
    public LinkParsingSteps(BasicParsingSteps basicParsingSteps)
    {
        this._basicParsingSteps = basicParsingSteps;
    }

    [Then(@"構文木に Link ノードが含まれる")]
    public void Then構文木にLinkノードが含まれる()
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree);
        var links = syntaxTree.Root.DescendantNodes().OfType<LinkSyntax>().ToList();
        Assert.IsNotEmpty(links, "Link ノードが見つかりません");
    }

    [Then(@"構文木に (\d+) 個の Link ノードが含まれる")]
    public void Then構文木に個のLinkノードが含まれる(int expectedCount)
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree);
        var links = syntaxTree.Root.DescendantNodes().OfType<LinkSyntax>().ToList();
        Assert.HasCount(expectedCount, links, $"Link ノードの数が一致しません。期待: {expectedCount}, 実際: {links.Count}");
    }

    // CA1054: テストステップのパラメータは feature ファイルからの文字列リテラル
#pragma warning disable CA1054
    [Then(@"Link ノードのターゲット URL は ""(.+)"" である")]
    public void ThenLinkノードのターゲットURLは_である(string expectedUrl)
#pragma warning restore CA1054
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree);
        var link = syntaxTree.Root.DescendantNodes().OfType<LinkSyntax>().FirstOrDefault();
        Assert.IsNotNull(link, "Link ノードが見つかりません");
        Assert.AreEqual(expectedUrl, link.Url, $"URL が一致しません。期待: '{expectedUrl}', 実際: '{link.Url}'");
    }

    [Then(@"Link ノードの表示テキストは ""(.+)"" である")]
    public void ThenLinkノードの表示テキストは_である(string expectedText)
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree);
        var link = syntaxTree.Root.DescendantNodes().OfType<LinkSyntax>().FirstOrDefault();
        Assert.IsNotNull(link, "Link ノードが見つかりません");
        Assert.AreEqual(expectedText, link.DisplayText, $"表示テキストが一致しません。期待: '{expectedText}', 実際: '{link.DisplayText}'");
    }

    // CA1054: テストステップのパラメータは feature ファイルからの文字列リテラル
#pragma warning disable CA1054
    [Then(@"(\d+) 番目の Link ノードのターゲット URL は ""(.+)"" である")]
    public void Then番目のLinkノードのターゲットURLは_である(int index, string expectedUrl)
#pragma warning restore CA1054
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree);
        var links = syntaxTree.Root.DescendantNodes().OfType<LinkSyntax>().ToList();
        Assert.IsTrue(index >= 1 && index <= links.Count, $"Link インデックス {index} は範囲外です。実際の Link 数: {links.Count}");
        var link = links[index - 1];
        Assert.AreEqual(expectedUrl, link.Url, $"{index} 番目の Link の URL が一致しません。期待: '{expectedUrl}', 実際: '{link.Url}'");
    }
}
