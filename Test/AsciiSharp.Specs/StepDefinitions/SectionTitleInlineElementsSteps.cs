
using System.Linq;

using AsciiSharp.Syntax;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reqnroll;

namespace AsciiSharp.Specs.StepDefinitions;

/// <summary>
/// SectionTitleSyntax の InlineElements に関するステップ定義。
/// </summary>
[Binding]
public sealed class SectionTitleInlineElementsSteps
{
    private readonly BasicParsingSteps _basicParsingSteps;

    /// <summary>
    /// SectionTitleInlineElementsSteps を作成する。
    /// </summary>
    public SectionTitleInlineElementsSteps(BasicParsingSteps basicParsingSteps)
    {
        this._basicParsingSteps = basicParsingSteps;
    }

    [Then(@"最初のセクションタイトルの InlineElements は (\d+) 個である")]
    public void Then最初のセクションタイトルのInlineElementsは個である(int expectedCount)
    {
        var tree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(tree, "構文木が null です。");

        var document = tree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        var sectionTitle = GetFirstSectionTitle(document);
        Assert.IsNotNull(sectionTitle, "セクションタイトルが見つかりません。");

        Assert.HasCount(expectedCount, sectionTitle.InlineElements,
            $"InlineElements の数が一致しません。期待: {expectedCount}, 実際: {sectionTitle.InlineElements.Length}");
    }

    [Then(@"最初のセクションタイトルの最初のインライン要素は InlineTextSyntax である")]
    public void Then最初のセクションタイトルの最初のインライン要素はInlineTextSyntaxである()
    {
        var tree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(tree, "構文木が null です。");

        var document = tree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        var sectionTitle = GetFirstSectionTitle(document);
        Assert.IsNotNull(sectionTitle, "セクションタイトルが見つかりません。");
        Assert.IsGreaterThan(0, sectionTitle.InlineElements.Length, "InlineElements が空です。");

        var firstElement = sectionTitle.InlineElements[0];
        Assert.IsInstanceOfType<InlineTextSyntax>(firstElement,
            $"最初のインライン要素は InlineTextSyntax である必要があります。実際の型: {firstElement.GetType().Name}");
    }

    [Then(@"最初のセクションタイトルの最初のインライン要素のテキストは ""(.*)"" である")]
    public void Then最初のセクションタイトルの最初のインライン要素のテキストはである(string expectedText)
    {
        var tree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(tree, "構文木が null です。");

        var document = tree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        var sectionTitle = GetFirstSectionTitle(document);
        Assert.IsNotNull(sectionTitle, "セクションタイトルが見つかりません。");
        Assert.IsGreaterThan(0, sectionTitle.InlineElements.Length, "InlineElements が空です。");

        var firstElement = sectionTitle.InlineElements[0] as InlineTextSyntax;
        Assert.IsNotNull(firstElement, "最初のインライン要素は InlineTextSyntax である必要があります。");

        Assert.AreEqual(expectedText, firstElement.Text,
            $"テキストが一致しません。期待: '{expectedText}', 実際: '{firstElement.Text}'");
    }

    [Then(@"ドキュメントタイトルの InlineElements は (\d+) 個である")]
    public void ThenドキュメントタイトルのInlineElementsは個である(int expectedCount)
    {
        var tree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(tree, "構文木が null です。");

        var document = tree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");
        Assert.IsNotNull(document.Header, "Document は Header を持つ必要があります。");
        Assert.IsNotNull(document.Header.Title, "Header は Title を持つ必要があります。");

        Assert.HasCount(expectedCount, document.Header.Title.InlineElements,
            $"InlineElements の数が一致しません。期待: {expectedCount}, 実際: {document.Header.Title.InlineElements.Length}");
    }

