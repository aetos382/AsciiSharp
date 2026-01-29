# Research: TCK 統合テスト基盤

**Feature**: TCK 統合テスト基盤
**Date**: 2026-01-29

## 概要

このドキュメントは、TCK (Technology Compatibility Kit) 統合に必要な技術調査の結果をまとめたものです。

---

## 1. TCK アーキテクチャと通信プロトコル

### 決定事項
TCK は CLI アダプターとの通信に標準入力/出力を使用する。入力は JSON 形式、出力は ASG (Abstract Semantic Graph) の JSON 形式。

### 根拠
- TCK README.adoc に明記されている仕様
- 言語非依存のインターフェースを実現するため
- プラットフォーム間の互換性を確保するため

### 調査内容

**入力フォーマット** (TCK → アダプター):
```json
{
  "contents": "AsciiDoc 文書の内容",
  "path": "/仮想/ファイル/パス",
  "type": "block"  // または "inline"
}
```

**出力フォーマット** (アダプター → TCK):
ASG JSON 形式。以下は paragraph を含む document の例:
```json
{
  "name": "document",
  "type": "block",
  "blocks": [
    {
      "name": "paragraph",
      "type": "block",
      "inlines": [
        {
          "name": "text",
          "type": "string",
          "value": "テキスト内容",
          "location": [{ "line": 1, "col": 1 }, { "line": 1, "col": 10 }]
        }
      ],
      "location": [{ "line": 1, "col": 1 }, { "line": 1, "col": 10 }]
    }
  ],
  "location": [{ "line": 1, "col": 1 }, { "line": 1, "col": 10 }]
}
```

### 代替案の検討
- **HTTP/REST API**: TCK が将来的にサポートを検討中だが、現時点では未実装
- **gRPC**: 言語間の相互運用性は高いが、TCK がサポートしていない

---

## 2. ASG モデルの実装状態

### 決定事項
既存の AsciiSharp.Asg プロジェクトに実装済みの ASG モデルとコンバーターを活用する。

### 根拠
- `AsgConverter` クラスが SyntaxTree → ASG 変換を実装済み
- ASG モデル (AsgDocument, AsgSection, AsgParagraph, AsgText 等) が TCK フォーマットに準拠
- `AsgLocationJsonConverter` が TCK の位置情報フォーマット `[{line, col}, {line, col}]` に対応

### 調査内容

**実装済みの ASG ノード**:
| クラス | TCK name | 対応する SyntaxNode |
|--------|----------|---------------------|
| AsgDocument | "document" | DocumentSyntax |
| AsgSection | "section" | SectionSyntax |
| AsgParagraph | "paragraph" | ParagraphSyntax |
| AsgText | "text" | TextSyntax |
| AsgHeader | "header" | DocumentHeaderSyntax |

**未実装または部分実装のノード** (TCK テスト結果により判明する予定):
- listing, sidebar, list 等のブロック要素
- strong, emphasis 等のインライン要素
- リンク、画像、マクロ等

### 代替案の検討
- **ASG モデルの再設計**: 不要。既存設計が TCK フォーマットに準拠している
- **外部ライブラリの使用**: 不要。System.Text.Json で AOT 対応のシリアライゼーションが可能

---

## 3. AOT コンパイルと Docker

### 決定事項
.NET 10.0 の PublishAot を使用してネイティブ実行可能ファイルを生成し、debian:trixie-slim をベースにした軽量 Docker イメージを作成する。

### 根拠
- 既存の Dockerfile と docker-bake.hcl が設定済み
- AOT コンパイルによりコンテナ起動時間を最小化
- .NET ランタイム不要によりイメージサイズを削減

### 調査内容

**既存の Docker 設定**:
```hcl
target "tck" {
  dockerfile = "Source/AsciiSharp.TckAdapter/Dockerfile"
  context = "."
  contexts = {
    "asciidoc-tck" = "https://gitlab.eclipse.org/eclipse/asciidoc-lang/asciidoc-tck.git"
  }
  tags = [ "asciisharp-tck" ]
}
```

**Dockerfile のマルチステージ構成**:
1. **adapter-build**: .NET SDK で AOT ビルド
2. **tck-build**: Node.js で TCK をビルド
3. **final**: debian:trixie-slim にアダプターと TCK をコピー

### 代替案の検討
- **Alpine ベース**: musl libc との互換性の問題が発生する可能性があるため、debian を選択
- **distroless**: 現時点では debian の方がデバッグしやすい

---

## 4. GitHub Actions ワークフロー

### 決定事項
新しいワークフロー `tck.yml` を作成し、Docker ビルドと TCK テスト実行を自動化する。

### 根拠
- 既存の build.yml とは独立した CI/CD パイプラインとして管理
- Docker ビルドに時間がかかるため、通常のビルド・テストと分離

### 調査内容

**必要なステップ**:
1. リポジトリをサブモジュール込みでチェックアウト
2. Docker Buildx のセットアップ
3. `docker buildx bake tck` でイメージビルド
4. コンテナ内で TCK テスト実行
5. テスト結果のレポート

**トリガー条件** (案):
- `main` ブランチへのプッシュ
- プルリクエスト
- 手動トリガー (workflow_dispatch)

### 代替案の検討
- **既存 workflow への統合**: ビルド時間が長くなるため、分離を選択
- **自己ホストランナー**: 現時点では GitHub-hosted ランナーで十分

---

## 5. CLI エントリポイントの設計

### 決定事項
Program.cs でトップレベル ステートメントを使用し、標準入力から JSON を読み取り、パース・変換後に標準出力へ ASG JSON を出力する。

### 根拠
- シンプルなワンショット CLI として設計
- AOT 対応のため、System.Text.Json のソースジェネレーターを活用

### 調査内容

**処理フロー**:
1. 標準入力から TCK 入力 JSON を読み取り
2. JSON をデシリアライズして `TckInput` オブジェクトに変換
3. `type` フィールドに基づいてパース方法を決定 ("block" or "inline")
4. AsciiSharp パーサーで AsciiDoc 文書をパース
5. `AsgConverter` で SyntaxTree を ASG に変換
6. ASG を JSON シリアライズして標準出力に出力
7. 成功時は終了コード 0、エラー時は非ゼロを返す

**エラー処理**:
- 不正な JSON 入力: エラーメッセージを stderr に出力、終了コード 1
- パース失敗: 可能な限り部分的な ASG を生成、またはエラー情報を含む応答

---

## まとめ

| 項目 | 決定事項 | 理由 |
|------|----------|------|
| 通信プロトコル | stdin/stdout JSON | TCK 仕様に準拠 |
| ASG 実装 | 既存コードを活用 | TCK フォーマットに準拠済み |
| Docker ベース | debian:trixie-slim + AOT | 軽量・高速起動 |
| CI ワークフロー | 独立した tck.yml | ビルド時間の分離 |
| CLI 設計 | トップレベル ステートメント | シンプルさと AOT 互換性 |
