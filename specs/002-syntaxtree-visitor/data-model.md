# Data Model: SyntaxTree Visitor パターン

**Date**: 2026-01-26
**Feature**: 002-syntaxtree-visitor

## インターフェイス定義

### ISyntaxVisitor

戻り値なしで構文木を走査するための Visitor インターフェイス。

```csharp
namespace AsciiSharp.Syntax;

/// <summary>
/// 構文木を走査するための Visitor インターフェイス。
/// </summary>
public interface ISyntaxVisitor
{
    /// <summary>
    /// DocumentSyntax ノードを訪問する。
    /// </summary>
    void VisitDocument(DocumentSyntax node);

    /// <summary>
    /// DocumentHeaderSyntax ノードを訪問する。
    /// </summary>
    void VisitDocumentHeader(DocumentHeaderSyntax node);

    /// <summary>
    /// AuthorLineSyntax ノードを訪問する。
    /// </summary>
    void VisitAuthorLine(AuthorLineSyntax node);

    /// <summary>
    /// DocumentBodySyntax ノードを訪問する。
    /// </summary>
    void VisitDocumentBody(DocumentBodySyntax node);

    /// <summary>
    /// SectionSyntax ノードを訪問する。
    /// </summary>
    void VisitSection(SectionSyntax node);

    /// <summary>
    /// SectionTitleSyntax ノードを訪問する。
    /// </summary>
    void VisitSectionTitle(SectionTitleSyntax node);

    /// <summary>
    /// ParagraphSyntax ノードを訪問する。
    /// </summary>
    void VisitParagraph(ParagraphSyntax node);

    /// <summary>
    /// TextSyntax ノードを訪問する。
    /// </summary>
    void VisitText(TextSyntax node);

    /// <summary>
    /// LinkSyntax ノードを訪問する。
    /// </summary>
    void VisitLink(LinkSyntax node);
}
```

### ISyntaxVisitor&lt;TResult&gt;

走査結果を返すための Visitor インターフェイス。

```csharp
namespace AsciiSharp.Syntax;

/// <summary>
/// 構文木を走査し、結果を返すための Visitor インターフェイス。
/// </summary>
/// <typeparam name="TResult">訪問結果の型。</typeparam>
public interface ISyntaxVisitor<TResult>
{
    /// <summary>
    /// DocumentSyntax ノードを訪問する。
    /// </summary>
    TResult VisitDocument(DocumentSyntax node);

    /// <summary>
    /// DocumentHeaderSyntax ノードを訪問する。
    /// </summary>
    TResult VisitDocumentHeader(DocumentHeaderSyntax node);

    /// <summary>
    /// AuthorLineSyntax ノードを訪問する。
    /// </summary>
    TResult VisitAuthorLine(AuthorLineSyntax node);

    /// <summary>
    /// DocumentBodySyntax ノードを訪問する。
    /// </summary>
    TResult VisitDocumentBody(DocumentBodySyntax node);

    /// <summary>
    /// SectionSyntax ノードを訪問する。
    /// </summary>
    TResult VisitSection(SectionSyntax node);

    /// <summary>
    /// SectionTitleSyntax ノードを訪問する。
    /// </summary>
    TResult VisitSectionTitle(SectionTitleSyntax node);

    /// <summary>
    /// ParagraphSyntax ノードを訪問する。
    /// </summary>
    TResult VisitParagraph(ParagraphSyntax node);

    /// <summary>
    /// TextSyntax ノードを訪問する。
    /// </summary>
    TResult VisitText(TextSyntax node);

    /// <summary>
    /// LinkSyntax ノードを訪問する。
    /// </summary>
    TResult VisitLink(LinkSyntax node);
}
```

## SyntaxNode への追加メソッド

### 抽象基底クラス (SyntaxNode)

```csharp
// SyntaxNode.cs に追加
public abstract class SyntaxNode
{
    // 既存のメンバー...

    /// <summary>
    /// 指定された Visitor でこのノードを訪問する。
    /// </summary>
    /// <param name="visitor">Visitor。</param>
    public abstract void Accept(ISyntaxVisitor visitor);

    /// <summary>
    /// 指定された Visitor でこのノードを訪問し、結果を返す。
    /// </summary>
    /// <typeparam name="TResult">訪問結果の型。</typeparam>
    /// <param name="visitor">Visitor。</param>
    /// <returns>訪問結果。</returns>
    public abstract TResult Accept<TResult>(ISyntaxVisitor<TResult> visitor);
}
```

### 各派生クラスの実装例

```csharp
// DocumentSyntax.cs
public sealed class DocumentSyntax : SyntaxNode
{
    // 既存のメンバー...

    public override void Accept(ISyntaxVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        visitor.VisitDocument(this);
    }

    public override TResult Accept<TResult>(ISyntaxVisitor<TResult> visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        return visitor.VisitDocument(this);
    }
}
```

## 既存ノード型一覧

| ノード型 | 説明 | VisitXxx メソッド |
|---------|------|------------------|
| DocumentSyntax | 文書全体 | VisitDocument |
| DocumentHeaderSyntax | 文書ヘッダー | VisitDocumentHeader |
| AuthorLineSyntax | 著者行 | VisitAuthorLine |
| DocumentBodySyntax | 文書本体 | VisitDocumentBody |
| SectionSyntax | セクション | VisitSection |
| SectionTitleSyntax | セクションタイトル | VisitSectionTitle |
| ParagraphSyntax | 段落 | VisitParagraph |
| TextSyntax | テキスト | VisitText |
| LinkSyntax | リンク | VisitLink |

## 対象外

以下は Visitor の対象外とする（構造体であり、SyntaxNode 派生でないため）:

- SyntaxToken
- SyntaxTrivia
