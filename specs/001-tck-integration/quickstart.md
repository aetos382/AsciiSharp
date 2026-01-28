# Quickstart: TCK 統合テスト基盤

このガイドでは、TCK (Technology Compatibility Kit) を使用して AsciiSharp の AsciiDoc Language Specification への準拠をテストする方法を説明します。

## 前提条件

- .NET 10.0 SDK
- Docker (Buildx 対応)
- Git サブモジュールの初期化

```bash
# サブモジュールの初期化
git submodule update --init --recursive
```

---

## ローカルでの実行

### 1. CLI アダプターのビルド

```bash
# プロジェクトのビルド
dotnet build Source/TckAdapter/AsciiSharp.TckAdapter.Cli/AsciiSharp.TckAdapter.Cli.csproj

# AOT ビルド（オプション）
dotnet publish Source/TckAdapter/AsciiSharp.TckAdapter.Cli/AsciiSharp.TckAdapter.Cli.csproj \
  --configuration Release \
  --runtime linux-x64
```

### 2. 手動テスト

CLI アダプターに直接 JSON を渡してテストできます。

```bash
# 入力 JSON を作成
echo '{"contents": "Hello, AsciiDoc!", "path": "/test.adoc", "type": "block"}' | \
  dotnet run --project Source/TckAdapter/AsciiSharp.TckAdapter.Cli/AsciiSharp.TckAdapter.Cli.csproj
```

期待される出力:
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
          "value": "Hello, AsciiDoc!",
          "location": [{ "line": 1, "col": 1 }, { "line": 1, "col": 16 }]
        }
      ],
      "location": [{ "line": 1, "col": 1 }, { "line": 1, "col": 16 }]
    }
  ],
  "location": [{ "line": 1, "col": 1 }, { "line": 1, "col": 16 }]
}
```

---

## Docker を使用した TCK 実行

### 1. Docker イメージのビルド

```bash
# docker-bake.hcl を使用してビルド
docker buildx bake tck
```

### 2. TCK テストの実行

```bash
# コンテナを実行して TCK テストを実行
docker run --rm asciisharp-tck
```

### 3. 特定のテストの実行（オプション）

```bash
# テストパターンを指定して実行
docker run --rm asciisharp-tck cli --adapter-command tck-adapter/AsciiSharp.TckAdapter.Cli --pattern "block/paragraph/*"
```

---

## CI での実行

GitHub Actions で自動的に TCK テストが実行されます。

### ワークフロー トリガー

- `main` ブランチへのプッシュ
- プルリクエストの作成・更新
- 手動トリガー (`workflow_dispatch`)

### サブモジュールの自動更新

CI ワークフローは毎回実行時に `asciidoc-tck` サブモジュールを**最新版に更新**します。

- サブモジュールの更新に失敗した場合、ワークフロー全体が失敗します
- これにより、常に最新の TCK テストスイートに対してテストが実行されます

### テスト結果の確認

1. GitHub リポジトリの "Actions" タブを開く
2. "TCK" ワークフローを選択
3. 実行結果を確認

---

## トラブルシューティング

### 「サブモジュールが見つからない」エラー

```bash
git submodule update --init --recursive
```

### Docker ビルドの失敗

```bash
# キャッシュをクリアして再ビルド
docker buildx bake tck --no-cache
```

### TCK テストのタイムアウト

TCK テストはすべてのテストケースを実行するため、初回実行には時間がかかる場合があります。CI 環境では 5 分以内に完了することを目標としています。

---

## 次のステップ

1. **失敗したテストの確認**: TCK テスト結果から、まだサポートされていない AsciiDoc 構文要素を特定
2. **パーサーの拡張**: 失敗したテストに対応する構文要素を AsciiSharp コアライブラリに実装
3. **ASG モデルの追加**: 新しい構文要素に対応する ASG ノードを TckAdapter に追加

---

## 関連ドキュメント

- [spec.md](spec.md) - 機能仕様
- [plan.md](plan.md) - 実装計画
- [research.md](research.md) - 技術調査
- [data-model.md](data-model.md) - データモデル
- [AsciiDoc TCK README](../../submodules/asciidoc-tck/README.adoc) - TCK 公式ドキュメント
