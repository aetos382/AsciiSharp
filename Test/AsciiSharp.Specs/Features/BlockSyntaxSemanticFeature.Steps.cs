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
        Assert.Inconclusive("未実装");
    }

    private void DocumentHeaderSyntaxはBlockSyntaxではない()
    {
        Assert.Inconclusive("未実装");
    }

    private void AuthorLineSyntaxはBlockSyntaxではない()
    {
        Assert.Inconclusive("未実装");
    }

    private void AttributeEntrySyntaxはBlockSyntaxではない()
    {
        Assert.Inconclusive("未実装");
    }

    private void DocumentBodySyntaxはBlockSyntaxではない()
    {
        Assert.Inconclusive("未実装");
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
