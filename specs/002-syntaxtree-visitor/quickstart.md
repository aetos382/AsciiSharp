# Quickstart: SyntaxTree Visitor パターン

**Date**: 2026-01-26
**Feature**: 002-syntaxtree-visitor

## 概要

AsciiSharp の構文木を Visitor パターンで走査する方法を説明します。

## 基本的な使い方

### 1. ISyntaxVisitor を実装してノードを走査する

```csharp
using AsciiSharp;
using AsciiSharp.Syntax;

// すべてのリンクを収集する Visitor
public class LinkCollector : ISyntaxVisitor
{
    public List<LinkSyntax> Links { get; } = new();

    public void VisitDocument(DocumentSyntax node)
    {
        // 子ノードを走査
        node.Header?.Accept(this);
        node.Body?.Accept(this);
    }

    public void VisitDocumentHeader(DocumentHeaderSyntax node)
    {
        // ヘッダー内の処理（必要に応じて）
    }

    public void VisitDocumentBody(DocumentBodySyntax node)
    {
        // すべての子ノードを走査
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
            if (child.IsNode)
            {
                child.AsNode()?.Accept(this);
            }
        }
    }

    public void VisitSectionTitle(SectionTitleSyntax node)
    {
        // セクションタイトルの処理
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

    public void VisitText(TextSyntax node)
    {
        // テキストノードの処理
    }

    public void VisitLink(LinkSyntax node)
    {
        // リンクを収集
        Links.Add(node);
    }
}

// 使用例
var source = SourceText.From("See https://example.com[example] for details.");
var syntaxTree = SyntaxTree.Parse(source);

var collector = new LinkCollector();
syntaxTree.Root.Accept(collector);

foreach (var link in collector.Links)
{
    Console.WriteLine($"Link: {link}");
}
```

### 2. ISyntaxVisitor&lt;TResult&gt; を実装して結果を返す

```csharp
using AsciiSharp;
using AsciiSharp.Syntax;

// テキストを抽出する Visitor
public class TextExtractor : ISyntaxVisitor<string>
{
    public string VisitDocument(DocumentSyntax node)
    {
        var header = node.Header?.Accept(this) ?? "";
        var body = node.Body?.Accept(this) ?? "";
        return header + body;
    }

    public string VisitDocumentHeader(DocumentHeaderSyntax node)
    {
        return node.ToString();
    }

    public string VisitDocumentBody(DocumentBodySyntax node)
    {
        var sb = new StringBuilder();
        foreach (var child in node.ChildNodesAndTokens())
        {
            if (child.IsNode)
            {
                sb.Append(child.AsNode()?.Accept(this));
            }
        }
        return sb.ToString();
    }

    public string VisitSection(SectionSyntax node)
    {
        var sb = new StringBuilder();
        if (node.Title != null)
        {
            sb.Append(node.Title.Accept(this));
        }
        foreach (var child in node.ChildNodesAndTokens())
        {
            if (child.IsNode)
            {
                sb.Append(child.AsNode()?.Accept(this));
            }
        }
        return sb.ToString();
    }

    public string VisitSectionTitle(SectionTitleSyntax node)
    {
        return node.ToString() + "\n";
    }

    public string VisitParagraph(ParagraphSyntax node)
    {
        var sb = new StringBuilder();
        foreach (var child in node.ChildNodesAndTokens())
        {
            if (child.IsNode)
            {
                sb.Append(child.AsNode()?.Accept(this));
            }
            else if (child.IsToken)
            {
                sb.Append(child.AsToken().Text);
            }
        }
        return sb.ToString() + "\n";
    }

    public string VisitText(TextSyntax node)
    {
        return node.ToString();
    }

    public string VisitLink(LinkSyntax node)
    {
        // リンクテキストのみを返す
        return node.ToString();
    }
}

// 使用例
var source = SourceText.From("= Title\n\nParagraph text.");
var syntaxTree = SyntaxTree.Parse(source);

var extractor = new TextExtractor();
var text = syntaxTree.Root.Accept(extractor);
Console.WriteLine(text);
```

## ポイント

1. **子ノードの走査は利用者の責任**: インターフェイスは各ノード型の Visit メソッドのみを提供し、子ノードの走査ロジックは含まれません。

2. **Accept メソッドの使用**: ノードを訪問するには、そのノードの `Accept` メソッドを呼び出します。これにより適切な `VisitXxx` メソッドが呼び出されます。

3. **null チェック**: オプショナルな子ノード（`Header`、`Body` など）は null の可能性があるため、適切にチェックしてください。

4. **ChildNodesAndTokens()**: 子ノードとトークンを列挙するには、既存の `ChildNodesAndTokens()` メソッドを使用します。
