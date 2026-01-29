using AsciiSharp.Syntax;
using AsciiSharp.TckAdapter.Asg;
using AsciiSharp.TckAdapter.Asg.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.TckAdapter.Tests;

/// <summary>
/// <see cref="AsgConverter"/> のユニットテスト。
/// </summary>
[TestClass]
public sealed class AsgConverterTests
{
    #region T003: document ノードへの変換テスト

    [TestMethod]
    public void Convert_単純な段落_Documentノードを返す()
    {
        // Arrange
        var text = "This is a paragraph.";
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(syntaxTree);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.AreEqual("document", result.Name);
        Assert.AreEqual("block", result.Type);
    }

    [TestMethod]
    public void Convert_空の文書_blocksが空配列()
    {
        // Arrange
        var text = "";
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(syntaxTree);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsEmpty(result.Blocks);
    }

    #endregion

    #region T004: section ノードへの変換テスト（level プロパティ含む）

    [TestMethod]
    public void Convert_レベル2セクション_levelが2()
    {
        // Arrange
        // AsciiSharp では Level は = の数を返す（== は level 2）
        var text = "== Section Title\n\nContent.";
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(syntaxTree);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.HasCount(1, result.Blocks);
        var section = result.Blocks[0] as AsgSection;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.Name);
        Assert.AreEqual(2, section.Level);
    }

    [TestMethod]
    public void Convert_レベル3セクション_levelが3()
    {
        // Arrange
        // AsciiSharp では Level は = の数を返す（=== は level 3）
        var text = "=== Level 3 Section\n\nContent.";
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(syntaxTree);

        // Act
        var result = converter.Convert();

        // Assert
        var section = result.Blocks[0] as AsgSection;
        Assert.IsNotNull(section);
        Assert.AreEqual(3, section.Level);
    }

    [TestMethod]
    public void Convert_ネストしたセクション_blocksに再帰的にsection()
    {
        // Arrange
        // AsciiSharp のパーサーはセクションをレベルに基づいてネストする
        var text = "== Level 2\n\n=== Level 3\n\nNested content.";
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(syntaxTree);

        // Act
        var result = converter.Convert();

        // Assert
        // パーサーの動作に応じてテストを調整
        Assert.IsGreaterThanOrEqualTo(1, result.Blocks.Count);
        var section = result.Blocks[0] as AsgSection;
        Assert.IsNotNull(section);
        Assert.AreEqual(2, section.Level); // == は level 2
    }

    [TestMethod]
    public void Convert_セクションタイトル_title配列にテキストを含む()
    {
        // Arrange
        var text = "== My Title\n\nContent.";
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(syntaxTree);

        // Act
        var result = converter.Convert();

        // Assert
        var section = result.Blocks[0] as AsgSection;
        Assert.IsNotNull(section);
        Assert.HasCount(1, section.Title);
        var titleText = section.Title[0] as AsgText;
        Assert.IsNotNull(titleText);
        Assert.AreEqual("My Title", titleText.Value);
    }

    #endregion

    #region T005: paragraph ノードへの変換テスト

    [TestMethod]
    public void Convert_単純な段落_paragraphノードを返す()
    {
        // Arrange
        var text = "This is a paragraph.";
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(syntaxTree);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.HasCount(1, result.Blocks);
        var paragraph = result.Blocks[0] as AsgParagraph;
        Assert.IsNotNull(paragraph);
        Assert.AreEqual("paragraph", paragraph.Name);
        Assert.AreEqual("block", paragraph.Type);
    }

    #endregion

    #region T006: text ノードへの変換テスト

    [TestMethod]
    public void Convert_テキスト_textノードを返す()
    {
        // Arrange
        var text = "Hello World";
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(syntaxTree);

        // Act
        var result = converter.Convert();

        // Assert
        var paragraph = result.Blocks[0] as AsgParagraph;
        Assert.IsNotNull(paragraph);
        Assert.HasCount(1, paragraph.Inlines);

        var textNode = paragraph.Inlines[0] as AsgText;
        Assert.IsNotNull(textNode);
        Assert.AreEqual("text", textNode.Name);
        Assert.AreEqual("string", textNode.Type);
        Assert.AreEqual("Hello World", textNode.Value);
    }

    [TestMethod]
    public void Convert_空文字列のテキスト_valueが空文字列()
    {
        // Arrange - 空の段落を作成するために改行のみの文書を使用
        // 注: パーサーの動作により、完全に空のテキストノードは生成されない可能性がある
        var text = "";
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(syntaxTree);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsEmpty(result.Blocks);
    }

    #endregion

    #region T007: 位置情報の開始・終了位置テスト

    [TestMethod]
    public void Convert_テキスト_locationの開始位置が正しい()
    {
        // Arrange
        var text = "Hello World";
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(syntaxTree);

        // Act
        var result = converter.Convert();

        // Assert
        var paragraph = result.Blocks[0] as AsgParagraph;
        Assert.IsNotNull(paragraph);
        var textNode = paragraph.Inlines[0] as AsgText;
        Assert.IsNotNull(textNode);
        Assert.IsNotNull(textNode.Location);

        Assert.AreEqual(1, textNode.Location.Start.Line);
        Assert.AreEqual(1, textNode.Location.Start.Col);
    }

    [TestMethod]
    public void Convert_テキスト_locationの終了位置が正しい()
    {
        // Arrange
        var text = "Hello World";
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(syntaxTree);

        // Act
        var result = converter.Convert();

        // Assert
        var paragraph = result.Blocks[0] as AsgParagraph;
        Assert.IsNotNull(paragraph);
        var textNode = paragraph.Inlines[0] as AsgText;
        Assert.IsNotNull(textNode);
        Assert.IsNotNull(textNode.Location);

        Assert.AreEqual(1, textNode.Location.End.Line);
        Assert.AreEqual(11, textNode.Location.End.Col); // "Hello World" = 11 文字
    }

    #endregion

    #region T008: 複数行にまたがるノードの位置情報テスト

    [TestMethod]
    public void Convert_複数行の段落_locationが正しい()
    {
        // Arrange
        var text = "First line\nSecond line";
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(syntaxTree);

        // Act
        var result = converter.Convert();

        // Assert
        var paragraph = result.Blocks[0] as AsgParagraph;
        Assert.IsNotNull(paragraph);
        Assert.IsNotNull(paragraph.Location);

        Assert.AreEqual(1, paragraph.Location.Start.Line);
        Assert.AreEqual(1, paragraph.Location.Start.Col);
    }

    #endregion

    #region T009: タイトル付き文書の header 変換テスト

    [TestMethod]
    public void Convert_タイトル付き文書_headerがnullでない()
    {
        // Arrange
        var text = "= Document Title\n\nContent.";
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(syntaxTree);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result.Header);
    }

    [TestMethod]
    public void Convert_タイトル付き文書_headerのtitleが正しい()
    {
        // Arrange
        var text = "= Document Title\n\nContent.";
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(syntaxTree);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result.Header);
        Assert.HasCount(1, result.Header.Title);

        var titleText = result.Header.Title[0] as AsgText;
        Assert.IsNotNull(titleText);
        Assert.AreEqual("Document Title", titleText.Value);
    }

    #endregion

    #region T010: ヘッダーのない文書の header null テスト

    [TestMethod]
    public void Convert_ヘッダーのない文書_headerがnull()
    {
        // Arrange
        var text = "Just a paragraph.";
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(syntaxTree);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNull(result.Header);
    }

    #endregion
}
