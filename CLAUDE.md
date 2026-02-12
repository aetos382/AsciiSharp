# CLAUDE.md

## クイック リファレンス

```bash
dotnet build              # ビルド
dotnet test               # テスト
```

## 環境要件

- .NET 10

## 一般的な原則

- チャットの受け答えは日本語で行ってください。
- Web を検索する際は、適宜、英語での検索も行ってください。結果を表示する際は日本語にしてください。
- Web 検索して見つけた情報については、出典となる URL を教えてください。
- ファイルを書き換える際は、何のための変更なのか、簡潔な日本語の説明を表示してください。
- プロジェクト内のすべてのテキスト ファイルは、特別な例外を除き、BOM なし UTF-8 でエンコードします。改行コードは LF (U+000A) で統一します。
- `main` ブランチへの直接コミットは禁止です。

## スクリプトの実行について

- PowerShell コマンドは `powershell` ではなく `pwsh` で実行してください。
- CLI コマンドを実行する際は、何をするコマンドなのか、簡潔な日本語の説明を表示してください。

## このプロジェクトについて

- C# 製の .NET 向け [AsciiDoc](https://asciidoc.org/) パーサーです。

### 仕様

- 生成された構文木はイミュータブルとします。
- コメントや空白の情報も保持し、元のテキストの完全な復元が可能です。
- エラーのある文書でも可能な限りパースすることを試みます。

### 目標

