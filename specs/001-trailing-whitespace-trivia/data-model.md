# Data Model: 行末空白 Trivia の識別と保持

**Branch**: `001-trailing-whitespace-trivia` | **Date**: 2026-02-19
**Phase**: 1 - 設計

---

## エンティティ変更

### 1. SyntaxKind 列挙型の変更

**ファイル**: `Source/AsciiSharp/SyntaxKind.cs`

#### 変更前（Trivia 範囲）

```csharp
WhitespaceTrivia = 200,
EndOfLineTrivia,          // 201
SingleLineCommentTrivia,  // 202
MultiLineCommentTrivia,   // 203
```

#### 変更後（Trivia 範囲）

```csharp
WhitespaceTrivia = 200,
SingleLineCommentTrivia,  // 201（繰り上げ）
MultiLineCommentTrivia,   // 202（繰り上げ）
TrailingWhitespaceTrivia, // 203（新規）
```

**制約**:
- `EndOfLineTrivia` は削除する。既存コードの参照箇所をすべて `TrailingWhitespaceTrivia` または削除に更新する。
- `TrailingWhitespaceTrivia` は種別識別のためのヘルパーメソッド `SyntaxKind.IsTrailingWhitespaceTrivia()` 相当の判定を `SyntaxKind.IsTrivia()` 等で提供する（既存パターンに従う）。

---

### 2. InternalTrivia の変更

**ファイル**: `Source/AsciiSharp/InternalSyntax/InternalTrivia.cs`

#### 変更内容

- `EndOfLine(string text)` ファクトリメソッドを削除
- `TrailingWhitespace(string text)` ファクトリメソッドを新規追加

#### 新規ファクトリメソッドのシグネチャ

```csharp
/// <summary>
/// 行末空白トリビアを作成する。
/// </summary>
/// <param name="text">
/// 行末空白テキスト（0 個以上の非改行空白文字と、それに続く 1 個の改行文字またはファイル末尾）。
/// </param>
/// <returns>新しい InternalTrivia。</returns>
public static InternalTrivia TrailingWhitespace(string text)
{
    return new InternalTrivia(SyntaxKind.TrailingWhitespaceTrivia, text);
}
```

**制約**:
- `text` の内容は `[非改行空白文字*][改行文字 or 空文字列（EOF）]` の形式であることを前提とする（検証はしない）
- CRLF（`"\r\n"`）は分割せず、1 文字の改行として扱う

---

### 3. Lexer の変更

**ファイル**: `Source/AsciiSharp/Parser/Lexer.cs`

#### 新規ヘルパーメソッド

```csharp
/// <summary>
/// 指定された文字が非改行空白文字かどうかを判定する。
/// Unicode White_Space 特性を持ち、かつ改行文字でない文字。
/// </summary>
private static bool IsNonNewlineWhitespace(char c)
{
    // Unicode 17.0 White_Space 文字のうち改行文字を除いたもの:
    // U+0009 (TAB), U+000B (VT), U+000C (FF) ← FF は改行文字でもある
    // U+0020 (SPACE), U+00A0, U+1680, U+2000-U+200A, U+202F, U+205F, U+3000
    // ※ FF (U+000C) は改行文字に含まれるため除外
    // ※ U+0085 (NEL), U+2028 (LS), U+2029 (PS) は改行文字に含まれるため除外
}

/// <summary>
/// 指定された文字が改行文字の開始文字かどうかを判定する。
/// </summary>
private static bool IsNewlineStart(char c)
{
    // CR (U+000D), LF (U+000A), NEL (U+0085), FF (U+000C),
    // LS (U+2028), PS (U+2029)
}
```

#### ScanTrailingTrivia() の変更

```csharp
// 変更前（static, 空実装）
private static void ScanTrailingTrivia() { }

// 変更後（instance メソッド, トリビアリストを返す）
private InternalTrivia[] ScanTrailingTrivia()
{
    // 非改行空白文字* + 改行文字（or EOF）を TrailingWhitespaceTrivia として収集
    // CRLF は 2 文字まとめて 1 つの改行として扱う
}
```

#### ScanLeadingTrivia() の変更

- 空白のみの行（`[非改行空白文字*][改行文字]` が続く行）を `TrailingWhitespaceTrivia` として先行トリビアに収集する処理を追加
- `ScanNewLineAsTrivia()` が生成するトリビアを `EndOfLineTrivia` から `TrailingWhitespaceTrivia` に変更

---

### 4. トークンストリームのモデル

#### 各パターンでのストリーム表現

| 入力 | トークンストリーム |
|------|-------------------|
| `"foo\n"` | `TextToken("foo"){ trailing: [TrailingWhitespaceTrivia("\n")] }` |
| `"foo   \n"` | `TextToken("foo"){ trailing: [TrailingWhitespaceTrivia("   \n")] }` |
| `"foo"` (EOF) | `TextToken("foo"){ trailing: [TrailingWhitespaceTrivia("")] }` ※空文字列 |
| `"foo   "` (EOF) | `TextToken("foo"){ trailing: [TrailingWhitespaceTrivia("   ")] }` |
| `"\n"` (空行) | 次トークンの leading: [TrailingWhitespaceTrivia("\n")] |
| `"   \n"` (空白のみ行) | 次トークンの leading: [TrailingWhitespaceTrivia("   \n")] |
| `"\r\n"` (CRLF) | `TrailingWhitespaceTrivia("\r\n")` (CRLF を 1 つの改行として扱う) |
| `"// comment\n"` | `[SingleLineCommentTrivia("// comment"), TrailingWhitespaceTrivia("\n")]` (先行トリビア) |

※ EOF のみの末尾（改行なし）の場合: `text` が改行なし空白のみ、または空文字列になる場合がある。この場合の `TrailingWhitespaceTrivia` のテキストは改行文字を含まない。FR-004「またはファイル末尾」に基づき許容。

---

### 5. Parser の変更

**ファイル**: `Source/AsciiSharp/Parser/Parser.cs`

#### `IsBlankLine()` の変更

```csharp
// 変更前
private bool IsBlankLine()
{
    return this.Current.Kind == SyntaxKind.NewLineToken ||
           (this.Current.Kind == SyntaxKind.WhitespaceToken &&
            this.Peek().Kind == SyntaxKind.NewLineToken);
}

// 変更後（次トークンの先行トリビアで判定）
private bool IsBlankLine()
{
    // 次のトークンの先行トリビアに TrailingWhitespaceTrivia が含まれていれば空白行が存在する
    // （前の行の TrailingWhitespaceTrivia が next token の leading trivia に移動しているため）
}
```

#### 影響を受けるメソッド（洗い出し）

- `IsBlankLine()`
- `SkipBlankLines()`
- `ParseParagraph()`
- `ParseSectionTitle()`
- その他 `NewLineToken` を参照している箇所すべて

---

## データフロー

```
ソーステキスト
    │
    ↓ Lexer.NextToken()
    ├─ ScanLeadingTrivia()  →  [TrailingWhitespaceTrivia（空白のみ行・コメント後改行）, ...]
    ├─ ScanToken()          →  InternalToken（内容トークン）
    └─ ScanTrailingTrivia() →  [TrailingWhitespaceTrivia（行末空白+改行）]
    │
    ↓ token.WithTrivia(leading, trailing)
    InternalToken with trivia
    │
    ↓ Parser（ブロック・インライン解析）
    SyntaxTree（TrailingWhitespaceTrivia を含む）
    │
    ├─ ToFullString() → 元のテキスト（ラウンドトリップ）
    └─ ASG 変換    → TrailingWhitespaceTrivia を無視（FR-007）
```
