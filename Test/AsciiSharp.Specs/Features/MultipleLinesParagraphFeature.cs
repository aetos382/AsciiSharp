using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;

namespace AsciiSharp.Specs.Features;

/// <summary>
/// 複数行パラグラフの SyntaxTree 修正に関するフィーチャー テスト。
/// </summary>
[FeatureDescription(@"複数行パラグラフの正しい解析
ライブラリユーザーとして、
複数行にわたるパラグラフを含む AsciiDoc 文書を解析し、
行をまたいだテキストが1つのインライン要素として取得したい")]
[Label("TCK:block/paragraph/multiple-lines")]
public sealed partial class MultipleLinesParagraphFeature : FeatureFixture
{
    /// <summary>
    /// 複数行パラグラフが単一の InlineTextSyntax ノードとして解析される。
    /// </summary>
    [Scenario]
    public void 複数行パラグラフが単一のInlineTextSyntaxノードとして解析される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 複数行にわたる以下のパラグラフがある(
                "This paragraph has multiple lines that wrap after reaching the 72\n" +
                "character limit set by the text editor.\n"),
            when => 文書を解析する(),
            then => パラグラフのインライン要素が_N個である(1),
            then => 最初のインライン要素がInlineTextSyntaxである());
    }

    /// <summary>
    /// 複数行 InlineTextSyntax のテキストが改行で結合される。
    /// </summary>
    [Scenario]
    public void 複数行InlineTextSyntaxのテキストが改行で結合される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 複数行にわたる以下のパラグラフがある(
                "This paragraph has multiple lines that wrap after reaching the 72\n" +
                "character limit set by the text editor.\n"),
            when => 文書を解析する(),
            then => InlineTextSyntaxのTextが(
                "This paragraph has multiple lines that wrap after reaching the 72\n" +
                "character limit set by the text editor."));
    }

    /// <summary>
    /// 複数行 InlineTextSyntax の Span が最終行の改行を含まない。
    /// </summary>
    [Scenario]
    public void 複数行InlineTextSyntaxのSpanが最終行の改行を含まない()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 複数行にわたる以下のパラグラフがある(
                "This paragraph has multiple lines that wrap after reaching the 72\n" +
                "character limit set by the text editor.\n"),
            when => 文書を解析する(),
            then => InlineTextSyntaxのSpanEndが最終行末尾コンテンツの次の位置である());
    }

    /// <summary>
    /// 単一行パラグラフの Span が行末の改行を含まない。
    /// </summary>
    [Scenario]
    public void 単一行パラグラフのSpanが行末の改行を含まない()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 以下の複数パラグラフ文書がある(
                "This document has multiple paragraphs.\n" +
                "\n" +
                "Paragraphs are separated by one or more empty lines.\n"),
            when => 文書を解析する(),
            then => 最初のパラグラフのSpanが改行を含まない(),
            then => 最後のパラグラフのSpanが改行を含まない());
    }

    /// <summary>
    /// 複数行パラグラフのラウンドトリップが保証される。
    /// </summary>
    [Scenario]
    public void 複数行パラグラフのラウンドトリップが保証される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 複数行にわたる以下のパラグラフがある(
                "This paragraph has multiple lines that wrap after reaching the 72\n" +
                "character limit set by the text editor.\n"),
            when => 文書を解析する(),
            then => 構文木から復元したテキストは元の文書と一致する());
    }

    /// <summary>
    /// リンクを含む行で InlineTextSyntax が分断される。
    /// </summary>
    [Scenario]
    public void リンクを含む行でInlineTextSyntaxが分断される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 複数行にわたる以下のパラグラフがある(
                "See https://example.org[example]\n"),
            when => 文書を解析する(),
            then => パラグラフのインライン要素が_N個である(2),
            then => 最初のインライン要素がInlineTextSyntaxである(),
            then => 二番目のインライン要素がLinkSyntaxである());
    }
}
