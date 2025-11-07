using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reqnroll;

using AsciiSharp.Syntax;

namespace AsciiSharp.Specs.StepDefinitions;

[Binding]
public sealed class ParserStepDefinitions
{
    private string _inputDocument = string.Empty;
    private SyntaxTree? _parsedTree;

    [Given(@"AsciiDoc文書 ""(.*)"" が与えられている")]
    public void AsciiDoc文書が与えられている(string document)
    {
        this._inputDocument = document;
    }

    [Given(@"空のAsciiDoc文書が与えられている")]
    public void 空のAsciiDoc文書が与えられている()
    {
        this._inputDocument = string.Empty;
    }

    [When(@"文書をパースする")]
    public void 文書をパースする()
    {
        this._parsedTree = SyntaxTree.Parse(this._inputDocument.AsSpan());
    }

    [Then(@"構文木のルートは ""(.*)"" ノードである")]
    public void 構文木のルートはノードである(string expectedNodeName)
    {
        Assert.IsNotNull(this._parsedTree);
        var expectedKind = SyntaxNodeKind.Parse(expectedNodeName);
        Assert.AreEqual(expectedKind, this._parsedTree.Root.Kind);
    }

    [Then(@"(\d+)つの ""(.*)"" ブロックを含む")]
    public void ブロックを含む(int count, string blockType)
    {
        Assert.IsNotNull(this._parsedTree);
        Assert.HasCount(count, this._parsedTree.Root.Blocks);
        if (count > 0)
        {
            var expectedKind = SyntaxNodeKind.Parse(blockType);
            Assert.AreEqual(expectedKind, this._parsedTree.Root.Blocks[0].Kind);
        }
    }

    [Then(@"パラグラフは (\d+)つの ""(.*)"" インラインを含む")]
    public void パラグラフはインラインを含む(int count, string inlineType)
    {
        Assert.IsNotNull(this._parsedTree);
        Assert.HasCount(1, this._parsedTree.Root.Blocks);

        var paragraph = this._parsedTree.Root.Blocks[0] as ParagraphSyntax;
        Assert.IsNotNull(paragraph);
        Assert.HasCount(count, paragraph.Inlines);

        if (count > 0)
        {
            var expectedKind = SyntaxNodeKind.Parse(inlineType);
            Assert.AreEqual(expectedKind, paragraph.Inlines[0].Kind);
        }
    }

    [Then(@"テキストの値は ""(.*)"" である")]
    public void テキストの値はである(string expectedValue)
    {
        Assert.IsNotNull(this._parsedTree);
        var paragraph = this._parsedTree.Root.Blocks[0] as ParagraphSyntax;
        Assert.IsNotNull(paragraph);

        var text = paragraph.Inlines[0] as TextSyntax;
        Assert.IsNotNull(text);
        Assert.AreEqual(expectedValue, text.Value);
    }

    [Then(@"位置情報が正しく設定されている")]
    public void 位置情報が正しく設定されている()
    {
        Assert.IsNotNull(this._parsedTree);
        var paragraph = this._parsedTree.Root.Blocks[0] as ParagraphSyntax;
        Assert.IsNotNull(paragraph);

        var text = paragraph.Inlines[0] as TextSyntax;
        Assert.IsNotNull(text);

        var endColumn = this._inputDocument.Length == 0 ? 1 : this._inputDocument.Length;
        var expectedLocation = new Location(new Position(1, 1), new Position(1, endColumn));
        Assert.AreEqual(expectedLocation, text.Location);
        Assert.AreEqual(expectedLocation, paragraph.Location);
        Assert.AreEqual(expectedLocation, this._parsedTree.Root.Location);
    }

    [Then("ブロックを含まない")]
    public void ブロックを含まない()
    {
        Assert.IsNotNull(this._parsedTree);
        Assert.IsEmpty(this._parsedTree.Root.Blocks);
    }
}
