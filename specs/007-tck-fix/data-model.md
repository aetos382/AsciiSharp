# Data Model: TCK block/document/body-only テスト修正

**Feature**: 007-tck-fix
**Date**: 2026-02-22

## 変更対象エンティティ

### AsgDocument

`Source/AsciiSharp.Asg/Models/AsgDocument.cs`

```
AsgDocument
├── Name: string         = "document"（変更なし）
├── Attributes: IReadOnlyDictionary<string, string>?   ← 変更（null 許容化）
│   ├── null             → JSON に "attributes" フィールドを出力しない
│   └── {} or {key: v}  → JSON に "attributes": {} または "attributes": {...} を出力
├── Header: AsgHeader?   = null（変更なし）
├── Blocks: IReadOnlyList<AsgBlockNode>  = []（変更なし）
└── Location: AsgLocation?  （変更なし）
```

**不変条件**:
- `Attributes` が `null` → ドキュメントにヘッダーが存在しない
- `Attributes` が `{}` → ドキュメントにヘッダーは存在するが、属性エントリがない
- `Attributes` が `{key: value, ...}` → ドキュメントにヘッダーと属性エントリが存在する

## 変更なしエンティティ

- `AsgBlockNode`（基底クラス）: 変更なし
- `AsgParagraph`: 変更なし
- `AsgHeader`: 変更なし
- `AsgText`, `AsgInlineNode` など: 変更なし

## JSON シリアライズへの影響

| ドキュメント構造 | `Attributes` 値 | JSON 出力 |
|---|---|---|
| ヘッダーなし | `null` | `"attributes"` フィールドなし |
| ヘッダーあり・属性なし | `{}` | `"attributes": {}` |
| ヘッダーあり・属性あり | `{"key": "value"}` | `"attributes": {"key": "value"}` |
