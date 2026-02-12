using System.Collections.Generic;
using System.Linq;

using AsciiSharp.Syntax;
using AsciiSharp.Text;

using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

/// <summary>
/// 構文木の不変性とクエリ機能のテスト
/// </summary>
[TestClass]
[FeatureDescription(
    @"構文木の不変性とクエリ
開発者として、
構文木を変更せずにクエリを行い、
必要に応じて元の構文木を保持したまま新しい構文木を作成したい")]
public sealed partial class ImmutabilityFeature : FeatureFixture
{
    private SourceText? _sourceText;
    private SyntaxTree? _syntaxTree;
    private SyntaxTree? _originalSyntaxTree;
    private SyntaxTree? _modifiedSyntaxTree;
    private IReadOnlyList<SyntaxNode>? _queriedNodes;

    [Scenario]
    public void ノード置換後も元の構文木は変更されない()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("段落テキスト\n"),
            when => 文書を解析する(),
            when => 元の構文木への参照を保持する(),
            when => 段落ノードを新しいテキストで置換する("新しい段落"),
            then => 元の構文木の段落テキストは("段落テキスト"),
            then => 新しい構文木の段落テキストは("新しい段落")
        );
    }

    [Scenario]
    public void 特定の種類のノードをクエリで検索()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("== セクション 1\n\n段落テキスト\n\n== セクション 2\n\n段落テキスト2\n"),
            when => 文書を解析する(),
            when => すべてのSectionノードをクエリする(),
            then => N個のSectionノードが返される(2),
            then => クエリ後も構文木は変更されていない()
        );
    }

    [Scenario]
    public void 別コンポーネントの変更は元の参照に影響しない()
    {
        Runner.RunScenario(
            given => 以下のAsciiDoc文書がある("段落テキスト\n"),
            when => 文書を解析する(),
            when => 元の構文木への参照を保持する(),
            when => 別の変数で構文木を変更する(),
            then => 元の参照が指す構文木は影響を受けない(),
            then => 元の参照の段落テキストは_のまま("段落テキスト")
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

    private void 元の構文木への参照を保持する()
    {
        Assert.IsNotNull(_syntaxTree);
        _originalSyntaxTree = _syntaxTree;
    }

    private void 段落ノードを新しいテキストで置換する(string newText)
    {
        Assert.IsNotNull(_syntaxTree);

        var paragraph = _syntaxTree.Root.DescendantNodes().OfType<ParagraphSyntax>().FirstOrDefault();
        Assert.IsNotNull(paragraph);

        var newParagraph = CreateParagraphWithText(newText);
        var newRoot = _syntaxTree.Root.ReplaceNode(paragraph, newParagraph);

        _modifiedSyntaxTree = _syntaxTree.WithRootAndOptions(newRoot);
    }

    private void すべてのSectionノードをクエリする()
    {
        Assert.IsNotNull(_syntaxTree);
        _queriedNodes = _syntaxTree.Root.DescendantNodes()
            .Where(n => n.Kind == SyntaxKind.Section)
            .ToList();
    }

    private void 別の変数で構文木を変更する()
    {
        Assert.IsNotNull(_syntaxTree);

        var paragraph = _syntaxTree.Root.DescendantNodes().OfType<ParagraphSyntax>().FirstOrDefault();
        Assert.IsNotNull(paragraph);

        var newParagraph = CreateParagraphWithText("変更されたテキスト");
        var newRoot = _syntaxTree.Root.ReplaceNode(paragraph, newParagraph);

        _modifiedSyntaxTree = _syntaxTree.WithRootAndOptions(newRoot);
    }

    private void 元の構文木の段落テキストは(string expectedText)
    {
        Assert.IsNotNull(_originalSyntaxTree);
        var paragraph = _originalSyntaxTree.Root.DescendantNodes().OfType<ParagraphSyntax>().FirstOrDefault();
        Assert.IsNotNull(paragraph);

        var actualText = GetParagraphText(paragraph);
        Assert.AreEqual(expectedText, actualText);
    }

    private void 新しい構文木の段落テキストは(string expectedText)
    {
        Assert.IsNotNull(_modifiedSyntaxTree);
        var paragraph = _modifiedSyntaxTree.Root.DescendantNodes().OfType<ParagraphSyntax>().FirstOrDefault();
        Assert.IsNotNull(paragraph);

        var actualText = GetParagraphText(paragraph);
        Assert.AreEqual(expectedText, actualText);
    }

    private void N個のSectionノードが返される(int expectedCount)
    {
        Assert.IsNotNull(_queriedNodes);
        Assert.HasCount(expectedCount, _queriedNodes);
    }

    private void クエリ後も構文木は変更されていない()
    {
        Assert.IsNotNull(_syntaxTree);
        Assert.IsNotNull(_sourceText);

        var reconstructed = _syntaxTree.Root.ToFullString();
        var original = _sourceText.ToString();

        Assert.AreEqual(original, reconstructed);
    }

    private void 元の参照が指す構文木は影響を受けない()
    {
        Assert.IsNotNull(_originalSyntaxTree);
        Assert.IsNotNull(_modifiedSyntaxTree);
        Assert.AreNotSame(_originalSyntaxTree, _modifiedSyntaxTree);
    }

    private void 元の参照の段落テキストは_のまま(string expectedText)
    {
        Assert.IsNotNull(_originalSyntaxTree);
        var paragraph = _originalSyntaxTree.Root.DescendantNodes().OfType<ParagraphSyntax>().FirstOrDefault();
        Assert.IsNotNull(paragraph);

        var actualText = GetParagraphText(paragraph);
        Assert.AreEqual(expectedText, actualText);
    }

    private static ParagraphSyntax CreateParagraphWithText(string text)
    {
        var tempSource = SourceText.From($"{text}\n");
        var tempTree = SyntaxTree.ParseText(tempSource);
        var paragraph = tempTree.Root.DescendantNodes().OfType<ParagraphSyntax>().FirstOrDefault();
        Assert.IsNotNull(paragraph);
        return paragraph;
    }

    private static string GetParagraphText(ParagraphSyntax paragraph)
    {
        return paragraph.ToString().Trim();
    }
}
