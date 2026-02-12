using System.Collections.Generic;
using System.Linq;

using AsciiSharp.Syntax;
using AsciiSharp.Text;

using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

/// <summary>
/// 構文ノードの階層構造のテスト
/// </summary>
[TestClass]
[FeatureDescription(
    @"構文ノードの階層構造
開発者として、
構文木を走査する際に、
型システムを利用してブロック要素とインライン要素を区別したい")]
public sealed partial class BlockInlineSyntaxFeature : FeatureFixture
{
    private SourceText? _sourceText;
    private SyntaxTree? _syntaxTree;
    private IReadOnlyList<SyntaxNode>? _queriedNodes;

    [Scenario]
    public void ブロック要素はBlockSyntaxとして識別できる()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("段落テキスト\n"),
            when => 文書を解析する(),
            then => DocumentノードはBlockSyntax(),
            then => ParagraphノードはBlockSyntax(),
            then => DocumentノードはInlineSyntaxではない(),
            then => ParagraphノードはInlineSyntaxではない()
        );
    }

    [Scenario]
    public void インライン要素はInlineSyntaxとして識別できる()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("https://example.com[リンク]\n"),
            when => 文書を解析する(),
            then => LinkノードはInlineSyntax(),
            then => LinkノードはBlockSyntaxではない()
        );
    }

    [Scenario]
    public void セクション関連ノードはBlockSyntaxとして識別できる()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("== セクションタイトル\n"),
            when => 文書を解析する(),
            then => SectionノードはBlockSyntax(),
            then => SectionTitleノードはBlockSyntax()
        );
    }

    [Scenario]
    public void すべてのブロックノードを一括で取得できる()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("== セクション\n\n段落テキスト https://example.com[リンク]\n"),
            when => 文書を解析する(),
            when => すべてのBlockSyntaxノードをクエリする(),
            then => クエリ結果にDocumentノードが含まれる(),
            then => クエリ結果にParagraphノードが含まれる(),
            then => クエリ結果にSectionノードが含まれる(),
            then => クエリ結果にLinkノードは含まれない()
        );
    }

    [Scenario]
    public void すべてのインラインノードを一括で取得できる()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("段落テキスト https://example.com[リンク]\n"),
            when => 文書を解析する(),
            when => すべてのInlineSyntaxノードをクエリする(),
            then => クエリ結果にLinkノードが含まれる(),
            then => クエリ結果にParagraphノードは含まれない()
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

    private void DocumentノードはBlockSyntax()
    {
        Assert.IsNotNull(_syntaxTree);
        Assert.IsInstanceOfType<BlockSyntax>(_syntaxTree.Root);
    }

    private void ParagraphノードはBlockSyntax()
    {
        Assert.IsNotNull(_syntaxTree);
        var paragraph = _syntaxTree.Root.DescendantNodes().OfType<ParagraphSyntax>().FirstOrDefault();
        Assert.IsNotNull(paragraph);
        Assert.IsInstanceOfType<BlockSyntax>(paragraph);
    }

    private void DocumentノードはInlineSyntaxではない()
    {
        Assert.IsNotNull(_syntaxTree);
        Assert.IsNotInstanceOfType<InlineSyntax>(_syntaxTree.Root);
    }

    private void ParagraphノードはInlineSyntaxではない()
    {
        Assert.IsNotNull(_syntaxTree);
        var paragraph = _syntaxTree.Root.DescendantNodes().OfType<ParagraphSyntax>().FirstOrDefault();
        Assert.IsNotNull(paragraph);
        Assert.IsNotInstanceOfType<InlineSyntax>(paragraph);
    }

    private void LinkノードはInlineSyntax()
    {
        Assert.IsNotNull(_syntaxTree);
        var link = _syntaxTree.Root.DescendantNodes().OfType<LinkSyntax>().FirstOrDefault();
        Assert.IsNotNull(link);
        Assert.IsInstanceOfType<InlineSyntax>(link);
    }

    private void LinkノードはBlockSyntaxではない()
    {
        Assert.IsNotNull(_syntaxTree);
        var link = _syntaxTree.Root.DescendantNodes().OfType<LinkSyntax>().FirstOrDefault();
        Assert.IsNotNull(link);
        Assert.IsNotInstanceOfType<BlockSyntax>(link);
    }

    private void SectionノードはBlockSyntax()
    {
        Assert.IsNotNull(_syntaxTree);
        var section = _syntaxTree.Root.DescendantNodes().OfType<SectionSyntax>().FirstOrDefault();
        Assert.IsNotNull(section);
        Assert.IsInstanceOfType<BlockSyntax>(section);
    }

    private void SectionTitleノードはBlockSyntax()
    {
        Assert.IsNotNull(_syntaxTree);
        var sectionTitle = _syntaxTree.Root.DescendantNodes().OfType<SectionTitleSyntax>().FirstOrDefault();
        Assert.IsNotNull(sectionTitle);
        Assert.IsInstanceOfType<BlockSyntax>(sectionTitle);
    }

    private void すべてのBlockSyntaxノードをクエリする()
    {
        Assert.IsNotNull(_syntaxTree);
        _queriedNodes = new SyntaxNode[] { _syntaxTree.Root }
            .Concat(_syntaxTree.Root.DescendantNodes())
            .OfType<BlockSyntax>().ToList();
    }

    private void すべてのInlineSyntaxノードをクエリする()
    {
        Assert.IsNotNull(_syntaxTree);
        _queriedNodes = new SyntaxNode[] { _syntaxTree.Root }
            .Concat(_syntaxTree.Root.DescendantNodes())
            .OfType<InlineSyntax>().ToList();
    }

    private void クエリ結果にDocumentノードが含まれる()
    {
        Assert.IsNotNull(_queriedNodes);
        Assert.IsTrue(_queriedNodes.OfType<DocumentSyntax>().Any());
    }

    private void クエリ結果にParagraphノードが含まれる()
    {
        Assert.IsNotNull(_queriedNodes);
        Assert.IsTrue(_queriedNodes.OfType<ParagraphSyntax>().Any());
    }

    private void クエリ結果にSectionノードが含まれる()
    {
        Assert.IsNotNull(_queriedNodes);
        Assert.IsTrue(_queriedNodes.OfType<SectionSyntax>().Any());
    }

    private void クエリ結果にLinkノードは含まれない()
    {
        Assert.IsNotNull(_queriedNodes);
        Assert.IsFalse(_queriedNodes.OfType<LinkSyntax>().Any());
    }

    private void クエリ結果にLinkノードが含まれる()
    {
        Assert.IsNotNull(_queriedNodes);
        Assert.IsTrue(_queriedNodes.OfType<LinkSyntax>().Any());
    }

    private void クエリ結果にParagraphノードは含まれない()
    {
        Assert.IsNotNull(_queriedNodes);
        Assert.IsFalse(_queriedNodes.OfType<ParagraphSyntax>().Any());
    }
}
