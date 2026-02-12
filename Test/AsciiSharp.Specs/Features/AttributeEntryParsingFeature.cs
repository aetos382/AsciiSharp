using System.Linq;

using AsciiSharp.Syntax;

using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

/// <summary>
/// 属性エントリの解析に関する BDD テストです。
/// </summary>
[TestClass]
[FeatureDescription(
    @"属性エントリの解析
ライブラリユーザーとして、
ドキュメント ヘッダー内の属性エントリ（:name: value 形式）を解析し、
属性名と属性値を取得したい")]
public partial class AttributeEntryParsingFeature : FeatureFixture
{
    private string? _sourceText;
    private SyntaxTree? _tree;
    private string? _reconstructedText;

    [Scenario]
    [TestMethod]
    public void 値あり属性エントリの解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("= Document Title\n:icons: font\n"),
            and => 文書を解析する(),
            then => 構文木のルートはDocumentノードである(),
            and => Documentノードは_Headerを持つ(),
            and => Headerは_N個の属性エントリを持つ(1),
            and => 属性エントリNの名前は(1, "icons"),
            and => 属性エントリNの値は(1, "font"));
    }

    [Scenario]
    [TestMethod]
    public void 値なし属性エントリの解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("= Document Title\n:toc:\n"),
            and => 文書を解析する(),
            then => Documentノードは_Headerを持つ(),
            and => Headerは_N個の属性エントリを持つ(1),
            and => 属性エントリNの名前は(1, "toc"),
            and => 属性エントリNの値は空(1));
    }

    [Scenario]
    [TestMethod]
    public void 複数の属性エントリの解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("= Document Title\n:icons: font\n:toc:\n"),
            and => 文書を解析する(),
            then => Documentノードは_Headerを持つ(),
            and => Headerは_N個の属性エントリを持つ(2),
            and => 属性エントリNの名前は(1, "icons"),
            and => 属性エントリNの値は(1, "font"),
            and => 属性エントリNの名前は(2, "toc"),
            and => 属性エントリNの値は空(2));
    }

    [Scenario]
    [TestMethod]
    public void 属性エントリを含む文書のラウンドトリップ()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("= Document Title\n:icons: font\n:toc:\n\nbody\n"),
            and => 文書を解析する(),
            and => 構文木から完全なテキストを取得する(),
            then => 再構築されたテキストは元の文書と一致する());
    }

    [Scenario]
    [TestMethod]
    public void ハイフンを含む属性名の解析()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("= Document Title\n:my-custom-attr: value\n"),
            and => 文書を解析する(),
            then => Headerは_N個の属性エントリを持つ(1),
            and => 属性エントリNの名前は(1, "my-custom-attr"),
            and => 属性エントリNの値は(1, "value"));
    }

    [Scenario]
    [TestMethod]
    public void 属性エントリのない文書()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            when => 以下のAsciiDoc文書がある("= Document Title\n\nbody\n"),
            and => 文書を解析する(),
            then => Documentノードは_Headerを持つ(),
            and => Headerは_N個の属性エントリを持つ(0));
    }

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
        _tree = SyntaxTree.ParseText(_sourceText);
    }

    private void 構文木から完全なテキストを取得する()
    {
        Assert.IsNotNull(_tree);
        _reconstructedText = _tree.Root.ToFullString();
    }

    private void 再構築されたテキストは元の文書と一致する()
    {
        Assert.IsNotNull(_sourceText);
        Assert.IsNotNull(_reconstructedText);
        Assert.AreEqual(_sourceText, _reconstructedText);
    }

    private void 構文木のルートはDocumentノードである()
    {
        Assert.IsNotNull(_tree);
        Assert.AreEqual(SyntaxKind.Document, _tree.Root.Kind);
    }

    private void Documentノードは_Headerを持つ()
    {
        Assert.IsNotNull(_tree);
        var document = _tree.Root as DocumentSyntax;
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Header);
    }

    private void Headerは_N個の属性エントリを持つ(int expectedCount)
    {
        Assert.IsNotNull(_tree);
        var document = _tree.Root as DocumentSyntax;
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Header);

        var entries = document.Header.AttributeEntries.ToList();
        Assert.AreEqual(expectedCount, entries.Count);
    }

    private void 属性エントリNの名前は(int index, string expectedName)
    {
        Assert.IsNotNull(_tree);
        var document = _tree.Root as DocumentSyntax;
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Header);

        var entries = document.Header.AttributeEntries.ToList();
        Assert.IsTrue(index >= 1 && index <= entries.Count, $"インデックス {index} が範囲外です");

        var entry = entries[index - 1];
        Assert.AreEqual(expectedName, entry.Name);
    }

    private void 属性エントリNの値は(int index, string expectedValue)
    {
        Assert.IsNotNull(_tree);
        var document = _tree.Root as DocumentSyntax;
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Header);

        var entries = document.Header.AttributeEntries.ToList();
        Assert.IsTrue(index >= 1 && index <= entries.Count, $"インデックス {index} が範囲外です");

        var entry = entries[index - 1];
        Assert.AreEqual(expectedValue, entry.Value);
    }

    private void 属性エントリNの値は空(int index)
    {
        Assert.IsNotNull(_tree);
        var document = _tree.Root as DocumentSyntax;
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Header);

        var entries = document.Header.AttributeEntries.ToList();
        Assert.IsTrue(index >= 1 && index <= entries.Count, $"インデックス {index} が範囲外です");

        var entry = entries[index - 1];
        Assert.AreEqual(string.Empty, entry.Value);
    }
}
