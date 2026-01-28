# Research: AsciiDoc ASG モデルクラス

**Date**: 2026-01-28
**Status**: 完了

## 調査項目

### 1. TCK ASG JSON フォーマット

**決定**: TCK が期待する JSON 構造に準拠

**根拠**: `submodules/asciidoc-tck/test/tests/` 配下のテストケースを調査し、以下の構造を確認:

```json
{
  "name": "document",
  "type": "block",
  "header": {
    "title": [{ "name": "text", "type": "string", "value": "..." }],
    "location": [{ "line": 1, "col": 1 }, { "line": 1, "col": 10 }]
  },
  "blocks": [...],
  "location": [{ "line": 1, "col": 1 }, { "line": 5, "col": 20 }]
}
```

**代替案**: 独自フォーマット → TCK テストに合格しないため却下

### 2. 位置情報 (Location) のエンコーディング

**決定**: `[{start}, {end}]` の配列形式、1-based 行番号・列番号

**根拠**: TCK テストケースの `expected.json` で確認した形式:
- 開始位置: 最初の文字の位置
- 終了位置: 最後の文字の位置（包含的）

**代替案**: オブジェクト形式 `{ "start": {...}, "end": {...} }` → TCK 形式と互換性がないため却下

### 3. JSON シリアライズ方式

**決定**: System.Text.Json の Source Generator (JsonSerializerContext) を使用

**根拠**:
- TckAdapter.csproj に `<IsAotCompatible>true</IsAotCompatible>` が設定済み
- リフレクションベースのシリアライズは AOT 環境で動作しない
- Source Generator により、コンパイル時に型情報が解決される

**代替案**:
- Newtonsoft.Json → AOT 非対応、追加依存が必要のため却下
- リフレクションベース System.Text.Json → AOT 非互換のため却下

### 4. SyntaxTree 走査パターン

**決定**: `ISyntaxVisitor<T>` インターフェースを実装

**根拠**:
- AsciiSharp は既に Visitor パターンをサポート (`Source/AsciiSharp/Syntax/ISyntaxVisitor.cs`)
- 型安全な方法で各 SyntaxNode タイプを処理可能
- 新しい SyntaxNode タイプが追加された場合、コンパイルエラーで検出可能

**代替案**: パターンマッチング (switch 式) → Visitor パターンが既に提供されているため冗長

### 5. プロパティ命名規則

**決定**: .NET 標準 (PascalCase) で定義し、`[JsonPropertyName]` 属性で TCK 形式 (lowercase) に変換

**根拠**:
- .NET のコーディング規約に準拠
- IDE のサポートが受けられる
- JSON 出力は属性で明示的に制御

**代替案**:
- プロパティ名を直接 lowercase → .NET 規約違反のため却下
- JsonNamingPolicy.CamelCase → TCK は lowercase を期待するため不適合

### 6. 未対応 SyntaxNode の処理

**決定**: `null` を返してスキップ、処理を継続

**根拠**:
- TCK の一部テストケースのみに対応する段階的実装が可能
- 例外をスローすると TCK テスト全体が失敗する

**代替案**: 例外をスロー → 段階的実装が困難になるため却下

## 参照資料

- `submodules/asciidoc-tck/test/tests/` - TCK テストケースとその期待出力
- `Source/AsciiSharp/Syntax/ISyntaxVisitor.cs` - Visitor インターフェース定義
- [System.Text.Json Source Generation](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation) - AOT 対応 JSON シリアライズ
