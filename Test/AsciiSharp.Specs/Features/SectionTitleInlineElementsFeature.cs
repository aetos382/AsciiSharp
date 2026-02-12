using System.Linq;

using AsciiSharp.Syntax;
using AsciiSharp.Text;

using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

/// <summary>
/// セクションタイトルのインライン要素取得機能のテスト
/// </summary>
[TestClass]
[FeatureDescription(
    @"セクションタイトルのインライン要素取得
ライブラリユーザーとして、
セクションタイトルの構文木からインライン要素を取得し、
タイトル内の構造化された情報にアクセスしたい")]
public sealed partial class SectionTitleInlineElementsFeature : FeatureFixture
{
    private SourceText? _sourceText;
    private SyntaxTree? _syntaxTree;

    [Scenario]
    public void セクションタイトルからインライン要素を取得する()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("== Hello World\n"),
            when => 文書を解析する(),
            then => 最初のセクションタイトルのInlineElementsは_N個(1),
            then => 最初のセクションタイトルの最初のインライン要素はInlineTextSyntax(),
            then => 最初のセクションタイトルの最初のインライン要素のテキストは("Hello World")
        );
    }

    [Scenario]
    public void ドキュメントタイトルからインライン要素を取得する()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("= ドキュメントタイトル\n"),
            when => 文書を解析する(),
            then => ドキュメントタイトルのInlineElementsは_N個(1),
            then => ドキュメントタイトルの最初のインライン要素のテキストは("ドキュメントタイトル")
        );
    }

    [Scenario]
    public void InlineElementsの順序が構文上の出現順()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("== Hello World\n"),
            when => 文書を解析する(),
            then => 最初のセクションタイトルのInlineElementsは構文上の出現順に並んでいる()
        );
    }

    [Scenario]
    public void 空のタイトル()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("== \n"),
            when => 文書を解析する(),
            then => 最初のセクションタイトルのInlineElementsは_N個(0)
        );
    }

    [Scenario]
    public void 複数のセクションタイトルからInlineElementsを取得する()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("= メインタイトル\n\n== セクション 1\n\n=== サブセクション 1-1\n\n== セクション 2\n"),
            when => 文書を解析する(),
            then => ドキュメントタイトルの最初のインライン要素のテキストは("メインタイトル"),
            then => セクションNのタイトルの最初のインライン要素のテキストは(1, "セクション 1"),
            then => セクションNのタイトルの最初のインライン要素のテキストは(3, "セクション 2")
        );
    }

    private void 以下のAsciiDoc文書がある(string text)
    {
        _sourceText = SourceText.From(text);
    }

    private void 文書を解析する()
    {
        Assert.IsNotNull(_sourceText);
        _syntaxTree = SyntaxTree.ParseText(_sourceText);
        Assert.IsNotNull(_syntaxTree);
    }

    private void 最初のセクションタイトルのInlineElementsは_N個(int count)
    {
        var title = GetFirstSectionTitle();
        Assert.IsNotNull(title);
        Assert.HasCount(count, title.InlineElements);
    }

    private void 最初のセクションタイトルの最初のインライン要素はInlineTextSyntax()
    {
        var title = GetFirstSectionTitle();
        Assert.IsNotNull(title);
        Assert.IsTrue(title.InlineElements.Count > 0);
        Assert.IsInstanceOfType<InlineTextSyntax>(title.InlineElements[0]);
    }

    private void 最初のセクションタイトルの最初のインライン要素のテキストは(string expectedText)
    {
        var title = GetFirstSectionTitle();
        Assert.IsNotNull(title);
        Assert.IsTrue(title.InlineElements.Count > 0);

        var inlineText = title.InlineElements[0] as InlineTextSyntax;
        Assert.IsNotNull(inlineText);
        Assert.AreEqual(expectedText, inlineText.Text);
    }

    private void ドキュメントタイトルのInlineElementsは_N個(int count)
    {
        Assert.IsNotNull(_syntaxTree);
        var document = (DocumentSyntax)_syntaxTree.Root;
        var header = document.Header;
        Assert.IsNotNull(header);
        Assert.IsNotNull(header.Title);
        Assert.HasCount(count, header.Title.InlineElements);
    }

    private void ドキュメントタイトルの最初のインライン要素のテキストは(string expectedText)
    {
        Assert.IsNotNull(_syntaxTree);
        var document = (DocumentSyntax)_syntaxTree.Root;
        var header = document.Header;
        Assert.IsNotNull(header);
        Assert.IsNotNull(header.Title);
        Assert.IsTrue(header.Title.InlineElements.Count > 0);

        var inlineText = header.Title.InlineElements[0] as InlineTextSyntax;
        Assert.IsNotNull(inlineText);
        Assert.AreEqual(expectedText, inlineText.Text);
    }

    private void 最初のセクションタイトルのInlineElementsは構文上の出現順に並んでいる()
    {
        var title = GetFirstSectionTitle();
        Assert.IsNotNull(title);

        var elements = title.InlineElements;
        for (int i = 1; i < elements.Count; i++)
        {
            Assert.IsTrue(elements[i - 1].Position <= elements[i].Position,
                $"要素 {i - 1} (Position={elements[i - 1].Position}) が要素 {i} (Position={elements[i].Position}) より後に配置されています");
        }
    }

    private void セクションNのタイトルの最初のインライン要素のテキストは(int sectionIndex, string expectedText)
    {
        Assert.IsNotNull(_syntaxTree);
        var sections = _syntaxTree.Root.DescendantNodes().OfType<SectionSyntax>().ToArray();
        Assert.IsTrue(sectionIndex > 0 && sectionIndex <= sections.Length,
            $"セクション {sectionIndex} が存在しません");

        var section = sections[sectionIndex - 1];
        Assert.IsTrue(section.Title.InlineElements.Count > 0);

        var inlineText = section.Title.InlineElements[0] as InlineTextSyntax;
        Assert.IsNotNull(inlineText);
        Assert.AreEqual(expectedText, inlineText.Text);
    }

    private SectionTitleSyntax? GetFirstSectionTitle()
    {
        Assert.IsNotNull(_syntaxTree);

        // ヘッダーのタイトルをチェック
        var document = (DocumentSyntax)_syntaxTree.Root;
        var header = document.Header;
        if (header?.Title != null)
        {
            return header.Title;
        }

        // 最初のセクションのタイトルを取得
        var firstSection = _syntaxTree.Root.DescendantNodes().OfType<SectionSyntax>().FirstOrDefault();
        return firstSection?.Title;
    }
}
