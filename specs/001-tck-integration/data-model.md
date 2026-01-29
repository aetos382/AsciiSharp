# Data Model: TCK 統合テスト基盤

**Feature**: TCK 統合テスト基盤
**Date**: 2026-01-29

## 概要

このドキュメントは、TCK 統合に関わるデータモデルとエンティティを定義します。

---

## エンティティ

### 1. TckInput

TCK から CLI アダプターに渡される入力データ。

```csharp
/// <summary>
/// TCK から渡される入力データを表す。
/// </summary>
public sealed class TckInput
{
    /// <summary>
    /// パース対象の AsciiDoc 文書の内容。
    /// </summary>
    [JsonPropertyName("contents")]
    public required string Contents { get; init; }

    /// <summary>
    /// 入力ファイルの仮想パス。
    /// </summary>
    [JsonPropertyName("path")]
    public required string Path { get; init; }

    /// <summary>
    /// パース タイプ。"block" または "inline"。
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }
}
```

**バリデーションルール**:
| フィールド | 必須 | 制約 |
|------------|------|------|
| Contents | Yes | null 不可、空文字列は許容 |
| Path | Yes | null 不可、空文字列は許容 |
| Type | Yes | "block" または "inline" のみ |

**状態遷移**: なし（イミュータブル）

---

### 2. ASG モデル（既存）

ASG (Abstract Semantic Graph) は、パースされた AsciiDoc 文書の意味構造を表現する。

#### 2.1 AsgNode（基底）

```csharp
/// <summary>
/// ASG ノードの基底クラス。
/// </summary>
public abstract class AsgNode
{
    /// <summary>
    /// ノードの位置情報（オプション）。
    /// </summary>
    [JsonPropertyName("location")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AsgLocation? Location { get; init; }
}
```

#### 2.2 AsgBlockNode

```csharp
/// <summary>
/// ブロック要素の基底クラス。
/// </summary>
public abstract class AsgBlockNode : AsgNode
{
    /// <summary>
    /// ノードのタイプ。常に "block"。
    /// </summary>
    [JsonPropertyName("type")]
    public string Type => "block";
}
```

#### 2.3 AsgInlineNode

```csharp
/// <summary>
/// インライン要素の基底クラス。
/// </summary>
public abstract class AsgInlineNode : AsgNode
{
    // type は派生クラスで定義（"string" など）
}
```

#### 2.4 AsgDocument

```csharp
/// <summary>
/// ASG の document ブロックを表す。
/// </summary>
public sealed class AsgDocument : AsgBlockNode
{
    [JsonPropertyName("name")]
    public string Name => "document";

    [JsonPropertyName("header")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AsgHeader? Header { get; init; }

    [JsonPropertyName("blocks")]
    public IReadOnlyList<AsgBlockNode> Blocks { get; init; } = [];
}
```

#### 2.5 AsgSection

```csharp
/// <summary>
/// ASG の section ブロックを表す。
/// </summary>
public sealed class AsgSection : AsgBlockNode
{
    [JsonPropertyName("name")]
    public string Name => "section";

    [JsonPropertyName("title")]
    public IReadOnlyList<AsgInlineNode> Title { get; init; } = [];

    [JsonPropertyName("level")]
    public int Level { get; init; }

    [JsonPropertyName("blocks")]
    public IReadOnlyList<AsgBlockNode> Blocks { get; init; } = [];
}
```

#### 2.6 AsgParagraph

```csharp
/// <summary>
/// ASG の paragraph ブロックを表す。
/// </summary>
public sealed class AsgParagraph : AsgBlockNode
{
    [JsonPropertyName("name")]
    public string Name => "paragraph";

    [JsonPropertyName("inlines")]
    public IReadOnlyList<AsgInlineNode> Inlines { get; init; } = [];
}
```

#### 2.7 AsgText

```csharp
/// <summary>
/// ASG の text インラインを表す。
/// </summary>
public sealed class AsgText : AsgInlineNode
{
    [JsonPropertyName("name")]
    public string Name => "text";

    [JsonPropertyName("type")]
    public string Type => "string";

    [JsonPropertyName("value")]
    public required string Value { get; init; }
}
```

---

### 3. 位置情報

#### 3.1 AsgLocation

```csharp
/// <summary>
/// ASG における位置情報（開始位置と終了位置）を表す。
/// </summary>
/// <remarks>
/// JSON では [{start}, {end}] 形式の配列としてシリアライズされる。
/// </remarks>
[JsonConverter(typeof(AsgLocationJsonConverter))]
public sealed class AsgLocation
{
    public AsgPosition Start { get; }
    public AsgPosition End { get; }

    public AsgLocation(AsgPosition start, AsgPosition end);
}
```

#### 3.2 AsgPosition

```csharp
/// <summary>
/// ASG における単一の位置情報（行・列）を表す。
/// </summary>
public sealed class AsgPosition
{
    /// <summary>
    /// 行番号（1-based）。
    /// </summary>
    [JsonPropertyName("line")]
    public int Line { get; }

    /// <summary>
    /// 列番号（1-based）。
    /// </summary>
    [JsonPropertyName("col")]
    public int Col { get; }

    public AsgPosition(int line, int col);
}
```

**JSON シリアライズ例**:
```json
[{ "line": 1, "col": 1 }, { "line": 3, "col": 10 }]
```

---

## エンティティ関係図

```
TckInput
    │
    │ (JSON デシリアライズ)
    ▼
┌─────────────────────────────────────────┐
│             CLI アダプター               │
│  1. TckInput をデシリアライズ           │
│  2. AsciiSharp パーサーで SyntaxTree 生成│
│  3. AsgConverter で ASG に変換          │
│  4. ASG を JSON シリアライズ            │
└─────────────────────────────────────────┘
    │
    │ (JSON シリアライズ)
    ▼
AsgDocument
    ├── Header?: AsgHeader
    │       └── Title: AsgInlineNode[]
    └── Blocks: AsgBlockNode[]
            ├── AsgSection
            │       ├── Title: AsgInlineNode[]
            │       ├── Level: int
            │       └── Blocks: AsgBlockNode[]
            └── AsgParagraph
                    └── Inlines: AsgInlineNode[]
                            └── AsgText
                                    └── Value: string
```

---

## バリデーション サマリー

| エンティティ | フィールド | ルール |
|-------------|-----------|--------|
| TckInput | Contents | null 不可 |
| TckInput | Path | null 不可 |
| TckInput | Type | "block" または "inline" |
| AsgPosition | Line | 1 以上 |
| AsgPosition | Col | 1 以上 |
| AsgLocation | Start | null 不可 |
| AsgLocation | End | null 不可 |
