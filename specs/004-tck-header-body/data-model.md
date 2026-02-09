# Data Model: TCK header-body-output テスト対応

**Branch**: `004-tck-header-body` | **Date**: 2026-02-08

## SyntaxTree レベル

### 新規エンティティ

#### AttributeEntrySyntax

AsciiDoc の属性エントリ（`:name: value`）を表す構文ノード。

| プロパティ | 型 | 説明 |
|-----------|-----|------|
| OpeningColon | SyntaxToken | 開きコロン `:` |
| Name | SyntaxToken | 属性名テキスト |
| ClosingColon | SyntaxToken | 閉じコロン `:`（値がある場合、空白をトリビアとして保持） |
| Value | SyntaxToken? | 属性値テキスト（省略可。改行をトリビアとして保持） |

**関連**:
- 親: DocumentHeaderSyntax
- 基底クラス: BlockSyntax

**バリデーション**:
- OpeningColon は `SyntaxKind.ColonToken` であること
- Name は `SyntaxKind.TextToken` であること
- ClosingColon は `SyntaxKind.ColonToken` であること
- Value が存在する場合、`SyntaxKind.TextToken` であること

### 変更エンティティ

#### DocumentHeaderSyntax（変更）

| プロパティ | 型 | 変更 |
|-----------|-----|------|
| Title | SectionTitleSyntax? | 既存 |
| AuthorLine | AuthorLineSyntax? | 既存 |
| AttributeEntries | SyntaxList\<AttributeEntrySyntax\> | **新規追加** |

#### SyntaxKind（変更）

| 値 | 説明 | 変更 |
|----|------|------|
| AttributeEntry | 属性エントリ ノード | **新規追加**（Blocks 範囲内） |

#### ISyntaxVisitor / ISyntaxVisitor\<TResult\>（変更）

| メソッド | 変更 |
|---------|------|
| VisitAttributeEntry(AttributeEntrySyntax node) | **新規追加** |

## ASG レベル

### 変更エンティティ

#### AsgDocument（変更）

| プロパティ | 型 | JSON 名 | 変更 |
|-----------|-----|---------|------|
| Name | string | "name" | 既存（"document"） |
| Type | string | "type" | 既存（"block"） |
| Attributes | IReadOnlyDictionary\<string, string\> | "attributes" | **新規追加** |
| Header | AsgHeader? | "header" | 既存 |
| Blocks | IReadOnlyList\<AsgBlockNode\> | "blocks" | 既存 |
| Location | AsgLocation? | "location" | 既存 |

**Attributes の値**:
- 属性エントリあり → `{ "name": "value", ... }`
- 値なし属性 → `{ "name": "" }`
- 属性なし → `{}`（空辞書、null ではない）

#### AsgJsonContext（変更）

| 登録型 | 変更 |
|--------|------|
| IReadOnlyDictionary\<string, string\> | **新規追加** |

## エンティティ関係図

```
DocumentSyntax
└── DocumentHeaderSyntax
    ├── SectionTitleSyntax? (Title)
    ├── AuthorLineSyntax? (AuthorLine)
    └── SyntaxList<AttributeEntrySyntax> (AttributeEntries)  ← 新規
        ├── ColonToken (OpeningColon)
        ├── TextToken (Name)
        ├── ColonToken (ClosingColon)
        └── TextToken? (Value)

AsgDocument
├── AsgHeader?
│   └── AsgInlineNode[] (Title)
├── IReadOnlyDictionary<string, string> (Attributes)  ← 新規
├── AsgBlockNode[] (Blocks)
│   └── AsgParagraph
│       └── AsgInlineNode[] (Inlines)
└── AsgLocation?
```
