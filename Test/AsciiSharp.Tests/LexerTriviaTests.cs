using System.Linq;

using AsciiSharp;
using AsciiSharp.Parser;
using AsciiSharp.Syntax;
using AsciiSharp.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Tests;

/// <summary>
/// Lexer のトリビア処理のテスト。
/// </summary>
[TestClass]
public class LexerTriviaTests
{
    private readonly TestContext _testContext;

    /// <summary>
    /// コンストラクタ。
    /// </summary>
    /// <param name="testContext">テストコンテキスト。</param>
    public LexerTriviaTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    [TestMethod]
    public void コメント行の後のテキストにはコメントトリビアが付加される()
    {
        // Arrange: 空行、コメント、テキスト
        var text = "\n// これはコメント\n本文テキスト";
        var source = SourceText.From(text);
        var lexer = new Lexer(source);

        // Act: トークンを取得
        var token1 = lexer.NextToken(); // \n (blank line)
        var token2 = lexer.NextToken(); // 本文テキスト (with comment trivia)

        // Assert
        Assert.AreEqual(SyntaxKind.NewLineToken, token1.Kind, "最初のトークンは改行");
        Assert.AreEqual(SyntaxKind.TextToken, token2.Kind, "2番目のトークンはテキスト");

        var leadingTrivia = token2.LeadingTrivia;
        Assert.IsGreaterThanOrEqualTo(1, leadingTrivia.Count, $"コメントトリビアがあるべき。実際: {leadingTrivia.Count}個のトリビア");

        // コメントトリビアを探す
        var hasComment = leadingTrivia.Any(t => t.Kind == SyntaxKind.SingleLineCommentTrivia);

        Assert.IsTrue(hasComment, "SingleLineCommentTrivia が存在するべき");
    }

    [TestMethod]
    public void 文書冒頭のコメントはトリビアとして付加される()
    {
        // Arrange: 冒頭コメント、テキスト
        var text = "// 冒頭のコメント\n本文テキスト";
        var source = SourceText.From(text);
        var lexer = new Lexer(source);

        // Act: トークンを取得
        var token = lexer.NextToken(); // 本文テキスト (with comment trivia)

        // Assert
        Assert.AreEqual(SyntaxKind.TextToken, token.Kind, "トークンはテキスト");

        var leadingTrivia = token.LeadingTrivia;
        Assert.IsGreaterThanOrEqualTo(1, leadingTrivia.Count, $"コメントトリビアがあるべき。実際: {leadingTrivia.Count}個のトリビア");

        // コメントトリビアを探す
        var hasComment = leadingTrivia.Any(t => t.Kind == SyntaxKind.SingleLineCommentTrivia);

        Assert.IsTrue(hasComment, "SingleLineCommentTrivia が存在するべき");
    }

    [TestMethod]
    public void 構文木を経由してもコメントトリビアが保持される()
    {
        // Arrange: タイトル + 空行 + コメント + テキスト
        var text = "= タイトル\n\n// これはコメント\n本文テキスト";
        var tree = SyntaxTree.ParseText(text);

        // Act: 全トークンからトリビアを収集
        var allTrivia = tree.Root.DescendantTokens()
            .SelectMany(t => t.LeadingTrivia.Concat(t.TrailingTrivia))
            .ToList();

        // Assert: コメントトリビアがあるべき
        var commentTrivia = allTrivia
            .Where(t => t.Kind == SyntaxKind.SingleLineCommentTrivia)
            .ToList();

        // デバッグ出力
        this._testContext.WriteLine($"全トリビア数: {allTrivia.Count}");
        foreach (var trivia in allTrivia)
        {
            this._testContext.WriteLine($"Trivia: Kind={trivia.Kind}, Text='{trivia.ToFullString()}'");
        }

        Assert.IsGreaterThanOrEqualTo(1, commentTrivia.Count, $"SingleLineCommentTrivia が存在するべき。見つかったトリビア: {allTrivia.Count}個");
    }
}
