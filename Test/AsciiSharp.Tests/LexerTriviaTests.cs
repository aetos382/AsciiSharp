using System.Linq;

using AsciiSharp;
using AsciiSharp.Parser;
using AsciiSharp.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Tests;

/// <summary>
/// Lexer のトリビア処理のテスト。
/// </summary>
[TestClass]
public class LexerTriviaTests
{
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
        Assert.IsTrue(leadingTrivia.Count >= 1, $"コメントトリビアがあるべき。実際: {leadingTrivia.Count}個のトリビア");

        // コメントトリビアを探す
        var hasComment = false;
        foreach (var trivia in leadingTrivia)
        {
            System.Console.WriteLine($"Trivia: Kind={trivia.Kind}, Text='{trivia.Text}'");
            if (trivia.Kind == SyntaxKind.SingleLineCommentTrivia)
            {
                hasComment = true;
            }
        }

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
        Assert.IsTrue(leadingTrivia.Count >= 1, $"コメントトリビアがあるべき。実際: {leadingTrivia.Count}個のトリビア");

        // コメントトリビアを探す
        var hasComment = false;
        foreach (var trivia in leadingTrivia)
        {
            System.Console.WriteLine($"Trivia: Kind={trivia.Kind}, Text='{trivia.Text}'");
            if (trivia.Kind == SyntaxKind.SingleLineCommentTrivia)
            {
                hasComment = true;
            }
        }

        Assert.IsTrue(hasComment, "SingleLineCommentTrivia が存在するべき");
    }

    [TestMethod]
    public void 構文木を経由してもコメントトリビアが保持される()
    {
        // Arrange: タイトル + 空行 + コメント + テキスト
        var text = "= タイトル\n\n// これはコメント\n本文テキスト";
        var tree = SyntaxTree.Parse(text);

        // Act: 全トークンからトリビアを収集
        var allTrivia = tree.Root.DescendantTokens()
            .SelectMany(t => t.LeadingTrivia.Concat(t.TrailingTrivia))
            .ToList();

        // デバッグ出力
        System.Console.WriteLine($"トークン数: {tree.Root.DescendantTokens().Count()}");
        foreach (var token in tree.Root.DescendantTokens())
        {
            System.Console.WriteLine($"Token: Kind={token.Kind}, Text='{token.Text}', LeadingTrivia.Count={token.LeadingTrivia.Count}");
            foreach (var trivia in token.LeadingTrivia)
            {
                System.Console.WriteLine($"  LeadingTrivia: Kind={trivia.Kind}, Text='{trivia.ToFullString()}'");
            }
        }

        System.Console.WriteLine($"全トリビア数: {allTrivia.Count}");
        foreach (var trivia in allTrivia)
        {
            System.Console.WriteLine($"Trivia: Kind={trivia.Kind}, Text='{trivia.ToFullString()}'");
        }

        // Assert: コメントトリビアがあるべき
        var commentTrivia = allTrivia
            .Where(t => t.Kind == SyntaxKind.SingleLineCommentTrivia)
            .ToList();

        Assert.IsTrue(commentTrivia.Count >= 1, $"SingleLineCommentTrivia が存在するべき。見つかったトリビア: {allTrivia.Count}個");
    }
}
