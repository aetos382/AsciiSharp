
using System.Linq;

using AsciiSharp.Syntax;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reqnroll;

namespace AsciiSharp.Specs.StepDefinitions;

/// <summary>
/// InlineTextSyntax に関するステップ定義。
/// </summary>
[Binding]
public sealed class InlineTextSyntaxSteps
{
    private readonly BasicParsingSteps _basicParsingSteps;

    private bool _visitInlineTextCalled;

    /// <summary>
    /// InlineTextSyntaxSteps を作成する。
    /// </summary>
    public InlineTextSyntaxSteps(BasicParsingSteps basicParsingSteps)
    {
        this._basicParsingSteps = basicParsingSteps;
    }

    [Then(@"段落の最初のインライン要素の SyntaxKind は InlineText である")]
    public void Then段落の最初のインライン要素のSyntaxKindはInlineTextである()
    {
        var tree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(tree, "構文木が null です。");

        var document = tree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        var paragraph = document.DescendantNodes()
            .OfType<ParagraphSyntax>()
            .FirstOrDefault();

        Assert.IsNotNull(paragraph, "段落が見つかりません。");
        Assert.IsGreaterThan(0, paragraph.InlineElements.Count, "段落のインライン要素が空です。");

        var firstElement = paragraph.InlineElements[0];
        Assert.AreEqual(SyntaxKind.InlineText, firstElement.Kind,
            $"最初のインライン要素の SyntaxKind が InlineText ではありません。実際: {firstElement.Kind}");
    }

    [When(@"Visitor でドキュメントを走査する")]
    public void WhenVisitorでドキュメントを走査する()
    {
        var tree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(tree, "構文木が null です。");

        var visitor = new InlineTextVisitor(this);
        tree.Root.Accept(visitor);
    }

    [Then(@"VisitInlineText メソッドが呼び出される")]
    public void ThenVisitInlineTextメソッドが呼び出される()
    {
        Assert.IsTrue(this._visitInlineTextCalled, "VisitInlineText メソッドが呼び出されていません。");
    }

    [Then(@"段落の最初のインライン要素のテキストは ""(.*)"" である")]
    public void Then段落の最初のインライン要素のテキストはである(string expectedText)
    {
        var tree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(tree, "構文木が null です。");

        var document = tree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        var paragraph = document.DescendantNodes()
            .OfType<ParagraphSyntax>()
            .FirstOrDefault();

        Assert.IsNotNull(paragraph, "段落が見つかりません。");
        Assert.IsGreaterThan(0, paragraph.InlineElements.Count, "段落のインライン要素が空です。");

        var firstElement = paragraph.InlineElements[0] as InlineTextSyntax;
        Assert.IsNotNull(firstElement, "最初のインライン要素は InlineTextSyntax である必要があります。");

        Assert.AreEqual(expectedText, firstElement.Text,
            $"テキストが一致しません。期待: '{expectedText}', 実際: '{firstElement.Text}'");
    }

    /// <summary>
    /// InlineTextSyntax の訪問をテストする Visitor。
    /// </summary>
    private sealed class InlineTextVisitor : ISyntaxVisitor
    {
        private readonly InlineTextSyntaxSteps _steps;

        public InlineTextVisitor(InlineTextSyntaxSteps steps)
        {
            this._steps = steps;
        }

        public void VisitDocument(DocumentSyntax node)
        {
            node.Header?.Accept(this);
            node.Body?.Accept(this);
        }

        public void VisitDocumentHeader(DocumentHeaderSyntax node)
        {
            node.Title?.Accept(this);
            node.AuthorLine?.Accept(this);
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
                    child.AsNode()!.Accept(this);
                }
            }
        }

        public void VisitSection(SectionSyntax node)
        {
            node.Title?.Accept(this);
            foreach (var child in node.Content)
            {
                child.Accept(this);
            }
        }

        public void VisitSectionTitle(SectionTitleSyntax node)
        {
        }

        public void VisitParagraph(ParagraphSyntax node)
        {
            foreach (var element in node.InlineElements)
            {
                element.Accept(this);
            }
        }

        public void VisitInlineText(InlineTextSyntax node)
        {
            this._steps._visitInlineTextCalled = true;
        }

        public void VisitLink(LinkSyntax node)
        {
        }
    }
}
