using System.Collections.Generic;
using System.Linq;

using AsciiSharp.Syntax;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

/// <summary>
/// <see cref="MultipleLinesParagraphFeature"/> のステップ実装。
/// </summary>
public sealed partial class MultipleLinesParagraphFeature
{
    private string _input = string.Empty;
    private SyntaxTree? _syntaxTree;
    private List<ParagraphSyntax>? _paragraphs;

    /// <summary>
    /// パーサーが初期化されている。
    /// ParseText 呼び出し時に初期化されるため、ここでは何もしない。
    /// </summary>
    private void パーサーが初期化されている()
    {
        // パーサーの初期化は ParseText 呼び出し時に行われる
    }

    private void 複数行にわたる以下のパラグラフがある(string input)
    {
        _input = input;
    }

    private void 以下の複数パラグラフ文書がある(string input)
    {
        _input = input;
    }

    private void 文書を解析する()
    {
        _syntaxTree = SyntaxTree.ParseText(_input);
        _paragraphs = _syntaxTree.Root
            .DescendantNodes()
            .OfType<ParagraphSyntax>()
            .ToList();
    }

    private void パラグラフのインライン要素が_N個である(int count)
    {
        Assert.IsNotNull(_paragraphs, "パラグラフリストが null です。");
        Assert.IsTrue(_paragraphs.Count > 0, "段落が見つかりません。");

        Assert.HasCount(
            count,
            _paragraphs[0].InlineElements,
            $"インライン要素数が一致しません。期待: {count}, 実際: {_paragraphs[0].InlineElements.Count}");
    }

    private void 最初のインライン要素がInlineTextSyntaxである()
    {
        Assert.IsNotNull(_paragraphs, "パラグラフリストが null です。");
        Assert.IsTrue(
            _paragraphs.Count > 0 && _paragraphs[0].InlineElements.Count > 0,
            "段落またはインライン要素が見つかりません。");

        Assert.IsInstanceOfType<InlineTextSyntax>(
            _paragraphs[0].InlineElements[0],
            "最初のインライン要素は InlineTextSyntax である必要があります。");
    }

    private void InlineTextSyntaxのTextが(string expected)
    {
        Assert.IsNotNull(_paragraphs, "パラグラフリストが null です。");
        Assert.IsTrue(
            _paragraphs.Count > 0 && _paragraphs[0].InlineElements.Count > 0,
            "段落またはインライン要素が見つかりません。");

        var inlineText = _paragraphs[0].InlineElements[0] as InlineTextSyntax;
        Assert.IsNotNull(inlineText, "最初のインライン要素は InlineTextSyntax である必要があります。");

        Assert.AreEqual(
            expected,
            inlineText.Text,
            $"InlineTextSyntax のテキストが一致しません。" +
            $"期待: '{expected.Replace("\n", "\\n", System.StringComparison.Ordinal)}', " +
            $"実際: '{inlineText.Text.Replace("\n", "\\n", System.StringComparison.Ordinal)}'");
    }

    private void InlineTextSyntaxのSpanEndが最終行末尾コンテンツの次の位置である()
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        Assert.IsNotNull(_paragraphs, "パラグラフリストが null です。");
        Assert.IsTrue(
            _paragraphs.Count > 0 && _paragraphs[0].InlineElements.Count > 0,
            "段落またはインライン要素が見つかりません。");

        var inlineText = _paragraphs[0].InlineElements[0] as InlineTextSyntax;
        Assert.IsNotNull(inlineText, "最初のインライン要素は InlineTextSyntax である必要があります。");

        // Span 範囲のテキストに末尾改行が含まれないことを確認（最終行の改行はトリビア）
        var spanText = _syntaxTree.Text.GetText(inlineText.Span);
        Assert.IsFalse(
            spanText.EndsWith('\n') || spanText.EndsWith('\r'),
            $"InlineTextSyntax の Span が末尾の改行を含んでいます。" +
            $"Span テキスト: '{spanText.Replace("\n", "\\n", System.StringComparison.Ordinal).Replace("\r", "\\r", System.StringComparison.Ordinal)}'");
    }

    private void 最初のパラグラフのSpanが改行を含まない()
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        Assert.IsNotNull(_paragraphs, "パラグラフリストが null です。");
        Assert.IsTrue(_paragraphs.Count > 0, "段落が見つかりません。");

        var paragraph = _paragraphs[0];
        var spanText = _syntaxTree.Text.GetText(paragraph.Span);

        Assert.IsFalse(
            spanText.EndsWith('\n') || spanText.EndsWith('\r'),
            $"最初の段落の Span が末尾の改行を含んでいます。" +
            $"Span テキスト: '{spanText.Replace("\n", "\\n", System.StringComparison.Ordinal).Replace("\r", "\\r", System.StringComparison.Ordinal)}'");
    }

    private void 最後のパラグラフのSpanが改行を含まない()
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");
        Assert.IsNotNull(_paragraphs, "パラグラフリストが null です。");
        Assert.IsTrue(_paragraphs.Count > 0, "段落が見つかりません。");

        var paragraph = _paragraphs[_paragraphs.Count - 1];
        var spanText = _syntaxTree.Text.GetText(paragraph.Span);

        Assert.IsFalse(
            spanText.EndsWith('\n') || spanText.EndsWith('\r'),
            $"最後の段落の Span が末尾の改行を含んでいます。" +
            $"Span テキスト: '{spanText.Replace("\n", "\\n", System.StringComparison.Ordinal).Replace("\r", "\\r", System.StringComparison.Ordinal)}'");
    }

    private void 構文木から復元したテキストは元の文書と一致する()
    {
        Assert.IsNotNull(_syntaxTree, "構文木が null です。");

        var reconstructed = _syntaxTree.Root.ToFullString();
        Assert.AreEqual(
            _input,
            reconstructed,
            $"ラウンドトリップが失敗しました。" +
            $"元テキスト: '{_input.Replace("\n", "\\n", System.StringComparison.Ordinal)}', " +
            $"復元テキスト: '{reconstructed.Replace("\n", "\\n", System.StringComparison.Ordinal)}'");
    }

    private void 二番目のインライン要素がLinkSyntaxである()
    {
        Assert.IsNotNull(_paragraphs, "パラグラフリストが null です。");
        Assert.IsTrue(
            _paragraphs.Count > 0 && _paragraphs[0].InlineElements.Count > 1,
            "段落またはインライン要素が 2 個以上存在しません。");

        Assert.IsInstanceOfType<LinkSyntax>(
            _paragraphs[0].InlineElements[1],
            "2 番目のインライン要素は LinkSyntax である必要があります。");
    }
}
