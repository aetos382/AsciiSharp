using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AsciiSharp.Syntax;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

public partial class SyntaxVisitorFeature
{
    private string _sourceText = string.Empty;
    private SyntaxTree? _syntaxTree;
    private int _visitedNodeCount;
    private List<LinkSyntax> _collectedLinks = [];
    private List<SectionTitleSyntax> _collectedSectionTitles = [];
    private bool _traversalCompleted;
    private Exception? _capturedException;
    private List<TocEntry> _tocEntries = [];
    private string _extractedText = string.Empty;

    private void 以下のAsciiDoc文書がある(string text)
    {
        _sourceText = text;
    }

    private void 不完全なAsciiDoc文書がある(string text)
    {
        _sourceText = text;
    }

    private void 文書を解析する()
    {
        _syntaxTree = SyntaxTree.ParseText(_sourceText);
    }

    private void 全ノードを訪問するVisitorで走査する()
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        var visitor = new NodeCountingVisitor();
        _syntaxTree.Root.Accept(visitor);
        _visitedNodeCount = visitor.Count;
        _traversalCompleted = true;
    }

    private void リンクを収集するVisitorで走査する()
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        var visitor = new LinkCollectingVisitor();
        _syntaxTree.Root.Accept(visitor);
        _collectedLinks = visitor.Links;
        _traversalCompleted = true;
    }

    private void セクションタイトルを収集するVisitorで走査する()
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        var visitor = new SectionTitleCollectingVisitor();
        _syntaxTree.Root.Accept(visitor);
        _collectedSectionTitles = visitor.SectionTitles;
        _traversalCompleted = true;
    }

    private void 例外を投げるVisitorで走査する()
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        var visitor = new ExceptionThrowingVisitor();
        try
        {
            _syntaxTree.Root.Accept(visitor);
        }
        catch (InvalidOperationException ex)
        {
            _capturedException = ex;
        }
    }

    private void 結果を返すVisitorで目次を生成する()
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        var visitor = new TocGeneratorVisitor();
        _tocEntries = _syntaxTree.Root.Accept(visitor);
        _traversalCompleted = true;
    }

    private void 結果を返すVisitorでプレーンテキストを抽出する()
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        var visitor = new PlainTextExtractorVisitor();
        _extractedText = _syntaxTree.Root.Accept(visitor);
        _traversalCompleted = true;
    }

    private void すべてのノードが訪問される()
    {
        Assert.IsTrue(_traversalCompleted, "走査が完了していません。");
        Assert.IsGreaterThan(0, _visitedNodeCount, "訪問されたノードが 0 です。");
    }

    private void 訪問されたノード数は(int expectedCount)
    {
        Assert.AreEqual(expectedCount, _visitedNodeCount, $"訪問されたノード数が一致しません。期待: {expectedCount}, 実際: {_visitedNodeCount}");
    }

    private void 収集されたリンク数は(int expectedCount)
    {
        Assert.HasCount(expectedCount, _collectedLinks, $"収集されたリンク数が一致しません。期待: {expectedCount}, 実際: {_collectedLinks.Count}");
    }

    private void 収集されたセクションタイトル数は(int expectedCount)
    {
        Assert.HasCount(expectedCount, _collectedSectionTitles, $"収集されたセクションタイトル数が一致しません。期待: {expectedCount}, 実際: {_collectedSectionTitles.Count}");
    }

    private void セクションタイトルは順番に(string title1, string title2)
    {
        Assert.IsGreaterThanOrEqualTo(2, _collectedSectionTitles.Count, "セクションタイトルが 2 つ未満です。");
        Assert.AreEqual(title1, _collectedSectionTitles[0].GetTitleContent(), $"最初のセクションタイトルが一致しません。期待: '{title1}', 実際: '{_collectedSectionTitles[0].GetTitleContent()}'");
        Assert.AreEqual(title2, _collectedSectionTitles[1].GetTitleContent(), $"2 番目のセクションタイトルが一致しません。期待: '{title2}', 実際: '{_collectedSectionTitles[1].GetTitleContent()}'");
    }

    private void エラーなく走査が完了する()
    {
        Assert.IsTrue(_traversalCompleted, "走査が完了していません。");
    }

    private void 例外が伝播する()
    {
        Assert.IsNotNull(_capturedException, "例外がキャプチャされていません。");
        Assert.IsInstanceOfType<InvalidOperationException>(_capturedException, "例外の型が InvalidOperationException ではありません。");
    }

    private void 欠落ノードも訪問される()
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        var hasMissingNodes = _syntaxTree.Root.DescendantNodesAndTokens().Any(n => n.IsMissing);
        Assert.IsTrue(hasMissingNodes, "不完全な文書なので欠落ノードが存在するはずです。");
        Assert.IsTrue(_traversalCompleted, "走査が完了していません。");
        Assert.IsGreaterThan(0, _visitedNodeCount, "訪問されたノードが 0 です。");
    }

    private void 目次項目数は(int expectedCount)
    {
        Assert.HasCount(expectedCount, _tocEntries, $"目次項目数が一致しません。期待: {expectedCount}, 実際: {_tocEntries.Count}");
    }

    private void 目次の階層構造が正しい()
    {
        Assert.IsGreaterThanOrEqualTo(3, _tocEntries.Count, "目次項目が 3 つ未満です。");
        var level2Count = _tocEntries.Count(e => e.Level == 2);
        var level3Count = _tocEntries.Count(e => e.Level == 3);
        Assert.AreEqual(2, level2Count, $"レベル 2 の目次項目数が一致しません。期待: 2, 実際: {level2Count}");
        Assert.AreEqual(1, level3Count, $"レベル 3 の目次項目数が一致しません。期待: 1, 実際: {level3Count}");
    }

    private void 抽出されたテキストに_が含まれる(string expectedText)
    {
        Assert.IsTrue(_extractedText.Contains(expectedText, StringComparison.Ordinal), $"抽出されたテキストに '{expectedText}' が含まれていません。実際: '{_extractedText}'");
    }

    private readonly record struct TocEntry(int Level, string Title);

    /// <summary>
    /// 訪問したノードの数をカウントする Visitor。
    /// </summary>
    private sealed class NodeCountingVisitor : ISyntaxVisitor
    {
        public int Count { get; private set; }

        public void VisitDocument(DocumentSyntax node)
        {
            Count++;
            node.Header?.Accept(this);
            node.Body?.Accept(this);
        }

        public void VisitDocumentHeader(DocumentHeaderSyntax node)
        {
            Count++;
            node.Title?.Accept(this);
            node.AuthorLine?.Accept(this);
            foreach (var attr in node.AttributeEntries)
            {
                attr.Accept(this);
            }
        }

        public void VisitAuthorLine(AuthorLineSyntax node)
        {
            Count++;
        }

        public void VisitDocumentBody(DocumentBodySyntax node)
        {
            Count++;
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
            Count++;
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
            Count++;
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
            Count++;
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
            Count++;
        }

        public void VisitAttributeEntry(AttributeEntrySyntax node)
        {
            Count++;
        }

        public void VisitLink(LinkSyntax node)
        {
            Count++;
        }
    }

    /// <summary>
    /// リンクノードを収集する Visitor。
    /// </summary>
    private sealed class LinkCollectingVisitor : ISyntaxVisitor
    {
        public List<LinkSyntax> Links { get; } = [];

        public void VisitDocument(DocumentSyntax node)
        {
            node.Header?.Accept(this);
            node.Body?.Accept(this);
        }

        public void VisitDocumentHeader(DocumentHeaderSyntax node)
        {
            node.Title?.Accept(this);
            node.AuthorLine?.Accept(this);
            foreach (var attr in node.AttributeEntries)
            {
                attr.Accept(this);
            }
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
        }

        public void VisitAttributeEntry(AttributeEntrySyntax node)
        {
        }

        public void VisitLink(LinkSyntax node)
        {
            Links.Add(node);
        }
    }

    /// <summary>
    /// セクションタイトルを収集する Visitor。
    /// </summary>
    private sealed class SectionTitleCollectingVisitor : ISyntaxVisitor
    {
        public List<SectionTitleSyntax> SectionTitles { get; } = [];

        public void VisitDocument(DocumentSyntax node)
        {
            node.Header?.Accept(this);
            node.Body?.Accept(this);
        }

        public void VisitDocumentHeader(DocumentHeaderSyntax node)
        {
            node.Title?.Accept(this);
            node.AuthorLine?.Accept(this);
            foreach (var attr in node.AttributeEntries)
            {
                attr.Accept(this);
            }
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
            SectionTitles.Add(node);
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
        }

        public void VisitAttributeEntry(AttributeEntrySyntax node)
        {
        }

        public void VisitLink(LinkSyntax node)
        {
        }
    }

    /// <summary>
    /// 例外を投げる Visitor。
    /// </summary>
    private sealed class ExceptionThrowingVisitor : ISyntaxVisitor
    {
        public void VisitDocument(DocumentSyntax node)
        {
            throw new InvalidOperationException("テスト用の例外です。");
        }

        public void VisitDocumentHeader(DocumentHeaderSyntax node)
        {
        }

        public void VisitAuthorLine(AuthorLineSyntax node)
        {
        }

        public void VisitDocumentBody(DocumentBodySyntax node)
        {
        }

        public void VisitSection(SectionSyntax node)
        {
        }

        public void VisitSectionTitle(SectionTitleSyntax node)
        {
        }

        public void VisitParagraph(ParagraphSyntax node)
        {
        }

        public void VisitInlineText(InlineTextSyntax node)
        {
        }

        public void VisitAttributeEntry(AttributeEntrySyntax node)
        {
        }

        public void VisitLink(LinkSyntax node)
        {
        }
    }

    /// <summary>
    /// 目次を生成する Visitor。
    /// </summary>
    private sealed class TocGeneratorVisitor : ISyntaxVisitor<List<TocEntry>>
    {
        public List<TocEntry> VisitDocument(DocumentSyntax node)
        {
            var entries = new List<TocEntry>();
            if (node.Body is not null)
            {
                var bodyEntries = node.Body.Accept(this);
                entries.AddRange(bodyEntries);
            }

            return entries;
        }

        public List<TocEntry> VisitDocumentHeader(DocumentHeaderSyntax node)
        {
            return [];
        }

        public List<TocEntry> VisitAuthorLine(AuthorLineSyntax node)
        {
            return [];
        }

        public List<TocEntry> VisitDocumentBody(DocumentBodySyntax node)
        {
            var entries = new List<TocEntry>();
            foreach (var child in node.ChildNodesAndTokens())
            {
                if (child.IsNode)
                {
                    var childEntries = child.AsNode()?.Accept(this);
                    if (childEntries is not null)
                    {
                        entries.AddRange(childEntries);
                    }
                }
            }

            return entries;
        }

        public List<TocEntry> VisitSection(SectionSyntax node)
        {
            var entries = new List<TocEntry>();
            if (node.Title is not null)
            {
                var titleContent = node.Title.GetTitleContent();
                var level = node.Title.Level;
                entries.Add(new TocEntry(level, titleContent ?? string.Empty));
            }

            foreach (var child in node.ChildNodesAndTokens())
            {
                if (child.IsNode && child.AsNode() is not SectionTitleSyntax)
                {
                    var childEntries = child.AsNode()?.Accept(this);
                    if (childEntries is not null)
                    {
                        entries.AddRange(childEntries);
                    }
                }
            }

            return entries;
        }

        public List<TocEntry> VisitSectionTitle(SectionTitleSyntax node)
        {
            return [];
        }

        public List<TocEntry> VisitParagraph(ParagraphSyntax node)
        {
            return [];
        }

        public List<TocEntry> VisitInlineText(InlineTextSyntax node)
        {
            return [];
        }

        public List<TocEntry> VisitAttributeEntry(AttributeEntrySyntax node)
        {
            return [];
        }

        public List<TocEntry> VisitLink(LinkSyntax node)
        {
            return [];
        }
    }

    /// <summary>
    /// プレーンテキストを抽出する Visitor。
    /// </summary>
    private sealed class PlainTextExtractorVisitor : ISyntaxVisitor<string>
    {
        public string VisitDocument(DocumentSyntax node)
        {
            var sb = new StringBuilder();
            if (node.Body is not null)
            {
                sb.Append(node.Body.Accept(this));
            }

            return sb.ToString();
        }

        public string VisitDocumentHeader(DocumentHeaderSyntax node)
        {
            return string.Empty;
        }

        public string VisitAuthorLine(AuthorLineSyntax node)
        {
            return string.Empty;
        }

        public string VisitDocumentBody(DocumentBodySyntax node)
        {
            var sb = new StringBuilder();
            foreach (var child in node.ChildNodesAndTokens())
            {
                if (child.IsNode)
                {
                    var text = child.AsNode()?.Accept(this);
                    if (!string.IsNullOrEmpty(text))
                    {
                        sb.Append(text);
                    }
                }
            }

            return sb.ToString();
        }

        public string VisitSection(SectionSyntax node)
        {
            var sb = new StringBuilder();
            foreach (var child in node.ChildNodesAndTokens())
            {
                if (child.IsNode && child.AsNode() is not SectionTitleSyntax)
                {
                    var text = child.AsNode()?.Accept(this);
                    if (!string.IsNullOrEmpty(text))
                    {
                        sb.Append(text);
                    }
                }
            }

            return sb.ToString();
        }

        public string VisitSectionTitle(SectionTitleSyntax node)
        {
            return string.Empty;
        }

        public string VisitParagraph(ParagraphSyntax node)
        {
            var sb = new StringBuilder();
            foreach (var child in node.ChildNodesAndTokens())
            {
                if (child.IsNode)
                {
                    var text = child.AsNode()?.Accept(this);
                    if (!string.IsNullOrEmpty(text))
                    {
                        sb.Append(text);
                    }
                }
            }

            return sb.ToString();
        }

        public string VisitInlineText(InlineTextSyntax node)
        {
            return node.ToString();
        }

        public string VisitAttributeEntry(AttributeEntrySyntax node)
        {
            return string.Empty;
        }

        public string VisitLink(LinkSyntax node)
        {
            return node.DisplayText ?? string.Empty;
        }
    }
}
