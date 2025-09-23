using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll;

namespace AsciiSharp.Specs.StepDefinitions;

[Binding]
public sealed class ParserStepDefinitions
{
    private string _inputDocument = string.Empty;
    private SyntaxTree? _parsedTree;

    [Given(@"AsciiDoc文書 ""(.*)"" が与えられている")]
    public void AsciiDoc文書が与えられている(string document)
    {
        _inputDocument = document;
    }

    [Given(@"空のAsciiDoc文書が与えられている")]
    public void 空のAsciiDoc文書が与えられている()
    {
        _inputDocument = string.Empty;
    }

    [When(@"文書をパースする")]
    public void 文書をパースする()
    {
        _parsedTree = SyntaxTree.Parse(_inputDocument.AsSpan());
    }

    [Then(@"構文木のルートは ""(.*)"" ノードである")]
    public void 構文木のルートはノードである(string expectedNodeName)
    {
        Assert.IsNotNull(_parsedTree);
        Assert.AreEqual(expectedNodeName, _parsedTree.Root.Name);
    }

    [Then(@"(\d+)つの ""(.*)"" ブロックを含む")]
    public void ブロックを含む(int count, string blockType)
    {
        Assert.IsNotNull(_parsedTree);
        Assert.AreEqual(count, _parsedTree.Root.Blocks.Count);
        if (count > 0)
        {
            Assert.AreEqual(blockType, _parsedTree.Root.Blocks[0].Name);
        }
    }

    [Then(@"パラグラフは (\d+)つの ""(.*)"" インラインを含む")]
    public void パラグラフはインラインを含む(int count, string inlineType)
    {
        Assert.IsNotNull(_parsedTree);
        Assert.AreEqual(1, _parsedTree.Root.Blocks.Count);

        var paragraph = _parsedTree.Root.Blocks[0] as Paragraph;
        Assert.IsNotNull(paragraph);
        Assert.AreEqual(count, paragraph.Inlines.Count);

        if (count > 0)
        {
            Assert.AreEqual(inlineType, paragraph.Inlines[0].Name);
        }
    }

    [Then(@"テキストの値は ""(.*)"" である")]
    public void テキストの値はである(string expectedValue)
    {
        Assert.IsNotNull(_parsedTree);
        var paragraph = _parsedTree.Root.Blocks[0] as Paragraph;
        Assert.IsNotNull(paragraph);

        var text = paragraph.Inlines[0] as Text;
        Assert.IsNotNull(text);
        Assert.AreEqual(expectedValue, text.Value);
    }

    [Then(@"位置情報が正しく設定されている")]
    public void 位置情報が正しく設定されている()
    {
        Assert.IsNotNull(_parsedTree);
        var paragraph = _parsedTree.Root.Blocks[0] as Paragraph;
        Assert.IsNotNull(paragraph);

        var text = paragraph.Inlines[0] as Text;
        Assert.IsNotNull(text);

        var endColumn = _inputDocument.Length == 0 ? 1 : _inputDocument.Length;
        var expectedLocation = new Location(new Position(1, 1), new Position(1, endColumn));
        Assert.AreEqual(expectedLocation, text.Location);
        Assert.AreEqual(expectedLocation, paragraph.Location);
        Assert.AreEqual(expectedLocation, _parsedTree.Root.Location);
    }

    [Then(@"ブロックを含まない")]
    public void ブロックを含まない()
    {
        Assert.IsNotNull(_parsedTree);
        Assert.AreEqual(0, _parsedTree.Root.Blocks.Count);
    }
}