using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

/// <summary>
/// 属性エントリの解析に関する BDD テストです。
/// </summary>
[TestClass]
[FeatureDescription(
    @"属性エントリの解析
ライブラリユーザーとして、
ドキュメント ヘッダー内の属性エントリ（:name: value 形式）を解析し、
属性名と属性値を取得したい")]
public sealed partial class AttributeEntryParsingFeature : FeatureFixture
{
    [Scenario]
    public void 値あり属性エントリの解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("= Document Title\n:icons: font\n"),
            and => 文書を解析する(),
            then => 構文木のルートはDocumentノードである(),
            and => Documentノードは_Headerを持つ(),
            and => Headerは_N個の属性エントリを持つ(1),
            and => 属性エントリNの名前は(1, "icons"),
            and => 属性エントリNの値は(1, "font"));
    }

    [Scenario]
    public void 値なし属性エントリの解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("= Document Title\n:toc:\n"),
            and => 文書を解析する(),
            then => Documentノードは_Headerを持つ(),
            and => Headerは_N個の属性エントリを持つ(1),
            and => 属性エントリNの名前は(1, "toc"),
            and => 属性エントリNの値は空(1));
    }

    [Scenario]
    public void 複数の属性エントリの解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("= Document Title\n:icons: font\n:toc:\n"),
            and => 文書を解析する(),
            then => Documentノードは_Headerを持つ(),
            and => Headerは_N個の属性エントリを持つ(2),
            and => 属性エントリNの名前は(1, "icons"),
            and => 属性エントリNの値は(1, "font"),
            and => 属性エントリNの名前は(2, "toc"),
            and => 属性エントリNの値は空(2));
    }

    [Scenario]
    public void 属性エントリを含む文書のラウンドトリップ()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("= Document Title\n:icons: font\n:toc:\n\nbody\n"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 再構築されたテキストは元の文書と一致する());
    }

    [Scenario]
    public void ハイフンを含む属性名の解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("= Document Title\n:my-custom-attr: value\n"),
            and => 文書を解析する(),
            then => Headerは_N個の属性エントリを持つ(1),
            and => 属性エントリNの名前は(1, "my-custom-attr"),
            and => 属性エントリNの値は(1, "value"));
    }

    [Scenario]
    public void 属性エントリのない文書()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("= Document Title\n\nbody\n"),
            and => 文書を解析する(),
            then => Documentノードは_Headerを持つ(),
            and => Headerは_N個の属性エントリを持つ(0));
    }
}
