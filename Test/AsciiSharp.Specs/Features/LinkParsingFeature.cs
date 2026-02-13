using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

/// <summary>
/// リンクの解析機能のテスト
/// </summary>
[TestClass]
[FeatureDescription(
    @"リンクの解析
URL リンクを含むテキストを解析し、
リンクの位置とターゲットを正確に把握できる")]
public partial class LinkParsingFeature : FeatureFixture
{
    [Scenario]
    public void URLリンクを含む段落の解析()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("これは https://example.com へのリンクです。\n"),
            when => 文書を解析する(),
            then => 構文木が生成される(),
            then => 構文木にLinkノードが含まれる(),
            then => LinkノードのターゲットURLは("https://example.com")
        );
    }

    [Scenario]
    public void 表示テキスト付きリンクの解析()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("詳細は https://example.com[公式サイト] を参照してください。\n"),
            when => 文書を解析する(),
            then => 構文木にLinkノードが含まれる(),
            then => LinkノードのターゲットURLは("https://example.com"),
            then => LinkノードのDisplayTextは("公式サイト")
        );
    }

    [Scenario]
    public void 複数のリンクを含む段落の解析()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("https://example1.com と https://example2.com の両方を参照してください。\n"),
            when => 文書を解析する(),
            then => 構文木に_N個のLinkノードが含まれる(2),
            then => N番目のLinkノードのターゲットURLは(1, "https://example1.com"),
            then => N番目のLinkノードのターゲットURLは(2, "https://example2.com")
        );
    }

    [Scenario]
    public void リンクを含む文書のラウンドトリップ()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("= ドキュメント\n\nリンク: https://example.com[Example]\n"),
            when => 文書を解析する(),
            then => 構文木から復元したテキストは元の文書と一致する()
        );
    }
}
