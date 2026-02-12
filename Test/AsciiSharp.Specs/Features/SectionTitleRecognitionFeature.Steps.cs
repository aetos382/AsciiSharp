using System.Linq;

using AsciiSharp.Syntax;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

public partial class SectionTitleRecognitionFeature
{
    private string _sourceText = string.Empty;
    private SyntaxTree? _syntaxTree;
    private string _reconstructedText = string.Empty;

    private void パーサーが初期化されている()
    {
    }

    private void 以下のAsciiDoc文書がある(string text)
    {
        this._sourceText = text;
    }

    private void 文書を解析する()
    {
        this._syntaxTree = SyntaxTree.ParseText(this._sourceText);
    }

    private void 構文木から完全なテキストを取得する()
    {
        Assert.IsNotNull(this._syntaxTree, "構文木が null です。");
        this._reconstructedText = this._syntaxTree.Root.ToFullString();
    }

    private void Documentノードは_Headerを持たない()
    {
        Assert.IsNotNull(this._syntaxTree, "構文木が null です。");
        var document = this._syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");
        Assert.IsNull(document.Header, "Document は Header を持ちません。");
    }

    private void Documentノードは_N個のセクションを持つ(int expectedCount)
    {
        Assert.IsNotNull(this._syntaxTree, "構文木が null です。");
        var document = this._syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        var sections = document.Body?.ChildNodesAndTokens()
            .Where(c => c.IsNode && c.AsNode()?.Kind == SyntaxKind.Section)
            .ToList();

        Assert.IsNotNull(sections, "セクションリストが取得できません。");
        Assert.HasCount(expectedCount, sections, $"セクション数が一致しません。期待: {expectedCount}, 実際: {sections.Count}");
    }

    private void Documentノードは_N個の段落を持つ(int expectedCount)
    {
        Assert.IsNotNull(this._syntaxTree, "構文木が null です。");
        var document = this._syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        var paragraphs = document.DescendantNodes()
            .Where(n => n.Kind == SyntaxKind.Paragraph)
            .ToList();

        Assert.HasCount(expectedCount, paragraphs, $"段落数が一致しません。期待: {expectedCount}, 実際: {paragraphs.Count}");
    }

    private void 再構築されたテキストは元の文書と一致する()
    {
        Assert.AreEqual(this._sourceText, this._reconstructedText, "再構築されたテキストが元の文書と一致しません。");
    }

    private void 最初のセクションタイトルのレベルは(int expectedLevel)
    {
        Assert.IsNotNull(this._syntaxTree, "構文木が null です。");
        var document = this._syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        SectionTitleSyntax? sectionTitle = null;

        if (document.Header?.Title is not null)
        {
            sectionTitle = document.Header.Title;
        }
        else
        {
            var firstSection = document.Body?.ChildNodesAndTokens()
                .Where(c => c.IsNode && c.AsNode()?.Kind == SyntaxKind.Section)
                .Select(c => c.AsNode() as SectionSyntax)
                .FirstOrDefault();
            sectionTitle = firstSection?.Title;
        }

        Assert.IsNotNull(sectionTitle, "セクションタイトルが見つかりません。");
        Assert.AreEqual(expectedLevel, sectionTitle.Level, $"レベルが一致しません。期待: {expectedLevel}, 実際: {sectionTitle.Level}");
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
}
