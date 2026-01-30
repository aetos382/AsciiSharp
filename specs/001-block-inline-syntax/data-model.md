# Data Model: BlockSyntax と InlineSyntax 階層

**Date**: 2026-01-30
**Feature**: 001-block-inline-syntax

## クラス階層図

### 変更前

```
SyntaxNode (abstract)
├── DocumentSyntax
├── DocumentHeaderSyntax
├── DocumentBodySyntax
├── SectionSyntax
├── SectionTitleSyntax
├── ParagraphSyntax
├── AuthorLineSyntax
├── TextSyntax
└── LinkSyntax
```

### 変更後

```
SyntaxNode (abstract)
├── BlockSyntax (abstract) [新規]
│   ├── DocumentSyntax
│   ├── DocumentHeaderSyntax
│   ├── DocumentBodySyntax
│   ├── SectionSyntax
│   ├── SectionTitleSyntax
│   ├── ParagraphSyntax
│   └── AuthorLineSyntax
└── InlineSyntax (abstract) [新規]
    ├── TextSyntax
    └── LinkSyntax
```

## エンティティ定義

### BlockSyntax

**目的**: ブロックレベル要素の中間抽象クラス

```csharp
/// <summary>
/// ブロックレベル要素を表す構文ノードの抽象基底クラス。
/// </summary>
/// <remarks>
/// ブロック要素は段落、セクション、ドキュメントなどの構造的要素を表す。
/// </remarks>
public abstract class BlockSyntax : SyntaxNode
{
    /// <summary>
    /// BlockSyntax を作成する。
    /// </summary>
    private protected BlockSyntax(
        InternalNode internalNode,
        SyntaxNode? parent,
        int position,
        SyntaxTree? syntaxTree)
        : base(internalNode, parent, position, syntaxTree)
    {
    }
}
```

**属性**: なし（マーカークラス）
**メソッド**: なし（`SyntaxNode` から継承）
**制約**: `private protected` コンストラクタによりアセンブリ外からの継承を防止

### InlineSyntax

**目的**: インラインレベル要素の中間抽象クラス

```csharp
/// <summary>
/// インラインレベル要素を表す構文ノードの抽象基底クラス。
/// </summary>
/// <remarks>
/// インライン要素はテキスト、リンク、書式設定などのフロー内要素を表す。
/// </remarks>
public abstract class InlineSyntax : SyntaxNode
{
    /// <summary>
    /// InlineSyntax を作成する。
    /// </summary>
    private protected InlineSyntax(
        InternalNode internalNode,
        SyntaxNode? parent,
        int position,
        SyntaxTree? syntaxTree)
        : base(internalNode, parent, position, syntaxTree)
    {
    }
}
```

**属性**: なし（マーカークラス）
**メソッド**: なし（`SyntaxNode` から継承）
**制約**: `private protected` コンストラクタによりアセンブリ外からの継承を防止

## 分類ルール

### BlockSyntax を継承するクラス

| クラス | SyntaxKind | 根拠 |
|--------|------------|------|
| DocumentSyntax | Document (300) | 文書全体を表すブロック要素 |
| DocumentHeaderSyntax | DocumentHeader (301) | 文書ヘッダーを表すブロック要素 |
| DocumentBodySyntax | DocumentBody (302) | 文書本体を表すブロック要素 |
| SectionSyntax | Section (303) | セクションを表すブロック要素 |
| SectionTitleSyntax | SectionTitle (304) | セクションタイトルを表すブロック要素 |
| ParagraphSyntax | Paragraph (305) | 段落を表すブロック要素 |
| AuthorLineSyntax | AuthorLine (306) | 著者行を表すブロック要素 |

### InlineSyntax を継承するクラス

| クラス | SyntaxKind | 根拠 |
|--------|------------|------|
| TextSyntax | Text (401) | プレーンテキストを表すインライン要素 |
| LinkSyntax | Link (402) | リンクを表すインライン要素 |

## 変更が必要なファイル

| ファイル | 変更内容 |
|----------|----------|
| `BlockSyntax.cs` | 新規作成 |
| `InlineSyntax.cs` | 新規作成 |
| `DocumentSyntax.cs` | `: SyntaxNode` → `: BlockSyntax` |
| `DocumentHeaderSyntax.cs` | `: SyntaxNode` → `: BlockSyntax` |
| `DocumentBodySyntax.cs` | `: SyntaxNode` → `: BlockSyntax` |
| `SectionSyntax.cs` | `: SyntaxNode` → `: BlockSyntax` |
| `SectionTitleSyntax.cs` | `: SyntaxNode` → `: BlockSyntax` |
| `ParagraphSyntax.cs` | `: SyntaxNode` → `: BlockSyntax` |
| `AuthorLineSyntax.cs` | `: SyntaxNode` → `: BlockSyntax` |
| `TextSyntax.cs` | `: SyntaxNode` → `: InlineSyntax` |
| `LinkSyntax.cs` | `: SyntaxNode` → `: InlineSyntax` |

## 検証ルール

1. **型階層の整合性**
   - `BlockSyntax` は `SyntaxNode` を継承する
   - `InlineSyntax` は `SyntaxNode` を継承する
   - ブロック要素クラスは `BlockSyntax` を継承する
   - インライン要素クラスは `InlineSyntax` を継承する

2. **排他性**
   - 同一クラスが `BlockSyntax` と `InlineSyntax` の両方を継承することはない

3. **網羅性**
   - すべての具象 `SyntaxNode` 派生クラスは `BlockSyntax` または `InlineSyntax` のいずれかを継承する
