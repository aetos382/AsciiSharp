# Research: BDD フレームワークの LightBDD 移行

**Branch**: `005-lightbdd-migration` | **Date**: 2026-02-10

## R-001: MSTest.Sdk と LightBDD.MsTest4 の互換性

**Decision**: `MSTest.Sdk` プロジェクト形式を維持する

**Rationale**: LightBDD の公式サンプルは `Microsoft.NET.Sdk` を使用しているが、MSTest.Sdk との互換性を否定する具体的な Issue や報告は見つかっていない。MSTest.Sdk は追加の PackageReference を許容しているため、LightBDD.MsTest4 を追加パッケージとして参照できる可能性が高い。具体的な互換性問題が発生した場合にのみ `Microsoft.NET.Sdk` への切り替えを検討する。

**Alternatives considered**:
- 先行して `Microsoft.NET.Sdk` に切り替え → 具体的な問題がない段階での変更は過剰対応

**出典**:
- https://github.com/LightBDD/LightBDD/wiki/Test-Framework-Integrations
- https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-sdk

## R-002: テストクラスの構成方針

**Decision**: 各フィーチャークラスを自己完結型とする。ステップ定義間での再利用は行わない

**Rationale**: 現在の Reqnroll 構成では BasicParsingSteps を共通基底として 11 クラスが依存しているが、LightBDD 移行後は各フィーチャークラスが自身に必要なステップメソッドをすべて持つ。コードの重複は発生するが、各フィーチャーテストが独立して理解・修正可能になり、メンテナンス性が向上する。

**構成**:
- 各フィーチャークラスは `FeatureFixture` を直接継承する
- partial class で定義ファイル（シナリオ）と実装ファイル（ステップ）を分離する
- 共通処理（パース実行等）は各クラスに個別に記述する

**Alternatives considered**:
- 基底クラス `ParsingFeatureBase` による継承 → クラス間の結合が生まれ、独立した修正が難しくなる
- `WithContext<T>()` パターン → 呼び出し構文が複雑化し、追加の学習コストが発生する
- DI コンテナ（`LightBDD.Extensions.DependencyInjection`）→ 追加パッケージが必要であり、この規模にはオーバースペック

## R-003: 日本語ステップメソッド名のレポート表示

**Decision**: 日本語メソッド名をそのまま使用する。問題発生時は `NameFormatterConfiguration` でカスタマイズ対応する

**Rationale**: LightBDD の `DefaultNameFormatter` はアンダースコアをスペースに変換するのみで、日本語文字はそのまま保持される。HTML レポートは `<meta charset="UTF-8">` を使用しており、UTF-8 エンコーディングに対応。例: `Given_パーサーが初期化されている` → `Given パーサーが初期化されている` と表示される。

**出典**:
- https://github.com/LightBDD/LightBDD/blob/master/src/LightBDD.Framework/Formatting/DefaultNameFormatter.cs

## R-004: 並列テスト実行の互換性

**Decision**: `[assembly: Parallelize]` は引き続き使用可能。クラスレベル並列実行を維持する

**Rationale**: LightBDD は基盤テストフレームワーク（MSTest）の並列実行機能に依存する。MSTest のデフォルトであるクラスレベル並列実行では、各フィーチャーテストクラスが独立して実行されるため問題ない。LightBDD は `ParallelProgressNotifierProvider` を標準搭載しており、並列実行時の進捗追跡にも対応。

**出典**:
- https://github.com/LightBDD/LightBDD/wiki/Test-Framework-Integrations

## R-005: LightBDD.MsTest4 のバージョンと依存関係

**Decision**: LightBDD.MsTest4 v3.11.2 を使用する

**Rationale**: 2026-02-02 に公開された初版。MSTest.TestFramework >= 4.0.2 と LightBDD.Framework >= 3.11.2 に依存。.NET 8.0+ / .NET Standard 2.0 / .NET Framework 4.6.2 をサポート。AsciiSharp.Specs のターゲット .NET 10.0 は .NET 8.0+ に含まれるため互換性あり。

**出典**:
- https://www.nuget.org/packages/LightBDD.MsTest4/

## R-006: 憲章（Constitution）の用語更新

**Decision**: 憲章内の「.feature ファイル」を「フィーチャ定義」に一般化する

**Rationale**: 憲章の BDD 原則（Given-When-Then、Red-Green-Refactor）はフレームワーク非依存。「.feature ファイル」は Gherkin 固有の表現であり、LightBDD では C# テストクラスがフィーチャ定義に相当する。用語をフレームワーク非依存に更新することで、BDD の原則自体は変更せずに、実装手段の柔軟性を確保する。各フェーズでやることは変わらない。

## R-007: 移行戦略

**Decision**: ビッグバン移行を採用する

**Rationale**: 68 シナリオの規模は段階的移行を必要とするほど大きくない。Reqnroll と LightBDD は同一テストプロジェクト内でパッケージ競合を起こす可能性がある。一括変換のほうが中間状態のビルド不整合を避けられる。

## R-008: シナリオインベントリ

調査の結果、既存の BDD シナリオは以下の通り:

| フィーチャーファイル | シナリオ数 | @ignore |
|---------------------|-----------|---------|
| BasicParsing.feature | 11 | 0 |
| SectionTitleRecognition.feature | 7 | 0 |
| CommentParsing.feature | 7 | 0 |
| ErrorRecovery.feature | 5 | 1 |
| Immutability.feature | 3 | 0 |
| IncrementalParsing.feature | 5 | 0 |
| LinkParsing.feature | 4 | 0 |
| SectionTitleInlineElements.feature | 5 | 0 |
| AttributeEntryParsing.feature | 7 | 0 |
| SectionTitleTrivia.feature | 5 | 0 |
| TrailingWhitespace.feature | 4 | 0 |
| InlineTextSyntaxRename.feature | 3 | 0 |
| SyntaxHierarchy/BlockInlineSyntax.feature | 5 | 0 |
| Visitor/SyntaxVisitor.feature | 8 | 0 |
| **合計** | **68** | **1** |

**注記**: 仕様書の「約 50 シナリオ」は 68 シナリオに修正が必要。
