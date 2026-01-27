
using System;
using System.Collections.Generic;
using System.Text;

using AsciiSharp.Syntax;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reqnroll;

namespace AsciiSharp.Specs.StepDefinitions;

/// <summary>
/// Visitor パターンのテスト用ステップ定義。
/// </summary>
[Binding]
public sealed class VisitorSteps
{
    private readonly BasicParsingSteps _basicParsingSteps;

    private int _visitedNodeCount;
    private List<LinkSyntax> _collectedLinks = [];
    private List<SectionTitleSyntax> _collectedSectionTitles = [];
    private bool _traversalCompleted;
    private Exception? _capturedException;

    // US2 用フィールド
    private List<TocEntry> _tocEntries = [];
    private string _extractedText = string.Empty;

    /// <summary>
    /// VisitorSteps を作成する。
    /// </summary>
    public VisitorSteps(BasicParsingSteps basicParsingSteps)
    {
        this._basicParsingSteps = basicParsingSteps;
    }

    [Given(@"以下の不完全な AsciiDoc 文書がある:")]
    public void Given以下の不完全なAsciiDoc文書がある(string multilineText)
    {
        // 不完全な文書も通常の文書と同様に BasicParsingSteps で処理
        this._basicParsingSteps.Given以下のAsciiDoc文書がある(multilineText);
    }

    [When(@"全ノードを訪問する Visitor で走査する")]
    public void When全ノードを訪問するVisitorで走査する()
    {
        var tree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(tree, "構文木が null です。");

        var visitor = new NodeCountingVisitor();
        tree.Root.Accept(visitor);

        this._visitedNodeCount = visitor.Count;
        this._traversalCompleted = true;
    }

    [When(@"リンクを収集する Visitor で走査する")]
    public void Whenリンクを収集するVisitorで走査する()
    {
        var tree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(tree, "構文木が null です。");

        var visitor = new LinkCollectingVisitor();
        tree.Root.Accept(visitor);

        this._collectedLinks = visitor.Links;
        this._traversalCompleted = true;
    }

    [When(@"セクションタイトルを収集する Visitor で走査する")]
    public void Whenセクションタイトルを収集するVisitorで走査する()
    {
        var tree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(tree, "構文木が null です。");

        var visitor = new SectionTitleCollectingVisitor();
        tree.Root.Accept(visitor);

        this._collectedSectionTitles = visitor.SectionTitles;
        this._traversalCompleted = true;
    }

    [When(@"例外を投げる Visitor で走査する")]
    public void When例外を投げるVisitorで走査する()
    {
        var tree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(tree, "構文木が null です。");

        var visitor = new ExceptionThrowingVisitor();

        try
        {
            tree.Root.Accept(visitor);
        }
        catch (InvalidOperationException ex)
        {
            this._capturedException = ex;
        }
    }

    [Then(@"すべてのノードが訪問される")]
    public void Thenすべてのノードが訪問される()
    {
        Assert.IsTrue(this._traversalCompleted, "走査が完了していません。");
        Assert.IsGreaterThan(0, this._visitedNodeCount, "訪問されたノードがありません。");
    }

    [Then(@"訪問されたノード数は (\d+) である")]
    public void Then訪問されたノード数はである(int expectedCount)
    {
        Assert.AreEqual(expectedCount, this._visitedNodeCount,
            $"訪問されたノード数が一致しません。期待: {expectedCount}, 実際: {this._visitedNodeCount}");
    }

    [Then(@"収集されたリンク数は (\d+) である")]
    public void Then収集されたリンク数はである(int expectedCount)
    {
        Assert.HasCount(expectedCount, this._collectedLinks,
            $"収集されたリンク数が一致しません。期待: {expectedCount}, 実際: {this._collectedLinks.Count}");
    }

    [Then(@"収集されたセクションタイトル数は (\d+) である")]
    public void Then収集されたセクションタイトル数はである(int expectedCount)
    {
        Assert.HasCount(expectedCount, this._collectedSectionTitles,
            $"収集されたセクションタイトル数が一致しません。期待: {expectedCount}, 実際: {this._collectedSectionTitles.Count}");
    }

    [Then(@"セクションタイトルは順番に ""(.*)"", ""(.*)"" である")]
    public void Thenセクションタイトルは順番にである(string title1, string title2)
    {
        Assert.IsGreaterThanOrEqualTo(2, this._collectedSectionTitles.Count,
            $"セクションタイトルが2個以上必要です。実際: {this._collectedSectionTitles.Count}");

        Assert.AreEqual(title1, this._collectedSectionTitles[0].TitleContent,
            $"1番目のセクションタイトルが一致しません。期待: '{title1}', 実際: '{this._collectedSectionTitles[0].TitleContent}'");

        Assert.AreEqual(title2, this._collectedSectionTitles[1].TitleContent,
            $"2番目のセクションタイトルが一致しません。期待: '{title2}', 実際: '{this._collectedSectionTitles[1].TitleContent}'");
    }

