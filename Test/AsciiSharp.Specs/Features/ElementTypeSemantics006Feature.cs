using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;


namespace AsciiSharp.Specs.Features;

/// <summary>
/// インライン要素とブロック要素のセマンティクス定義のテスト
/// </summary>
[FeatureDescription(
    @"インライン要素とブロック要素のセマンティクス定義
ライブラリ利用者として、
BlockSyntax の分類が AsciiDoc 言語仕様のブロック定義と一致していることを確認したい")]
public sealed partial class ElementTypeSemantics006Feature : FeatureFixture
{
    [Scenario]
    public void AsciiDoc仕様のブロックとされないノードはBlockSyntaxではない()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("= ドキュメントタイトル\n著者名\n:key: value\n\n== セクション\n\n段落テキスト\n"),
            when => 文書を解析する(),
            then => SectionTitleSyntaxはBlockSyntaxではない(),
            then => DocumentHeaderSyntaxはBlockSyntaxではない(),
            then => AuthorLineSyntaxはBlockSyntaxではない(),
            then => AttributeEntrySyntaxはBlockSyntaxではない()
        );
    }

    [Scenario]
    public void StructuredTriviaSyntaxはSyntaxNodeを継承している()
    {
        Runner.RunScenario(
            then => StructuredTriviaSyntaxはSyntaxNodeのサブクラスである(),
            then => StructuredTriviaSyntaxはBlockSyntaxのサブクラスではない(),
            then => StructuredTriviaSyntaxはInlineSyntaxのサブクラスではない()
        );
    }
}
