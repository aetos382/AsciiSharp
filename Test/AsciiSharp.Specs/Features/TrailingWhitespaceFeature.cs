using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

/// <summary>
/// 行末の空白や改行が正規化されずそのまま保持される機能のテスト。
/// </summary>
[TestClass]
[FeatureDescription(
    @"行末の空白や改行が正規化されずそのまま保持される
ライブラリユーザーとして、
AsciiDoc 文書の行末空白や末尾改行が削除・正規化されることなく、
ToFullString() で元のテキストを完全に復元したい")]
public sealed partial class TrailingWhitespaceFeature : FeatureFixture
{
    [Scenario]
    public void セクションタイトルの末尾に空白がある場合に保持される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("== タイトル "),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 再構築されたテキストは元の文書と一致する());
    }

    [Scenario]
    public void 段落の末尾に空白がある場合に保持される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("本文テキスト   "),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 再構築されたテキストは元の文書と一致する());
    }

    [Scenario]
    public void セクションタイトルの末尾に改行がある場合に保持される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("== タイトル\n"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 再構築されたテキストは元の文書と一致する());
    }

    [Scenario]
    public void 段落の末尾に改行がある場合に保持される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("本文テキスト\n"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 再構築されたテキストは元の文書と一致する());
    }
}
