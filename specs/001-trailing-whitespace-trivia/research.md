# Research: 要素境界における行末トリビアの統一

**Branch**: `001-trailing-whitespace-trivia` | **Date**: 2026-02-20
**Phase**: 0 - コードベース調査

---

## 調査の目的

仕様 `spec.md`（2026-02-20 改訂版）の実装に先立ち、既存コードベースの状況を調査する。
本仕様は新たな SyntaxKind を追加せず、Parser の変更のみで行末トリビア処理を統一する。

---

## 調査結果

### 1. 既存の Trivia 実装

#### `SyntaxKind.cs` の Trivia 種別（変更なし）

| 値  | 名称                      | 変更内容 |
|-----|---------------------------|---------|
| 200 | `WhitespaceTrivia`        | 変更なし |
| 201 | `EndOfLineTrivia`         | 変更なし |
| 202 | `SingleLineCommentTrivia` | 変更なし |
| 203 | `MultiLineCommentTrivia`  | 変更なし |

**Decision**: `SyntaxKind.cs` は変更しない（FR-006）。

#### `InternalTrivia.cs` の現状（変更なし）

既存のファクトリメソッド: `Whitespace(string text)`, `EndOfLine(string text)`, `SingleLineComment(string text)`, `MultiLineComment(string text)`

**Decision**: `InternalTrivia.cs` は変更しない。既存の `Whitespace()` と `EndOfLine()` で十分。

---

### 2. Parser の現状と変更対象

#### `ParseAttributeEntry()` — 既存の正しい実装（参照パターン）

`ParseAttributeEntry()`（Parser.cs L629-697）はすでに正しい実装を持つ：

```csharp
// 閉じコロン後の空白を WhitespaceTrivia に変換
var whitespaceTrivia = InternalTrivia.Whitespace(this.Current.Text);

// 改行を EndOfLineTrivia に変換して最終トークンのトリビアに付与
var newLineTrivia = InternalTrivia.EndOfLine(this.Current.Text);
valueToken = valueToken.WithTrivia(null, [newLineTrivia]);
```

これが本フィーチャーの実装パターンとなる。

#### `ParseSectionTitle()` — 変更対象

現在（Parser.cs L302-357）：

```csharp
// タイトルテキストのループ（行末 WhitespaceToken をそのまま emit）
while (!this.IsAtEnd() && this.Current.Kind != SyntaxKind.NewLineToken && ...)
{
    this.EmitCurrentToken();  // 末尾の WhitespaceToken も emit されてしまう
}

// 改行を NewLineToken として emit
if (this.Current.Kind == SyntaxKind.NewLineToken)
{
    this.EmitCurrentToken();  // NewLineToken がトークンとして残る
}
```

**変更方針**: ループ終了後に行末の `WhitespaceToken` と `NewLineToken` を検出し、最終コンテンツトークンの後続トリビアとして付与する（`ParseAttributeEntry()` と同パターン）。

#### `ParseAuthorLine()` — 変更対象

現在（Parser.cs L193-210）：

```csharp
// 行末まで読み取る（WhitespaceToken をそのまま emit）
while (!this.IsAtEnd() && this.Current.Kind != SyntaxKind.NewLineToken && ...)
{
    this.EmitCurrentToken();  // 末尾の WhitespaceToken も emit されてしまう
}

// 改行を NewLineToken として emit
if (this.Current.Kind == SyntaxKind.NewLineToken)
{
    this.EmitCurrentToken();  // NewLineToken がトークンとして残る
}
```

**変更方針**: `ParseSectionTitle()` と同様のパターンを適用する。

#### `IsBlankLine()` / `SkipBlankLines()` — 変更なし

`IsBlankLine()`（Parser.cs L702-706）は `NewLineToken` で空行を検出しており、今回の変更対象外（段落間の空行は本仕様のスコープ外）。

---

### 3. Lexer の現状（変更なし）

Lexer は変更しない。`NewLineToken` の出力は Lexer の責務のまま。Parser が要素の文脈に基づいてトリビアに変換する。

| メソッド                  | 現在の動作               | 変更内容 |
|--------------------------|--------------------------|---------|
| `ScanLeadingTrivia()`    | 行頭コメントをトリビア化   | 変更なし |
| `ScanTrailingTrivia()`   | ノーオペレーション         | 変更なし |
| `ScanNewLineAsTrivia()`  | コメント後の改行を EndOfLineTrivia として収集 | 変更なし |

---

### 4. 設計決定

#### 決定 1: 実装範囲

本フィーチャーで変更するのは Parser のみ。以下の 3 メソッドを対象とする：

| メソッド | 変更内容 |
|----------|---------|
| `ParseSectionTitle()` | 行末 `WhitespaceToken` → `WhitespaceTrivia`、`NewLineToken` → `EndOfLineTrivia` として最終コンテンツトークンに付与 |
| `ParseAuthorLine()` | 同上 |
| `ParseAttributeEntry()` | 変更なし（すでに正しい実装） |

#### 決定 2: トークンストリームのモデル（変更後）

| 入力 | 最終コンテンツトークン |
|------|----------------------|
| `"== Title   \n"` | `TextToken("Title")` `{ trailing: [WhitespaceTrivia("   "), EndOfLineTrivia("\n")] }` |
| `"== Title\n"` | `TextToken("Title")` `{ trailing: [EndOfLineTrivia("\n")] }` |
| `"== Title\r\n"` | `TextToken("Title")` `{ trailing: [EndOfLineTrivia("\r\n")] }` |
| `"== Title   "` (EOF) | `TextToken("Title")` `{ trailing: [WhitespaceTrivia("   ")] }` |
| `"== Title"` (EOF) | `TextToken("Title")` `{ trailing: [] }` |
| `"Author Name   \n"` | 最終 `TextToken` `{ trailing: [WhitespaceTrivia("   "), EndOfLineTrivia("\n")] }` |

CRLF は `EndOfLineTrivia("\r\n")` として 1 つのトリビアにまとめる（FR-003）。

#### 決定 3: スコープ外（FR-007）

以下は本フィーチャーのスコープ外：

- 段落内の改行（`ParseParagraph()` の `NewLineToken`）
- リンクの `[...]` 内の改行
- `NewLineToken` を `EndOfLineTrivia` に変換する処理（`SkipBlankLines()` など）

---

### 5. アーキテクチャ設計サマリー

```
変更なし:
  SyntaxKind.cs       → WhitespaceTrivia(200), EndOfLineTrivia(201) をそのまま使用
  InternalTrivia.cs   → Whitespace(), EndOfLine() をそのまま使用
  Lexer.cs            → NewLineToken の出力はそのまま

変更あり:
  Parser.cs
    ParseSectionTitle()  → 行末 WhitespaceToken → WhitespaceTrivia
                           行末 NewLineToken → EndOfLineTrivia（最終トークンの trailing に付与）
    ParseAuthorLine()    → 同上（ParseSectionTitle と同パターン）
    ParseAttributeEntry() → 変更なし（すでに実装済み）
```

---

## 残課題・リスク

| リスク | 影響度 | 対応方針 |
|--------|--------|---------|
| `ParseSectionTitle()` のループが `WhitespaceToken` を emit した後の最終トークン追跡 | 中 | ループ前に「最終トークン」変数を追跡するリファクタリングが必要 |
| `ParseAuthorLine()` が複数のトークンを emit するため最終トークン特定が必要 | 中 | `ParseSectionTitle()` と同様の手法で対応 |
| 既存テストが `NewLineToken` の存在を前提にしている可能性 | 中 | 変更後にテスト実行して確認 |
