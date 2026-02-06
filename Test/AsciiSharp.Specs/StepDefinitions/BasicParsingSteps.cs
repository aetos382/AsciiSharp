
using System;
using System.Linq;

using AsciiSharp.Syntax;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reqnroll;

namespace AsciiSharp.Specs.StepDefinitions;

[Binding]
public sealed class BasicParsingSteps
{
    private string _reconstructedText = string.Empty;

    /// <summary>
    /// 現在の構文木を取得する（他のステップ定義から使用）。
    /// </summary>
    public SyntaxTree? CurrentSyntaxTree { get; private set; }

    /// <summary>
    /// 現在のソーステキストを取得する（他のステップ定義から使用）。
    /// </summary>
    public string CurrentSourceText { get; private set; } = string.Empty;

    [Given(@"AsciiDoc パーサーが初期化されている")]
    public static void Givenパーサーが初期化されている()
    {
        // パーサーの初期化は ParseText 呼び出し時に行われる
    }

    [Given(@"以下の AsciiDoc 文書がある:")]
    [Given(@"AsciiDoc テキスト ""(.*)"" がある")]
    public void Given以下のAsciiDoc文書がある(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        // Gherkin はエスケープシーケンスを解釈しないため、
        // feature ファイル内の \n を実際の改行文字に変換する。
        this.CurrentSourceText = text
            .Replace(@"\n", "\n", StringComparison.Ordinal);
    }

    [Given(@"空の AsciiDoc 文書がある")]
    public void Given空のAsciiDoc文書がある()
    {
        this.CurrentSourceText = string.Empty;
    }

    [Given(@"BOM 付きの以下の AsciiDoc 文書がある:")]
    public void GivenBOM付きの以下のAsciiDoc文書がある(string multilineText)
    {
        // BOM (U+FEFF) を先頭に追加
        this.CurrentSourceText = "\uFEFF" + multilineText;
    }

