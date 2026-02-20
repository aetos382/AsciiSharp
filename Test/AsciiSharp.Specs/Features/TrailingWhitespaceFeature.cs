using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;


namespace AsciiSharp.Specs.Features;

/// <summary>
/// 要素境界における行末トリビアの統一のテスト。
/// </summary>
[FeatureDescription(
    @"セクションタイトル・属性エントリ・著者行の要素境界で
行末の空白や改行を WhitespaceTrivia / EndOfLineTrivia として後続トリビアに付与し、
ASG での要素境界を正確に表現する")]
public sealed partial class TrailingWhitespaceFeature : FeatureFixture
{
    // ========================================
    // User Story 2: SyntaxTree における行末トリビアの識別
    // ========================================

    [Scenario]
    public void セクションタイトルの末尾空白が_WhitespaceTrivia_と_EndOfLineTrivia_として識別される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("== タイトル   \n"),
            and => 文書を解析する(),
            then => セクションタイトルの最終コンテンツトークンの後続トリビアにWhitespaceTriviaとEndOfLineTriviaが含まれる());
    }

    [Scenario]
    public void セクションタイトルの末尾に空白なしの_EndOfLineTrivia_のみが識別される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("== タイトル\n"),
            and => 文書を解析する(),
            then => セクションタイトルの最終コンテンツトークンの後続トリビアにEndOfLineTriviaのみが含まれる());
    }

    [Scenario]
    public void セクションタイトルの末尾CRLFが単一の_EndOfLineTrivia_として識別される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("== タイトル\r\n"),
            and => 文書を解析する(),
            then => セクションタイトルの最終コンテンツトークンの後続トリビアにCRLFを含むEndOfLineTriviaが含まれる());
    }

    [Scenario]
    public void 著者行の末尾空白が_WhitespaceTrivia_と_EndOfLineTrivia_として識別される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("= タイトル\n著者名   \n"),
            and => 文書を解析する(),
            then => 著者行の最終コンテンツトークンの後続トリビアにWhitespaceTriviaとEndOfLineTriviaが含まれる());
    }

    // ========================================
    // User Story 2b: Text プロパティの正確性
    // ========================================

    [Scenario]
    public void 行末空白付きセクションタイトルのTextプロパティは空白を含まない()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("== タイトル   \n"),
            and => 文書を解析する(),
            then => セクションタイトルのTextプロパティが期待値と一致する("タイトル"));
    }

    [Scenario]
    public void 行末空白付き著者行のTextプロパティは空白を含まない()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("= タイトル\n著者名   \n"),
            and => 文書を解析する(),
            then => 著者行のTextプロパティが期待値と一致する("著者名"));
    }

    [Scenario]
    public void タイトル内部空白は保持され末尾空白はトリビア化される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("== 単語1 単語2   \n"),
            and => 文書を解析する(),
            then => セクションタイトルのTextプロパティが期待値と一致する("単語1 単語2"),
            and => セクションタイトルの最終コンテンツトークンの後続トリビアにWhitespaceTriviaとEndOfLineTriviaが含まれる());
    }

    [Scenario]
    public void 著者行がEOFで終わる場合の後続トリビアは空である()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("= タイトル\n著者名"),
            and => 文書を解析する(),
            then => 著者行の最終コンテンツトークンの後続トリビアが空である());
    }

    // ========================================
    // User Story 3: 元テキストの完全復元
    // ========================================

    [Scenario]
    public void 行末空白を含むセクションタイトルのラウンドトリップが保証される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("== タイトル   \n"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 再構築されたテキストは元の文書と一致する());
    }

    [Scenario]
    public void 行末空白を含む著者行のラウンドトリップが保証される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("= タイトル\n著者名   \n"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 再構築されたテキストは元の文書と一致する());
    }

    [Scenario]
    public void CRLFを含むセクションタイトルのラウンドトリップが保証される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("== タイトル\r\n"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 再構築されたテキストは元の文書と一致する());
    }

    // ========================================
    // Edge Cases
    // ========================================

    [Scenario]
    public void 文書末尾に改行なしの行末空白のみがある場合のラウンドトリップが保証される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("== タイトル   "),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 再構築されたテキストは元の文書と一致する());
    }

    [Scenario]
    public void 行末空白なしで改行もないセクションタイトルのラウンドトリップが保証される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("== タイトル"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 再構築されたテキストは元の文書と一致する());
    }
}
