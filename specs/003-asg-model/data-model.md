# Data Model: AsciiDoc ASG モデルクラス

**Date**: 2026-01-28
**Status**: 実装済み

## クラス階層

```
AsgNode (abstract)
├── AsgBlockNode (abstract)
│   ├── AsgDocument
│   ├── AsgSection
│   └── AsgParagraph
├── AsgInlineNode (abstract)
│   └── AsgText
└── AsgHeader
```

## エンティティ定義

### AsgNode (基底クラス)

| プロパティ | 型 | JSON 名 | 説明 |
|-----------|-----|---------|------|
| Location | AsgLocation? | location | ソース内の位置情報（オプション） |

**備考**: `name` と `type` は派生クラスで定義（JSON シリアライズの制約による）

### AsgBlockNode (ブロック要素基底)

| プロパティ | 型 | JSON 名 | 説明 |
|-----------|-----|---------|------|
| Type | string | type | 常に "block" |

**継承**: AsgNode

### AsgInlineNode (インライン要素基底)

| プロパティ | 型 | JSON 名 | 説明 |
|-----------|-----|---------|------|
| (なし) | - | - | 将来の拡張用プレースホルダー |

**継承**: AsgNode

### AsgDocument

| プロパティ | 型 | JSON 名 | 説明 |
|-----------|-----|---------|------|
| Name | string | name | 常に "document" |
| Header | AsgHeader? | header | 文書ヘッダー（null の場合は出力されない） |
| Blocks | IReadOnlyList<AsgBlockNode> | blocks | 子ブロック要素 |

**継承**: AsgBlockNode
**JSON 出力時の条件**: Header が null の場合、`header` プロパティは省略

### AsgSection

| プロパティ | 型 | JSON 名 | 説明 |
|-----------|-----|---------|------|
| Name | string | name | 常に "section" |
| Title | IReadOnlyList<AsgInlineNode> | title | タイトルのインライン要素 |
| Level | int | level | セクションレベル（1-6） |
| Blocks | IReadOnlyList<AsgBlockNode> | blocks | 子ブロック要素 |

**継承**: AsgBlockNode

### AsgParagraph

| プロパティ | 型 | JSON 名 | 説明 |
|-----------|-----|---------|------|
| Name | string | name | 常に "paragraph" |
| Inlines | IReadOnlyList<AsgInlineNode> | inlines | インライン要素 |

**継承**: AsgBlockNode

### AsgHeader

| プロパティ | 型 | JSON 名 | 説明 |
|-----------|-----|---------|------|
| Title | IReadOnlyList<AsgInlineNode> | title | タイトルのインライン要素 |

**継承**: AsgNode

### AsgText

| プロパティ | 型 | JSON 名 | 説明 |
|-----------|-----|---------|------|
| Name | string | name | 常に "text" |
| Type | string | type | 常に "string" |
| Value | string | value | テキスト内容 |

**継承**: AsgInlineNode

### AsgPosition (値型)

| プロパティ | 型 | JSON 名 | 説明 |
|-----------|-----|---------|------|
| Line | int | line | 1-based 行番号 |
| Col | int | col | 1-based 列番号 |

**型**: record struct

### AsgLocation (値型)

| プロパティ | 型 | 説明 |
|-----------|-----|------|
| Start | AsgPosition | 開始位置 |
| End | AsgPosition | 終了位置（包含的） |

**型**: record struct
**JSON 出力**: `[{start}, {end}]` 配列形式（AsgLocationJsonConverter による）

## 変換マッピング

| SyntaxNode | AsgNode | 備考 |
|------------|---------|------|
| DocumentSyntax | AsgDocument | ルートノード |
| DocumentHeaderSyntax | AsgHeader | ConvertHeader 経由 |
| DocumentBodySyntax | - | ConvertBlocks 経由でブロック抽出 |
| SectionSyntax | AsgSection | ネスト可能 |
| SectionTitleSyntax | - | ConvertTitleInlines 経由でインライン抽出 |
| ParagraphSyntax | AsgParagraph | |
| TextSyntax | AsgText | |
| LinkSyntax | (スキップ) | 未対応 |
| AuthorLineSyntax | (スキップ) | 未対応 |

## 不変条件 (Invariants)

1. **AsgDocument.Blocks**: 空でも常にリストを持つ（null にならない）
2. **AsgSection.Title**: 空でも常にリストを持つ
3. **AsgSection.Level**: 1 以上の整数
4. **AsgPosition.Line/Col**: 1 以上の整数
5. **AsgLocation.End**: Start と同じか後の位置
