using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

/// <summary>
/// エラー耐性解析に関するフィーチャー テスト。
/// </summary>
[TestClass]
[FeatureDescription(@"エラー耐性解析
エディタ開発者として、
構文エラーを含む AsciiDoc 文書を解析しても、
正常な部分を最大限に解析し、有用な情報を提供したい")]
public partial class ErrorRecoveryFeature : FeatureFixture
{
    [Scenario]
    public void 不完全なセクションタイトルを含む文書の解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 以下のAsciiDoc文書がある("= ドキュメントタイトル\n\n== セクション1\n\nこれは正常な段落です。\n\n== \n\n== セクション2\n\nこれも正常な段落です。\n"),
            when => 文書を解析する(),
            then => 構文木が生成される(),
            then => 構文木に診断情報が含まれる(),
            then => 診断情報の数は_以上(1),
            then => セクションが正しく解析される("セクション1"),
            then => セクションが正しく解析される("セクション2"));
    }

    [Scenario]
    public void 正常部分の最大解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 以下のAsciiDoc文書がある("= タイトル\n\n== 正常セクション1\n\n正常な段落1。\n\n== [不正な属性\n\n不正なセクション。\n\n== 正常セクション2\n\n正常な段落2。\n"),
            when => 文書を解析する(),
            then => 構文木が生成される(),
            then => 正常な段落の数は_以上(2),
            then => 構文木からテキストを再構築できる());
    }

    [Scenario]
    public void 診断情報の位置情報が正確である()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 以下のAsciiDoc文書がある("= タイトル\n\n== \n"),
            when => 文書を解析する(),
            then => 構文木に診断情報が含まれる(),
            then => 診断情報に位置情報が含まれる());
    }

    [Scenario]
    public void 欠落トークンの検出()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 以下のAsciiDoc文書がある("= タイトル\n\n== \n\n段落テキスト。\n"),
            when => 文書を解析する(),
            then => 構文木が生成される(),
            then => 構文木に欠落ノードが含まれる(),
            then => 欠落ノードのIsMissingプロパティがtrue());
    }

    [Scenario]
    public void スキップされたトークンの保持()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 以下のAsciiDoc文書がある("= タイトル\n\n@@@不正なトークン@@@\n\n正常な段落。\n"),
            when => 文書を解析する(),
            then => 構文木が生成される(),
            then => 構文木からテキストを再構築できる());
    }
}
