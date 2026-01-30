# Research: BlockSyntax と InlineSyntax 階層の導入

**Date**: 2026-01-30
**Feature**: 001-block-inline-syntax

## 概要

本機能は内部リファクタリングであり、外部調査が必要な不明点は存在しない。以下は既存コードベースの分析結果と設計決定の根拠を記録する。

## 調査項目

### 1. 既存の継承パターン

**調査対象**: `SyntaxNode` クラスとその派生クラス

**結果**:

現在のクラス階層:
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

**決定**: 既存パターンに従い、`private protected` コンストラクタを持つ抽象クラスを追加する

**根拠**: `SyntaxNode` が既に `private protected` コンストラクタを使用しており、一貫性を維持するため

**却下した代替案**: `interface` による分類 - 継承階層の変更なしに分類を追加できるが、将来的な共通実装の追加が困難になる

### 2. SyntaxKind による分類

**調査対象**: `SyntaxKind.cs` の定義

**結果**:

```csharp
// Nodes (Blocks) - MVP スコープ (300番台)
Document = 300,
DocumentHeader,
DocumentBody,
Section,
SectionTitle,
Paragraph,
AuthorLine,

// Nodes (Inlines) - MVP スコープ (400番台)
TextSpan = 400,
Text,
Link,
```

**決定**: `SyntaxKind` のコメントで既に Block/Inline の区別が示されている。この分類に従う

**根拠**: 既存の設計意図を尊重し、型システムでも同じ分類を表現する

### 3. .NET Standard 2.0 互換性

**調査対象**: `private protected` 修飾子の .NET Standard 2.0 サポート

**結果**: `private protected` は C# 7.2 で導入され、.NET Standard 2.0 でサポートされる

**決定**: `private protected` を使用する

**根拠**: 既存コードで既に使用されており、Polyfill なしで動作する

### 4. Visitor パターンへの影響

**調査対象**: `ISyntaxVisitor` インターフェースの設計

**結果**:
- `ISyntaxVisitor` は具象クラスごとに `Visit*` メソッドを持つ
- 抽象クラス（`SyntaxNode`）用の Visit メソッドはない

**決定**: `BlockSyntax` / `InlineSyntax` 用の Visit メソッドは追加しない

**根拠**:
- マーカークラスとして機能するため、訪問ロジックは不要
- 必要な場合は `is BlockSyntax` パターンマッチングで対応可能
- Visitor インターフェースの変更は既存コードへの影響が大きい

## まとめ

| 項目 | 決定 | 根拠 |
|------|------|------|
| クラス種別 | 抽象クラス | 将来の共通実装追加を可能にする |
| コンストラクタ | `private protected` | 既存パターンとの一貫性 |
| 追加メンバー | なし（マーカークラス） | FR-007 の要件 |
| Visitor 変更 | なし | 影響範囲の最小化 |

すべての不明点が解消され、Phase 1 に進行可能。
