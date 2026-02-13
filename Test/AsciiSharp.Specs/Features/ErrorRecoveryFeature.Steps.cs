using System;
using System.Linq;

using AsciiSharp.Syntax;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

public partial class ErrorRecoveryFeature
{
    private string _sourceText = string.Empty;
    private SyntaxTree? _syntaxTree;

    private void パーサーが初期化されている()
    {
        // パーサーの初期化は ParseText 呼び出し時に行われる
    }

    private void 以下のAsciiDoc文書がある(string text)
    {
        _sourceText = text;
    }

    private void 文書を解析する()
    {
        _syntaxTree = SyntaxTree.ParseText(_sourceText);
    }

    private void 構文木が生成される()
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        Assert.IsNotNull(_syntaxTree.Root, "ルートノードが null です。");
    }

    private void 構文木に診断情報が含まれる()
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        Assert.IsNotEmpty(_syntaxTree.Diagnostics, "診断情報が含まれていません。");
    }

    private void 診断情報の数は_以上(int minCount)
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        Assert.IsGreaterThanOrEqualTo(minCount, _syntaxTree.Diagnostics.Count, $"診断情報の数が {minCount} 以上ではありません。実際: {_syntaxTree.Diagnostics.Count}");
    }

    private void セクションが正しく解析される(string sectionTitle)
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        var document = _syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        var sections = document.DescendantNodes()
            .OfType<SectionSyntax>()
            .ToList();

        var found = sections.Any(s => s.Title?.GetTitleContent() == sectionTitle);
        Assert.IsTrue(found, $"セクション '{sectionTitle}' が見つかりません。");
    }

    private void 正常な段落の数は_以上(int minCount)
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        var document = _syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        var paragraphs = document.DescendantNodes()
            .OfType<ParagraphSyntax>()
            .ToList();

        Assert.IsGreaterThanOrEqualTo(minCount, paragraphs.Count, $"段落の数が {minCount} 以上ではありません。実際: {paragraphs.Count}");
    }

    private void 構文木からテキストを再構築できる()
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        var reconstructedText = _syntaxTree.Root.ToFullString();
        Assert.AreEqual(_sourceText, reconstructedText, "再構築されたテキストが元の文書と一致しません。");
    }

    private void 診断情報に位置情報が含まれる()
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        Assert.IsNotEmpty(_syntaxTree.Diagnostics, "診断情報が含まれていません。");

        foreach (var diagnostic in _syntaxTree.Diagnostics)
        {
            Assert.IsGreaterThanOrEqualTo(0, diagnostic.Location.Length, $"診断情報の位置情報が不正です。診断: {diagnostic}");
        }
    }

    private void 構文木に欠落ノードが含まれる()
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");

        var hasMissingNodes = _syntaxTree.Root
            .DescendantNodesAndTokens()
            .Any(n => n.IsMissing);

        Assert.IsTrue(hasMissingNodes, "欠落ノードが見つかりません。");
    }

    private void 欠落ノードのIsMissingプロパティがtrue()
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");

        var missingNodes = _syntaxTree.Root
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
