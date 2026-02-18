using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;


namespace AsciiSharp.Specs.Features;

/// <summary>
/// 基本的な AsciiDoc 文書の解析に関するフィーチャー テスト。
/// </summary>
[FeatureDescription(@"基本的な AsciiDoc 文書の解析
ライブラリユーザーとして、
セクションと段落を含む AsciiDoc 文書を解析し、
ロスレスな構文木を取得したい")]
public sealed partial class BasicParsingFeature : FeatureFixture
{
    [Scenario]
    public void 本文のみを含む文章の解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 以下のAsciiDoc文書がある(
                "Hello, AsciiDoc.\n"),
            when => 文書を解析する(),
            when => 構文木から完全なテキストを取得する(),
            then => 構文木のルートはDocumentノードである(),
            then => Documentノードは_Headerを持たない(),
            then => Documentノードは_N個の段落を持つ(1),
            then => 再構築されたテキストは元の文書と一致する());
    }

    [Scenario]
    public void ドキュメントタイトルとセクションを含む文書の解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 以下のAsciiDoc文書がある(
                "= ドキュメントタイトル\n\n== セクション 1\n\nこれは段落です。\n\n== セクション 2\n\nこれも段落です。\n"),
            when => 文書を解析する(),
            then => 構文木のルートはDocumentノードである(),
            then => Documentノードは_Headerを持つ(),
            then => Headerのタイトルは("ドキュメントタイトル"),
            then => Documentノードは_N個のセクションを持つ(2),
            then => セクションNのタイトルは(1, "セクション 1"),
            then => セクションNのタイトルは(2, "セクション 2"));
    }

    [Scenario]
    public void 構文木からの元テキスト再構築_ラウンドトリップ()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 以下のAsciiDoc文書がある(
                "= タイトル\n\n== セクション\n\n段落テキスト。\n複数行の段落。\n\n別の段落。\n"),
            when => 文書を解析する(),
            when => 構文木から完全なテキストを取得する(),
            then => 再構築されたテキストは元の文書と一致する());
    }

    [Scenario]
    public void ネストされたセクションの解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 以下のAsciiDoc文書がある(
                "= メインタイトル\n\n== レベル 2 セクション\n\nレベル 2 の内容。\n\n=== レベル 3 セクション\n\nレベル 3 の内容。\n\n==== レベル 4 セクション\n\nレベル 4 の内容。\n"),
            when => 文書を解析する(),
            then => 構文木のルートはDocumentノードである(),
            then => セクションのネスト構造が正しく解析されている());
    }

    [Scenario]
    public void 複数の段落の解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 以下のAsciiDoc文書がある(
                "= タイトル\n\n最初の段落。\n\n2番目の段落。\nこれは同じ段落の続き。\n\n3番目の段落。\n"),
            when => 文書を解析する(),
            then => Documentノードは_N個の段落を持つ(3));
    }

    [Scenario]
    public void ドキュメントヘッダーのみの文書()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 以下のAsciiDoc文書がある(
                "= ドキュメントタイトル\n著者名\n"),
            when => 文書を解析する(),
            then => 構文木のルートはDocumentノードである(),
            then => Headerのタイトルは("ドキュメントタイトル"),
            then => Headerは著者行を持つ());
    }

    [Scenario]
    public void 空白と改行の保持()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 以下のAsciiDoc文書がある(
                "= タイトル\n\n段落テキスト。\n"),
            when => 文書を解析する(),
            when => 構文木から完全なテキストを取得する(),
            then => 再構築されたテキストは元の文書と一致する(),
            then => すべての空白と改行が保持されている());
    }

    [Scenario]
    public void 空の文書の解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 空のAsciiDoc文書がある(),
            when => 文書を解析する(),
            when => 構文木から完全なテキストを取得する(),
            then => 構文木のルートはDocumentノードである(),
            then => Documentノードは_Headerを持たない(),
            then => Documentノードは_N個の段落を持つ(0),
            then => 再構築されたテキストは元の文書と一致する());
    }

    [Scenario]
    public void BOMを含む文書の解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => BOM付きの以下のAsciiDoc文書がある(
                "= タイトル\n\n本文テキスト\n"),
            when => 文書を解析する(),
            then => 構文木のルートはDocumentノードである(),
            then => ソーステキストはBOMを含む(),
            then => Headerのタイトルは("タイトル"),
            then => BOMを含む元のテキストを復元できる());
    }

    [Scenario]
    public void CRLF改行コードを含む文書の解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => CRLF改行コードの以下のAsciiDoc文書がある(
                "= タイトル\n\n本文テキスト\n"),
            when => 文書を解析する(),
            when => 構文木から完全なテキストを取得する(),
            then => 構文木のルートはDocumentノードである(),
            then => 再構築されたテキストは元の文書と一致する());
    }

    [Scenario]
    public void CR改行コードを含む文書の解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => CR改行コードの以下のAsciiDoc文書がある(
                "= タイトル\n\n本文テキスト\n"),
            when => 文書を解析する(),
            when => 構文木から完全なテキストを取得する(),
            then => 構文木のルートはDocumentノードである(),
            then => 再構築されたテキストは元の文書と一致する());
    }

    [Scenario]
    public void 混在する改行コードを含む文書の解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 混在する改行コードの以下のAsciiDoc文書がある(
                "= タイトル\n\n本文テキスト\n"),
            when => 文書を解析する(),
            when => 構文木から完全なテキストを取得する(),
            then => 構文木のルートはDocumentノードである(),
            then => 再構築されたテキストは元の文書と一致する());
    }
}
