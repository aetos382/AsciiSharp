
using System.Collections.Generic;
using System.Linq;

using AsciiSharp.InternalSyntax;
using AsciiSharp.Syntax;
using AsciiSharp.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reqnroll;

namespace AsciiSharp.Specs.StepDefinitions;

/// <summary>
/// 増分解析機能のステップ定義。
/// </summary>
[Binding]
public sealed class IncrementalParsingSteps
{
    private readonly BasicParsingSteps _basicParsingSteps;

    private SyntaxTree? _incrementalSyntaxTree;
    private string _modifiedSourceText = string.Empty;
    private readonly Dictionary<int, InternalNode> _sectionInternalNodesByIndex = [];
    private readonly Dictionary<string, InternalNode> _sectionInternalNodesByName = [];

    /// <summary>
    /// コンストラクタ。依存するステップ定義を注入。
    /// </summary>
    /// <param name="basicParsingSteps">基本パーサーのステップ定義。</param>
    public IncrementalParsingSteps(BasicParsingSteps basicParsingSteps)
    {
        this._basicParsingSteps = basicParsingSteps;
    }

    [When(@"すべてのセクションの内部ノードへの参照を保持する")]
    public void Whenすべてのセクションの内部ノードへの参照を保持する()
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree, "構文木が null です。");

        var document = syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        var sections = document.DescendantNodes()
            .OfType<SectionSyntax>()
            .ToList();

        for (var i = 0; i < sections.Count; i++)
        {
            this._sectionInternalNodesByIndex[i + 1] = sections[i].Internal;
        }
    }

    [When(@"名前 ""(.+)"" のセクションの内部ノードへの参照を保持する")]
    public void Whenセクションの内部ノードへの参照を保持する(string sectionName)
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree, "構文木が null です。");

        var document = syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        var section = document.DescendantNodes()
            .OfType<SectionSyntax>()
            .FirstOrDefault(s => s.Title?.TitleContent?.Contains(sectionName, System.StringComparison.Ordinal) == true);
        Assert.IsNotNull(section, $"セクション '{sectionName}' が見つかりません。");

        this._sectionInternalNodesByName[sectionName] = section.Internal;
    }

    [When(@"""(.+)"" を ""(.+)"" に変更して増分解析する")]
    public void When文字列を変更して増分解析する(string oldText, string newText)
    {
        System.ArgumentNullException.ThrowIfNull(oldText);
        System.ArgumentNullException.ThrowIfNull(newText);

        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree, "構文木が null です。");

        var sourceText = this._basicParsingSteps.CurrentSourceText;
        var startIndex = sourceText.IndexOf(oldText, System.StringComparison.Ordinal);
        Assert.IsGreaterThanOrEqualTo(0, startIndex, $"'{oldText}' が文書内に見つかりません。");

        // 変更後のテキストを保存
#pragma warning disable CA1845, IDE0057 // テストコードのため文字列操作を簡潔に
        this._modifiedSourceText = sourceText[..startIndex] + newText + sourceText[(startIndex + oldText.Length)..];
#pragma warning restore CA1845, IDE0057

        // 増分解析を実行
        var change = new TextChange(new TextSpan(startIndex, oldText.Length), newText);
        this._incrementalSyntaxTree = syntaxTree.WithChanges(change);
    }

    [Then(@"インデックス (\d+) のセクションの内部ノードは再利用されている")]
    public void Thenセクションの内部ノードは再利用されているByIndex(int sectionIndex)
    {
        Assert.IsTrue(
            this._sectionInternalNodesByIndex.ContainsKey(sectionIndex),
            $"セクション {sectionIndex} の参照が保持されていません。");

        Assert.IsNotNull(this._incrementalSyntaxTree, "増分解析された構文木が null です。");

        var originalInternal = this._sectionInternalNodesByIndex[sectionIndex];

        var newSection = GetSectionByIndex(this._incrementalSyntaxTree, sectionIndex);
        Assert.IsNotNull(newSection, $"新しい構文木にセクション {sectionIndex} が見つかりません。");

        Assert.AreSame(
            originalInternal,
            newSection.Internal,
            $"セクション {sectionIndex} の内部ノードが再利用されていません。");
    }

    [Then(@"名前 ""(.+)"" のセクションの内部ノードは再利用されている")]
    public void Thenセクションの内部ノードは再利用されているByName(string sectionName)
    {
        Assert.IsTrue(
            this._sectionInternalNodesByName.ContainsKey(sectionName),
            $"セクション '{sectionName}' の参照が保持されていません。");

        Assert.IsNotNull(this._incrementalSyntaxTree, "増分解析された構文木が null です。");

        var originalInternal = this._sectionInternalNodesByName[sectionName];

        var newSection = this._incrementalSyntaxTree.Root.DescendantNodes()
            .OfType<SectionSyntax>()
            .FirstOrDefault(s => s.Title?.TitleContent?.Contains(sectionName, System.StringComparison.Ordinal) == true);
        Assert.IsNotNull(newSection, $"新しい構文木にセクション '{sectionName}' が見つかりません。");

        Assert.AreSame(
            originalInternal,
            newSection.Internal,
            $"セクション '{sectionName}' の内部ノードが再利用されていません。");
    }

    [Then(@"再構築されたテキストは変更後の文書と一致する")]
    public void Then再構築されたテキストは変更後の文書と一致する()
    {
        Assert.IsNotNull(this._incrementalSyntaxTree, "増分解析された構文木が null です。");

        var reconstructed = this._incrementalSyntaxTree.Root.ToFullString();
        Assert.AreEqual(
            this._modifiedSourceText,
            reconstructed,
            "再構築されたテキストが変更後の文書と一致しません。");
    }

    /// <summary>
    /// 指定されたインデックスのセクションを取得する。
    /// </summary>
    /// <param name="tree">構文木。</param>
    /// <param name="index">セクションインデックス（1ベース）。</param>
    /// <returns>セクション。</returns>
    private static SectionSyntax? GetSectionByIndex(SyntaxTree tree, int index)
    {
        var sections = tree.Root.DescendantNodes()
            .OfType<SectionSyntax>()
            .ToList();

        if (index < 1 || index > sections.Count)
        {
            return null;
        }

        return sections[index - 1];
    }
}