    [Then(@"エラーなく走査が完了する")]
    public void Thenエラーなく走査が完了する()
    {
        Assert.IsTrue(this._traversalCompleted, "走査が完了していません。");
    }

    [Then(@"例外が伝播する")]
    public void Then例外が伝播する()
    {
        Assert.IsNotNull(this._capturedException, "例外が伝播していません。");
        Assert.IsInstanceOfType<InvalidOperationException>(this._capturedException);
    }

    [Then(@"欠落ノードも訪問される")]
    public void Then欠落ノードも訪問される()
    {
        // 欠落ノードを含む構文木でも、走査が正常に完了することを確認
        Assert.IsTrue(this._traversalCompleted, "走査が完了していません。");
        Assert.IsGreaterThan(0, this._visitedNodeCount, "訪問されたノードがありません。");
    }

    // US2 用ステップ定義

    [When(@"結果を返す Visitor で目次を生成する")]
    public void When結果を返すVisitorで目次を生成する()
    {
        var tree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(tree, "構文木が null です。");

        var visitor = new TocGeneratorVisitor();
        this._tocEntries = tree.Root.Accept(visitor);
        this._traversalCompleted = true;
    }

    [Then(@"目次項目数は (\d+) である")]
    public void Then目次項目数はである(int expectedCount)
    {
        Assert.HasCount(expectedCount, this._tocEntries,
            $"目次項目数が一致しません。期待: {expectedCount}, 実際: {this._tocEntries.Count}");
    }

    [Then(@"目次の階層構造が正しい")]
    public void Then目次の階層構造が正しい()
    {
        // 3 項目（セクション1、サブセクション1-1、セクション2）が正しい階層で存在することを確認
        Assert.IsGreaterThanOrEqualTo(3, this._tocEntries.Count, "目次項目が3つ以上必要です。");

        // レベル 2 のセクションが 2 つ、レベル 3 のサブセクションが 1 つ
        var level2Count = 0;
        var level3Count = 0;
        foreach (var entry in this._tocEntries)
        {
            if (entry.Level == 2)
            {
                level2Count++;
            }
            else if (entry.Level == 3)
            {
                level3Count++;
            }
        }

        Assert.AreEqual(2, level2Count, "レベル 2 のセクションは 2 つ必要です。");
        Assert.AreEqual(1, level3Count, "レベル 3 のサブセクションは 1 つ必要です。");
    }

    [When(@"結果を返す Visitor でプレーンテキストを抽出する")]
    public void When結果を返すVisitorでプレーンテキストを抽出する()
    {
        var tree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(tree, "構文木が null です。");

        var visitor = new PlainTextExtractorVisitor();
        this._extractedText = tree.Root.Accept(visitor);
        this._traversalCompleted = true;
    }

    [Then(@"抽出されたテキストに ""(.*)"" が含まれる")]
    public void Then抽出されたテキストにが含まれる(string expectedText)
    {
        Assert.IsTrue(this._extractedText.Contains(expectedText, StringComparison.Ordinal),
            $"抽出されたテキストに '{expectedText}' が含まれていません。実際のテキスト: '{this._extractedText}'");
    }

    /// <summary>
    /// 目次エントリを表す構造体。
    /// </summary>
    private readonly record struct TocEntry(int Level, string Title);

    /// <summary>
    /// すべてのノードをカウントする Visitor。
    /// </summary>
    private sealed class NodeCountingVisitor : ISyntaxVisitor
    {
        public int Count { get; private set; }

        public void VisitDocument(DocumentSyntax node)
        {
            this.Count++;
            node.Header?.Accept(this);
            node.Body?.Accept(this);
        }

        public void VisitDocumentHeader(DocumentHeaderSyntax node)
        {
            this.Count++;
            node.Title?.Accept(this);
            node.AuthorLine?.Accept(this);
        }

        public void VisitAuthorLine(AuthorLineSyntax node)
        {
            this.Count++;
        }

        public void VisitDocumentBody(DocumentBodySyntax node)
        {
            this.Count++;
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
            this.Count++;
            node.Title?.Accept(this);
            foreach (var child in node.Content)
            {
                child.Accept(this);
            }
        }

        public void VisitSectionTitle(SectionTitleSyntax node)
        {
            this.Count++;
        }

        public void VisitParagraph(ParagraphSyntax node)
        {
            this.Count++;
            foreach (var element in node.InlineElements)
            {
                element.Accept(this);
            }
        }

        public void VisitText(TextSyntax node)
        {
            this.Count++;
        }

        public void VisitLink(LinkSyntax node)
        {
            this.Count++;
        }
    }

