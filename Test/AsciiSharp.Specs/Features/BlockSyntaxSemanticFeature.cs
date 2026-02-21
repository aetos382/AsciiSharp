using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;


namespace AsciiSharp.Specs.Features;

/// <summary>
/// BlockSyntax の意味的分類が AsciiDoc 言語仕様と一致することを検証するテスト
/// </summary>
[FeatureDescription(
    @"BlockSyntax のセマンティクス定義
ライブラリ利用者として、
BlockSyntax の分類が AsciiDoc 言語仕様のブロック定義と一致していることを確認したい")]
public sealed partial class BlockSyntaxSemanticFeature : FeatureFixture
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
            then => AttributeEntrySyntaxはBlockSyntaxではない(),
            then => DocumentBodySyntaxはBlockSyntaxではない()
        );
    }

    [Scenario]
    public void StructuredTriviaSyntaxはSyntaxNodeを継承している()
    {
        Runner.RunScenario(
            given => StructuredTriviaSyntaxクラスが定義されている(),
            then => StructuredTriviaSyntaxはSyntaxNodeのサブクラスである(),
            then => StructuredTriviaSyntaxはBlockSyntaxのサブクラスではない(),
            then => StructuredTriviaSyntaxはInlineSyntaxのサブクラスではない()
        );
    }
}
