using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Tests;

[TestClass]
public class BodyOnlyTest
{
    [TestMethod]
    public void BodyOnlyInput_ShouldParseCorrectly()
    {
        var input = "body only";
        var tree = SyntaxTree.Parse(input.AsSpan());

        Assert.IsNotNull(tree.Root);
        Assert.AreEqual(SyntaxNodeKind.Document, tree.Root.Kind);
        Assert.AreEqual(SyntaxNodeType.Block, tree.Root.NodeType);
        Assert.AreEqual(1, tree.Root.Blocks.Count);

        var paragraph = tree.Root.Blocks[0] as Paragraph;
        Assert.IsNotNull(paragraph);
        Assert.AreEqual(SyntaxNodeKind.Paragraph, paragraph.Kind);
        Assert.AreEqual(SyntaxNodeType.Block, paragraph.NodeType);
        Assert.AreEqual(1, paragraph.Inlines.Count);

        var text = paragraph.Inlines[0] as Text;
        Assert.IsNotNull(text);
        Assert.AreEqual(SyntaxNodeKind.Text, text.Kind);
        Assert.AreEqual(SyntaxNodeType.String, text.NodeType);
        Assert.AreEqual("body only", text.Value);

        var expectedLocation = new Location(new Position(1, 1), new Position(1, 9));
        Assert.AreEqual(expectedLocation, text.Location);
        Assert.AreEqual(expectedLocation, paragraph.Location);
        Assert.AreEqual(expectedLocation, tree.Root.Location);
    }
}