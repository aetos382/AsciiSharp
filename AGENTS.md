# AGENTS.md

## 一般的な原則

- チャットの受け答えは日本語で行ってください。
- Web を検索する際は、適宜、英語での検索も行ってください。結果を表示する際は日本語にしてください。
- Web 検索して見つけた情報については、出典となる URL を教えてください。
- ファイルを書き換える際は、何のための変更なのか、簡潔な日本語の説明を表示してください。
- プロジェクト内のすべてのテキスト ファイルは、特別な例外を除き、BOM なし UTF-8 でエンコードします。改行コードは LF (U+000A) で統一します。

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
│  └─ TckAdapter
│     ├─ AsciiSharp.TckAdapter: SyntaxTree を ASG (Abstract Syntax Graph) 形式に変換するためのライブラリです。
│     └─ AsciiSharp.TckAdapter.Cli: TCK と連携して準拠テストを行うコンソール アプリケーションです。
├─ Polyfills: .NET Standard 2.0 でサポートされない言語機能やフレームワーク機能を補うためのコードです。
├─ Test
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
- ビルドとテストが通ったら一度コミットし、`dotnet format` で整形してからプッシュします。

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
- `AsciiSharp.Specs` プロジェクトでは [Reqnroll](https://www.nuget.org/packages/Reqnroll) を使用します。
- テストは以下のいずれかの方法で実行します。
  - ソリューション ディレクトリまたは個別のプロジェクト ディレクトリで `dotnet test`
  - 個別のプロジェクトディレクトリで `dotnet run`

### 仕様策定と .feature ファイルについて

- 仕様策定（`/speckit.specify` または `/speckit.plan`）の完了時には、必ず対応する .feature ファイルを `Test/AsciiSharp.Specs/Features/` に作成します。
- .feature ファイルが作成されるまで、実装フェーズに進みません。
- .feature ファイルは日本語で記述し、Gherkin の Given-When-Then 形式に従います。

## コード レビューについて

### レビュー コメントの分類

以下のプレフィックスを使用してレビューコメントを分類してください：
- `[must]` - 必須修正項目（セキュリティ、バグ、重大な設計問題）
- `[recommend]` - 推奨修正項目（パフォーマンス、可読性の大幅改善）
- `[nits]` - 軽微な指摘（コードスタイル、タイポ等）


## 未決定の考慮事項

### プリプロセッサについて

- AsciiDoc には [Includes](https://docs.asciidoctor.org/asciidoc/latest/directives/include/) および [Conditionals](https://docs.asciidoctor.org/asciidoc/latest/directives/conditionals/) の 2 種類のプリプロセッサがあります。
- フロントマターの処理を行うなど、独自の[プリプロセッサ拡張](https://docs.asciidoctor.org/asciidoctor/latest/extensions/preprocessor/)を提供することもできます。
- [AsciiDoctor](https://docs.asciidoctor.org/) の仕様では、[プリプロセッサはドキュメントが解析される前に処理される](https://docs.asciidoctor.org/asciidoctorj/latest/extensions/preprocessor/)とあります。
- しかし、`include` は外部ファイルの読み込みを伴うことから、これをパースの時点で処理すると、パースの純粋性が失われ、また、パースに非同期 I/O 処理が絡むことになるため、好ましくありません。
- そのため、まず、プリプロセッサ指令を処理せず、そのまま保持した構文木を得ることができ、その後、必要に応じてプリプロセスを行うことができるようにします。
- `include` に関しては、追加で `IDictionary<string, Document>` のようなパース済みのコレクションを与えることで、同期的に適用できる設計にします。
- ただし、利便性のため、最初からプリプロセスした構文木を得られる非同期ユーティリティ メソッドも提供します。その場合、外部ドキュメントを取得する方法は `IExternalDocumentResolver` 的なインターフェイスで抽象化します。
- 文書に関するエラー報告は、プリプロセッサの処理が行われる前の座標で行われます。

### 文書ヘッダーについて

- AsciiDoc では文書の冒頭に[文書ヘッダー](https://docs.asciidoctor.org/asciidoc/latest/document/header/)が配置される場合があります。文書ヘッダーとコンテンツは空行で区切られます。
- `include` によって取り込まれる外部文書内に最初の空行がある場合や、`ifdef` の条件によって最初の空行の位置が変動する場合があります。その場合、文書ヘッダーとコンテンツの区切りは、プリプロセッサを処理してみるまで決定されません。
- この不確定性は、パースとプリプロセスを分離することに対する抵抗になり得ます。

### 正規化と内部エンコーディングについて

- AsciiDoc ではドキュメントは [UTF-8 に正規化される](https://docs.asciidoctor.org/asciidoc/latest/normalization/)と規定されています。
- 構文木の内部表現として .NET の `string` で保持するのか、UTF-8 エンコードされた `byte[]` で持つのかを検討します。
- いずれの場合でも、コード上の位置情報は Unicode のコードポイント単位で数えるものとします。
- 正規化の仕様では、レンダリングされた文書の生成においては、改行コードは `\n` とすると規定されています。
- 入力ソースファイルはその他の改行コード（`\r\n`等）も受理します。
- 正規化仕様は元のテキスト文字列の完全再現が可能であるという仕様と競合するため、方針を決定する必要があります。
  - オプション 1: 構文木からの再文字列化は「HTML、DocBook 等のレンダリング」ではないから正規化仕様は適用されない。
  - オプション 2: 出力文書の改行コードはオプションで指定することができる。
