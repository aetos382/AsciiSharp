# Research: 行末空白 Trivia の識別と保持

**Branch**: `001-trailing-whitespace-trivia` | **Date**: 2026-02-19
**Phase**: 0 - コードベース調査

> **注記**: アーキテクチャ上の決定事項は暫定的なものを含む。後の実装フェーズで再考する可能性がある。

## 調査の目的

仕様 `spec.md` の実装に先立ち、既存コードベースの状況を調査し、実装上の課題と設計決定を明確にする。

---

## 調査結果

### 1. 既存の Trivia 実装

#### `SyntaxKind.cs` の Trivia 種別（変更前後）

| 変更後の値 | 名称                      | 変更内容                                      |
|-----------|---------------------------|----------------------------------------------|
| 200       | `WhitespaceTrivia`        | 変更なし                                     |
| 201       | `SingleLineCommentTrivia` | 繰り上げ（旧 202）                            |
| 202       | `MultiLineCommentTrivia`  | 繰り上げ（旧 203）                            |
| 203       | `TrailingWhitespaceTrivia`| **新規追加**                                 |
| ~~201~~   | ~~`EndOfLineTrivia`~~     | **削除** → `TrailingWhitespaceTrivia` に統合 |

**Decision**: `EndOfLineTrivia` を削除し、番号を詰めたうえで `TrailingWhitespaceTrivia = 203` を追加する。

#### `InternalTrivia.cs` の現状

現行のファクトリメソッド: `Whitespace()`, `EndOfLine()`, `SingleLineComment()`, `MultiLineComment()`

変更:
- `EndOfLine()` → 削除（または内部的に `TrailingWhitespace()` に委譲）
- `TrailingWhitespace(string text)` ファクトリメソッドを新規追加

---

### 2. Lexer の現状と課題

#### `ScanTrailingTrivia()` は現在ノーオペレーション（static）

```csharp
private static void ScanTrailingTrivia()
{
    // 現時点では後続トリビアとして収集するものはない
}
```

**変更方針**: インスタンスメソッドに変更し、トリビアリストを返す形にする。
行末の `[非改行空白文字*][改行文字|EOF]` を `TrailingWhitespaceTrivia` として収集する。

#### コメント行末の扱い

`ScanNewLineAsTrivia()` が現在生成している `EndOfLineTrivia` を `TrailingWhitespaceTrivia` に置き換える。

コメント行末も行末空白 Trivia の対象: `"// コメント\n"` の `"\n"` は `TrailingWhitespaceTrivia`。

#### Unicode 対応の不足

| メソッド                  | 現在の対応               | 必要な対応                                      |
|--------------------------|--------------------------|------------------------------------------------|
| `ScanWhitespace()`       | `' '`、`'\t'` のみ       | 全 Unicode White_Space 文字（仕様参照）         |
| `ScanNewLineAsTrivia()`  | `'\r'`、`'\n'` のみ      | NEL (U+0085)、LS (U+2028)、PS (U+2029)、FF (U+000C) |
| `NextRawLineToken()`     | `'\r'`、`'\n'` を直接参照 | Unicode 対応ヘルパーを経由                      |

**Decision**: `IsNonNewlineWhitespace(char)` と `IsNewlineStart(char)` のヘルパーメソッドを導入し、全メソッドから参照する。

---

### 3. Parser の現状と課題

#### `IsBlankLine()` の現状

```csharp
private bool IsBlankLine()
{
    return this.Current.Kind == SyntaxKind.NewLineToken ||
           (this.Current.Kind == SyntaxKind.WhitespaceToken &&
            this.Peek().Kind == SyntaxKind.NewLineToken);
}
```

**変更方針**: 次トークンの先行トリビアに `TrailingWhitespaceTrivia` が含まれる（= 空白のみの行 = ブロック区切り）かを検査する。

#### トークンストリームの変化

**変更前** (`"foo   \n\nbar\n"` の場合):
```
TextToken("foo")
WhitespaceToken("   ")
NewLineToken("\n")
NewLineToken("\n")     ← 空行
TextToken("bar")
NewLineToken("\n")
EndOfFileToken
```

**変更後**（Option A: Trivia ベース）:
```
TextToken("foo")   trailing: [TrailingWhitespaceTrivia("   \n")]
TextToken("bar")   leading:  [TrailingWhitespaceTrivia("\n")]   ← 空行
                   trailing: [TrailingWhitespaceTrivia("\n")]
EndOfFileToken
```

#### ゼロ幅 EndOfLineToken は不採用（暫定）

「各行の末尾に必ずゼロ幅の `EndOfLineToken` がある」Option B は不採用（暫定）。

**理由**: インラインマクロが改行を跨ぐ場合に矛盾が生じる。インラインマクロの `[...]` 内解析中に `EndOfLineToken` が現れると、パーサーがマクロを打ち切るか継続するかを文脈依存で判定しなければならず、実装が複雑化する。

