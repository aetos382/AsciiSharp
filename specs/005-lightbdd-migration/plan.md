# Implementation Plan: BDD フレームワークの LightBDD 移行

**Branch**: `005-lightbdd-migration` | **Date**: 2026-02-10 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/005-lightbdd-migration/spec.md`

## Summary

Reqnroll（Gherkin ベースの BDD フレームワーク）を LightBDD.MsTest4（C# ネイティブの BDD フレームワーク）に移行する。14 個の .feature ファイル（67 シナリオ、@ignore の 1 件は移行対象外）を C# テストクラスに変換し、Reqnroll 依存を完全除去する。各フィーチャークラスは自己完結型とし、ステップ定義間の再利用は行わない。

## Technical Context

**Language/Version**: C# 14 / .NET 10.0
**Primary Dependencies**: LightBDD.MsTest4 v3.11.2, MSTest.Sdk
**Storage**: N/A
**Testing**: MSTest.Sdk + LightBDD.MsTest4
**Target Platform**: .NET 10.0
**Project Type**: テストフレームワーク移行（単一プロジェクト）
**Performance Goals**: N/A（テスト基盤の変更であり、パフォーマンス要件なし）
**Constraints**: MSTest.Sdk プロジェクト形式を維持、Central Package Management
**Scale/Scope**: 14 フィーチャーファイル、67 シナリオ（@ignore の 1 件は移行対象外）、12 ステップ定義クラス

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原則 | ステータス | 備考 |
|------|-----------|------|
| I. コード品質ファースト | ✅ PASS | 型安全性・IDE 支援の向上 |
| II. モジュール設計 | ✅ PASS | 各フィーチャークラスは自己完結型で疎結合 |
| III. BDD必須 | ✅ PASS | Given-When-Then、Red-Green-Refactor を継続。フレームワーク変更のみ |
| IV. 継続的品質保証 | ✅ PASS | 全テスト成功を維持 |
| V. 警告ゼロポリシー | ✅ PASS | 移行後も警告ゼロを維持 |
| VI. フェーズ順序の厳守 | ⚠️ 要更新 | 「.feature ファイル」→「フィーチャ定義」に用語を一般化する必要あり |

**VI の対応**: 憲章内の「.feature ファイル」を BDD の一般用語「フィーチャ定義」に更新する。各フェーズの役割や順序は変更しない。この更新は移行タスクの一部として実施する。

## Project Structure

### Documentation (this feature)

```text
specs/005-lightbdd-migration/
├── spec.md              # フィーチャー仕様
├── plan.md              # この計画書
├── research.md          # Phase 0 技術調査結果
├── quickstart.md        # LightBDD テスト作成ガイド
├── checklists/
│   └── requirements.md  # 品質チェックリスト
└── tasks.md             # Phase 2 タスクリスト（/speckit.tasks で生成）
```

### Source Code (repository root)

```text
Test/AsciiSharp.Specs/
├── AsciiSharp.Specs.csproj          # MSTest.Sdk + LightBDD.MsTest4
├── ConfiguredLightBddScope.cs       # LightBDD 初期化・設定
├── Features/
│   ├── BasicParsingFeature.cs       # シナリオ定義（partial）
│   ├── BasicParsingFeature.Steps.cs # ステップ実装（partial）
│   ├── SectionTitleRecognitionFeature.cs
│   ├── SectionTitleRecognitionFeature.Steps.cs
│   ├── CommentParsingFeature.cs
│   ├── CommentParsingFeature.Steps.cs
│   ├── ErrorRecoveryFeature.cs
│   ├── ErrorRecoveryFeature.Steps.cs
│   ├── ImmutabilityFeature.cs
│   ├── ImmutabilityFeature.Steps.cs
│   ├── IncrementalParsingFeature.cs
│   ├── IncrementalParsingFeature.Steps.cs
│   ├── LinkParsingFeature.cs
│   ├── LinkParsingFeature.Steps.cs
│   ├── SectionTitleInlineElementsFeature.cs
│   ├── SectionTitleInlineElementsFeature.Steps.cs
│   ├── AttributeEntryParsingFeature.cs
│   ├── AttributeEntryParsingFeature.Steps.cs
│   ├── SectionTitleTriviaFeature.cs
│   ├── SectionTitleTriviaFeature.Steps.cs
│   ├── TrailingWhitespaceFeature.cs
│   ├── TrailingWhitespaceFeature.Steps.cs
│   ├── InlineTextSyntaxRenameFeature.cs
│   ├── InlineTextSyntaxRenameFeature.Steps.cs
│   ├── BlockInlineSyntaxFeature.cs
│   ├── BlockInlineSyntaxFeature.Steps.cs
│   ├── SyntaxVisitorFeature.cs
│   └── SyntaxVisitorFeature.Steps.cs
└── [削除]
    ├── StepDefinitions/             # 旧 Reqnroll ステップ定義
    ├── Features/*.feature           # 旧 Gherkin ファイル
    └── reqnroll.json                # 旧 Reqnroll 設定
```

**Structure Decision**: 既存の `Test/AsciiSharp.Specs/` プロジェクトをそのまま使用。ファイル構成を Reqnroll 形式（.feature + StepDefinitions/）から LightBDD 形式（partial class ペア）に変更。Features/ ディレクトリ内のサブディレクトリ構造（SyntaxHierarchy/, Visitor/）はフラット化する。

## LightBDD テストクラス設計

### クラス構成パターン

各フィーチャーは 2 つの partial ファイルで構成される:

**定義ファイル** (`XxxFeature.cs`):
```csharp
[TestClass]
[FeatureDescription(@"フィーチャーの説明")]
public partial class XxxFeature : FeatureFixture
{
    [Scenario]
    public async Task シナリオ名()
    {
        await Runner.RunScenarioAsync(
            Given_パーサーが初期化されている,
            Given_以下のAsciiDoc文書がある,
            When_文書を解析する,
            Then_構文木が生成される);
    }
}
```

**実装ファイル** (`XxxFeature.Steps.cs`):
```csharp
public partial class XxxFeature
{
    private SyntaxTree? _currentSyntaxTree;
    private string _currentSourceText = string.Empty;

    private void Given_パーサーが初期化されている()
    {
        // パーサーは静的メソッドのため初期化不要
    }

    private void Given_以下のAsciiDoc文書がある()
    {
        this._currentSourceText = "...";
    }

    private void When_文書を解析する()
    {
        this._currentSyntaxTree = SyntaxTree.ParseText(this._currentSourceText);
    }

    private void Then_構文木が生成される()
    {
        Assert.IsNotNull(this._currentSyntaxTree);
    }
}
```

### パラメータ化ステップ

Reqnroll の正規表現キャプチャ（`(\d+)`、`""(.+)""`）は、LightBDD のメソッド引数に直接マッピングする:

```csharp
[Scenario]
public async Task セクション数の検証()
{
    await Runner.RunScenarioAsync(
        Given_パーサーが初期化されている,
        Given_以下のAsciiDoc文書がある,
        When_文書を解析する,
        _ => Then_Documentノードはn個のセクションを持つ(2));
}

private void Then_Documentノードはn個のセクションを持つ(int expectedCount)
{
    // ...
}
```

### 複数行テキスト入力

Reqnroll の DocString（`"""`）は、C# の raw string literal または定数フィールドに変換する:

```csharp
private void Given_以下のAsciiDoc文書がある()
{
    this._currentSourceText = """
        = タイトル

        本文テキスト
        """;
}
```

シナリオごとに入力テキストが異なる場合は、各シナリオの Given ステップメソッドに直接記述するか、パラメータとして渡す。

### @ignore シナリオの扱い

ErrorRecovery.feature の 1 件の @ignore シナリオ（区切りブロックのパース）は移行せず削除する。当該シナリオは MVP スコープ外であり、将来必要になった時点で新規作成する。

### LightBDD 初期化

`ConfiguredLightBddScope.cs`:
```csharp
[TestClass]
public class ConfiguredLightBddScope
{
    [AssemblyInitialize]
    public static void Setup(TestContext testContext)
    {
        LightBddScope.Initialize(cfg => cfg
            .ReportWritersConfiguration(rw => rw
                .AddFileWriter<HtmlReportFormatter>("~\\Reports\\FeaturesReport.html")));
    }

    [AssemblyCleanup]
    public static void Cleanup()
    {
        LightBddScope.Cleanup();
    }
}
```

## Complexity Tracking

| 違反 | 必要な理由 | より単純な代替案を却下した理由 |
|------|-----------|-------------------------------|
| 憲章 VI の用語変更 | LightBDD は .feature ファイルを使用しない | BDD の原則（Given-When-Then、Red-Green-Refactor）は変更しない。具体的なファイル形式を一般用語に置き換えるだけ |
