# Quickstart: 行末空白 Trivia の識別と保持

**Branch**: `001-trailing-whitespace-trivia` | **Date**: 2026-02-19

## 概要

本機能が実装されると、AsciiDoc テキストの各行末にある空白文字（改行文字を含む）が `TrailingWhitespaceTrivia`（SyntaxKind = 203）として SyntaxTree に保持される。

---

## 使用例

### 行末空白の検出

```csharp
var syntaxTree = SyntaxTree.ParseText("== セクションタイトル   \n本文テキスト\n");
var root = syntaxTree.Root;

// SyntaxToken の後続トリビアを確認する
foreach (var token in root.DescendantTokens())
{
    foreach (var trivia in token.TrailingTrivia)
    {
        if (trivia.IsKind(SyntaxKind.TrailingWhitespaceTrivia))
        {
            // 行末空白 Trivia が見つかった
            Console.WriteLine($"行末空白: \"{EscapeText(trivia.ToString())}\"");
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
| `行末空白 Trivia の識別` | `TrailingWhitespaceTrivia` が SyntaxTree に格納されること |
| `TrailingWhitespaceTrivia として識別される` | トリビア種別が `SyntaxKind.TrailingWhitespaceTrivia` であること |
| `ラウンドトリップ完全性` | `ToFullString()` が元テキストと完全一致すること |

---

## テストの実行

```bash
# BDD フィーチャーテストのみ実行
dotnet test Test/AsciiSharp.Specs

# すべてのテストを実行
dotnet test
```
