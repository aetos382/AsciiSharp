# Quickstart: LightBDD によるテスト作成ガイド

**Branch**: `005-lightbdd-migration` | **Date**: 2026-02-10

## テストの実行

```bash
# 全テスト実行
dotnet test

# BDD テストのみ実行
dotnet test Test/AsciiSharp.Specs/

# 詳細な出力（LightBDD のシナリオ進捗を表示）
dotnet test Test/AsciiSharp.Specs/ --logger:"console;verbosity=normal"
```

## テストレポート

テスト実行後、HTML レポートが `Test/AsciiSharp.Specs/Reports/FeaturesReport.html` に生成される。

## 新しいシナリオの追加方法

### 1. 既存フィーチャーにシナリオを追加

定義ファイル（`XxxFeature.cs`）に `[Scenario]` メソッドを追加:

```csharp
[Scenario]
public async Task 新しいシナリオ名()
{
    await Runner.RunScenarioAsync(
        Given_パーサーが初期化されている,
        Given_以下のAsciiDoc文書がある,
        When_文書を解析する,
        Then_期待する結果);
}
```

実装ファイル（`XxxFeature.Steps.cs`）に必要なステップメソッドを追加:

```csharp
private void Then_期待する結果()
{
    Assert.IsNotNull(this._currentSyntaxTree);
}
```

### 2. 新しいフィーチャーを作成

2 つの partial ファイルを作成:

**`Features/NewFeature.cs`**:
```csharp
using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;

namespace AsciiSharp.Specs.Features;

[TestClass]
[FeatureDescription(@"新しいフィーチャーの説明")]
public partial class NewFeature : FeatureFixture
{
    [Scenario]
    public async Task シナリオ名()
    {
        await Runner.RunScenarioAsync(
            Given_前提条件,
            When_操作,
            Then_結果);
    }
}
```

**`Features/NewFeature.Steps.cs`**:
```csharp
namespace AsciiSharp.Specs.Features;

public partial class NewFeature
{
    private void Given_前提条件()
    {
        // セットアップ
    }

    private void When_操作()
    {
        // 実行
    }

    private void Then_結果()
    {
        // アサーション
    }
}
```

### 3. パラメータ付きステップ

引数を渡す場合はラムダ式で呼び出す:

```csharp
[Scenario]
public async Task パラメータ付きシナリオ()
{
    await Runner.RunScenarioAsync(
        Given_パーサーが初期化されている,
        Given_以下のAsciiDoc文書がある,
        When_文書を解析する,
        _ => Then_セクション数は(3));
}

private void Then_セクション数は(int expected)
{
    var sections = this._currentSyntaxTree!.Root.ChildNodes
        .OfType<SectionSyntax>();
    Assert.AreEqual(expected, sections.Count());
}
```

## 命名規約

- ステップメソッド名: `Given_`、`When_`、`Then_` プレフィックス + 日本語の説明
- アンダースコアはレポート上でスペースに変換される
- 例: `Given_パーサーが初期化されている` → レポート表示: `Given パーサーが初期化されている`
