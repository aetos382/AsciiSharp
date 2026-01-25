
using System.Linq;

using AsciiSharp.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reqnroll;

namespace AsciiSharp.Specs.StepDefinitions;

/// <summary>
/// コメント解析機能のステップ定義。
/// </summary>
[Binding]
public sealed class CommentParsingSteps
{
    private readonly BasicParsingSteps _basicParsingSteps;

    /// <summary>
    /// コンストラクタ。依存するステップ定義を注入。
    /// </summary>
    /// <param name="basicParsingSteps">基本パーサーのステップ定義。</param>
    public CommentParsingSteps(BasicParsingSteps basicParsingSteps)
    {
        this._basicParsingSteps = basicParsingSteps;
    }

    [Then(@"解析は成功する")]
    public void Then解析は成功する()
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree, "構文木が null です。");

        var errors = syntaxTree.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        Assert.IsEmpty(errors, $"解析エラーが発生しました: {string.Join(", ", errors.Select(e => e.Message))}");
    }

    [Then(@"段落のテキストは ""(.+)"" を含まない")]
    public void Then段落のテキストはを含まない(string unexpectedText)
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree, "構文木が null です。");

        var paragraphs = syntaxTree.Root.DescendantNodes()
            .Where(n => n.Kind == SyntaxKind.Paragraph)
            .ToList();

        foreach (var paragraph in paragraphs)
        {
            var paragraphText = paragraph.ToFullString();
            Assert.DoesNotContain(
                unexpectedText,
                paragraphText,
                $"段落のテキストに '{unexpectedText}' が含まれています: '{paragraphText}'");
        }
    }

    [Then(@"Document ノードはタイトル ""(.*)"" を持つ")]
    public void ThenDocumentノードはタイトルを持つ(string expectedTitle)
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree, "構文木が null です。");

        var document = syntaxTree.Root as Syntax.DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");
        Assert.IsNotNull(document.Header, "Document は Header を持つ必要があります。");

        var actualTitle = document.Header.Title?.TitleContent;
        Assert.AreEqual(expectedTitle, actualTitle, $"タイトルが一致しません。期待: '{expectedTitle}', 実際: '{actualTitle}'");
    }

    [Then(@"構文木のトリビアに ""(.*)"" が含まれる")]
    public void Then構文木のトリビアにが含まれる(string expectedText)
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree, "構文木が null です。");

        // すべてのトークンのトリビアを収集
        var allTrivia = syntaxTree.Root.DescendantTokens()
            .SelectMany(t => t.LeadingTrivia.Concat(t.TrailingTrivia))
            .ToList();

        var triviaTexts = allTrivia.Select(t => t.ToFullString()).ToList();
        var combinedTriviaText = string.Join(string.Empty, triviaTexts);

        Assert.IsTrue(
            combinedTriviaText.Contains(expectedText, System.StringComparison.Ordinal),
            $"トリビアに '{expectedText}' が含まれていません。トリビア全体: '{combinedTriviaText}'");
    }
}
