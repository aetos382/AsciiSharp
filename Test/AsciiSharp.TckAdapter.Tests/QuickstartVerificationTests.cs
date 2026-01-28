using System;
using System.Text.Json;

using AsciiSharp.Syntax;
using AsciiSharp.TckAdapter.Asg;
using AsciiSharp.TckAdapter.Asg.Models;
using AsciiSharp.TckAdapter.Asg.Serialization;
using AsciiSharp.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.TckAdapter.Tests;

/// <summary>
/// quickstart.md のサンプルコードが正しく動作することを検証するテスト。
/// </summary>
[TestClass]
public sealed class QuickstartVerificationTests
{
    [TestMethod]
    public void Quickstart_サンプルコードが動作する()
    {
        // Arrange - quickstart.md サンプル 1
        var text = "= Hello World\n\nThis is a paragraph.";
        var sourceText = SourceText.From(text);
        var syntaxTree = SyntaxTree.ParseText(text);
        var document = (DocumentSyntax)syntaxTree.Root;

        // Act
        var converter = new AsgConverter(sourceText);
        var asgDocument = converter.Convert(document);

        // Assert - 基本的な検証
        Assert.AreEqual("document", asgDocument.Name);
        Assert.IsNotNull(asgDocument.Header);
        Assert.HasCount(1, asgDocument.Blocks);
    }

    [TestMethod]
    public void Quickstart_JSONシリアライズが動作する()
    {
        // Arrange
        var text = "= Hello World\n\nThis is a paragraph.";
        var sourceText = SourceText.From(text);
        var syntaxTree = SyntaxTree.ParseText(text);
        var document = (DocumentSyntax)syntaxTree.Root;
        var converter = new AsgConverter(sourceText);
        var asgDocument = converter.Convert(document);

        // Act - quickstart.md サンプル 2（シリアライズが例外なく実行できることを確認）
        var json = JsonSerializer.Serialize(
            asgDocument,
            AsgJsonContext.Default.AsgDocument);

        // Assert - JSON が生成されることを確認
        Assert.IsNotNull(json);
        Assert.IsGreaterThan(0, json.Length, "JSON should not be empty");

        // document ノード情報が含まれていることを確認
        Assert.IsTrue(json.Contains("document", StringComparison.Ordinal), "JSON should contain 'document'");
        Assert.IsTrue(json.Contains("header", StringComparison.Ordinal), "JSON should contain 'header'");
        Assert.IsTrue(json.Contains("blocks", StringComparison.Ordinal), "JSON should contain 'blocks'");
    }
}