**採用（暫定）**: **Option A（Trivia のみ）**。インラインマクロパーサーは `]` デリミタで終端を判断し、行末トリビアを意識しない。

#### InlineText と行境界

インラインマクロが改行を跨ぐ場合のモデル（`"link:url[テキスト   \n続き] 後続"`）:

```
TextToken("link:url[テキスト")   trailing: [TrailingWhitespaceTrivia("   \n")]
TextToken("続き] 後続")          trailing: [TrailingWhitespaceTrivia("\n")]
```

`TrailingWhitespaceTrivia` はインラインマクロの内外を問わず行末に適用される。
続き行の先頭空白（`"  続き"` の `"  "`）は `WhitespaceToken` のまま残る（行末でないため）。

---

### 4. 設計決定

#### 決定 1: TrailingWhitespaceTrivia の組成

FR-004 に基づく: **`[非改行空白文字 ×0..N][改行文字 or EOF]` = 1 つの `TrailingWhitespaceTrivia`**

- CRLF は FR-005 に従い 1 つの改行として扱う（`"\r\n"` を分割しない）
- EOF で終わる場合も同様に 1 つの `TrailingWhitespaceTrivia`

#### 決定 2: EndOfLineTrivia の廃止と番号詰め

`EndOfLineTrivia`（旧 201）を廃止し、番号を詰める。コメント行末も `TrailingWhitespaceTrivia`（203）で統一。

#### 決定 3: 空白のみの行が連続する場合（暫定）

**1 行 = 1 `TrailingWhitespaceTrivia` を採用**（FR-004 の「1 つの Trivia = 1 行」定義に準拠）

```
// 入力: "   \n\n   \n"（3 行の空白のみ）
// 次のコンテンツトークンの先行トリビアリスト:
  [TrailingWhitespaceTrivia("   \n"), TrailingWhitespaceTrivia("\n"), TrailingWhitespaceTrivia("   \n")]
```

#### 決定 4: エッジケース別の設計

| エッジケース | 設計 |
|-------------|------|
| 本文段落・セクションタイトル後の行末空白 | そのトークンの後続トリビアとして `TrailingWhitespaceTrivia` を付与 |
| 空白のみの行が連続 | 各行が個別の `TrailingWhitespaceTrivia`（次コンテンツの先行トリビアに格納） |
| 文書末尾に改行なしの行末空白 | EOF を改行の代替として `TrailingWhitespaceTrivia` として格納 |
| 文書冒頭に空白のみの行 | 最初のコンテンツトークンの先行トリビアに `TrailingWhitespaceTrivia` |
| インラインマクロ内の行末空白 | 各行末に個別の `TrailingWhitespaceTrivia`（他のケースと同一ルール） |

---

### 5. アーキテクチャ設計サマリー（Option A: Trivia ベース、暫定）

#### Lexer 変更

```
NextToken() の流れ（変更後）:
  1. ScanLeadingTrivia()
     - 行頭コメントをスキャン（既存）
     - コメント後の改行 → TrailingWhitespaceTrivia（EndOfLineTrivia 廃止）
     - 空白のみの行 → TrailingWhitespaceTrivia として先行トリビアに収集（新規）
  2. ScanToken()
     - 変更なし（行末以外の WhitespaceToken は維持）
  3. ScanTrailingTrivia()（static → instance メソッドに変更）
     - [非改行空白文字*][改行文字 or EOF] → TrailingWhitespaceTrivia として後続トリビアに追加
```

#### Parser 変更

| 変更対象 | 変更内容 |
|----------|----------|
| `IsBlankLine()` | 次トークンの先行トリビアに `TrailingWhitespaceTrivia` があるかを検査 |
| `ParseParagraph()` | 行内終端を後続トリビアの `TrailingWhitespaceTrivia` で検出 |
| `ParseSectionTitle()` | `NewLineToken` → `TrailingWhitespaceTrivia` で終端検出 |
| `SkipBlankLines()` | 先行トリビアの `TrailingWhitespaceTrivia` を消費するロジックに変更 |

---

## 残課題・リスク

| リスク | 影響度 | 対応方針 |
|--------|--------|----------|
| Parser の `NewLineToken` 依存箇所が予想より多い | 高 | 実装前に全依存箇所を洗い出す（tasks.md に記載） |
| `EndOfLineTrivia` の番号変更が既存テストの期待値に影響 | 中 | 既存テストの `SyntaxKind` 参照箇所を更新 |
| 空白のみの行の先読み実装の複雑さ | 中 | `ScanLeadingTrivia()` で先読み（Peek）して改行が続くか確認してから消費 |
| `NextRawLineToken()` が `'\r'`/`'\n'` を直接参照 | 低 | Unicode 対応ヘルパーメソッドに置き換え |
| Option A → Option B への変更が必要になる可能性（後で再考） | 低 | 実装時に再評価 |
