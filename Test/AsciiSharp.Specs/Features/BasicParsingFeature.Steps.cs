using System;
using System.Linq;
using System.Text;

using AsciiSharp.Syntax;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

public partial class BasicParsingFeature
{
    private string _sourceText = string.Empty;
    private SyntaxTree? _syntaxTree;
    private string _reconstructedText = string.Empty;

    private void パーサーが初期化されている()
    {
        // パーサーの初期化は ParseText 呼び出し時に行われる
    }

    private void 以下のAsciiDoc文書がある(string text)
    {
        _sourceText = text;
    }

    private void 空のAsciiDoc文書がある()
    {
        _sourceText = string.Empty;
    }

    private void BOM付きの以下のAsciiDoc文書がある(string text)
    {
        // BOM (U+FEFF) を先頭に追加
        _sourceText = "\uFEFF" + text;
    }

    private void CRLF改行コードの以下のAsciiDoc文書がある(string text)
    {
        // 改行コードを CRLF に統一
        _sourceText = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal)
            .Replace("\n", "\r\n", StringComparison.Ordinal);
    }

    private void CR改行コードの以下のAsciiDoc文書がある(string text)
    {
        // 改行コードを CR に統一
        _sourceText = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\n", "\r", StringComparison.Ordinal);
    }

    private void 混在する改行コードの以下のAsciiDoc文書がある(string text)
    {
        // 改行コードを意図的に混在させる（LF, CRLF, CR の順）
        var normalizedText = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal);

        var lines = normalizedText.Split('\n');
        var result = new StringBuilder();

        for (var i = 0; i < lines.Length; i++)
        {
            result.Append(lines[i]);

            if (i < lines.Length - 1)
            {
                // 3 種類の改行コードを順番に使う
                result.Append((i % 3) switch
                {
                    0 => "\n",      // LF
                    1 => "\r\n",    // CRLF
                    _ => "\r"       // CR
                });
            }
        }

        _sourceText = result.ToString();
    }

    private void 文書を解析する()
    {
        _syntaxTree = SyntaxTree.ParseText(_sourceText);
    }

    private void 構文木から完全なテキストを取得する()
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        _reconstructedText = _syntaxTree.Root.ToFullString();
    }

    private void 構文木のルートはDocumentノードである()
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        Assert.IsNotNull(_syntaxTree.Root, "ルートノードが null です。");
        Assert.AreEqual(SyntaxKind.Document, _syntaxTree.Root.Kind, "ルートノードは Document である必要があります。");
    }

    private void Documentノードは_Headerを持つ()
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        var document = _syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");
        Assert.IsNotNull(document.Header, "Document は Header を持つ必要があります。");
    }

    private void Documentノードは_Headerを持たない()
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        var document = _syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");
        Assert.IsNull(document.Header, "Document は Header を持ちません。");
    }

    private void Headerのタイトルは(string expectedTitle)
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        var document = _syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");
        Assert.IsNotNull(document.Header, "Document は Header を持つ必要があります。");
        var actualTitle = document.Header.Title?.GetTitleContent();
        Assert.AreEqual(expectedTitle, actualTitle, $"タイトルが一致しません。期待: '{expectedTitle}', 実際: '{actualTitle}'");
    }

    private void Documentノードは_N個のセクションを持つ(int expectedCount)
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        var document = _syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        var sections = document.Body?.ChildNodesAndTokens()
            .Where(c => c.IsNode && c.AsNode()?.Kind == SyntaxKind.Section)
            .ToList();

        Assert.IsNotNull(sections, "セクションリストが取得できません。");
        Assert.HasCount(expectedCount, sections, $"セクション数が一致しません。期待: {expectedCount}, 実際: {sections.Count}");
    }

    private void セクションNのタイトルは(int sectionIndex, string expectedTitle)
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        var document = _syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        var sections = document.Body?.ChildNodesAndTokens()
            .Where(c => c.IsNode && c.AsNode()?.Kind == SyntaxKind.Section)
            .Select(c => c.AsNode() as SectionSyntax)
            .ToList();

        Assert.IsNotNull(sections, "セクションリストが取得できません。");
        Assert.IsTrue(sectionIndex >= 1 && sectionIndex <= sections.Count, $"セクションインデックス {sectionIndex} は範囲外です。");

        var section = sections[sectionIndex - 1];
        Assert.IsNotNull(section, $"セクション {sectionIndex} が null です。");
        var actualTitle = section.Title?.GetTitleContent();
        Assert.AreEqual(expectedTitle, actualTitle, $"セクション {sectionIndex} のタイトルが一致しません。期待: '{expectedTitle}', 実際: '{actualTitle}'");
    }

    private void 再構築されたテキストは元の文書と一致する()
    {
        Assert.AreEqual(_sourceText, _reconstructedText, "再構築されたテキストが元の文書と一致しません。");
    }

    private void セクションのネスト構造が正しく解析されている()
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        var document = _syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        var hasSections = document.DescendantNodes()
            .Any(n => n.Kind == SyntaxKind.Section);
        Assert.IsTrue(hasSections, "セクションが見つかりません。");
    }

    private void Documentノードは_N個の段落を持つ(int expectedCount)
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        var document = _syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        var paragraphs = document.DescendantNodes()
            .Where(n => n.Kind == SyntaxKind.Paragraph)
            .ToList();

        Assert.HasCount(expectedCount, paragraphs, $"段落数が一致しません。期待: {expectedCount}, 実際: {paragraphs.Count}");
    }

    private void Headerは著者行を持つ()
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        var document = _syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");
        Assert.IsNotNull(document.Header, "Document は Header を持つ必要があります。");
        Assert.IsNotNull(document.Header.AuthorLine, "Header は著者行を持つ必要があります。");
    }

    private void すべての空白と改行が保持されている()
    {
        // ラウンドトリップが成功していれば、空白と改行も保持されている
        Assert.AreEqual(_sourceText, _reconstructedText, "空白または改行が保持されていません。");
    }

    private void ソーステキストはBOMを含む()
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        Assert.IsTrue(_syntaxTree.Text.HasBom, "ソーステキストは BOM を含む必要があります。");
    }

    private void BOMを含む元のテキストを復元できる()
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        var originalText = _syntaxTree.ToOriginalString();
        Assert.AreEqual(_sourceText, originalText, "BOM を含む元のテキストと一致しません。");
    }
}
