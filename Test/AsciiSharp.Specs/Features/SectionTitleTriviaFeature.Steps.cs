using System.Linq;

using AsciiSharp.Syntax;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

public partial class SectionTitleTriviaFeature
{
    private string? _sourceText;
    private SyntaxTree? _syntaxTree;
    private string? _reconstructedText;

    private void パーサーが初期化されている()
    {
        // 特に初期化処理は不要
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

    private void セクションタイトルのマーカーはTrailingTriviaに空白を持つ()
    {
        var tree = _syntaxTree;
        Assert.IsNotNull(tree);
        var document = tree.Root as DocumentSyntax;
        Assert.IsNotNull(document);

        // 最初のセクションタイトルを取得（ヘッダーまたは本文の最初のセクション）
        SectionTitleSyntax? sectionTitle = document.Header?.Title;
        if (sectionTitle is null)
        {
            var firstSection = document.Body?.ChildNodesAndTokens()
                .Where(c => c.IsNode && c.AsNode()?.Kind == SyntaxKind.Section)
                .Select(c => c.AsNode() as SectionSyntax)
                .FirstOrDefault();
            sectionTitle = firstSection?.Title;
        }

        Assert.IsNotNull(sectionTitle);
        Assert.IsNotNull(sectionTitle.Marker);
        var marker = sectionTitle.Marker.Value;
        var hasWhitespaceTrivia = marker.TrailingTrivia
            .Any(t => t.Kind == SyntaxKind.WhitespaceTrivia);
        Assert.IsTrue(hasWhitespaceTrivia, "マーカーの TrailingTrivia に空白がありません。");
    }
}
