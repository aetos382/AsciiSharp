# Quickstart: 要素境界における行末トリビアの統一

**Branch**: `001-trailing-whitespace-trivia` | **Date**: 2026-02-20

## 概要

本機能が実装されると、セクションタイトル・属性エントリ・著者行の行末にある空白文字と改行文字が、既存の `WhitespaceTrivia`（SyntaxKind = 200）および `EndOfLineTrivia`（SyntaxKind = 201）として最終コンテンツトークンの後続トリビアに格納される。

---

## 使用例

### 行末トリビアの検出

```csharp
var syntaxTree = SyntaxTree.ParseText("== セクションタイトル   \n本文テキスト\n");
var root = syntaxTree.Root;

// SyntaxToken の後続トリビアを確認する
foreach (var token in root.DescendantTokens())
{
    foreach (var trivia in token.TrailingTrivia)
    {
        if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
        {
            Console.WriteLine($"行末空白: \"{EscapeText(trivia.ToString())}\"");
        }
        else if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
        {
            Console.WriteLine($"改行: \"{EscapeText(trivia.ToString())}\"");
        }
    }
}
```

### ラウンドトリップの確認

```csharp
var original = "== タイトル   \n\n本文テキスト  \r\n";
var syntaxTree = SyntaxTree.ParseText(original);
var reconstructed = syntaxTree.Root.ToFullString();

// original == reconstructed が保証される
```

---

## BDD シナリオとの対応

| シナリオ | 検証内容 |
|---------|---------|
| `セクションタイトルの末尾空白が WhitespaceTrivia と EndOfLineTrivia として識別される` | 末尾空白が `SyntaxKind.WhitespaceTrivia`、改行が `SyntaxKind.EndOfLineTrivia` として後続トリビアに格納されること |
| `セクションタイトルの末尾 CRLF が単一の EndOfLineTrivia として識別される` | CRLF が分割されず 1 つの `EndOfLineTrivia("\r\n")` になること |
| `行末空白を含むセクションタイトルのラウンドトリップが保証される` | `ToFullString()` が元テキストと完全一致すること |

---

## テストの実行

```bash
# BDD フィーチャーテストのみ実行
dotnet test --project Test/AsciiSharp.Specs

# すべてのテストを実行
dotnet test
```
