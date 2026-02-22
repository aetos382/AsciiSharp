# Feature Specification: TCK block/document/body-only テスト修正

**Feature Branch**: `007-tck-fix`
**Created**: 2026-02-22
**Status**: Draft
**Input**: TCKの失敗しているテストを1つ選び、それが通るために必要な仕様を決めて修正する

## 背景

AsciiDoc Technology Compatibility Kit（TCK）は、AsciiDocパーサーの準拠テストを実施するための公式テストスイートである。AsciiSharpは現在、TCKの12個のテストで失敗している。その中から `block/document/body-only` テストを選択し、修正する。

### 選択した失敗テスト

**テスト名**: `block/document/body-only`

**入力** (`body-only-input.adoc`):
```
body only
```

**TCKが期待する出力**:
```json
{
  "name": "document",
  "type": "block",
  "blocks": [
    {
      "name": "paragraph",
      "type": "block",
      "inlines": [
        {
          "name": "text",
          "type": "string",
          "value": "body only",
          "location": [{ "line": 1, "col": 1 }, { "line": 1, "col": 9 }]
        }
      ],
      "location": [{ "line": 1, "col": 1 }, { "line": 1, "col": 9 }]
    }
  ],
  "location": [{ "line": 1, "col": 1 }, { "line": 1, "col": 9 }]
}
```

**現在の実際の出力**:
```json
{
  "name": "document",
  "attributes": {},
  "blocks": [...],
  "type": "block",
  "location": [...]
}
```

**差異**: `"attributes": {}` フィールドが余分に含まれている。

## User Scenarios & Testing *(mandatory)*

### User Story 1 - ボディのみのドキュメントをASGに変換する (Priority: P1)

AsciiDocパーサーは、ヘッダーを持たない（ボディのみの）AsciiDocドキュメントをパースし、TCK準拠のASG（Abstract Semantic Graph）形式に変換できる。変換されたASGには、ドキュメントの構造（ブロック、インライン要素）が正確に表現され、余分なフィールドが含まれない。

**Why this priority**: TCKの `block/document` カテゴリにある基本的なテストであり、パーサーの基礎動作を検証する最重要テストである。この修正によって、同じ原因で失敗している他の10件のblockテストも同時に修正される見込みがある。

**Independent Test**: TCKの `block/document/body-only` テストを実行し、actualとexpectedが一致することで独立して検証できる。

**Acceptance Scenarios**:

1. **Given** ヘッダーを持たない1行のテキストのAsciiDocドキュメント（例：`body only`）、**When** パーサーがドキュメントをパースしASGに変換する、**Then** 変換結果のJSONには `attributes` フィールドが含まれない
2. **Given** ヘッダーを持たない1行のテキストのAsciiDocドキュメント（例：`body only`）、**When** パーサーがドキュメントをパースしASGに変換する、**Then** 変換結果のJSONは TCKの期待する出力と完全に一致する
3. **Given** タイトルを持つヘッダー付きのAsciiDocドキュメント（例：`= Document Title`）、**When** パーサーがドキュメントをパースしASGに変換する、**Then** 変換結果のJSONには `attributes: {}` フィールドが含まれる

---

### Edge Cases

- ヘッダーがあるが属性エントリがない場合、`attributes: {}` フィールドを出力する（TCK `block/document/header-body` の期待値から確認済み）
- ヘッダーがなく、かつ属性エントリも存在しない場合、`attributes` フィールド自体を省略する

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: パーサーはヘッダーなしのAsciiDocドキュメントをパースした場合、ASG変換結果の `document` ノードに `attributes` フィールドを含めてはならない
- **FR-002**: パーサーはヘッダーあり（タイトル行 `= Title` で始まる）のAsciiDocドキュメントをパースした場合、ASG変換結果の `document` ノードに `attributes` フィールドを常に含めなければならない（属性エントリが存在しない場合でも空オブジェクト `{}` を含める）
- **FR-003**: `attributes` フィールドの有無はドキュメントヘッダーの有無によってのみ決定され、属性エントリの数には依存しない
- **FR-004**: 上記の変更はTCKの `block/document/body-only` テストを通過させなければならない
- **FR-005**: 上記の変更はTCKの `block/document/header-body` テストにおいて、新たなエラーを発生させてはならない（当該テストは現在別の問題で失敗中であり、今回の修正によってその失敗状態を悪化させないこと）

### Key Entities

