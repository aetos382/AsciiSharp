using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

/// <summary>
/// セクションタイトルの認識条件に関するフィーチャー テスト。
/// </summary>
[TestClass]
[FeatureDescription(@"セクションタイトルの認識条件
ライブラリユーザーとして、
セクション見出しとして認識される条件を理解し、
条件を満たさない行が段落として扱われることを確認したい")]
public sealed partial class SectionTitleRecognitionFeature : FeatureFixture
{
    [Scenario]
    public void 等号が7個以上の行は段落として扱われる()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 以下のAsciiDoc文書がある(
                "======= タイトル\n"),
            when => 文書を解析する(),
            when => 構文木から完全なテキストを取得する(),
            then => Documentノードは_Headerを持たない(),
            then => Documentノードは_N個のセクションを持つ(0),
            then => Documentノードは_N個の段落を持つ(1),
            then => 再構築されたテキストは元の文書と一致する());
    }

    [Scenario]
    public void 等号が6個の行はセクション見出しとして認識される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 以下のAsciiDoc文書がある(
                "====== レベル6\n"),
            when => 文書を解析する(),
            then => 最初のセクションタイトルのレベルは(6));
    }

    [Scenario]
    public void 等号の後に空白がない行は段落として扱われる()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 以下のAsciiDoc文書がある(
                "==タイトル\n"),
            when => 文書を解析する(),
            when => 構文木から完全なテキストを取得する(),
            then => Documentノードは_Headerを持たない(),
            then => Documentノードは_N個のセクションを持つ(0),
            then => Documentノードは_N個の段落を持つ(1),
            then => 再構築されたテキストは元の文書と一致する());
    }

    [Scenario]
    public void 等号の後に空白がある行はセクション見出しとして認識される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 以下のAsciiDoc文書がある(
                "== タイトル\n"),
            when => 文書を解析する(),
            then => Documentノードは_N個のセクションを持つ(1));
    }

    [Scenario]
    public void 単一の等号の後に空白がない行は段落として扱われる()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 以下のAsciiDoc文書がある(
                "=タイトル\n"),
            when => 文書を解析する(),
            when => 構文木から完全なテキストを取得する(),
            then => Documentノードは_Headerを持たない(),
            then => Documentノードは_N個のセクションを持つ(0),
            then => Documentノードは_N個の段落を持つ(1),
            then => 再構築されたテキストは元の文書と一致する());
    }

    [Scenario]
    public void ドキュメントタイトル位置の空タイトルは診断情報を伴う()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 以下のAsciiDoc文書がある(
                "= \n"),
            when => 文書を解析する(),
            then => 構文木が生成される(),
            then => 構文木に診断情報が含まれる());
    }

    [Scenario]
    public void 等号が7個以上で空白ありの行も段落として扱われる()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 以下のAsciiDoc文書がある(
                "======= これは見出しではない\n\n======== これも見出しではない\n"),
            when => 文書を解析する(),
            when => 構文木から完全なテキストを取得する(),
            then => Documentノードは_Headerを持たない(),
            then => Documentノードは_N個のセクションを持つ(0),
            then => Documentノードは_N個の段落を持つ(2),
            then => 再構築されたテキストは元の文書と一致する());
    }
}
