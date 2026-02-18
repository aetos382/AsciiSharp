using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;


namespace AsciiSharp.Specs.Features;

/// <summary>
/// 構文木の走査に関するフィーチャー テスト。
/// </summary>
[FeatureDescription(@"構文木の走査
ライブラリ利用者として、
AsciiDoc 文書の構文木を走査し、
各ノードを訪問して処理を行いたい")]
public sealed partial class SyntaxVisitorFeature : FeatureFixture
{
    [Scenario]
    public void Visitorで全ノードを走査する()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("= タイトル\n\n段落テキスト\n"),
            when => 文書を解析する(),
            when => 全ノードを訪問するVisitorで走査する(),
            then => すべてのノードが訪問される(),
            then => 訪問されたノード数は(7));
    }

    [Scenario]
    public void Visitorでリンクノードのみを処理する()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("= タイトル\n\nhttps://example.com[リンク1] と https://example.org[リンク2] を含む段落\n"),
            when => 文書を解析する(),
            when => リンクを収集するVisitorで走査する(),
            then => 収集されたリンク数は(2));
    }

    [Scenario]
    public void Visitorでセクションタイトルを収集する()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("== セクション1\n\n段落1\n\n== セクション2\n\n段落2\n"),
            when => 文書を解析する(),
            when => セクションタイトルを収集するVisitorで走査する(),
            then => 収集されたセクションタイトル数は(2),
            then => セクションタイトルは順番に("セクション1", "セクション2"));
    }

    [Scenario]
    public void 空の文書を走査する()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある(""),
            when => 文書を解析する(),
            when => 全ノードを訪問するVisitorで走査する(),
            then => エラーなく走査が完了する(),
            then => 訪問されたノード数は(2));
    }

    [Scenario]
    public void Visitorで例外が発生した場合に伝播する()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("= タイトル\n\n段落テキスト\n"),
            when => 文書を解析する(),
            when => 例外を投げるVisitorで走査する(),
            then => 例外が伝播する());
    }

    [Scenario]
    public void 欠落ノードを含む構文木を走査する()
    {
        Runner.RunScenario(
            given => 不完全なAsciiDoc文書がある("== \n"),
            when => 文書を解析する(),
            when => 全ノードを訪問するVisitorで走査する(),
            then => 欠落ノードも訪問される());
    }

    [Scenario]
    public void 結果を返すVisitorパターンで目次を生成できる()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("= ドキュメントタイトル\n\n== セクション1\n\n段落1\n\n=== サブセクション1-1\n\n段落2\n\n== セクション2\n\n段落3\n"),
            when => 文書を解析する(),
            when => 結果を返すVisitorで目次を生成する(),
            then => 目次項目数は(3),
            then => 目次の階層構造が正しい());
    }

    [Scenario]
    public void 結果を返すVisitorパターンでプレーンテキストを抽出できる()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("= タイトル\n\nhttps://example.com[リンクテキスト] を含む段落\n"),
            when => 文書を解析する(),
            when => 結果を返すVisitorでプレーンテキストを抽出する(),
            then => 抽出されたテキストに_が含まれる("リンクテキスト"));
    }
}
