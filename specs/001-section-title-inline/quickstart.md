# Quickstart: SectionTitleSyntax の新しい API

**Feature Branch**: `001-section-title-inline`
**Date**: 2026-02-05

## 概要

このドキュメントでは、`SectionTitleSyntax` の構成改定と `TextSyntax` の `InlineTextSyntax` へのリネーム後の API 使用方法を説明します。

## 変更点のまとめ

| 変更前 | 変更後 |
|--------|--------|
| `TextSyntax` | `InlineTextSyntax` |
| `SyntaxKind.Text` | `SyntaxKind.InlineText` |
| `VisitText(TextSyntax)` | `VisitInlineText(InlineTextSyntax)` |
| `sectionTitle.TitleContent` | `sectionTitle.InlineElements` から取得 |

## 基本的な使用方法

### セクションタイトルのパース

```csharp
using AsciiSharp;
using AsciiSharp.Syntax;

// AsciiDoc をパース
var text = "== Hello World";
var tree = SyntaxTree.Parse(text);

// セクションタイトルを取得
var document = tree.Root;
var section = document.Body?.Sections.FirstOrDefault();
var title = section?.Title;

if (title is not null)
{
    // セクションレベルの取得
    int level = title.Level;  // => 2

    // インライン要素の走査
    foreach (var inline in title.InlineElements)
    {
        if (inline is InlineTextSyntax textNode)
        {
            Console.WriteLine(textNode.Text);  // => "Hello World"
        }
    }
}
```

### タイトル文字列の取得（移行ガイド）

**変更前（旧 API）**:
```csharp
// 非推奨: TitleContent は削除されます
string content = sectionTitle.TitleContent;
```

**変更後（新 API）**:
```csharp
// 方法 1: 単一の InlineTextSyntax から取得
var titleText = sectionTitle.InlineElements
    .OfType<InlineTextSyntax>()
    .Select(t => t.Text)
    .FirstOrDefault() ?? string.Empty;

// 方法 2: すべてのインライン要素を結合
var fullTitle = string.Join("", sectionTitle.InlineElements.Select(e => e.ToFullString()));

// 方法 3: ToFullString() から復元（マーカーと空白を含む）
var fullString = sectionTitle.ToFullString();  // => "== Hello World"
```

### Visitor パターンの使用

**変更前（旧 API）**:
```csharp
public class MyVisitor : ISyntaxVisitor
{
    public void VisitText(TextSyntax node)
    {
        Console.WriteLine(node.Text);
    }
    // ... その他のメソッド
}
```

**変更後（新 API）**:
```csharp
public class MyVisitor : ISyntaxVisitor
{
    public void VisitInlineText(InlineTextSyntax node)
    {
        Console.WriteLine(node.Text);
    }
    // ... その他のメソッド
}
```

### SyntaxKind の判定

**変更前（旧 API）**:
```csharp
if (node.Kind == SyntaxKind.Text)
{
    var textNode = (TextSyntax)node;
}
```

**変更後（新 API）**:
```csharp
if (node.Kind == SyntaxKind.InlineText)
{
    var textNode = (InlineTextSyntax)node;
}
```

## InlineElements の順序保証

`InlineElements` は構文上の出現順に並んでいることが保証されています。

```csharp
// 順序の検証例
for (int i = 1; i < sectionTitle.InlineElements.Length; i++)
{
    Debug.Assert(
        sectionTitle.InlineElements[i].Position >= sectionTitle.InlineElements[i - 1].Position,
        "InlineElements は構文上の出現順に並んでいます");
}
```

## トリビアの扱い

マーカー（`=`）とタイトル本文の間の空白は、マーカートークンの TrailingTrivia として保持されます。

```csharp
var marker = sectionTitle.Marker;
if (marker is not null)
{
    // マーカー後の空白を取得
    var trailingTrivia = marker.TrailingTrivia;
    foreach (var trivia in trailingTrivia)
    {
        if (trivia.Kind == SyntaxKind.WhitespaceTrivia)
        {
            Console.WriteLine($"空白: '{trivia.ToFullString()}'");
        }
    }
}
```

## セクション見出しの認識条件

セクション見出しとして認識されるには、以下の条件をすべて満たす必要があります:

1. 行頭が `=` で始まること
2. `=` の数が 1〜6 個であること（7 個以上は段落として扱われる）
3. `=` の後に少なくとも 1 つの空白があること

```csharp
// セクション見出しとして認識される例
"= タイトル"          // Level 1（ドキュメントタイトル）
"== タイトル"         // Level 2
"====== タイトル"     // Level 6

// セクション見出しとして認識されない例（段落として扱われる）
"======= タイトル"    // = が 7 個以上 → 段落
"==タイトル"          // 空白なし → 段落
```

## 完全なテキスト復元

構文木からの完全なテキスト復元は、`ToFullString()` メソッドで行います。

```csharp
var originalText = "==  Hello World";  // 2つのスペース
var tree = SyntaxTree.Parse(originalText);
var restored = tree.Root.ToFullString();

Debug.Assert(originalText == restored, "完全な復元が可能");
```
