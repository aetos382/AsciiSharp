using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

/// <summary>
/// 構文木の不変性とクエリ機能のテスト
/// </summary>
[TestClass]
[FeatureDescription(
    @"構文木の不変性とクエリ
開発者として、
構文木を変更せずにクエリを行い、
必要に応じて元の構文木を保持したまま新しい構文木を作成したい")]
public sealed partial class ImmutabilityFeature : FeatureFixture
{
    [Scenario]
    public void ノード置換後も元の構文木は変更されない()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("段落テキスト\n"),
            when => 文書を解析する(),
            when => 元の構文木への参照を保持する(),
            when => 段落ノードを新しいテキストで置換する("新しい段落"),
            then => 元の構文木の段落テキストは("段落テキスト"),
            then => 新しい構文木の段落テキストは("新しい段落")
        );
    }

    [Scenario]
    public void 特定の種類のノードをクエリで検索()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("== セクション 1\n\n段落テキスト\n\n== セクション 2\n\n段落テキスト2\n"),
            when => 文書を解析する(),
            when => すべてのSectionノードをクエリする(),
            then => N個のSectionノードが返される(2),
            then => クエリ後も構文木は変更されていない()
        );
    }

    [Scenario]
    public void 別コンポーネントの変更は元の参照に影響しない()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("段落テキスト\n"),
            when => 文書を解析する(),
            when => 元の構文木への参照を保持する(),
            when => 別の変数で構文木を変更する(),
            then => 元の参照が指す構文木は影響を受けない(),
            then => 元の参照の段落テキストは_のまま("段落テキスト")
        );
    }
}
