# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## プロジェクト概要

AsciiSharpは.NET 10.0をターゲットとするAsciiDoc処理ライブラリです。コアライブラリ、各種形式のコンバーター、テストインフラストラクチャで構成されています。

## アーキテクチャ

### ソリューション構成
- `Source/AsciiSharp/` - AsciiDoc処理のコアライブラリ
- `Source/Converter/` - 出力形式コンバーター（CLI、DocBook、HTML、Core）
- `Source/TckAdapter/` - Test Compatibility Kit アダプター
- `Test/AsciiSharp.Tests/` - MSTestベースの単体テスト
- `Design/AsciiSharp.Concept/` - コンセプト・設計検討プロジェクト

### ビルドシステム
- .NET 10.0 SDK、C# 14言語機能を使用
- `Directory.Packages.props`による集中パッケージ管理
- `packages.lock.json`による再現可能ビルド
- アーティファクトは`artifacts/`ディレクトリに出力

## 開発コマンド

### ビルド
```bash
dotnet build --configuration Debug
dotnet build --configuration Release
```

### テスト実行
```bash
dotnet test --configuration Debug --logger trx --collect:"XPlat Code Coverage"
dotnet test --configuration Release
```

### 単一テスト実行
```bash
dotnet test --filter "TestMethodName"
```

## 開発設定

### コード品質設定
- Nullable参照型有効
- Implicit usings無効（明示的using文が必要）
- ドキュメント生成有効
- 最新レベルのコード分析
- オーバーフロー・アンダーフローチェック有効

### パッケージ管理
- 集中パッケージ管理有効
- 推移的パッケージ固定有効
- パッケージロックファイル必須

### CI/CD
- プルリクエストでDebugビルド（テストレポート・カバレッジ付き）
- mainブランチプッシュでReleaseビルド

## 主要機能
- AOT（Ahead of Time）コンパイル対応
- トリミング対応
- CLS準拠
- Apache-2.0ライセンス