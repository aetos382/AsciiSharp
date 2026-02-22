using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;


namespace AsciiSharp.Specs.Features;

/// <summary>
/// 構文ノードの階層構造のテスト
/// </summary>
[FeatureDescription(
    @"構文ノードの階層構造
開発者として、
構文木を走査する際に、
型システムを利用してブロック要素とインライン要素を区別したい")]
public sealed partial class BlockInlineSyntaxFeature : FeatureFixture
{
    [Scenario]
    public void ブロック要素はBlockSyntaxとして識別できる()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("段落テキスト\n"),
            when => 文書を解析する(),
            then => DocumentノードはBlockSyntax(),
            then => ParagraphノードはBlockSyntax(),
            then => DocumentノードはInlineSyntaxではない(),
            then => ParagraphノードはInlineSyntaxではない()
        );
    }

    [Scenario]
    public void インライン要素はInlineSyntaxとして識別できる()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("https://example.com[リンク]\n"),
            when => 文書を解析する(),
            then => LinkノードはInlineSyntax(),
            then => LinkノードはBlockSyntaxではない()
        );
    }

    [Scenario]
    public void セクション関連ノードのBlockSyntax分類を確認できる()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("== セクションタイトル\n"),
            when => 文書を解析する(),
            then => SectionノードはBlockSyntax(),
            then => SectionTitleノードはBlockSyntaxではない()
        );
    }

    [Scenario]
    public void すべてのブロックノードを一括で取得できる()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("== セクション\n\n段落テキスト https://example.com[リンク]\n"),
            when => 文書を解析する(),
            when => すべてのBlockSyntaxノードをクエリする(),
            then => クエリ結果にDocumentノードが含まれる(),
            then => クエリ結果にParagraphノードが含まれる(),
            then => クエリ結果にSectionノードが含まれる(),
            then => クエリ結果にLinkノードは含まれない()
        );
    }

    [Scenario]
    public void すべてのインラインノードを一括で取得できる()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("段落テキスト https://example.com[リンク]\n"),
            when => 文書を解析する(),
            when => すべてのInlineSyntaxノードをクエリする(),
            then => クエリ結果にLinkノードが含まれる(),
            then => クエリ結果にParagraphノードは含まれない()
        );
    }
}
