using System.Linq;

using AsciiSharp.Syntax;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

public partial class AttributeEntryParsingFeature
{
    private string? _sourceText;
    private SyntaxTree? _syntaxTree;
    private string? _reconstructedText;

    private void パーサーが初期化されている()
    {
        // パーサーは初期化済み（特別な準備不要）
    }

    private void 以下のAsciiDoc文書がある(string text)
    {
        _sourceText = text;
    }

    private void 文書を解析する()
    {
        Assert.IsNotNull(_sourceText);
        _syntaxTree = SyntaxTree.ParseText(_sourceText);
    }

    private void 構文木から完全なテキストを取得する()
    {
        Assert.IsNotNull(_syntaxTree);
        _reconstructedText = _syntaxTree.Root.ToFullString();
    }

    private void 再構築されたテキストは元の文書と一致する()
    {
        Assert.IsNotNull(_sourceText);
        Assert.IsNotNull(_reconstructedText);
        Assert.AreEqual(_sourceText, _reconstructedText);
    }

    private void 構文木のルートはDocumentノードである()
    {
        Assert.IsNotNull(_syntaxTree);
        Assert.AreEqual(SyntaxKind.Document, _syntaxTree.Root.Kind);
    }

    private void Documentノードは_Headerを持つ()
    {
        Assert.IsNotNull(_syntaxTree);
        var document = _syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Header);
    }

    private void Headerは_N個の属性エントリを持つ(int expectedCount)
    {
        Assert.IsNotNull(_syntaxTree);
        var document = _syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Header);

        var entries = document.Header.AttributeEntries.ToList();
        Assert.AreEqual(expectedCount, entries.Count);
    }

    private void 属性エントリNの名前は(int index, string expectedName)
    {
        Assert.IsNotNull(_syntaxTree);
        var document = _syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Header);

        var entries = document.Header.AttributeEntries.ToList();
        Assert.IsTrue(index >= 1 && index <= entries.Count, $"インデックス {index} が範囲外です");

        var entry = entries[index - 1];
        Assert.AreEqual(expectedName, entry.Name);
    }

    private void 属性エントリNの値は(int index, string expectedValue)
    {
        Assert.IsNotNull(_syntaxTree);
        var document = _syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Header);

        var entries = document.Header.AttributeEntries.ToList();
        Assert.IsTrue(index >= 1 && index <= entries.Count, $"インデックス {index} が範囲外です");

        var entry = entries[index - 1];
        Assert.AreEqual(expectedValue, entry.Value);
    }

    private void 属性エントリNの値は空(int index)
    {
        Assert.IsNotNull(_syntaxTree);
        var document = _syntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Header);

        var entries = document.Header.AttributeEntries.ToList();
        Assert.IsTrue(index >= 1 && index <= entries.Count, $"インデックス {index} が範囲外です");

        var entry = entries[index - 1];
        Assert.AreEqual(string.Empty, entry.Value);
    }
}
