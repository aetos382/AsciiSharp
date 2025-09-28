# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## プロジェクト概要

AsciiSharpは.NET 10.0をターゲットとするAsciiDoc処理ライブラリです。コアライブラリ、各種形式のコンバーター、テストインフラストラクチャで構成されています。

## アーキテクチャ

### ソリューション構成
- `Source/AsciiSharp/` - AsciiDoc処理のコアライブラリ
- `Source/Converter/` - 出力形式コンバーター（CLI、DocBook、HTML、Core）
- `Source/TckAdapter/` - Technology Compatibility Kit アダプター
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

### 言語
- .NET 10.0 をターゲットとし、C# 14 で記述する。
- 最新のフレームワークや言語の機能を積極的に利用する。

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

## コンセプト
- パーサーは Roslyn をモデルとする。
- 生成された構文木は Immutable とする。
- エラーのある文書も最大限にパースできる設計とする。
- 空白も完全に保持して元の文書を忠実に再現できるようにする。

## テスト
- テスト フレームワークは MSTest を使用する。
  - テストケース名は日本語で命名する。
- Reqnroll を使った BDD による設計を採用する。
  - Cucumber ファイルは日本語で記述する。
- ファイル名は英語で命名する。

## 参照
- AsciiDoc 言語の仕様については、@submodules/asciidoc-lang/spec/ を参照する。

## submodules
- submodules/asciidoc-lang: AsciiDoc 言語仕様プロジェクトのリポジトリ
- submodules/asciidoc-tck: AsciiDoc Technology Compatibility Kit (TCK) のリポジトリ
