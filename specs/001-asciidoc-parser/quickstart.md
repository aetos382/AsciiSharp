# Quickstart: AsciiDoc パーサー

**Feature**: 001-asciidoc-parser
**Date**: 2026-01-18

## 概要

AsciiSharp は AsciiDoc 文書を解析し、ロスレスな構文木（Concrete Syntax Tree）を生成するパーサーライブラリです。

## 基本的な使用方法

### 文書の解析

```csharp
using AsciiSharp;
using AsciiSharp.Syntax;

// 文字列から解析
string source = @"= Document Title
Author Name

== Section 1

This is a paragraph with *bold* and _italic_ text.

* Item 1
* Item 2
";

SyntaxTree tree = SyntaxTree.Parse(source);
DocumentSyntax document = tree.Root;
```

### 構文木の走査

```csharp
// 直接の子ノードを走査
foreach (var block in document.Body.Blocks)
{
    Console.WriteLine($"Block: {block.Kind}");
}

// すべての子孫ノードを走査
foreach (var node in document.DescendantNodes())
{
    if (node is SectionSyntax section)
    {
        Console.WriteLine($"Section Level {section.Level}: {section.Title}");
    }
}

// 特定の種類のノードを検索
var paragraphs = document.DescendantNodes()
    .OfType<ParagraphSyntax>();
```

### トークンとトリビアへのアクセス

```csharp
// トークンの取得
foreach (var token in document.DescendantTokens())
{
    Console.WriteLine($"Token: {token.Kind} = '{token.Text}'");

    // 先行トリビア（空白、コメントなど）
    foreach (var trivia in token.LeadingTrivia)
    {
        Console.WriteLine($"  Leading: {trivia.Kind}");
    }

    // 後続トリビア
    foreach (var trivia in token.TrailingTrivia)
    {
        Console.WriteLine($"  Trailing: {trivia.Kind}");
    }
}
```

### 位置情報の取得

```csharp
var section = document.DescendantNodes()
    .OfType<SectionSyntax>()
    .First();

// トリビアを除いた範囲
TextSpan span = section.Span;
Console.WriteLine($"Start: {span.Start}, Length: {span.Length}");

// トリビアを含む全範囲
TextSpan fullSpan = section.FullSpan;

// 行・列番号への変換
var (line, column) = tree.Text.GetLineAndColumn(span.Start);
Console.WriteLine($"Line: {line}, Column: {column}");
```

### 元のテキストの再構築

```csharp
// 構文木から元のテキストを完全に再構築
string reconstructed = document.ToFullString();

// トリビアを除いたテキスト
string withoutTrivia = document.ToString();
```

## エラー処理

### 診断情報の取得

```csharp
SyntaxTree tree = SyntaxTree.Parse(invalidSource);

// 構文エラーの確認
if (tree.Diagnostics.Any())
{
    foreach (var diagnostic in tree.Diagnostics)
    {
        Console.WriteLine($"[{diagnostic.Severity}] {diagnostic.Code}: {diagnostic.Message}");
        Console.WriteLine($"  at {diagnostic.Location}");
    }
}

// 特定のノードの診断情報
var node = tree.Root.DescendantNodes().First();
foreach (var diagnostic in node.GetDiagnostics())
{
    Console.WriteLine(diagnostic.Message);
}
```

### 欠落ノードの検出

```csharp
// エラー回復により挿入された欠落ノード
var missingNodes = document.DescendantNodes()
    .Where(n => n.IsMissing);

foreach (var missing in missingNodes)
{
    Console.WriteLine($"Missing {missing.Kind} at {missing.Span.Start}");
}
```

## 構文木の変更

### ノードの置換

```csharp
// 不変なので、変更は新しい構文木を返す
var oldParagraph = document.DescendantNodes()
    .OfType<ParagraphSyntax>()
    .First();

var newParagraph = SyntaxFactory.Paragraph(
    SyntaxFactory.Text("New content")
);

// 置換して新しい構文木を取得
DocumentSyntax newDocument = document.ReplaceNode(oldParagraph, newParagraph);

// 元の構文木は変更されない
Debug.Assert(document != newDocument);
Debug.Assert(document.ToFullString() != newDocument.ToFullString());
```

### 増分解析

```csharp
// テキスト変更を適用
var change = new TextChange(
    span: new TextSpan(start: 10, length: 5),
    newText: "replacement"
);

// 増分解析で新しい構文木を取得
SyntaxTree newTree = tree.WithChanges(change);

// 変更されていない部分は内部的に再利用される
```

## プロジェクト設定

### パッケージ参照

```xml
<ItemGroup>
  <PackageReference Include="AsciiSharp" Version="1.0.0" />
</ItemGroup>
```

### 対応フレームワーク

- .NET Standard 2.0 以上
- .NET Core 2.0 以上
- .NET Framework 4.6.1 以上
- .NET 5.0 以上

## パフォーマンスのヒント

### 大きな文書

```csharp
// 大きな文書では、必要な部分のみ走査
var firstSection = document.Body.Blocks
    .OfType<SectionSyntax>()
    .FirstOrDefault();

// DescendantNodes() は遅延評価
var firstBold = document.DescendantNodes()
    .OfType<FormattedTextSyntax>()
    .FirstOrDefault(f => f.Style == SyntaxKind.BoldText);
```

### 構文木の再利用

```csharp
// 構文木は不変なので、安全にキャッシュ可能
private static readonly ConcurrentDictionary<string, SyntaxTree> _cache = new();

SyntaxTree GetOrParse(string source)
{
    return _cache.GetOrAdd(source, s => SyntaxTree.Parse(s));
}
```

## 次のステップ

- [データモデル](./data-model.md) - 構文木の詳細な構造
- [API リファレンス](#) - 完全な API ドキュメント
- [サンプル](#) - より高度な使用例
