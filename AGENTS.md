# AGENTS.md

このファイルは、AIコーディングエージェントがこのリポジトリで作業する際のガイダンスを提供します。

## プロジェクト概要

AsciiSharpは.NET 10.0をターゲットとするAsciiDoc処理ライブラリです。Roslynをモデルとした不変構文木アーキテクチャを採用し、パーサーはすべての空白を保持し、不正な形式の文書も可能な限りパースします。

## アーキテクチャ

### ソリューション構成
- `Source/AsciiSharp/` - コアパーサーライブラリ（パッケージ化可能、AOT互換、CLS準拠）
- `Source/Converter/` - 出力フォーマット変換ツール（CLI、DocBook、HTML、Core）※コアライブラリ完成後に実装予定
- `Source/TckAdapter/` - AsciiDoc TCKアダプター - SyntaxTreeをAsciiDoc TCK形式に変換して互換性テストを行う（AOTコンパイル済みCLI）
- `Test/AsciiSharp.Tests/` - MSTest単体テスト（テスト名は日本語）
- `Test/AsciiSharp.Specs/` - Reqnroll BDD仕様（Cucumber機能ファイルは日本語）
- `Design/AsciiSharp.Concept/` - 設計検討プロジェクト

### 構文木の設計
パーサーはRoslynのパターンに従います:
- すべての構文ノードは`SyntaxNode`を継承し、`Kind`、`NodeType`、`Location`プロパティを持つ
- init専用プロパティによる不変構文木
- `ISyntaxVisitor<TResult, TState>`によるVisitorパターン
- 位置追跡により元の文書構造を保持（行/列位置）
- ノードは階層的に構成: `DocumentSyntax` → `BlockSyntax` → `InlineSyntax`

構文ノード構造の例:
```csharp
public class ParagraphSyntax : BlockSyntax
{
    public IReadOnlyList<InlineSyntax> Inlines { get; init; }
    public override Location? Location { get; init; }
}
```

## 開発規約

### 言語とフレームワーク
- ターゲット: .NET 10.0（SDKバージョン `10.0.100-rc.1.25451.107`）
- 言語: C# 14の最新機能を積極的に使用
- **Implicit usings無効** - 常に明示的な`using`文を記述すること
- Nullable参照型をすべての場所で有効化
- ファイル名は英語、テストケース名とBDD機能ファイルは日本語

### コード品質
- オーバーフロー/アンダーフローチェック有効（`CheckForOverflowUnderflow`）
- 分析レベル: `latest-all`、警告レベル9999
- ビルド時にコードスタイルを強制
- 本番コードにはXMLドキュメント生成が必須
- AOT互換性: コアライブラリは`IsAotCompatible=true`

### パッケージ管理
- `Directory.Packages.props`による集中パッケージ管理
- 推移的固定有効（`CentralPackageTransitivePinningEnabled`）
- ロックファイル必須（`packages.lock.json`をリポジトリにコミット）
- `.csproj`ではバージョン番号なしでパッケージを参照

### ビルドシステム
- 成果物は`artifacts/`ディレクトリに出力（`UseArtifactsOutput=true`）
- 埋め込みデバッグシンボル（`DebugType=embedded`）
- ソースプロジェクトは`Source/Directory.Build.props`を継承
- テストプロジェクトは`Test/Directory.Build.props`を継承（CA1515、CA1707、CA2007を無効化）

## 主要な開発ワークフロー

### ビルド
```powershell
dotnet build --configuration Debug
dotnet build --configuration Release
```

### テスト実行
```powershell
# カバレッジ付きですべてのテストを実行
dotnet test --configuration Debug --logger trx --collect:"XPlat Code Coverage"

# 特定のテストを実行
dotnet test --filter "TestMethodName"
```

### AOT CLI公開
```powershell
dotnet publish Source/TckAdapter/AsciiSharp.TckAdapter.Cli/AsciiSharp.TckAdapter.Cli.csproj --configuration Release
```

### Dockerビルド（TCK Adapter）
```powershell
docker buildx bake -f docker-bake.hcl tck
```

## テスト規約

### MSTest (AsciiSharp.Tests)
- テストメソッド名は日本語: `[TestMethod] public void BodyOnlyInput_ShouldParseCorrectly()`
- `Assert.AreEqual`、`Assert.IsNotNull`パターンを使用
- `Location(Position, Position)`比較で位置精度をテスト

### Reqnroll BDD (AsciiSharp.Specs)
- 機能ファイルは日本語で記述（language: ja）
- ステップ定義は`StepDefinitions/`ディレクトリ
- 機能ファイル構造の例:
```gherkin
# language: ja
機能: 基本的なAsciiDocパーサー
  シナリオ: 単純なテキストをパースする
    前提 AsciiDoc文書 "body only" が与えられている
    もし 文書をパースする
    ならば 構文木のルートは "document" ノードである
```

## 外部参照
- AsciiDoc言語仕様: `submodules/asciidoc-lang/spec/`
- AsciiDoc TCK (Technology Compatibility Kit): `submodules/asciidoc-tck/tests/`
  - TCK Adapterはパース済みSyntaxTreeをTCK形式に変換し、公式テストスイートとの互換性テストを実施

## CI/CD
- mainへのPR: Debugビルドとテストレポート・カバレッジ（`.github/workflows/build-debug.yml`）
- mainへのプッシュ: Releaseビルド（`.github/workflows/build-release.yml`）
- テスト結果は`bibipkins/dotnet-test-reporter`経由でPRに投稿

## 共通パターン

### 新しい構文ノードの追加
1. `Source/AsciiSharp/Syntax/`に適切な基底クラス（`BlockSyntax`、`InlineSyntax`）を継承したクラスを作成
2. 対応する`SyntaxNodeKind`列挙値を追加
3. `ISyntaxVisitor`に新しいVisitメソッドを追加
4. ビジターを呼び出す`Accept`メソッドを実装
5. `Parser.cs`にパーサーロジックを追加
6. `AsciiSharp.Tests`に日本語名のテストを記述

### 新プロジェクトのパッケージ構成
```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <!-- NuGetパッケージの場合はIsPackable=trueを設定 -->
  <IsPackable>false</IsPackable>
</PropertyGroup>
```

パッケージ化可能なプロジェクトは、`Source/Directory.Build.props`からApache-2.0ライセンスとシンボルパッケージ設定を自動的に継承します。
