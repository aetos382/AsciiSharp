using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

/// <summary>
/// コメント解析に関する BDD テストです。
/// </summary>
[TestClass]
[FeatureDescription(
    @"コメント解析
ライブラリユーザーとして、
AsciiDoc のコメントを正しく解析し、
ラウンドトリップが可能な構文木を取得したい")]
public partial class CommentParsingFeature : FeatureFixture
{
    [Scenario]
    public void 単一行コメントを含む文書の解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("= タイトル\n\n// これはコメント\n本文テキスト\n"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 解析は成功する(),
            and => 再構築されたテキストは元の文書と一致する(),
            and => Documentノードは_N個の段落を持つ(1),
            and => 段落のテキストは_を含まない("// これはコメント"),
            and => 構文木に_を含むコメントがある("これはコメント"),
            and => 構文木に_N個の単一行コメントがある(1));
    }

    [Scenario]
    public void ブロックコメントを含む文書の解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("= タイトル\n\n////\nブロック\nコメント\n////\n本文テキスト\n"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 解析は成功する(),
            and => 再構築されたテキストは元の文書と一致する(),
            and => Documentノードは_N個の段落を持つ(1),
            and => 構文木に_を含むコメントがある("ブロック"),
            and => 構文木に_を含むコメントがある("コメント"),
            and => 構文木に_N個のブロックコメントがある(1));
    }

    [Scenario]
    public void URL内のスラッシュはコメントとして扱わない()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("https://example.com/path\n"),
            and => 文書を解析する(),
            then => 解析は成功する(),
            and => LinkノードのターゲットURLは("https://example.com/path"));
    }

    [Scenario]
    public void セクション内の単一行コメント()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("= タイトル\n\n== セクション 1\n// セクション内コメント\n段落テキスト\n\n== セクション 2\n別の段落\n"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 解析は成功する(),
            and => 再構築されたテキストは元の文書と一致する(),
            and => Documentノードは_N個のセクションを持つ(2),
            and => 構文木に_を含むコメントがある("セクション内コメント"),
            and => 構文木に_N個の単一行コメントがある(1));
    }

    [Scenario]
    public void 冒頭に単一行コメントがある文書の解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("// 冒頭のコメント\n= タイトル\n\n本文テキスト\n"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 解析は成功する(),
            and => 再構築されたテキストは元の文書と一致する(),
            and => Documentノードはタイトル_を持つ("タイトル"),
            and => Documentノードは_N個の段落を持つ(1),
            and => 構文木のトリビアに_が含まれる("// 冒頭のコメント"),
            and => 構文木に_を含むコメントがある("冒頭のコメント"),
            and => 構文木に_N個の単一行コメントがある(1));
    }

    [Scenario]
    public void 冒頭にブロックコメントがある文書の解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("////\n冒頭の\nブロックコメント\n////\n= タイトル\n\n本文テキスト\n"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 解析は成功する(),
            and => 再構築されたテキストは元の文書と一致する(),
            and => Documentノードはタイトル_を持つ("タイトル"),
            and => Documentノードは_N個の段落を持つ(1),
            and => 構文木のトリビアに_が含まれる("冒頭の"),
            and => 構文木に_を含むコメントがある("ブロックコメント"),
            and => 構文木に_N個のブロックコメントがある(1));
    }

    [Scenario]
    public void 冒頭に複数のコメントがある文書の解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("// 最初のコメント\n// 2番目のコメント\n= タイトル\n\n本文テキスト\n"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 解析は成功する(),
            and => 再構築されたテキストは元の文書と一致する(),
            and => Documentノードはタイトル_を持つ("タイトル"),
            and => Documentノードは_N個の段落を持つ(1),
            and => 構文木のトリビアに_が含まれる("// 最初のコメント"),
            and => 構文木のトリビアに_が含まれる("// 2番目のコメント"),
            and => 構文木に_を含むコメントがある("最初のコメント"),
            and => 構文木に_を含むコメントがある("2番目のコメント"),
            and => 構文木に_N個の単一行コメントがある(2));
    }
}
