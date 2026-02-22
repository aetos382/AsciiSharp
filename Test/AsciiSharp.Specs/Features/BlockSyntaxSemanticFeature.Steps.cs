using System.Linq;

using AsciiSharp.Syntax;
using AsciiSharp.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

public partial class BlockSyntaxSemanticFeature
{
    private SourceText? _sourceText;
    private SyntaxTree? _syntaxTree;

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

    private void SectionTitleSyntaxはBlockSyntaxではない()
    {
        Assert.IsNotNull(_syntaxTree);
        var node = _syntaxTree.Root.DescendantNodes().OfType<SectionTitleSyntax>().FirstOrDefault();
        Assert.IsNotNull(node);
        Assert.IsNotInstanceOfType<BlockSyntax>(node);
    }

    private void DocumentHeaderSyntaxはBlockSyntaxではない()
    {
        Assert.IsNotNull(_syntaxTree);
        var node = _syntaxTree.Root.DescendantNodes().OfType<DocumentHeaderSyntax>().FirstOrDefault();
        Assert.IsNotNull(node);
        Assert.IsNotInstanceOfType<BlockSyntax>(node);
    }

    private void AuthorLineSyntaxはBlockSyntaxではない()
    {
        Assert.IsNotNull(_syntaxTree);
        var node = _syntaxTree.Root.DescendantNodes().OfType<AuthorLineSyntax>().FirstOrDefault();
        Assert.IsNotNull(node);
        Assert.IsNotInstanceOfType<BlockSyntax>(node);
    }

    private void AttributeEntrySyntaxはBlockSyntaxではない()
    {
        Assert.IsNotNull(_syntaxTree);
        var node = _syntaxTree.Root.DescendantNodes().OfType<AttributeEntrySyntax>().FirstOrDefault();
        Assert.IsNotNull(node);
        Assert.IsNotInstanceOfType<BlockSyntax>(node);
    }

    private void DocumentBodySyntaxはBlockSyntaxではない()
    {
        Assert.IsNotNull(_syntaxTree);
        var node = _syntaxTree.Root.DescendantNodes().OfType<DocumentBodySyntax>().FirstOrDefault();
        Assert.IsNotNull(node);
        Assert.IsNotInstanceOfType<BlockSyntax>(node);
    }

    private void StructuredTriviaSyntaxクラスが定義されている()
    {
        // StructuredTriviaSyntax はリフレクションで検証するため、前提条件の確認のみ
    }

    private void StructuredTriviaSyntaxはSyntaxNodeのサブクラスである()
    {
        Assert.Inconclusive("未実装");
    }

    private void StructuredTriviaSyntaxはBlockSyntaxのサブクラスではない()
    {
        Assert.Inconclusive("未実装");
    }

    private void StructuredTriviaSyntaxはInlineSyntaxのサブクラスではない()
    {
        Assert.Inconclusive("未実装");
    }
}
