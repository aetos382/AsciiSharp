using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;


namespace AsciiSharp.Specs.Features;

/// <summary>
/// セクションタイトルのインライン要素取得機能のテスト
/// </summary>
[FeatureDescription(
    @"セクションタイトルのインライン要素取得
ライブラリユーザーとして、
セクションタイトルの構文木からインライン要素を取得し、
タイトル内の構造化された情報にアクセスしたい")]
public sealed partial class SectionTitleInlineElementsFeature : FeatureFixture
{
    [Scenario]
    public void セクションタイトルからインライン要素を取得する()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("== Hello World\n"),
            when => 文書を解析する(),
            then => 最初のセクションタイトルのInlineElementsは_N個(1),
            then => 最初のセクションタイトルの最初のインライン要素はInlineTextSyntax(),
            then => 最初のセクションタイトルの最初のインライン要素のテキストは("Hello World")
        );
    }

    [Scenario]
    public void ドキュメントタイトルからインライン要素を取得する()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("= ドキュメントタイトル\n"),
            when => 文書を解析する(),
            then => ドキュメントタイトルのInlineElementsは_N個(1),
            then => ドキュメントタイトルの最初のインライン要素のテキストは("ドキュメントタイトル")
        );
    }

    [Scenario]
    public void InlineElementsの順序が構文上の出現順()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("== Hello World\n"),
            when => 文書を解析する(),
            then => 最初のセクションタイトルのInlineElementsは構文上の出現順に並んでいる()
        );
    }

    [Scenario]
    public void 空のタイトル()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("== \n"),
            when => 文書を解析する(),
            then => 最初のセクションタイトルのInlineElementsは_N個(0)
        );
    }

    [Scenario]
    public void 複数のセクションタイトルからInlineElementsを取得する()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("= メインタイトル\n\n== セクション 1\n\n=== サブセクション 1-1\n\n== セクション 2\n"),
            when => 文書を解析する(),
            then => ドキュメントタイトルの最初のインライン要素のテキストは("メインタイトル"),
            then => セクションNのタイトルの最初のインライン要素のテキストは(1, "セクション 1"),
            then => セクションNのタイトルの最初のインライン要素のテキストは(3, "セクション 2")
        );
    }
}