    [Then(@"ドキュメントタイトルの最初のインライン要素のテキストは ""(.*)"" である")]
    public void Thenドキュメントタイトルの最初のインライン要素のテキストはである(string expectedText)
    {
        var tree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(tree, "構文木が null です。");

        var document = tree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");
        Assert.IsNotNull(document.Header, "Document は Header を持つ必要があります。");
        Assert.IsNotNull(document.Header.Title, "Header は Title を持つ必要があります。");
        Assert.IsGreaterThan(0, document.Header.Title.InlineElements.Length, "InlineElements が空です。");

        var firstElement = document.Header.Title.InlineElements[0] as InlineTextSyntax;
        Assert.IsNotNull(firstElement, "最初のインライン要素は InlineTextSyntax である必要があります。");

        Assert.AreEqual(expectedText, firstElement.Text,
            $"テキストが一致しません。期待: '{expectedText}', 実際: '{firstElement.Text}'");
    }

    [Then(@"最初のセクションタイトルの InlineElements は構文上の出現順に並んでいる")]
    public void Then最初のセクションタイトルのInlineElementsは構文上の出現順に並んでいる()
    {
        var tree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(tree, "構文木が null です。");

        var document = tree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        var sectionTitle = GetFirstSectionTitle(document);
        Assert.IsNotNull(sectionTitle, "セクションタイトルが見つかりません。");

        // 各要素の Position が前の要素以上であることを確認
        for (var i = 1; i < sectionTitle.InlineElements.Length; i++)
        {
            var prev = sectionTitle.InlineElements[i - 1];
            var curr = sectionTitle.InlineElements[i];

            Assert.IsGreaterThanOrEqualTo(prev.Position,
curr.Position, $"InlineElements[{i}].Position ({curr.Position}) が InlineElements[{i - 1}].Position ({prev.Position}) より小さいです。");
        }
    }

    [Then(@"セクション (\d+) のタイトルの最初のインライン要素のテキストは ""(.*)"" である")]
    public void Thenセクションのタイトルの最初のインライン要素のテキストはである(int sectionIndex, string expectedText)
    {
        var tree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(tree, "構文木が null です。");

        var document = tree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        var sections = document.Body?.ChildNodesAndTokens()
            .Where(c => c.IsNode && c.AsNode()?.Kind == SyntaxKind.Section)
            .Select(c => c.AsNode() as SectionSyntax)
            .ToList();

        Assert.IsNotNull(sections, "セクションリストが取得できません。");
        Assert.IsTrue(sectionIndex >= 1 && sectionIndex <= sections.Count, $"セクションインデックス {sectionIndex} は範囲外です。");

        var section = sections[sectionIndex - 1];
        Assert.IsNotNull(section, $"セクション {sectionIndex} が null です。");
        Assert.IsNotNull(section.Title, $"セクション {sectionIndex} のタイトルが null です。");
        Assert.IsGreaterThan(0, section.Title.InlineElements.Length, "InlineElements が空です。");

        var firstElement = section.Title.InlineElements[0] as InlineTextSyntax;
        Assert.IsNotNull(firstElement, "最初のインライン要素は InlineTextSyntax である必要があります。");

        Assert.AreEqual(expectedText, firstElement.Text,
            $"テキストが一致しません。期待: '{expectedText}', 実際: '{firstElement.Text}'");
    }

    /// <summary>
    /// 最初のセクションタイトルを取得する（ヘッダーのタイトルまたはボディの最初のセクションのタイトル）。
    /// </summary>
    private static SectionTitleSyntax? GetFirstSectionTitle(DocumentSyntax document)
    {
        // まずヘッダーのタイトルを確認
        if (document.Header?.Title is not null)
        {
            return document.Header.Title;
        }

        // ボディの最初のセクションのタイトルを確認
        var firstSection = document.Body?.ChildNodesAndTokens()
            .Where(c => c.IsNode && c.AsNode()?.Kind == SyntaxKind.Section)
            .Select(c => c.AsNode() as SectionSyntax)
            .FirstOrDefault();

        return firstSection?.Title;
    }
}
