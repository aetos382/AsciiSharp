
using System.Linq;

using AsciiSharp.Syntax;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reqnroll;

namespace AsciiSharp.Specs.StepDefinitions;

/// <summary>
/// SectionTitleSyntax のトリビアに関するステップ定義。
/// </summary>
[Binding]
public sealed class SectionTitleTriviaSteps
{
    private readonly BasicParsingSteps _basicParsingSteps;

    /// <summary>
    /// SectionTitleTriviaSteps を作成する。
    /// </summary>
    public SectionTitleTriviaSteps(BasicParsingSteps basicParsingSteps)
    {
        this._basicParsingSteps = basicParsingSteps;
    }

    [Then(@"セクションタイトルのマーカーは TrailingTrivia に空白を持つ")]
    public void ThenセクションタイトルのマーカーはTrailingTriviaに空白を持つ()
    {
        var tree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(tree, "構文木が null です。");

        var document = tree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        var sectionTitle = this.GetFirstSectionTitle(document);
        Assert.IsNotNull(sectionTitle, "セクションタイトルが見つかりません。");
        Assert.IsNotNull(sectionTitle.Marker, "セクションタイトルのマーカーが null です。");

        // TODO: Phase 5 で TrailingTrivia プロパティを実装後に有効化
        // var hasWhitespaceTrivia = sectionTitle.Marker.TrailingTrivia
        //     .Any(t => t.Kind == SyntaxKind.WhitespaceTrivia);
        // Assert.IsTrue(hasWhitespaceTrivia,
        //     "セクションタイトルのマーカーの TrailingTrivia に空白がありません。");

        // Phase 5 まで失敗させる
        Assert.Fail("TrailingTrivia プロパティは Phase 5 で実装予定です。");
    }

    /// <summary>
    /// 最初のセクションタイトルを取得する。
    /// </summary>
    private SectionTitleSyntax? GetFirstSectionTitle(DocumentSyntax document)
    {
        // まずヘッダーのタイトルを確認
        if (document.Header?.Title is not null)
        {
            return document.Header.Title;
        }

        // ボディの最初のセクションのタイトルを確認
        var firstSection = document.Body?.ChildNodesAndTokens()
            .Where(c => c.IsNode && c.AsNode()?.Kind == SyntaxKind.Section)
            .Select(c => c.AsNode() as SectionSyntax)
            .FirstOrDefault();

        return firstSection?.Title;
    }
}