    [Given(@"CRLF 改行コードの以下の AsciiDoc 文書がある:")]
    public void GivenCRLF改行コードの以下のAsciiDoc文書がある(string multilineText)
    {
        ArgumentNullException.ThrowIfNull(multilineText);

        // 改行コードを CRLF に統一
        this.CurrentSourceText = multilineText
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal)
            .Replace("\n", "\r\n", StringComparison.Ordinal);
    }

    [Given(@"CR 改行コードの以下の AsciiDoc 文書がある:")]
    public void GivenCR改行コードの以下のAsciiDoc文書がある(string multilineText)
    {
        ArgumentNullException.ThrowIfNull(multilineText);

        // 改行コードを CR に統一
        this.CurrentSourceText = multilineText
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\n", "\r", StringComparison.Ordinal);
    }

    [Given(@"混在する改行コードの以下の AsciiDoc 文書がある:")]
    public void Given混在する改行コードの以下のAsciiDoc文書がある(string multilineText)
    {
        ArgumentNullException.ThrowIfNull(multilineText);

        // 改行コードを意図的に混在させる（LF, CRLF, CR の順）
        var normalizedText = multilineText
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal);

        var lines = normalizedText.Split('\n');
        var result = new System.Text.StringBuilder();

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

        this.CurrentSourceText = result.ToString();
    }

    [When(@"文書を解析する")]
    public void When文書を解析する()
    {
        this.CurrentSyntaxTree = SyntaxTree.ParseText(this.CurrentSourceText);
    }

    [When(@"構文木から完全なテキストを取得する")]
    public void When構文木から完全なテキストを取得する()
    {
        Assert.IsNotNull(this.CurrentSyntaxTree, "構文木が null です。");
        this._reconstructedText = this.CurrentSyntaxTree.Root.ToFullString();
    }

    [Then(@"構文木のルートは Document ノードである")]
    public void Then構文木のルートはDocumentノードである()
    {
        Assert.IsNotNull(this.CurrentSyntaxTree, "構文木が null です。");
        Assert.IsNotNull(this.CurrentSyntaxTree.Root, "ルートノードが null です。");
        Assert.AreEqual(SyntaxKind.Document, this.CurrentSyntaxTree.Root.Kind, "ルートノードは Document である必要があります。");
    }

    [Then(@"Document ノードは Header を持つ")]
    public void ThenDocumentノードはHeaderを持つ()
    {
        Assert.IsNotNull(this.CurrentSyntaxTree, "構文木が null です。");
        var document = this.CurrentSyntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");
        Assert.IsNotNull(document.Header, "Document は Header を持つ必要があります。");
    }

    [Then(@"Document ノードは Header を持たない")]
    public void ThenDocumentノードはHeaderを持たない()
    {
        Assert.IsNotNull(this.CurrentSyntaxTree, "構文木が null です。");

        var document = this.CurrentSyntaxTree.Root as DocumentSyntax;

        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");
        Assert.IsNull(document.Header, "Document は Header を持ちません。");
    }

    [Then(@"Header のタイトルは ""(.*)"" である")]
    public void ThenHeaderのタイトルはである(string expectedTitle)
    {
        Assert.IsNotNull(this.CurrentSyntaxTree, "構文木が null です。");
        var document = this.CurrentSyntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");
        Assert.IsNotNull(document.Header, "Document は Header を持つ必要があります。");
        var actualTitle = document.Header.Title?.GetTitleContent();
        Assert.AreEqual(expectedTitle, actualTitle, $"タイトルが一致しません。期待: '{expectedTitle}', 実際: '{actualTitle}'");
    }

    [Then(@"Document ノードは (\d+) 個のセクションを持つ")]
    public void ThenDocumentノードは個のセクションを持つ(int expectedCount)
    {
        Assert.IsNotNull(this.CurrentSyntaxTree, "構文木が null です。");
        var document = this.CurrentSyntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        var sections = document.Body?.ChildNodesAndTokens()
            .Where(c => c.IsNode && c.AsNode()?.Kind == SyntaxKind.Section)
            .ToList();

        Assert.IsNotNull(sections, "セクションリストが取得できません。");
        Assert.HasCount(expectedCount, sections, $"セクション数が一致しません。期待: {expectedCount}, 実際: {sections.Count}");
    }

    [Then(@"セクション (\d+) のタイトルは ""(.*)"" である")]
    public void Thenセクションのタイトルはである(int sectionIndex, string expectedTitle)
    {
        Assert.IsNotNull(this.CurrentSyntaxTree, "構文木が null です。");
        var document = this.CurrentSyntaxTree.Root as DocumentSyntax;
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

    [Then(@"再構築されたテキストは元の文書と一致する")]
    public void Then再構築されたテキストは元の文書と一致する()
    {
        Assert.AreEqual(this.CurrentSourceText, this._reconstructedText, "再構築されたテキストが元の文書と一致しません。");
    }

    [Then(@"セクションのネスト構造が正しく解析されている")]
    public void Thenセクションのネスト構造が正しく解析されている()
    {
        Assert.IsNotNull(this.CurrentSyntaxTree, "構文木が null です。");
        var document = this.CurrentSyntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        // セクションが存在することを確認
        var hasSections = document.DescendantNodes()
            .Any(n => n.Kind == SyntaxKind.Section);
        Assert.IsTrue(hasSections, "セクションが見つかりません。");
    }

    [Then(@"Document ノードは (\d+) 個の段落を持つ")]
    public void ThenDocumentノードは個の段落を持つ(int expectedCount)
    {
        Assert.IsNotNull(this.CurrentSyntaxTree, "構文木が null です。");
        var document = this.CurrentSyntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");

        var paragraphs = document.DescendantNodes()
            .Where(n => n.Kind == SyntaxKind.Paragraph)
            .ToList();

        Assert.HasCount(expectedCount, paragraphs, $"段落数が一致しません。期待: {expectedCount}, 実際: {paragraphs.Count}");
    }

    [Then(@"Header は著者行を持つ")]
    public void ThenHeaderは著者行を持つ()
    {
        Assert.IsNotNull(this.CurrentSyntaxTree, "構文木が null です。");
        var document = this.CurrentSyntaxTree.Root as DocumentSyntax;
        Assert.IsNotNull(document, "ルートノードは DocumentSyntax である必要があります。");
        Assert.IsNotNull(document.Header, "Document は Header を持つ必要があります。");
        Assert.IsNotNull(document.Header.AuthorLine, "Header は著者行を持つ必要があります。");
    }

    [Then(@"すべての空白と改行が保持されている")]
    public void Thenすべての空白と改行が保持されている()
    {
        // ラウンドトリップが成功していれば、空白と改行も保持されている
        Assert.AreEqual(this.CurrentSourceText, this._reconstructedText, "空白または改行が保持されていません。");
    }

    [Then(@"ソーステキストは BOM を含む")]
    public void ThenソーステキストはBOMを含む()
    {
        Assert.IsNotNull(this.CurrentSyntaxTree, "構文木が null です。");
        Assert.IsTrue(this.CurrentSyntaxTree.Text.HasBom, "ソーステキストは BOM を含む必要があります。");
    }

    [Then(@"BOM を含む元のテキストを復元できる")]
    public void ThenBOMを含む元のテキストを復元できる()
    {
        Assert.IsNotNull(this.CurrentSyntaxTree, "構文木が null です。");
        var originalText = this.CurrentSyntaxTree.ToOriginalString();
        Assert.AreEqual(this.CurrentSourceText, originalText, "BOM を含む元のテキストと一致しません。");
    }
}
