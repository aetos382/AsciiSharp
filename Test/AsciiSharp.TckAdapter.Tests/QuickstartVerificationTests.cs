using System;
using System.Text.Json;

using AsciiSharp.Syntax;
using AsciiSharp.TckAdapter.Asg;
using AsciiSharp.TckAdapter.Asg.Serialization;

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
        var syntaxTree = SyntaxTree.ParseText(text);

        // Act
        var converter = new AsgConverter(syntaxTree);
        var asgDocument = converter.Convert();

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
        var syntaxTree = SyntaxTree.ParseText(text);
        var converter = new AsgConverter(syntaxTree);
        var asgDocument = converter.Convert();

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
