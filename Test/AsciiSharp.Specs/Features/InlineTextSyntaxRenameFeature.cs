using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

/// <summary>
/// InlineTextSyntax の名前・型に関するフィーチャー テスト。
/// </summary>
[TestClass]
[FeatureDescription(
    @"TextSyntax を InlineTextSyntax として参照する
ライブラリユーザーとして、
プレーンテキストのインライン要素を InlineTextSyntax として参照し、
一貫した命名規則で構文木を操作したい")]
public partial class InlineTextSyntaxRenameFeature : FeatureFixture
{
    [Scenario]
    public void InlineTextSyntaxのSyntaxKindはInlineTextである()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("= タイトル\n\n段落テキスト。\n"),
            when => 文書を解析する(),
            then => 段落の最初のインライン要素のSyntaxKindはInlineTextである());
    }

    [Scenario]
    public void VisitorでInlineTextSyntaxを訪問する()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("= タイトル\n\n段落テキスト。\n"),
            when => 文書を解析する(),
            when => Visitorでドキュメントを走査する(),
            then => VisitInlineTextメソッドが呼び出される());
    }

    [Scenario]
    public void InlineTextSyntaxからTextプロパティを取得する()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("= タイトル\n\nHello, World!\n"),
            when => 文書を解析する(),
            then => 段落の最初のインライン要素のテキストは("Hello, World!"));
    }
}
