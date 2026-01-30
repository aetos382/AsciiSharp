
using System.Collections.Generic;
using System.Linq;

using AsciiSharp.Syntax;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reqnroll;

namespace AsciiSharp.Specs.StepDefinitions;

/// <summary>
/// 構文ノードの階層構造（BlockSyntax/InlineSyntax）に関するステップ定義。
/// </summary>
[Binding]
public sealed class SyntaxHierarchySteps
{
    private readonly BasicParsingSteps _basicParsingSteps;
    private IReadOnlyList<SyntaxNode>? _queriedNodes;

    /// <summary>
    /// SyntaxHierarchySteps を作成する。
    /// </summary>
    /// <param name="basicParsingSteps">基本パース ステップ。</param>
    public SyntaxHierarchySteps(BasicParsingSteps basicParsingSteps)
    {
        this._basicParsingSteps = basicParsingSteps;
    }

    // --- BlockSyntax 判定ステップ ---

    [Then(@"Document ノードは BlockSyntax である")]
    public void ThenDocumentノードはBlockSyntaxである()
    {
        var document = this.GetCurrentSyntaxTree().Root;
        Assert.IsInstanceOfType<BlockSyntax>(document, "Document ノードは BlockSyntax である必要があります。");
    }

    [Then(@"Paragraph ノードは BlockSyntax である")]
    public void ThenParagraphノードはBlockSyntaxである()
    {
        var paragraph = this.FindFirstNodeOfKind(SyntaxKind.Paragraph);
        Assert.IsNotNull(paragraph, "Paragraph ノードが見つかりません。");
        Assert.IsInstanceOfType<BlockSyntax>(paragraph, "Paragraph ノードは BlockSyntax である必要があります。");
    }

    [Then(@"Section ノードは BlockSyntax である")]
    public void ThenSectionノードはBlockSyntaxである()
    {
        var section = this.FindFirstNodeOfKind(SyntaxKind.Section);
        Assert.IsNotNull(section, "Section ノードが見つかりません。");
        Assert.IsInstanceOfType<BlockSyntax>(section, "Section ノードは BlockSyntax である必要があります。");
    }

    [Then(@"SectionTitle ノードは BlockSyntax である")]
    public void ThenSectionTitleノードはBlockSyntaxである()
    {
        var sectionTitle = this.FindFirstNodeOfKind(SyntaxKind.SectionTitle);
        Assert.IsNotNull(sectionTitle, "SectionTitle ノードが見つかりません。");
        Assert.IsInstanceOfType<BlockSyntax>(sectionTitle, "SectionTitle ノードは BlockSyntax である必要があります。");
    }

    // --- InlineSyntax 判定ステップ ---

    [Then(@"Text ノードは InlineSyntax である")]
    public void ThenTextノードはInlineSyntaxである()
    {
        var text = this.FindFirstNodeOfKind(SyntaxKind.Text);
        Assert.IsNotNull(text, "Text ノードが見つかりません。");
        Assert.IsInstanceOfType<InlineSyntax>(text, "Text ノードは InlineSyntax である必要があります。");
    }

    [Then(@"Link ノードは InlineSyntax である")]
    public void ThenLinkノードはInlineSyntaxである()
    {
        var link = this.FindFirstNodeOfKind(SyntaxKind.Link);
        Assert.IsNotNull(link, "Link ノードが見つかりません。");
        Assert.IsInstanceOfType<InlineSyntax>(link, "Link ノードは InlineSyntax である必要があります。");
    }

    // --- 否定判定ステップ ---

    [Then(@"Document ノードは InlineSyntax ではない")]
    public void ThenDocumentノードはInlineSyntaxではない()
    {
        var document = this.GetCurrentSyntaxTree().Root;
        Assert.IsNotInstanceOfType<InlineSyntax>(document, "Document ノードは InlineSyntax ではありません。");
    }

    [Then(@"Paragraph ノードは InlineSyntax ではない")]
    public void ThenParagraphノードはInlineSyntaxではない()
    {
        var paragraph = this.FindFirstNodeOfKind(SyntaxKind.Paragraph);
        Assert.IsNotNull(paragraph, "Paragraph ノードが見つかりません。");
        Assert.IsNotInstanceOfType<InlineSyntax>(paragraph, "Paragraph ノードは InlineSyntax ではありません。");
    }

    [Then(@"Text ノードは BlockSyntax ではない")]
    public void ThenTextノードはBlockSyntaxではない()
    {
        var text = this.FindFirstNodeOfKind(SyntaxKind.Text);
        Assert.IsNotNull(text, "Text ノードが見つかりません。");
        Assert.IsNotInstanceOfType<BlockSyntax>(text, "Text ノードは BlockSyntax ではありません。");
    }

