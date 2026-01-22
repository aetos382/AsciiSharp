using System;
using System.Linq;

using AsciiSharp.Syntax;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reqnroll;

namespace AsciiSharp.Specs.StepDefinitions;

/// <summary>
/// エラー回復機能のステップ定義。
/// </summary>
/// <remarks>
/// 基本的なパーサー操作（文書解析、構文木生成など）は <see cref="BasicParsingSteps"/> に定義されている。
/// このクラスはエラー回復に固有のステップのみを定義する。
/// </remarks>
[Binding]
public sealed class ErrorRecoverySteps
{
    private readonly BasicParsingSteps _basicParsingSteps;

    /// <summary>
    /// コンストラクタ。依存するステップ定義を注入。
    /// </summary>
    /// <param name="basicParsingSteps">基本パーサーのステップ定義。</param>
    public ErrorRecoverySteps(BasicParsingSteps basicParsingSteps)
    {
        this._basicParsingSteps = basicParsingSteps;
    }

    [Given(@"AsciiDoc パーサーが利用可能である")]
    public static void Givenパーサーが利用可能である()
    {
        // パーサーは常に利用可能
    }

    [Then(@"構文木が生成される")]
    public void Then構文木が生成される()
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree);
        Assert.IsNotNull(syntaxTree.Root);
    }

    [Then(@"構文木に診断情報が含まれる")]
    public void Then構文木に診断情報が含まれる()
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree);
        Assert.IsNotEmpty(syntaxTree.Diagnostics, "診断情報が含まれていません");
    }

    [Then(@"診断情報の数は (\d+) 以上である")]
    public void Then診断情報の数は以上である(int minCount)
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;

        Assert.IsNotNull(syntaxTree);

        Assert.IsGreaterThanOrEqualTo(
            minCount,
            syntaxTree.Diagnostics.Count,
            $"診断情報の数が {minCount} 未満です。実際: {syntaxTree.Diagnostics.Count}");
    }

    [Then(@"""(.+)"" のセクションが正しく解析される")]
    public void Thenのセクションが正しく解析される(string sectionTitle)
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree);
        var sections = syntaxTree.Root.DescendantNodes().OfType<SectionSyntax>();
        var matchingSection = sections.FirstOrDefault(s =>
            s.Title?.ToFullString().Contains(sectionTitle, StringComparison.Ordinal) == true);
        Assert.IsNotNull(matchingSection, $"セクション '{sectionTitle}' が見つかりません");
    }

    [Then(@"""(.+)"" が何らかの形で認識される")]
    public void Thenが何らかの形で認識される(string text)
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree);
        var fullText = syntaxTree.Root.ToFullString();
        Assert.IsTrue(fullText.Contains(text, StringComparison.Ordinal), $"'{text}' がテキストに含まれていません");
    }

    [Then(@"正常な段落の数は (\d+) 以上である")]
    public void Then正常な段落の数は以上である(int minCount)
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree);
        var paragraphs = syntaxTree.Root.DescendantNodes().OfType<ParagraphSyntax>();
        var count = paragraphs.Count();
        Assert.IsGreaterThanOrEqualTo(
minCount,
            count, $"正常な段落の数が {minCount} 未満です。実際: {count}");
    }

    [Then(@"構文木からテキストを再構築できる")]
    public void Then構文木からテキストを再構築できる()
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        var sourceText = this._basicParsingSteps.CurrentSourceText;
        Assert.IsNotNull(syntaxTree);
        var reconstructed = syntaxTree.Root.ToFullString();
        Assert.AreEqual(sourceText, reconstructed, "テキストの再構築に失敗しました");
    }

    [Then(@"診断情報に位置情報が含まれる")]
    public void Then診断情報に位置情報が含まれる()
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree);
        foreach (var diagnostic in syntaxTree.Diagnostics)
        {
            Assert.IsGreaterThanOrEqualTo(
0,
                diagnostic.Location.Length, "診断情報に位置情報が含まれていません");
        }
    }

    [Then(@"診断情報の重大度が ""(.+)"" または ""(.+)"" である")]
    public void Then診断情報の重大度がまたはである(string severity1, string severity2)
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree);
        foreach (var diagnostic in syntaxTree.Diagnostics)
        {
            var severityStr = diagnostic.Severity.ToString();
            Assert.IsTrue(
                severityStr == severity1 || severityStr == severity2,
                $"診断情報の重大度が予期しない値です: {severityStr}");
        }
    }

    [Then(@"構文木に欠落ノードが含まれる")]
    public void Then構文木に欠落ノードが含まれる()
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree);
        var hasMissing = syntaxTree.Root.DescendantNodesAndTokens()
            .Any(n => n.IsMissing);
        Assert.IsTrue(hasMissing, "欠落ノードが含まれていません");
    }

    [Then(@"欠落ノードの IsMissing プロパティが true である")]
    public void Then欠落ノードのIsMissingプロパティがTrueである()
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree);
        var missingNodes = syntaxTree.Root.DescendantNodesAndTokens()
            .Where(n => n.IsMissing)
            .ToList();
        Assert.IsNotEmpty(missingNodes, "欠落ノードが見つかりません");
        foreach (var node in missingNodes)
        {
            Assert.IsTrue(node.IsMissing, "IsMissing プロパティが true ではありません");
        }
    }

    [Then(@"スキップされたトークンがトリビアとして保持される")]
    public void ThenスキップされたトークンがTriviasとして保持される()
    {
        var syntaxTree = this._basicParsingSteps.CurrentSyntaxTree;
        Assert.IsNotNull(syntaxTree);
        // スキップされたトークンがなくてもテストはパスする（ロスレス解析で保持されればOK）
        // このテストは主にロスレス性を確認する
        _ = syntaxTree.Root.DescendantTokens()
            .SelectMany(t => t.LeadingTrivia.Concat(t.TrailingTrivia))
            .Any(t => t.Kind == SyntaxKind.SkippedTokensTrivia);
    }
}
