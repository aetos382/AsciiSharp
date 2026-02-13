using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

/// <summary>
/// SectionTitleSyntax の空白トリビアが適切に保持される機能のテスト。
/// </summary>
[TestClass]
[FeatureDescription(
    @"SectionTitleSyntax の空白トリビアが適切に保持される
ライブラリユーザーとして、
セクションタイトルのマーカーとタイトル本文の間の空白が適切に保持され、
ToFullString() で元のテキストを完全に復元したい")]
public sealed partial class SectionTitleTriviaFeature : FeatureFixture
{
    [Scenario]
    public void 単一のスペースを持つセクションタイトルの復元()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("== タイトル\n"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 再構築されたテキストは元の文書と一致する());
    }

    [Scenario]
    public void 複数のスペースを持つセクションタイトルの復元()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("==  タイトル\n"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 再構築されたテキストは元の文書と一致する());
    }

    [Scenario]
    public void スペースなしの行の復元_段落として解析される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("==タイトル\n"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 再構築されたテキストは元の文書と一致する());
    }

    [Scenario]
    public void マーカー後の空白がマーカーのTrailingTriviaとして保持される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("==  タイトル\n"),
            and => 文書を解析する(),
            then => セクションタイトルのマーカーはTrailingTriviaに空白を持つ());
    }

    [Scenario]
    public void 様々な空白パターンの文書の完全復元()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("= ドキュメントタイトル\n\n== セクション 1\n\n===   三つのスペース\n\n====タイトル直後\n"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 再構築されたテキストは元の文書と一致する());
    }
}
