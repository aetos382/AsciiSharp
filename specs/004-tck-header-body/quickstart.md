# Quickstart: TCK header-body-output テスト対応

**Branch**: `004-tck-header-body` | **Date**: 2026-02-08

## ビルドと実行

```bash
# ビルド
dotnet build

# 全テスト実行
dotnet test

# BDD テストのみ実行
dotnet test Test/AsciiSharp.Specs/

# ASG ユニット テストのみ実行
dotnet test Test/AsciiSharp.Asg.Tests/
```

## 手動での TCK テスト確認

TckAdapter を使用して、header-body テスト ケースを手動で実行する:

```bash
echo '{"contents":"= Document Title\n\nbody\n","path":"test.adoc","type":"block"}' | \
  dotnet run --project Source/AsciiSharp.TckAdapter/
```

期待出力（プロパティ順序は異なる場合がある）:
```json
{
  "name": "document",
  "type": "block",
  "attributes": {},
  "header": {
    "title": [
      {
        "name": "text",
        "type": "string",
        "value": "Document Title",
        "location": [{ "line": 1, "col": 3 }, { "line": 1, "col": 16 }]
      }
    ],
    "location": [{ "line": 1, "col": 1 }, { "line": 1, "col": 16 }]
  },
  "blocks": [
    {
      "name": "paragraph",
      "type": "block",
      "inlines": [
        {
          "name": "text",
          "type": "string",
          "value": "body",
          "location": [{ "line": 3, "col": 1 }, { "line": 3, "col": 4 }]
        }
      ],
      "location": [{ "line": 3, "col": 1 }, { "line": 3, "col": 4 }]
    }
  ],
  "location": [{ "line": 1, "col": 1 }, { "line": 3, "col": 4 }]
}
```

## 属性エントリの確認

```bash
echo '{"contents":"= Document Title\n:icons: font\n:toc:\n","path":"test.adoc","type":"block"}' | \
  dotnet run --project Source/AsciiSharp.TckAdapter/
```

期待: `"attributes": { "icons": "font", "toc": "" }` を含む出力

## 変更対象ファイル一覧

### コア ライブラリ（AsciiSharp）
| ファイル | 変更内容 |
|---------|---------|
| `SyntaxKind.cs` | `AttributeEntry` 列挙値を追加 |
| `Parser/Parser.cs` | `ParseAttributeEntry()` メソッドを追加 |
| `Syntax/AttributeEntrySyntax.cs` | 新規 Red Tree ノード クラス |
| `Syntax/DocumentHeaderSyntax.cs` | `AttributeEntries` プロパティを追加 |
| `Syntax/ISyntaxVisitor.cs` | `VisitAttributeEntry` メソッドを追加 |
| `Syntax/ISyntaxVisitorOfT.cs` | `VisitAttributeEntry` メソッドを追加 |

### ASG ライブラリ（AsciiSharp.Asg）
| ファイル | 変更内容 |
|---------|---------|
| `Models/AsgDocument.cs` | `Attributes` プロパティを追加 |
| `AsgConverter.cs` | 属性エントリ変換ロジックを追加 |
| `Serialization/AsgJsonContext.cs` | `IReadOnlyDictionary<string, string>` を登録 |

### テスト
| ファイル | 変更内容 |
|---------|---------|
| `Features/AttributeEntryParsing.feature` | 新規 BDD テスト |
| `StepDefinitions/AttributeEntrySteps.cs` | 新規ステップ定義 |
| `AsgConverterTests.cs` | attributes 関連テストを追加 |
