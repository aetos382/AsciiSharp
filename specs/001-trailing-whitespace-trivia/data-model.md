# Data Model: 要素境界における行末トリビアの統一

**Branch**: `001-trailing-whitespace-trivia` | **Date**: 2026-02-20
**Phase**: 1 - 設計

---

## エンティティ変更

### 1. SyntaxKind 列挙型（変更なし）

**ファイル**: `Source/AsciiSharp/SyntaxKind.cs`

変更なし。既存の Trivia 種別をそのまま使用する。

```csharp
WhitespaceTrivia = 200,       // 変更なし
EndOfLineTrivia,              // 201 - 変更なし
SingleLineCommentTrivia,      // 202 - 変更なし
MultiLineCommentTrivia,       // 203 - 変更なし
```

---

### 2. InternalTrivia（変更なし）

**ファイル**: `Source/AsciiSharp/InternalSyntax/InternalTrivia.cs`

変更なし。既存のファクトリメソッドをそのまま使用する。

```csharp
InternalTrivia.Whitespace(string text)   // WhitespaceTrivia 生成
InternalTrivia.EndOfLine(string text)    // EndOfLineTrivia 生成
```

---

### 3. Lexer（変更なし）

**ファイル**: `Source/AsciiSharp/Parser/Lexer.cs`

変更なし。`NewLineToken` の生成は Lexer の責務のまま。

---

### 4. Parser — 変更対象

**ファイル**: `Source/AsciiSharp/Parser/Parser.cs`

#### `ParseSectionTitle()` の変更

```csharp
// 変更前: WhitespaceToken と NewLineToken をそのまま emit
while (!this.IsAtEnd() && this.Current.Kind != SyntaxKind.NewLineToken && ...)
{
    hasTitleText = true;
    this.EmitCurrentToken();  // 末尾 WhitespaceToken もトークンとして emit
}

if (this.Current.Kind == SyntaxKind.NewLineToken)
{
    this.EmitCurrentToken();  // NewLineToken がトークンとして残る
}

// 変更後: 最終コンテンツトークンの後続トリビアとして付与
InternalToken? lastContentToken = null;

while (!this.IsAtEnd() && this.Current.Kind != SyntaxKind.NewLineToken && ...)
{
    hasTitleText = true;
    lastContentToken = this.Current;
    this.Advance();
}

if (lastContentToken != null)
{
    // 行末の後続トリビアを構築
    List<InternalTrivia> trailingTrivia = [];

    if (lastContentToken.Kind == SyntaxKind.WhitespaceToken)
    {
        // 末尾の WhitespaceToken を WhitespaceTrivia に変換
        trailingTrivia.Add(InternalTrivia.Whitespace(lastContentToken.Text));
        // 実際の最終コンテンツは 1 つ前のトークン（別途追跡が必要）
    }

    if (this.Current.Kind == SyntaxKind.NewLineToken)
    {
        trailingTrivia.Add(InternalTrivia.EndOfLine(this.Current.Text));
        this.Advance();
    }

    // ...最終コンテンツトークンにトリビアを付与して emit
}
```

**注意**: ループを通じて「最終コンテンツトークン」と「末尾の WhitespaceToken」を区別して追跡する必要がある。具体的なリファクタリング方法は tasks.md に記載する。

#### `ParseAuthorLine()` の変更

```csharp
// 変更前: WhitespaceToken と NewLineToken をそのまま emit
while (!this.IsAtEnd() && this.Current.Kind != SyntaxKind.NewLineToken && ...)
{
    this.EmitCurrentToken();
}

if (this.Current.Kind == SyntaxKind.NewLineToken)
{
    this.EmitCurrentToken();
}

// 変更後: ParseSectionTitle と同パターン
// 末尾 WhitespaceToken → WhitespaceTrivia
// NewLineToken → EndOfLineTrivia
// 最終コンテンツトークンの trailing に付与
```

#### `ParseAttributeEntry()` — 変更なし

すでに正しい実装を持つ（参照パターン）。

---

### 5. トークンストリームのモデル

#### ParseSectionTitle 後のストリーム

| 入力 | 変更前 | 変更後 |
|------|--------|--------|
| `"== Title   \n"` | `EqualsToken, TextToken("Title"), WhitespaceToken("   "), NewLineToken("\n")` | `EqualsToken{ trailing:[WhitespaceTrivia(" ")] }, TextToken("Title"){ trailing:[WhitespaceTrivia("   "), EndOfLineTrivia("\n")] }` |
| `"== Title\n"` | `EqualsToken, TextToken("Title"), NewLineToken("\n")` | `EqualsToken{ trailing:[WhitespaceTrivia(" ")] }, TextToken("Title"){ trailing:[EndOfLineTrivia("\n")] }` |
| `"== Title\r\n"` | `EqualsToken, TextToken("Title"), NewLineToken("\r\n")` | `EqualsToken{ trailing:[WhitespaceTrivia(" ")] }, TextToken("Title"){ trailing:[EndOfLineTrivia("\r\n")] }` |
| `"== Title   "` (EOF) | `EqualsToken, TextToken("Title"), WhitespaceToken("   ")` | `EqualsToken{ trailing:[WhitespaceTrivia(" ")] }, TextToken("Title"){ trailing:[WhitespaceTrivia("   ")] }` |

#### ParseAuthorLine 後のストリーム

| 入力 | 変更前 | 変更後 |
|------|--------|--------|
| `"Author Name   \n"` | `TextToken("Author"), WhitespaceToken(" "), TextToken("Name"), WhitespaceToken("   "), NewLineToken("\n")` | `TextToken("Author"), WhitespaceToken(" "), TextToken("Name"){ trailing:[WhitespaceTrivia("   "), EndOfLineTrivia("\n")] }` |

---

### 6. ASG の Span 計算への影響

後続トリビア（`WhitespaceTrivia`・`EndOfLineTrivia`）は要素の `Span` に算入しない（FR-005）。

変更後、セクションタイトルや著者行の `Span.End` が行末空白を除いたコンテンツ末尾を指すようになる。

---

## データフロー

```
ソーステキスト
    │
    ↓ Lexer.NextToken()（変更なし）
    NewLineToken が出力される
    │
    ↓ Parser.ParseSectionTitle() / ParseAuthorLine()（変更あり）
    末尾 WhitespaceToken → WhitespaceTrivia
    末尾 NewLineToken → EndOfLineTrivia
    最終コンテンツトークンの trailing に付与
    │
    ↓ Parser.ParseAttributeEntry()（変更なし）
    すでに WhitespaceTrivia + EndOfLineTrivia を付与している
    │
    ↓ SyntaxTree
    要素境界に行末トリビアが統一的に付与される
    │
    ├─ ToFullString() → 元のテキスト（ラウンドトリップ保証）
    └─ ASG 変換    → trailing trivia を Span に算入しない（FR-005）
```
