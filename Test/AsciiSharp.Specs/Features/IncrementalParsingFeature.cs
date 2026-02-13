using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

/// <summary>
/// 増分解析機能のテスト
/// </summary>
[TestClass]
[FeatureDescription(
    @"増分解析
開発者として、
エディタで文書の一部を編集したとき、
パーサーが変更された部分のみを再解析し、
変更されていない部分の解析結果を再利用したい")]
public sealed partial class IncrementalParsingFeature : FeatureFixture
{
    [Scenario]
    public void 単一ブロック内の編集で他のブロックは再利用される()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("= Document Title\n\n== Section 1\n\nFirst paragraph.\n\n== Section 2\n\nSecond paragraph.\n\n== Section 3\n\nThird paragraph.\n"),
            when => 文書を解析する(),
            when => すべてのセクションの内部ノードへの参照を保持する(),
            when => テキストを変更して増分解析する("First paragraph.", "Edited paragraph."),
            then => インデックスのセクションの内部ノードは再利用されている(2),
            then => インデックスのセクションの内部ノードは再利用されている(3)
        );
    }

    [Scenario]
    public void 同一インスタンスの維持()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("= Document Title\n\n== Section A\n\nContent of section A.\n\n== Section B\n\nContent of section B.\n"),
            when => 文書を解析する(),
            when => 名前のセクションの内部ノードへの参照を保持する("Section B"),
            when => テキストを変更して増分解析する("Content of section A.", "Modified content."),
            then => 名前のセクションの内部ノードは再利用されている("Section B")
        );
    }

    [Scenario]
    public void 増分解析後もラウンドトリップが成功する()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("= Document Title\n\nOriginal content.\n"),
            when => 文書を解析する(),
            when => テキストを変更して増分解析する("Original content.", "Modified content."),
            when => 増分解析後の構文木から完全なテキストを取得する(),
            then => 再構築されたテキストは変更後の文書と一致する()
        );
    }

    [Scenario]
    public void 構造共有により変更されていないノードは再利用される()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("= Document Title\n\n== Section 1\n\nParagraph 1.\n\n== Section 2\n\nParagraph 2.\n"),
            when => 文書を解析する(),
            when => すべてのセクションの内部ノードへの参照を保持する(),
            when => テキストを変更して増分解析する("Paragraph 2.", "Modified."),
            then => インデックスのセクションの内部ノードは再利用されている(1)
        );
    }
}
