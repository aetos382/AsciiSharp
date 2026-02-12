using System.Linq;

using AsciiSharp.Syntax;

using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

/// <summary>
/// InlineTextSyntax の名前・型に関するフィーチャー テスト。
/// </summary>
[TestClass]
[FeatureDescription(
    @"TextSyntax を InlineTextSyntax として参照する
ライブラリユーザーとして、
プレーンテキストのインライン要素を InlineTextSyntax として参照し、
一貫した命名規則で構文木を操作したい")]
public sealed partial class InlineTextSyntaxRenameFeature : FeatureFixture
{
    private string _sourceText = string.Empty;
    private SyntaxTree? _syntaxTree;
    private bool _visitInlineTextCalled;

    [Scenario]
    public void InlineTextSyntaxのSyntaxKindはInlineTextである()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("= タイトル\n\n段落テキスト。\n"),
            when => 文書を解析する(),
            then => 段落の最初のインライン要素のSyntaxKindはInlineTextである());
    }

    [Scenario]
    public void VisitorでInlineTextSyntaxを訪問する()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("= タイトル\n\n段落テキスト。\n"),
            when => 文書を解析する(),
            when => Visitorでドキュメントを走査する(),
            then => VisitInlineTextメソッドが呼び出される());
    }

    [Scenario]
    public void InlineTextSyntaxからTextプロパティを取得する()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("= タイトル\n\nHello, World!\n"),
            when => 文書を解析する(),
            then => 段落の最初のインライン要素のテキストは("Hello, World!"));
    }

    private void 以下のAsciiDoc文書がある(string text)
    {
        this._sourceText = text;
    }

    private void 文書を解析する()
    {
        this._syntaxTree = SyntaxTree.ParseText(this._sourceText);
    }

    private void Visitorでドキュメントを走査する()
    {
        Assert.IsNotNull(this._syntaxTree);
        var visitor = new InlineTextDetectingVisitor();
        this._syntaxTree.Root.Accept(visitor);
        this._visitInlineTextCalled = visitor.Called;
    }

    private void 段落の最初のインライン要素のSyntaxKindはInlineTextである()
    {
        Assert.IsNotNull(this._syntaxTree);
        var paragraph = this._syntaxTree.Root.DescendantNodes().OfType<ParagraphSyntax>().FirstOrDefault();
        Assert.IsNotNull(paragraph);

        var firstInline = paragraph.DescendantNodes().OfType<InlineTextSyntax>().FirstOrDefault();
        Assert.IsNotNull(firstInline);
        Assert.AreEqual(SyntaxKind.InlineText, firstInline.Kind);
    }

    private void VisitInlineTextメソッドが呼び出される()
    {
        Assert.IsTrue(this._visitInlineTextCalled, "VisitInlineText メソッドが呼び出されませんでした。");
    }

    private void 段落の最初のインライン要素のテキストは(string expectedText)
    {
        Assert.IsNotNull(this._syntaxTree);
        var paragraph = this._syntaxTree.Root.DescendantNodes().OfType<ParagraphSyntax>().FirstOrDefault();
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
            this.Called = true;
        }

        public void VisitAttributeEntry(AttributeEntrySyntax node)
        {
        }

        public void VisitLink(LinkSyntax node)
        {
        }
    }
}