- インタラクティブなテキスト エディタでの使用に耐えることを目標とします。
- AsciiDoc [LSP](https://microsoft.github.io/language-server-protocol/) の実装に使うことができることを目標とします。

### プロジェクト構造

```
AsciiSharp.slnx
├─ Source
│  ├─ AsciiSharp: コア ライブラリです。
│  ├─ AsciiSharp.Asg: SyntaxTree を ASG (Abstract Semantic Graph) 形式に変換するためのライブラリです。
│  └─ AsciiSharp.TckAdapter: TCK と連携して準拠テストを行うコンソール アプリケーションです。
├─ Polyfills: .NET Standard 2.0 でサポートされない言語機能やフレームワーク機能を補うためのコードです。
├─ Test
│  ├─ AsciiSharp.Asg.Tests: AsciiSharp.Asg のユニット テスト プロジェクトです。
│  ├─ AsciiSharp.Specs: BDD によるテスト プロジェクトです。
│  └─ AsciiSharp.Tests: ユニット テスト プロジェクトです。
├─ Benchmark: ベンチマーク プロジェクトです。
└─ submodules: Git サブ モジュールが格納されます。
   ├─ asciidoc-lang: AsciiDoc 言語仕様のリポジトリです。
   └─ asciidoc-tck: AsciiDoc Technology Compatibility Kit (TCK) のリポジトリです。
```

## 重視する品質特性について

以下の品質を重視してコーディングします。

- 可読性
- メンテナンス性
- テスト可能性

以下の特性については、初期においては重視しません。

- 可観測性
- カバレッジ レート
- パフォーマンス

### 可観測性について

AsciiSharp コア ライブラリでは以下の方針に従います。
- [ActivitySource](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.activitysource) を用いたアクティビティ トレースと、[Meter](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.metrics.meter) を用いたメトリクス出力を行うことができます。
- ログは出力しません。

## コードの修正について

- コードを修正した場合、次のステップに進む前にビルドとテストを行います。
- BDD の Green または Refactor ステップで、ビルドまたはテストに失敗した場合、修正してビルドとテストが通るようにしてから、次のステップに進みます。
- ビルド警告は、Refactor ステップ中に修正または無効化します。次の Red ステップに進む際には警告ゼロの状態です。
- ビルドとテストが通ったらコミットします。コード整形は pre-commit フック（Husky.NET）で自動実行されます。

## 言語について

- 仕様書や技術的ドキュメントは日本語で記述してください。
- コードを書く際も、コード内のコメントやドキュメントは日本語で書いてください。
- コミット メッセージも日本語で記述してください。
- テスト メソッド名は日本語で書いてください。
- クラスやメソッドには XML ドキュメント コメントを日本語で書いてください。
- BDD の feature ファイルは日本語で書いてください。
- 例外のメッセージに関してはハード コーディングせず、リソースを使うものとします。また、リソースの第一言語は英語とします。

## C# と .NET について

- C# の言語バージョンは 14 とします。
- 以下の特例を除き、すべてのプロジェクトのターゲット フレームワークは .NET 10.0 とします。
  - `Source/AsciiSharp/AsciiSharp.csproj` は .NET 10.0 と .NET Standard 2.0 です。
    - すべてのフレームワークで同等の機能性を提供しなければなりません。
  - `Test/AsciiSharp.Tests` は .NET 10.0 と .NET Framework 4.8.1 です。

### .NET Standard 2.0 対応について

- .NET Standard 2.0 で標準的にサポートされない言語機能（例：`init` プロパティ アクセッサ）については、Polyfill を使用することで対応します。
- .NET Standard 2.0 でサポートされない特定のランタイム機能が必要な言語機能（例：`allows ref struct`）については、原則として使用を控えます。
- .NET Standard 2.0 で標準的にサポートされないライブラリ機能については、追加のライブラリ（例：[System.Threading.Tasks.Extensions](https://www.nuget.org/packages/System.Threading.Tasks.Extensions/)）を導入することで、同じコードにできる場合、ライブラリを用いて対応します。
  - この場合、使用してよい NuGet パッケージは、`System.*` もしくは `Microsoft.*` に限ります。
- 条件付きコンパイル（`#if NETSTANDARD`）は使用可能です。
- .NET 10.0 より前の .NET Standard 2.0 準拠ランタイムで実行した際に、.NET 10.0 と同じパフォーマンスが実現できないことは許容します。

### パッケージ参照について

- [Central Package Management](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management) を採用しています。
  - 各 `*.csproj` ファイルにはパッケージのバージョン（`Version` 属性）を記載しません。
  - パッケージ バージョンは `Directory.Packages.props` で集中管理します。
- パッケージ参照を追加する場合は、特別な理由がある場合を除いて、その時点で最新のバージョンを調べて使用します。
  - 特別な理由で古いバージョンを参照する必要がある場合は、そのことをコメントで明記します。
- パッケージ ソースは `nuget.org` のみとします。

### コーディング スタイルについて

- .editorconfig および .globalconfig の指示に従います。

### パラメーターの有効性チェックについて

可視性が `private` なメソッド以外は、パラメーターの有効性チェックを行います。

`ArgumentNullException` や `ArgumentOutOfRangeException` を投げる可能性のある範囲チェックに関しては、それらの例外型のメソッドを使用します。

例：

```cs
ArgumentNullException.ThrowIfNull(parameter);
```

以下のようなスタイルは使用しません。
```cs
if (parameter == null) { throw new ArgumentNullException(nameof(parameter)); }
```
```cs
this._field = parameter ?? throw new ArgumentNullException(nameof(parameter));
```

## 継続的インテグレーションについて

- GitHub Actions を用いた継続的インテグレーション (CI) を実施します。

## パッケージングについて

- AsciiSharp コア ライブラリはビルド時に NuGet パッケージを生成します。
- 当面、生成されたパッケージは NuGet リポジトリに push しません。

## テストについて

- テスト フレームワークには [MSTest.Sdk](https://www.nuget.org/packages/MSTest.Sdk) を使用します。
- Behavior Driven Development (BDD) を行います。
- `AsciiSharp.Specs` プロジェクトでは [LightBDD.MsTest4](https://www.nuget.org/packages/LightBDD.MsTest4) を使用します。
- テストは以下のいずれかの方法で実行します。
  - ソリューション ディレクトリまたは個別のプロジェクト ディレクトリで `dotnet test`
  - 個別のプロジェクトディレクトリで `dotnet run`

### BDD の対象範囲

- BDD（LightBDD の C# フィーチャー クラスによる振る舞いテスト）の対象は **コア ライブラリ（Source/AsciiSharp）のみ** とします。
- その他のプロジェクト（Asg、TckAdapter 等）は通常のユニット テストで検証します。

### テスト プロジェクトの作成

新しいテスト プロジェクトを作成する際は、`dotnet new` コマンドを使用します。

```bash
dotnet new mstest --language C# --framework net10.0 --sdk --test-runner Microsoft.Testing.Platform
```

### プロジェクト ファイルの操作

プロジェクト ファイル（`.csproj`）は直接編集せず、以下の CLI コマンドを使用します。

- **プロジェクト参照の追加**: `dotnet add reference <参照先プロジェクト>`
- **NuGet パッケージの追加**: `dotnet add package <パッケージ名>`
- **ソリューションへのプロジェクト追加**: `dotnet sln add <プロジェクト>`

### 仕様策定とフィーチャー定義について

- 仕様策定（`/speckit.specify` または `/speckit.plan`）の完了時には、必ず対応する LightBDD フィーチャー クラスを `Test/AsciiSharp.Specs/Features/` に作成します。
- フィーチャー クラスが作成されるまで、実装フェーズに進みません。
- フィーチャー クラスは partial class パターンで作成し、シナリオ定義（`XxxFeature.cs`）とステップ実装（`XxxFeature.Steps.cs`）に分割します。
- シナリオ名・ステップ メソッド名は日本語で記述します。

## コード レビューについて

### レビュー コメントの分類

以下のプレフィックスを使用してレビューコメントを分類してください：
- `[must]` - 必須修正項目（セキュリティ、バグ、重大な設計問題）
- `[recommend]` - 推奨修正項目（パフォーマンス、可読性の大幅改善）
- `[nits]` - 軽微な指摘（コードスタイル、タイポ等）