    /// <summary>
    /// リンクを収集する Visitor。
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
        }

        public void VisitAuthorLine(AuthorLineSyntax node)
        {
            // AuthorLine にはリンクがない
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
            // SectionTitle にはリンクがない
        }

        public void VisitParagraph(ParagraphSyntax node)
        {
            foreach (var element in node.InlineElements)
            {
                element.Accept(this);
            }
        }

        public void VisitText(TextSyntax node)
        {
            // Text にはリンクがない
        }

        public void VisitLink(LinkSyntax node)
        {
            this.Links.Add(node);
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
            // DocumentHeader のタイトルは収集しない（セクションタイトルではない）
        }

        public void VisitAuthorLine(AuthorLineSyntax node)
        {
            // AuthorLine にはセクションタイトルがない
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
            this.SectionTitles.Add(node);
        }

        public void VisitParagraph(ParagraphSyntax node)
        {
            // Paragraph にはセクションタイトルがない
        }

        public void VisitText(TextSyntax node)
        {
            // Text にはセクションタイトルがない
        }

        public void VisitLink(LinkSyntax node)
        {
            // Link にはセクションタイトルがない
        }
    }

    /// <summary>
    /// 例外を投げる Visitor。
    /// </summary>
    private sealed class ExceptionThrowingVisitor : ISyntaxVisitor
    {
        public void VisitDocument(DocumentSyntax node)
        {
            throw new InvalidOperationException("テスト用の例外");
        }

        public void VisitDocumentHeader(DocumentHeaderSyntax node)
        {
            throw new InvalidOperationException("テスト用の例外");
        }

        public void VisitAuthorLine(AuthorLineSyntax node)
        {
            throw new InvalidOperationException("テスト用の例外");
        }

        public void VisitDocumentBody(DocumentBodySyntax node)
        {
            throw new InvalidOperationException("テスト用の例外");
        }

        public void VisitSection(SectionSyntax node)
        {
            throw new InvalidOperationException("テスト用の例外");
        }

        public void VisitSectionTitle(SectionTitleSyntax node)
        {
            throw new InvalidOperationException("テスト用の例外");
        }

        public void VisitParagraph(ParagraphSyntax node)
        {
            throw new InvalidOperationException("テスト用の例外");
        }

        public void VisitText(TextSyntax node)
        {
            throw new InvalidOperationException("テスト用の例外");
        }

        public void VisitLink(LinkSyntax node)
        {
            throw new InvalidOperationException("テスト用の例外");
        }
    }

    /// <summary>
    /// 目次を生成する Visitor（ISyntaxVisitor&lt;TResult&gt; を使用）。
    /// </summary>
    private sealed class TocGeneratorVisitor : ISyntaxVisitor<List<TocEntry>>
    {
        public List<TocEntry> VisitDocument(DocumentSyntax node)
        {
            var entries = new List<TocEntry>();

            // Body 内のセクションを走査
            if (node.Body is not null)
            {
                entries.AddRange(node.Body.Accept(this));
            }

            return entries;
        }

        public List<TocEntry> VisitDocumentHeader(DocumentHeaderSyntax node)
        {
            // ドキュメントヘッダーからは目次を生成しない
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
                    entries.AddRange(child.AsNode()!.Accept(this));
                }
            }

            return entries;
        }

        public List<TocEntry> VisitSection(SectionSyntax node)
        {
            var entries = new List<TocEntry>();

            // セクションタイトルを目次に追加
            if (node.Title is not null)
            {
                entries.Add(new TocEntry(node.Title.Level, node.Title.TitleContent ?? string.Empty));
            }

            // ネストされたセクションを再帰的に処理
            foreach (var child in node.Content)
            {
                entries.AddRange(child.Accept(this));
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

        public List<TocEntry> VisitText(TextSyntax node)
        {
            return [];
        }

        public List<TocEntry> VisitLink(LinkSyntax node)
        {
            return [];
        }
    }

    /// <summary>
    /// プレーンテキストを抽出する Visitor（ISyntaxVisitor&lt;TResult&gt; を使用）。
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
                    sb.Append(child.AsNode()!.Accept(this));
                }
            }

            return sb.ToString();
        }

        public string VisitSection(SectionSyntax node)
        {
            var sb = new StringBuilder();

            foreach (var child in node.Content)
            {
                sb.Append(child.Accept(this));
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

            foreach (var element in node.InlineElements)
            {
                sb.Append(element.Accept(this));
            }

            return sb.ToString();
        }

        public string VisitText(TextSyntax node)
        {
            return node.Text ?? string.Empty;
        }

        public string VisitLink(LinkSyntax node)
        {
            // リンクの表示テキストを返す
            return node.DisplayText ?? string.Empty;
        }
    }
}
