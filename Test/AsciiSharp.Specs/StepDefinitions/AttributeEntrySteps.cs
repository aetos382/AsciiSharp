
using AsciiSharp.Syntax;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reqnroll;

namespace AsciiSharp.Specs.StepDefinitions;

/// <summary>
/// 属性エントリの解析に関するステップ定義。
/// </summary>
[Binding]
public sealed class AttributeEntrySteps
{
    private readonly BasicParsingSteps _basicParsingSteps;

    /// <summary>
    /// AttributeEntrySteps を作成する。
    /// </summary>
    public AttributeEntrySteps(BasicParsingSteps basicParsingSteps)
    {
        this._basicParsingSteps = basicParsingSteps;
    }

    [Then(@"Header は (\d+) 個の属性エントリを持つ")]
    public void ThenHeaderは個の属性エントリを持つ(int expectedCount)
    {
        var header = this.GetDocumentHeader();
        Assert.HasCount(expectedCount, header.AttributeEntries,
            $"属性エントリの数が一致しません。期待: {expectedCount}, 実際: {header.AttributeEntries.Count}");
    }

    [Then(@"属性エントリ (\d+) の名前は ""(.*)"" である")]
    public void Then属性エントリの名前はである(int index, string expectedName)
    {
        var entry = this.GetAttributeEntry(index);
        Assert.AreEqual(expectedName, entry.Name,
            $"属性名が一致しません。期待: '{expectedName}', 実際: '{entry.Name}'");
    }

    [Then(@"属性エントリ (\d+) の値は ""(.*)"" である")]
    public void Then属性エントリの値はである(int index, string expectedValue)
    {
        var entry = this.GetAttributeEntry(index);
        Assert.AreEqual(expectedValue, entry.Value,
            $"属性値が一致しません。期待: '{expectedValue}', 実際: '{entry.Value}'");
    }

    [Then(@"属性エントリ (\d+) の値は空である")]
    public void Then属性エントリの値は空である(int index)
    {
        var entry = this.GetAttributeEntry(index);
        Assert.AreEqual(string.Empty, entry.Value,
            $"属性値が空ではありません。実際: '{entry.Value}'");
    }

    /// <summary>
    /// ドキュメントヘッダーを取得する。
    /// </summary>
    private DocumentHeaderSyntax GetDocumentHeader()
    {
        var tree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(tree, "構文木が null です。");

        var document = tree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");
        Assert.IsNotNull(document.Header, "Document は Header を持つ必要があります。");

        return document.Header;
    }

    /// <summary>
    /// 指定されたインデックスの属性エントリを取得する（1-based）。
    /// </summary>
    private AttributeEntrySyntax GetAttributeEntry(int index)
    {
        var header = this.GetDocumentHeader();
        Assert.IsTrue(index >= 1 && index <= header.AttributeEntries.Count,
            $"属性エントリインデックス {index} は範囲外です。属性エントリ数: {header.AttributeEntries.Count}");

        return header.AttributeEntries[index - 1];
    }
}
