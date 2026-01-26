
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
            // トークンのテキストのみを収集（トリビアは除外）
            // コメントは Trivia として付加されるため、トークンテキストには含まれない
            var tokenTexts = paragraph.DescendantTokens().Select(t => t.Text);
            var paragraphText = string.Concat(tokenTexts);

            Assert.DoesNotContain(
                unexpectedText,
                paragraphText,
                $"段落のトークンテキストに '{unexpectedText}' が含まれています: '{paragraphText}'");
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

    [Then(@"構文木に ""(.*)"" を含むコメントがある")]
    public void Then構文木にを含むコメントがある(string expectedText)
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree, "構文木が null です。");

        // コメントトリビアを検索（Roslyn パターンではすべてのコメントは Trivia）
        var commentTrivia = syntaxTree.Root.DescendantTokens()
            .SelectMany(t => t.LeadingTrivia.Concat(t.TrailingTrivia))
            .Where(t => t.Kind is SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia)
            .ToList();

        var allCommentTexts = commentTrivia.Select(t => t.ToFullString()).ToList();

        var foundComment = allCommentTexts.Any(text => text.Contains(expectedText, System.StringComparison.Ordinal));

        Assert.IsTrue(
            foundComment,
            $"'{expectedText}' を含むコメントが見つかりませんでした。見つかったコメント: [{string.Join(", ", allCommentTexts.Select(t => $"'{t}'"))}]");
    }

    [Then(@"構文木に (\d+) 個の単一行コメントがある")]
    public void Then構文木に個の単一行コメントがある(int expectedCount)
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree, "構文木が null です。");

        // 単一行コメントトリビア（Roslyn パターンではすべてのコメントは Trivia）
        var commentTrivia = syntaxTree.Root.DescendantTokens()
            .SelectMany(t => t.LeadingTrivia.Concat(t.TrailingTrivia))
            .Where(t => t.Kind == SyntaxKind.SingleLineCommentTrivia)
            .ToList();

        Assert.HasCount(expectedCount, commentTrivia, $"単一行コメントの数が一致しません。");
    }

    [Then(@"構文木に (\d+) 個のブロックコメントがある")]
    public void Then構文木に個のブロックコメントがある(int expectedCount)
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree, "構文木が null です。");

        // ブロックコメントトリビア（Roslyn パターンではすべてのコメントは Trivia）
        var commentTrivia = syntaxTree.Root.DescendantTokens()
            .SelectMany(t => t.LeadingTrivia.Concat(t.TrailingTrivia))
            .Where(t => t.Kind == SyntaxKind.MultiLineCommentTrivia)
            .ToList();

        Assert.HasCount(expectedCount, commentTrivia, $"ブロックコメントの数が一致しません。");
    }
}
