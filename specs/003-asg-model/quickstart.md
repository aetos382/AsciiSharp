# Quickstart: AsciiDoc ASG 変換

## 概要

AsciiSharp の SyntaxTree を AsciiDoc TCK 準拠の ASG (Abstract Semantic Graph) JSON に変換する方法を説明します。

## 基本的な使用方法

### 1. SyntaxTree から ASG への変換

```csharp
using AsciiSharp;
using AsciiSharp.Syntax;
using AsciiSharp.Text;
using AsciiSharp.TckAdapter.Asg;
using AsciiSharp.TckAdapter.Asg.Models;

// AsciiDoc テキストをパース
var text = "= Hello World\n\nThis is a paragraph.";
var sourceText = SourceText.From(text);
var syntaxTree = SyntaxTree.ParseText(text);
var document = (DocumentSyntax)syntaxTree.Root;

// ASG に変換
var converter = new AsgConverter(sourceText);
var asgDocument = converter.Convert(document);
```

### 2. JSON へのシリアライズ

```csharp
using System.Text.Json;
using AsciiSharp.TckAdapter.Asg.Serialization;

// AOT 互換の JSON シリアライズ
var json = JsonSerializer.Serialize(
    asgDocument,
    AsgJsonContext.Default.AsgDocument);

Console.WriteLine(json);
```

### 出力例

```json
{
  "name": "document",
  "type": "block",
  "header": {
    "title": [
      {
        "name": "text",
        "type": "string",
        "value": "Hello World",
        "location": [{"line": 1, "col": 3}, {"line": 1, "col": 13}]
      }
    ],
    "location": [{"line": 1, "col": 1}, {"line": 1, "col": 13}]
  },
  "blocks": [
    {
      "name": "paragraph",
      "type": "block",
      "inlines": [
        {
          "name": "text",
          "type": "string",
          "value": "This is a paragraph.",
          "location": [{"line": 3, "col": 1}, {"line": 3, "col": 20}]
        }
      ],
      "location": [{"line": 3, "col": 1}, {"line": 3, "col": 20}]
    }
  ],
  "location": [{"line": 1, "col": 1}, {"line": 3, "col": 20}]
}
```

## クラス構成

| クラス | 用途 |
|--------|------|
| `AsgConverter` | SyntaxTree を ASG に変換 |
| `AsgDocument` | document ブロック |
| `AsgSection` | section ブロック |
| `AsgParagraph` | paragraph ブロック |
| `AsgText` | text インライン |
| `AsgHeader` | 文書ヘッダー |
| `AsgJsonContext` | AOT 互換 JSON シリアライズ |

## 注意事項

### 未対応ノード

以下の SyntaxNode タイプは現時点でスキップされます:
- `LinkSyntax`
- `AuthorLineSyntax`

### 位置情報

- 行番号・列番号は 1-based
- 終了位置は包含的（最後の文字の位置）

### AOT 互換性

`AsgJsonContext` を使用することで、Native AOT 環境でも動作します:

```csharp
// ✅ AOT 互換
JsonSerializer.Serialize(asgDocument, AsgJsonContext.Default.AsgDocument);

// ❌ AOT 非互換（リフレクション使用）
JsonSerializer.Serialize(asgDocument);
```
