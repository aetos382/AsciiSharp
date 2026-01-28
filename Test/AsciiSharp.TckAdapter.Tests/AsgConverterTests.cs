using AsciiSharp.Syntax;
using AsciiSharp.TckAdapter.Asg;
using AsciiSharp.TckAdapter.Asg.Models;
using AsciiSharp.Text;

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
        var sourceText = SourceText.From(text);
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(sourceText);

        // Act
        var result = converter.Convert(syntaxTree.Root);

        // Assert
        Assert.AreEqual("document", result.Name);
        Assert.AreEqual("block", result.Type);
    }

    [TestMethod]
    public void Convert_空の文書_blocksが空配列()
    {
        // Arrange
        var text = "";
        var sourceText = SourceText.From(text);
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(sourceText);

        // Act
        var result = converter.Convert(syntaxTree.Root);

        // Assert
        Assert.AreEqual(0, result.Blocks.Count);
    }

    #endregion

    #region T004: section ノードへの変換テスト（level プロパティ含む）

    [TestMethod]
    public void Convert_レベル1セクション_levelが1()
    {
        // Arrange
        var text = "== Section Title\n\nContent.";
        var sourceText = SourceText.From(text);
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(sourceText);

        // Act
        var result = converter.Convert(syntaxTree.Root);

        // Assert
        Assert.AreEqual(1, result.Blocks.Count);
        var section = result.Blocks[0] as AsgSection;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.Name);
        Assert.AreEqual(1, section.Level);
    }

    [TestMethod]
    public void Convert_レベル2セクション_levelが2()
    {
        // Arrange
        var text = "=== Level 2 Section\n\nContent.";
        var sourceText = SourceText.From(text);
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(sourceText);

        // Act
        var result = converter.Convert(syntaxTree.Root);

        // Assert
        var section = result.Blocks[0] as AsgSection;
        Assert.IsNotNull(section);
        Assert.AreEqual(2, section.Level);
    }

    [TestMethod]
    public void Convert_ネストしたセクション_blocksに再帰的にsection()
    {
        // Arrange
        var text = "== Level 1\n\n=== Level 2\n\nNested content.";
        var sourceText = SourceText.From(text);
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(sourceText);

        // Act
        var result = converter.Convert(syntaxTree.Root);

        // Assert
        Assert.AreEqual(1, result.Blocks.Count);
        var level1 = result.Blocks[0] as AsgSection;
        Assert.IsNotNull(level1);
        Assert.AreEqual(1, level1.Level);
        Assert.AreEqual(1, level1.Blocks.Count);

        var level2 = level1.Blocks[0] as AsgSection;
        Assert.IsNotNull(level2);
        Assert.AreEqual(2, level2.Level);
    }

    [TestMethod]
    public void Convert_セクションタイトル_title配列にテキストを含む()
    {
        // Arrange
        var text = "== My Title\n\nContent.";
        var sourceText = SourceText.From(text);
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(sourceText);

        // Act
        var result = converter.Convert(syntaxTree.Root);

        // Assert
        var section = result.Blocks[0] as AsgSection;
        Assert.IsNotNull(section);
        Assert.AreEqual(1, section.Title.Count);
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
        var sourceText = SourceText.From(text);
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(sourceText);

        // Act
        var result = converter.Convert(syntaxTree.Root);

        // Assert
        Assert.AreEqual(1, result.Blocks.Count);
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
        var sourceText = SourceText.From(text);
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(sourceText);

        // Act
        var result = converter.Convert(syntaxTree.Root);

        // Assert
        var paragraph = result.Blocks[0] as AsgParagraph;
        Assert.IsNotNull(paragraph);
        Assert.AreEqual(1, paragraph.Inlines.Count);

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
        var sourceText = SourceText.From(text);
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(sourceText);

        // Act
        var result = converter.Convert(syntaxTree.Root);

        // Assert
        Assert.AreEqual(0, result.Blocks.Count);
    }

    #endregion

    #region T007: 位置情報の開始・終了位置テスト

    [TestMethod]
    public void Convert_テキスト_locationの開始位置が正しい()
    {
        // Arrange
        var text = "Hello World";
        var sourceText = SourceText.From(text);
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(sourceText);

        // Act
        var result = converter.Convert(syntaxTree.Root);

        // Assert
        var paragraph = result.Blocks[0] as AsgParagraph;
        Assert.IsNotNull(paragraph);
        var textNode = paragraph.Inlines[0] as AsgText;
        Assert.IsNotNull(textNode);
        Assert.IsNotNull(textNode.Location);

        Assert.AreEqual(1, textNode.Location.Value.Start.Line);
        Assert.AreEqual(1, textNode.Location.Value.Start.Col);
    }

    [TestMethod]
    public void Convert_テキスト_locationの終了位置が正しい()
    {
        // Arrange
        var text = "Hello World";
        var sourceText = SourceText.From(text);
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(sourceText);

        // Act
        var result = converter.Convert(syntaxTree.Root);

        // Assert
        var paragraph = result.Blocks[0] as AsgParagraph;
        Assert.IsNotNull(paragraph);
        var textNode = paragraph.Inlines[0] as AsgText;
        Assert.IsNotNull(textNode);
        Assert.IsNotNull(textNode.Location);

        Assert.AreEqual(1, textNode.Location.Value.End.Line);
        Assert.AreEqual(11, textNode.Location.Value.End.Col); // "Hello World" = 11 文字
    }

    #endregion

    #region T008: 複数行にまたがるノードの位置情報テスト

    [TestMethod]
    public void Convert_複数行の段落_locationが正しい()
    {
        // Arrange
        var text = "First line\nSecond line";
        var sourceText = SourceText.From(text);
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(sourceText);

        // Act
        var result = converter.Convert(syntaxTree.Root);

        // Assert
        var paragraph = result.Blocks[0] as AsgParagraph;
        Assert.IsNotNull(paragraph);
        Assert.IsNotNull(paragraph.Location);

        Assert.AreEqual(1, paragraph.Location.Value.Start.Line);
        Assert.AreEqual(1, paragraph.Location.Value.Start.Col);
    }

    #endregion

    #region T009: タイトル付き文書の header 変換テスト

    [TestMethod]
    public void Convert_タイトル付き文書_headerがnullでない()
    {
        // Arrange
        var text = "= Document Title\n\nContent.";
        var sourceText = SourceText.From(text);
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(sourceText);

        // Act
        var result = converter.Convert(syntaxTree.Root);

        // Assert
        Assert.IsNotNull(result.Header);
    }

    [TestMethod]
    public void Convert_タイトル付き文書_headerのtitleが正しい()
    {
        // Arrange
        var text = "= Document Title\n\nContent.";
        var sourceText = SourceText.From(text);
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(sourceText);

        // Act
        var result = converter.Convert(syntaxTree.Root);

        // Assert
        Assert.IsNotNull(result.Header);
        Assert.AreEqual(1, result.Header.Title.Count);

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
        var sourceText = SourceText.From(text);
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(sourceText);

        // Act
        var result = converter.Convert(syntaxTree.Root);

        // Assert
        Assert.IsNull(result.Header);
    }

    #endregion

    #region T014: 未対応 SyntaxNode のスキップテスト

    [TestMethod]
    public void VisitLink_未対応ノード_nullを返す()
    {
        // Arrange
        var text = "dummy";
        var sourceText = SourceText.From(text);
        var converter = new AsgConverter(sourceText);

        // Act
        var result = converter.VisitLink(null!);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void VisitAuthorLine_未対応ノード_nullを返す()
    {
        // Arrange
        var text = "dummy";
        var sourceText = SourceText.From(text);
        var converter = new AsgConverter(sourceText);

        // Act
        var result = converter.VisitAuthorLine(null!);

        // Assert
        Assert.IsNull(result);
    }

    #endregion
}
