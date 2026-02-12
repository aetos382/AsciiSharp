using System;
using System.Linq;

using AsciiSharp.Syntax;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

public partial class ErrorRecoveryFeature
{
    private string _sourceText = string.Empty;
    private SyntaxTree? _syntaxTree;

    private void パーサーが利用可能である()
    {
        // パーサーの初期化は ParseText 呼び出し時に行われる
    }

    private void 以下のAsciiDoc文書がある(string text)
    {
        this._sourceText = text;
    }

    private void 文書を解析する()
    {
        this._syntaxTree = SyntaxTree.ParseText(this._sourceText);
    }

    private void 構文木が生成される()
    {
        Assert.IsNotNull(this._syntaxTree, "構文木が null です。");
        Assert.IsNotNull(this._syntaxTree.Root, "ルートノードが null です。");
    }

    private void 構文木に診断情報が含まれる()
    {
        Assert.IsNotNull(this._syntaxTree, "構文木が null です。");
        Assert.IsNotEmpty(this._syntaxTree.Diagnostics, "診断情報が含まれていません。");
    }

    private void 診断情報の数は_以上(int minCount)
    {
        Assert.IsNotNull(this._syntaxTree, "構文木が null です。");
        Assert.IsGreaterThanOrEqualTo(minCount, this._syntaxTree.Diagnostics.Count, $"診断情報の数が {minCount} 以上ではありません。実際: {this._syntaxTree.Diagnostics.Count}");
    }

    private void セクションが正しく解析される(string sectionTitle)
    {
        Assert.IsNotNull(this._syntaxTree, "構文木が null です。");
        var document = this._syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        var sections = document.DescendantNodes()
            .OfType<SectionSyntax>()
            .ToList();

        var found = sections.Any(s => s.Title?.GetTitleContent() == sectionTitle);
        Assert.IsTrue(found, $"セクション '{sectionTitle}' が見つかりません。");
    }

    private void 正常な段落の数は_以上(int minCount)
    {
        Assert.IsNotNull(this._syntaxTree, "構文木が null です。");
        var document = this._syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        var paragraphs = document.DescendantNodes()
            .OfType<ParagraphSyntax>()
            .ToList();

        Assert.IsGreaterThanOrEqualTo(minCount, paragraphs.Count, $"段落の数が {minCount} 以上ではありません。実際: {paragraphs.Count}");
    }

    private void 構文木からテキストを再構築できる()
    {
        Assert.IsNotNull(this._syntaxTree, "構文木が null です。");
        var reconstructedText = this._syntaxTree.Root.ToFullString();
        Assert.AreEqual(this._sourceText, reconstructedText, "再構築されたテキストが元の文書と一致しません。");
    }

    private void 診断情報に位置情報が含まれる()
    {
        Assert.IsNotNull(this._syntaxTree, "構文木が null です。");
        Assert.IsNotEmpty(this._syntaxTree.Diagnostics, "診断情報が含まれていません。");

        foreach (var diagnostic in this._syntaxTree.Diagnostics)
        {
            Assert.IsGreaterThanOrEqualTo(0, diagnostic.Location.Length, $"診断情報の位置情報が不正です。診断: {diagnostic}");
        }
    }

    private void 診断情報の重大度が正しい(string severity1, string severity2)
    {
        Assert.IsNotNull(this._syntaxTree, "構文木が null です。");
        Assert.IsNotEmpty(this._syntaxTree.Diagnostics, "診断情報が含まれていません。");

        var severities = this._syntaxTree.Diagnostics
            .Select(d => d.Severity.ToString())
            .ToHashSet();

        Assert.IsTrue(
            severities.Contains(severity1) || severities.Contains(severity2),
            $"診断情報に重大度 '{severity1}' または '{severity2}' が含まれていません。実際: {string.Join(", ", severities)}");
    }

    private void 構文木に欠落ノードが含まれる()
    {
        Assert.IsNotNull(this._syntaxTree, "構文木が null です。");

        var hasMissingNodes = this._syntaxTree.Root
            .DescendantNodesAndTokens()
            .Any(n => n.IsMissing);

        Assert.IsTrue(hasMissingNodes, "欠落ノードが見つかりません。");
    }

    private void 欠落ノードのIsMissingプロパティがtrue()
    {
        Assert.IsNotNull(this._syntaxTree, "構文木が null です。");

        var missingNodes = this._syntaxTree.Root
            .DescendantNodesAndTokens()
            .Where(n => n.IsMissing)
            .ToList();

        Assert.IsNotEmpty(missingNodes, "欠落ノードが見つかりません。");

        foreach (var node in missingNodes)
        {
            Assert.IsTrue(node.IsMissing, "欠落ノードの IsMissing プロパティが true ではありません。");
        }
    }
}
