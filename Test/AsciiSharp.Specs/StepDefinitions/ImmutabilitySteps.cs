
using System.Collections.Generic;
using System.Linq;

using AsciiSharp.Syntax;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reqnroll;

namespace AsciiSharp.Specs.StepDefinitions;

/// <summary>
/// 構文木の不変性とクエリ機能のステップ定義。
/// </summary>
[Binding]
public sealed class ImmutabilitySteps
{
    private readonly BasicParsingSteps _basicParsingSteps;

    private SyntaxTree? _originalSyntaxTree;
    private SyntaxTree? _modifiedSyntaxTree;
    private List<SyntaxNode>? _queriedNodes;

    /// <summary>
    /// コンストラクタ。依存するステップ定義を注入。
    /// </summary>
    /// <param name="basicParsingSteps">基本パーサーのステップ定義。</param>
    public ImmutabilitySteps(BasicParsingSteps basicParsingSteps)
    {
        this._basicParsingSteps = basicParsingSteps;
    }

    [When(@"元の構文木への参照を保持する")]
    public void When元の構文木への参照を保持する()
    {
        this._originalSyntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(this._originalSyntaxTree, "構文木が null です。");
    }

    [When(@"段落ノードを新しいテキスト ""(.+)"" で置換する")]
    public void When段落ノードを新しいテキストで置換する(string newText)
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree, "構文木が null です。");

        var document = syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        // 最初の段落を取得
        var paragraph = document.DescendantNodes()
            .OfType<ParagraphSyntax>()
            .FirstOrDefault();
        Assert.IsNotNull(paragraph, "段落ノードが見つかりません。");

        // 新しい段落を作成して置換
        var newParagraph = CreateParagraphWithText(newText);
        var newRoot = document.ReplaceNode(paragraph, newParagraph);

        // 新しい構文木を作成
        this._modifiedSyntaxTree = syntaxTree.WithRootAndOptions((DocumentSyntax)newRoot);
    }

    [When(@"すべての Section ノードをクエリする")]
    public void WhenすべてのSectionノードをクエリする()
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree, "構文木が null です。");

        this._queriedNodes = [.. syntaxTree.Root.DescendantNodes()
            .Where(n => n.Kind == SyntaxKind.Section)];
    }

    [When(@"別の変数で構文木を変更する")]
    public void When別の変数で構文木を変更する()
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree, "構文木が null です。");

        var document = syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        // 段落を取得して置換を試みる
        var paragraph = document.DescendantNodes()
            .OfType<ParagraphSyntax>()
            .FirstOrDefault();

        if (paragraph != null)
        {
            var newParagraph = CreateParagraphWithText("変更後のテキスト");
            var newRoot = document.ReplaceNode(paragraph, newParagraph);
            this._modifiedSyntaxTree = syntaxTree.WithRootAndOptions((DocumentSyntax)newRoot);
        }
    }

    [Then(@"元の構文木の段落テキストは ""(.+)"" である")]
    public void Then元の構文木の段落テキストはである(string expectedText)
    {
        Assert.IsNotNull(this._originalSyntaxTree, "元の構文木が保持されていません。");

        var document = this._originalSyntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        var paragraph = document.DescendantNodes()
            .OfType<ParagraphSyntax>()
            .FirstOrDefault();
        Assert.IsNotNull(paragraph, "段落ノードが見つかりません。");

        var actualText = GetParagraphText(paragraph);
        Assert.AreEqual(expectedText, actualText, $"元の構文木の段落テキストが一致しません。期待: '{expectedText}', 実際: '{actualText}'");
    }

    [Then(@"新しい構文木の段落テキストは ""(.+)"" である")]
    public void Then新しい構文木の段落テキストはである(string expectedText)
    {
        Assert.IsNotNull(this._modifiedSyntaxTree, "新しい構文木が作成されていません。");

        var document = this._modifiedSyntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        var paragraph = document.DescendantNodes()
            .OfType<ParagraphSyntax>()
            .FirstOrDefault();
        Assert.IsNotNull(paragraph, "段落ノードが見つかりません。");

        var actualText = GetParagraphText(paragraph);
        Assert.AreEqual(expectedText, actualText, $"新しい構文木の段落テキストが一致しません。期待: '{expectedText}', 実際: '{actualText}'");
    }

    [Then(@"(\d+) 個の Section ノードが返される")]
    public void Then個のSectionノードが返される(int expectedCount)
    {
        Assert.IsNotNull(this._queriedNodes, "クエリ結果が null です。");
        Assert.HasCount(expectedCount, this._queriedNodes, $"Section ノードの数が一致しません。期待: {expectedCount}, 実際: {this._queriedNodes.Count}");
    }

    [Then(@"クエリ後も構文木は変更されていない")]
    public void Thenクエリ後も構文木は変更されていない()
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree, "構文木が null です。");

        // 構文木から再度テキストを取得し、元のソーステキストと一致することを確認
        var reconstructed = syntaxTree.Root.ToFullString();
        Assert.AreEqual(this._basicParsingSteps.CurrentSourceText, reconstructed, "クエリ後に構文木が変更されています。");
    }

    [Then(@"元の参照が指す構文木は影響を受けない")]
    public void Then元の参照が指す構文木は影響を受けない()
    {
        Assert.IsNotNull(this._originalSyntaxTree, "元の構文木が保持されていません。");
        Assert.IsNotNull(this._modifiedSyntaxTree, "変更された構文木が作成されていません。");

        // 元の構文木と変更された構文木が異なるインスタンスであることを確認
        Assert.AreNotSame(this._originalSyntaxTree, this._modifiedSyntaxTree, "元の構文木と変更された構文木が同じインスタンスです。");

        // 元の構文木のルートと変更された構文木のルートが異なることを確認
        Assert.AreNotSame(this._originalSyntaxTree.Root, this._modifiedSyntaxTree.Root, "元の構文木のルートと変更された構文木のルートが同じインスタンスです。");
    }

    [Then(@"元の参照の段落テキストは ""(.+)"" のままである")]
    public void Then元の参照の段落テキストはのままである(string expectedText)
    {
        this.Then元の構文木の段落テキストはである(expectedText);
    }

    /// <summary>
    /// 指定されたテキストを持つ新しい段落を作成する。
    /// </summary>
    /// <param name="text">段落のテキスト。</param>
    /// <returns>新しい段落構文ノード。</returns>
    private static ParagraphSyntax CreateParagraphWithText(string text)
    {
        // 新しいテキストで文書を解析して段落を取得
        var tempTree = SyntaxTree.ParseText($"= Temp\n\n{text}\n");
        var tempDocument = tempTree.Root as DocumentSyntax;
        var newParagraph = tempDocument?.DescendantNodes()
            .OfType<ParagraphSyntax>()
            .FirstOrDefault();

        Assert.IsNotNull(newParagraph, "新しい段落の作成に失敗しました。");
        return newParagraph;
    }

    /// <summary>
    /// 段落からテキストを取得する。
    /// </summary>
    /// <param name="paragraph">段落構文ノード。</param>
    /// <returns>段落のテキスト（トリビアを除く）。</returns>
    private static string GetParagraphText(ParagraphSyntax paragraph)
    {
        // 段落のテキストを取得（トリビアを除く）
        return paragraph.ToString().Trim();
    }
}
