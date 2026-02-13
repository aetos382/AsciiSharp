using System.Linq;

using AsciiSharp.Syntax;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

public partial class InlineTextSyntaxRenameFeature
{
    private string _sourceText = string.Empty;
    private SyntaxTree? _syntaxTree;
    private bool _visitInlineTextCalled;

    private void 以下のAsciiDoc文書がある(string text)
    {
        _sourceText = text;
    }

    private void 文書を解析する()
    {
        _syntaxTree = SyntaxTree.ParseText(_sourceText);
    }

    private void Visitorでドキュメントを走査する()
    {
        Assert.IsNotNull(_syntaxTree);
        var visitor = new InlineTextDetectingVisitor();
        _syntaxTree.Root.Accept(visitor);
        _visitInlineTextCalled = visitor.Called;
    }

    private void 段落の最初のインライン要素のSyntaxKindはInlineTextである()
    {
        Assert.IsNotNull(_syntaxTree);
        var paragraph = _syntaxTree.Root.DescendantNodes().OfType<ParagraphSyntax>().FirstOrDefault();
        Assert.IsNotNull(paragraph);

        var firstInline = paragraph.DescendantNodes().OfType<InlineTextSyntax>().FirstOrDefault();
        Assert.IsNotNull(firstInline);
        Assert.AreEqual(SyntaxKind.InlineText, firstInline.Kind);
    }

    private void VisitInlineTextメソッドが呼び出される()
    {
        Assert.IsTrue(_visitInlineTextCalled, "VisitInlineText メソッドが呼び出されませんでした。");
    }

    private void 段落の最初のインライン要素のテキストは(string expectedText)
    {
        Assert.IsNotNull(_syntaxTree);
        var paragraph = _syntaxTree.Root.DescendantNodes().OfType<ParagraphSyntax>().FirstOrDefault();
        Assert.IsNotNull(paragraph);

        var firstInline = paragraph.DescendantNodes().OfType<InlineTextSyntax>().FirstOrDefault();
        Assert.IsNotNull(firstInline);
        Assert.AreEqual(expectedText, firstInline.Text);
    }

    /// <summary>
    /// InlineTextSyntax の訪問を検出する Visitor。
    /// </summary>
    private sealed class InlineTextDetectingVisitor : ISyntaxVisitor
    {
        public bool Called { get; private set; }

        public void VisitDocument(DocumentSyntax node)
        {
            node.Header?.Accept(this);
            node.Body?.Accept(this);
        }

        public void VisitDocumentHeader(DocumentHeaderSyntax node)
        {
            node.Title?.Accept(this);
        }

        public void VisitAuthorLine(AuthorLineSyntax node)
        {
        }

        public void VisitDocumentBody(DocumentBodySyntax node)
        {
            foreach (var child in node.ChildNodesAndTokens())
            {
                if (child.IsNode)
                {
                    child.AsNode()?.Accept(this);
                }
            }
        }

        public void VisitSection(SectionSyntax node)
        {
            node.Title?.Accept(this);
            foreach (var child in node.ChildNodesAndTokens())
            {
                if (child.IsNode && child.AsNode() is not SectionTitleSyntax)
                {
                    child.AsNode()?.Accept(this);
                }
            }
        }

        public void VisitSectionTitle(SectionTitleSyntax node)
        {
            foreach (var child in node.ChildNodesAndTokens())
            {
                if (child.IsNode)
                {
                    child.AsNode()?.Accept(this);
                }
            }
        }

        public void VisitParagraph(ParagraphSyntax node)
        {
            foreach (var child in node.ChildNodesAndTokens())
            {
                if (child.IsNode)
                {
                    child.AsNode()?.Accept(this);
                }
            }
        }

        public void VisitInlineText(InlineTextSyntax node)
        {
            Called = true;
        }

        public void VisitAttributeEntry(AttributeEntrySyntax node)
        {
        }

        public void VisitLink(LinkSyntax node)
        {
        }
    }
}
