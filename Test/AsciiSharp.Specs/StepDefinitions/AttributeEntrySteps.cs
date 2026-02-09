
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
        // T011 で DocumentHeaderSyntax.AttributeEntries 実装後に有効化
        throw new PendingStepException();
    }

    [Then(@"属性エントリ (\d+) の名前は ""(.*)"" である")]
    public void Then属性エントリの名前はである(int index, string expectedName)
    {
        // T010 で AttributeEntrySyntax 実装後に有効化
        throw new PendingStepException();
    }

    [Then(@"属性エントリ (\d+) の値は ""(.*)"" である")]
    public void Then属性エントリの値はである(int index, string expectedValue)
    {
        // T010 で AttributeEntrySyntax 実装後に有効化
        throw new PendingStepException();
    }

    [Then(@"属性エントリ (\d+) の値は空である")]
    public void Then属性エントリの値は空である(int index)
    {
        // T010 で AttributeEntrySyntax 実装後に有効化
        throw new PendingStepException();
    }
}
