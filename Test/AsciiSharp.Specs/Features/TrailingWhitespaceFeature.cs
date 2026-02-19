using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;


namespace AsciiSharp.Specs.Features;

/// <summary>
/// 行末の空白や改行が正規化されずそのまま保持される機能のテスト。
/// </summary>
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

    // ========================================
    // User Story 1: 行末空白 Trivia の識別
    // ========================================

    [Scenario]
    public void 行末の空白と改行がTrailingWhitespaceTriviaとして識別される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("本文テキスト   \n"),
            and => 文書を解析する(),
            then => 最後のコンテンツトークンの後続トリビアがTrailingWhitespaceTriviaである());
    }

    [Scenario]
    public void CRLFのみの行末がTrailingWhitespaceTriviaとして識別される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("本文テキスト\r\n"),
            and => 文書を解析する(),
            then => 最後のコンテンツトークンの後続トリビアがTrailingWhitespaceTriviaである());
    }

    [Scenario]
    public void 改行のみの行末がTrailingWhitespaceTriviaとして識別される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("本文テキスト\n"),
            and => 文書を解析する(),
            then => 最後のコンテンツトークンの後続トリビアがTrailingWhitespaceTriviaである());
    }

    [Scenario]
    public void 空白のみの行全体がTrailingWhitespaceTriviaとして識別される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("   \n"),
            and => 文書を解析する(),
            then => 行全体が単一のTrailingWhitespaceTriviaとして識別される());
    }

    // ========================================
    // Edge Cases
    // ========================================

    [Scenario]
    public void 文書末尾に改行なしの行末空白が保持される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("本文テキスト   "),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 再構築されたテキストは元の文書と一致する());
    }

    [Scenario]
    public void 文書冒頭の空白のみ行がTrailingWhitespaceTriviaとして保持される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("   \n== タイトル\n"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 再構築されたテキストは元の文書と一致する());
    }

    [Scenario]
    public void 連続する空白のみ行がそれぞれTrailingWhitespaceTriviaとして保持される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("段落1\n\n\n段落2\n"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 再構築されたテキストは元の文書と一致する());
    }

    [Scenario]
    public void 連続する空白のみ行が各行独立したTrailingWhitespaceTriviaとして識別される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("段落1\n\n\n段落2\n"),
            and => 文書を解析する(),
            then => 各空白行が個別のTrailingWhitespaceTriviaとして識別される());
    }
}