    [Then(@"Link ノードは BlockSyntax ではない")]
    public void ThenLinkノードはBlockSyntaxではない()
    {
        var link = this.FindFirstNodeOfKind(SyntaxKind.Link);
        Assert.IsNotNull(link, "Link ノードが見つかりません。");
        Assert.IsNotInstanceOfType<BlockSyntax>(link, "Link ノードは BlockSyntax ではありません。");
    }

    // --- 一括クエリ ステップ ---

    [When(@"すべての BlockSyntax ノードをクエリする")]
    public void WhenすべてのBlockSyntaxノードをクエリする()
    {
        var root = this.GetCurrentSyntaxTree().Root;
        this._queriedNodes = root.DescendantNodes()
            .Prepend(root)
            .OfType<BlockSyntax>()
            .Cast<SyntaxNode>()
            .ToList();
    }

    [When(@"すべての InlineSyntax ノードをクエリする")]
    public void WhenすべてのInlineSyntaxノードをクエリする()
    {
        var root = this.GetCurrentSyntaxTree().Root;
        this._queriedNodes = root.DescendantNodes()
            .OfType<InlineSyntax>()
            .Cast<SyntaxNode>()
            .ToList();
    }

    // --- 結果検証ステップ ---

    [Then(@"取得したノードに Document が含まれる")]
    public void Then取得したノードにDocumentが含まれる()
    {
        Assert.IsNotNull(this._queriedNodes, "クエリが実行されていません。");
        Assert.IsTrue(
            this._queriedNodes.Any(n => n.Kind == SyntaxKind.Document),
            "取得したノードに Document が含まれる必要があります。");
    }

    [Then(@"取得したノードに Paragraph が含まれる")]
    public void Then取得したノードにParagraphが含まれる()
    {
        Assert.IsNotNull(this._queriedNodes, "クエリが実行されていません。");
        Assert.IsTrue(
            this._queriedNodes.Any(n => n.Kind == SyntaxKind.Paragraph),
            "取得したノードに Paragraph が含まれる必要があります。");
    }

    [Then(@"取得したノードに Section が含まれる")]
    public void Then取得したノードにSectionが含まれる()
    {
        Assert.IsNotNull(this._queriedNodes, "クエリが実行されていません。");
        Assert.IsTrue(
            this._queriedNodes.Any(n => n.Kind == SyntaxKind.Section),
            "取得したノードに Section が含まれる必要があります。");
    }

    [Then(@"取得したノードに Text が含まれる")]
    public void Then取得したノードにTextが含まれる()
    {
        Assert.IsNotNull(this._queriedNodes, "クエリが実行されていません。");
        Assert.IsTrue(
            this._queriedNodes.Any(n => n.Kind == SyntaxKind.Text),
            "取得したノードに Text が含まれる必要があります。");
    }

    [Then(@"取得したノードに Link が含まれる")]
    public void Then取得したノードにLinkが含まれる()
    {
        Assert.IsNotNull(this._queriedNodes, "クエリが実行されていません。");
        Assert.IsTrue(
            this._queriedNodes.Any(n => n.Kind == SyntaxKind.Link),
            "取得したノードに Link が含まれる必要があります。");
    }

    [Then(@"取得したノードに Text は含まれない")]
    public void Then取得したノードにTextは含まれない()
    {
        Assert.IsNotNull(this._queriedNodes, "クエリが実行されていません。");
        Assert.IsFalse(
            this._queriedNodes.Any(n => n.Kind == SyntaxKind.Text),
            "取得したノードに Text は含まれないはずです。");
    }

    [Then(@"取得したノードに Paragraph は含まれない")]
    public void Then取得したノードにParagraphは含まれない()
    {
        Assert.IsNotNull(this._queriedNodes, "クエリが実行されていません。");
        Assert.IsFalse(
            this._queriedNodes.Any(n => n.Kind == SyntaxKind.Paragraph),
            "取得したノードに Paragraph は含まれないはずです。");
    }

    [Then(@"取得したノードに Link は含まれない")]
    public void Then取得したノードにLinkは含まれない()
    {
        Assert.IsNotNull(this._queriedNodes, "クエリが実行されていません。");
        Assert.IsFalse(
            this._queriedNodes.Any(n => n.Kind == SyntaxKind.Link),
            "取得したノードに Link は含まれないはずです。");
    }

    // --- ヘルパーメソッド ---

    private SyntaxTree GetCurrentSyntaxTree()
    {
        var tree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(tree, "構文木が null です。先に文書を解析してください。");
        return tree;
    }

    private SyntaxNode? FindFirstNodeOfKind(SyntaxKind kind)
    {
        var tree = this.GetCurrentSyntaxTree();
        return tree.Root.DescendantNodes().FirstOrDefault(n => n.Kind == kind);
    }
}
