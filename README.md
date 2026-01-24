# AsciiSharp

.NET 向けの AsciiDoc パーサーライブラリです。

## 特徴

- **ロスレス構文木**: コメントや空白も含めた完全な構文情報を保持し、元のテキストを正確に復元可能
- **エラー耐性**: 構文エラーがあっても可能な限り解析を継続
- **増分解析**: 編集された部分のみを再解析し、変更されていない部分を再利用
- **イミュータブル**: 構文木は不変で、変更操作は新しい構文木を返す

## 対象フレームワーク

- .NET 10.0
- .NET Standard 2.0

## インストール

```bash
dotnet add package AsciiSharp
```

## クイックスタート

### 基本的な解析

```csharp
using AsciiSharp.Syntax;

// AsciiDoc テキストを解析
var text = "= ドキュメントタイトル\n\n== セクション 1\n\n段落のテキスト。\n\n== セクション 2\n\n別の段落。\n";

var tree = SyntaxTree.ParseText(text);

// ルートノードにアクセス
var document = tree.Root as DocumentSyntax;

// ヘッダーを取得
Console.WriteLine($"タイトル: {document?.Header?.Title?.TitleContent}");

// セクションを列挙
foreach (var node in document!.DescendantNodes())
{
    if (node is SectionSyntax section)
    {
        Console.WriteLine($"セクション: {section.Title?.TitleContent}");
    }
}
```

### ラウンドトリップ

```csharp
// 構文木から元のテキストを復元
var originalText = tree.ToOriginalString();
Console.WriteLine(originalText); // 元のテキストと完全に一致
```

### エラー処理

```csharp
// 診断情報を確認
if (tree.HasErrors)
{
    foreach (var diagnostic in tree.Diagnostics)
    {
        Console.WriteLine($"{diagnostic.Severity}: {diagnostic.Message}");
    }
}
```

### 増分解析

```csharp
using AsciiSharp.Text;

// テキストの変更を適用
var change = new TextChange(new TextSpan(0, 5), "新しいテキスト");
var newTree = tree.WithChanges(change);

// 変更されていない部分の内部ノードは再利用される
```

### 構文木の変更

```csharp
// ノードを置換して新しい構文木を作成
var section = document.DescendantNodes().OfKind(SyntaxKind.Section).First();
var newSection = /* 新しいセクション */;
var newRoot = document.ReplaceNode(section, newSection);
```

## API 概要

### 主要なクラス

| クラス | 説明 |
|--------|------|
| `SyntaxTree` | 解析結果の構文木。`ParseText()` でテキストを解析 |
| `SyntaxNode` | 構文ノードの基底クラス |
| `SyntaxToken` | トークン（=, テキスト、改行など） |
| `SyntaxTrivia` | トリビア（コメント、空白など） |
| `SourceText` | ソーステキストの抽象化 |
| `TextSpan` | テキスト内の位置と長さ |
| `Diagnostic` | 診断情報（エラー、警告） |

### 構文ノードの種類

| ノード | 説明 |
|--------|------|
| `DocumentSyntax` | ドキュメント全体 |
| `DocumentHeaderSyntax` | ドキュメントヘッダー |
| `DocumentBodySyntax` | ドキュメント本体 |
| `SectionSyntax` | セクション |
| `SectionTitleSyntax` | セクションタイトル |
| `ParagraphSyntax` | 段落 |
| `LinkSyntax` | リンク |
| `TextSyntax` | プレーンテキスト |

## ライセンス

MIT License