- **AsgDocument**: ドキュメント全体のルートノード。`attributes` フィールドの有無を制御する対象
- **DocumentHeaderSyntax**: パーサーが認識するドキュメントヘッダー。存在するかどうかが `attributes` フィールドの出力を決定する
- **AsgConverter**: SyntaxTreeをASGに変換するコンポーネント。`AsgDocument` の `attributes` プロパティの値をヘッダーの有無に応じて設定する

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: TCKの `block/document/body-only` テストがパスする（失敗 → 成功）
- **SC-002**: TCKの通過テスト数が1件以上増加する（現在1件 → 2件以上）
- **SC-003**: 既存のパス済みテスト（`block/header/attribute entries below title`）が引き続き通過する

## Assumptions

- TCKの `block/document/header-body` テストは今回の修正対象外である（別の問題が混在しているため、別のフィーチャーで対応）
- `attributes` フィールドの出力制御はJSON シリアライズ層で行うのではなく、AsgConverter のモデル生成層で行う
- 今回の修正対象は `AsgConverter` の `VisitDocument` メソッドの `Attributes` プロパティの設定ロジックのみ

## 実装詳細

本セクションでは、FR-001〜FR-003 を満たすための具体的なコード変更を示す。

### 変更ファイル一覧

| ファイル | 変更内容 |
|---|---|
| `Source/AsciiSharp.Asg/Models/AsgDocument.cs` | `Attributes` プロパティを null 許容に変更し、null 時はJSON出力から省略する |
| `Source/AsciiSharp.Asg/AsgConverter.cs` | `VisitDocument` でヘッダーの有無に応じて `Attributes` を設定する |

---

### 変更1: `AsgDocument.cs` — `Attributes` プロパティを null 許容に変更

**変更前**:
```csharp
/// <summary>
/// ドキュメント属性。属性エントリがない場合は空の辞書。
/// </summary>
[JsonPropertyName("attributes")]
public IReadOnlyDictionary<string, string> Attributes { get; init; } = new Dictionary<string, string>();
```

**変更後**:
```csharp
/// <summary>
/// ドキュメント属性。ヘッダーが存在する場合は辞書（属性なしなら空の辞書）、
/// ヘッダーが存在しない場合は null（JSON 出力から省略される）。
/// </summary>
[JsonPropertyName("attributes")]
[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
public IReadOnlyDictionary<string, string>? Attributes { get; init; }
```

**理由**: `JsonIgnoreCondition.WhenWritingNull` を付与することで、`Attributes` が `null` の場合に JSON シリアライズ時に `attributes` フィールド自体を省略できる。ヘッダーあり（属性エントリなし）の場合は `{}` が出力され、ヘッダーなしの場合は省略される。

---

### 変更2: `AsgConverter.cs` — `VisitDocument` でヘッダーの有無に応じて `Attributes` を設定

**変更前**:
```csharp
AsgNode? ISyntaxVisitor<AsgNode?>.VisitDocument(DocumentSyntax node)
{
    ArgumentNullException.ThrowIfNull(node);

    var header = node.Header is not null
        ? this.ConvertHeader(node.Header)
        : null;

    var attributes = ConvertAttributes(node.Header);

    return new AsgDocument
    {
        Attributes = attributes,
        Header = header,
        Blocks = this.ConvertBlocks(node.Body).ToList(),
        Location = this.GetLocation(node)
    };
}
```

**変更後**:
```csharp
AsgNode? ISyntaxVisitor<AsgNode?>.VisitDocument(DocumentSyntax node)
{
    ArgumentNullException.ThrowIfNull(node);

    var header = node.Header is not null
        ? this.ConvertHeader(node.Header)
        : null;

    // ヘッダーが存在する場合のみ attributes フィールドを含める（FR-001、FR-002）
    var attributes = node.Header is not null
        ? ConvertAttributes(node.Header)
        : null;

    return new AsgDocument
    {
        Attributes = attributes,
        Header = header,
        Blocks = this.ConvertBlocks(node.Body).ToList(),
        Location = this.GetLocation(node)
    };
}
```

**理由**: `ConvertAttributes` は常に辞書（最低でも空の辞書）を返す。`node.Header is not null` の条件を追加することで、ヘッダーなしの場合は `null` が `Attributes` に設定され、変更1の `JsonIgnore` と組み合わせてフィールドが省略される。

---

### 変更の影響範囲

- `block/document/body-only`（ヘッダーなし）: `attributes` 省略 → **テスト通過**
- `block/document/header-body`（ヘッダーあり、属性なし）: `attributes: {}` 出力 → 別の問題が原因で依然失敗（影響なし）
- `block/header/attribute entries below title`（ヘッダーあり、属性あり）: 引き続き通過（影響なし）
- その他の block テスト（ヘッダーなし）: `attributes` 省略により、一部テストのblockレベルの差異が解消される可能性がある
