using System.Collections.Generic;
using System.Linq;

using AsciiSharp.Syntax;
using AsciiSharp.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

public partial class BlockInlineSyntaxFeature
{
    private SourceText? _sourceText;
    private SyntaxTree? _syntaxTree;
    private IReadOnlyList<SyntaxNode>? _queriedNodes;

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

    private void SectionTitleノードはBlockSyntaxではない()
    {
        Assert.IsNotNull(_syntaxTree);
        var sectionTitle = _syntaxTree.Root.DescendantNodes().OfType<SectionTitleSyntax>().FirstOrDefault();
        Assert.IsNotNull(sectionTitle);
        Assert.IsNotInstanceOfType<BlockSyntax>(sectionTitle);
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
