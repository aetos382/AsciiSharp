using AsciiSharp.Parser;
using AsciiSharp.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Tests;

/// <summary>
/// Lexer のトークン生成に関するテスト。
/// </summary>
[TestClass]
public class LexerTests
{
    [TestMethod]
    public void 単一のイコールが1つのEqualsTokenになる()
    {
        var source = SourceText.From("=");
        var lexer = new Lexer(source);

        var token = lexer.NextToken();

        Assert.AreEqual(SyntaxKind.EqualsToken, token.Kind);
        Assert.AreEqual("=", token.Text);
    }

    [TestMethod]
    public void 連続する2つのイコールが単一のEqualsTokenにまとめられる()
    {
        var source = SourceText.From("==");
        var lexer = new Lexer(source);

        var token = lexer.NextToken();

        Assert.AreEqual(SyntaxKind.EqualsToken, token.Kind);
        Assert.AreEqual("==", token.Text);
    }

    [TestMethod]
    public void 連続する6つのイコールが単一のEqualsTokenにまとめられる()
    {
        var source = SourceText.From("======");
        var lexer = new Lexer(source);

        var token = lexer.NextToken();

        Assert.AreEqual(SyntaxKind.EqualsToken, token.Kind);
        Assert.AreEqual("======", token.Text);
    }

    [TestMethod]
    public void 連続する7つ以上のイコールが単一のEqualsTokenにまとめられる()
    {
        var source = SourceText.From("=======");
        var lexer = new Lexer(source);

        var token = lexer.NextToken();

        Assert.AreEqual(SyntaxKind.EqualsToken, token.Kind);
        Assert.AreEqual("=======", token.Text);
    }

    [TestMethod]
    public void イコールの後に空白が続く場合は別のトークンになる()
    {
        var source = SourceText.From("== ");
        var lexer = new Lexer(source);

        var token1 = lexer.NextToken();
        var token2 = lexer.NextToken();

        Assert.AreEqual(SyntaxKind.EqualsToken, token1.Kind);
        Assert.AreEqual("==", token1.Text);
        Assert.AreEqual(SyntaxKind.WhitespaceToken, token2.Kind);
    }

    [TestMethod]
    public void イコールの後にテキストが続く場合は別のトークンになる()
    {
        var source = SourceText.From("==abc");
        var lexer = new Lexer(source);

        var token1 = lexer.NextToken();
        var token2 = lexer.NextToken();

        Assert.AreEqual(SyntaxKind.EqualsToken, token1.Kind);
        Assert.AreEqual("==", token1.Text);
        Assert.AreEqual(SyntaxKind.TextToken, token2.Kind);
        Assert.AreEqual("abc", token2.Text);
    }
}
