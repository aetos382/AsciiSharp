using System.Linq;

using AsciiSharp.Syntax;
using AsciiSharp.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

public partial class LinkParsingFeature
{
    private SourceText? _sourceText;
    private SyntaxTree? _syntaxTree;

    private void 以下のAsciiDoc文書がある(string text)
    {
        _sourceText = SourceText.From(text);
    }

    private void 文書を解析する()
    {
        Assert.IsNotNull(_sourceText);
        _syntaxTree = SyntaxTree.ParseText(_sourceText);
        Assert.IsNotNull(_syntaxTree);
    }

    private void 構文木が生成される()
    {
        Assert.IsNotNull(_syntaxTree);
    }

    private void 構文木にLinkノードが含まれる()
    {
        Assert.IsNotNull(_syntaxTree);
        var links = _syntaxTree.Root.DescendantNodes().OfType<LinkSyntax>();
        Assert.IsTrue(links.Any(), "Linkノードが見つかりませんでした");
    }

#pragma warning disable CA1054 // URI-like parameters should not be strings
    private void LinkノードのターゲットURLは(string expectedUrl)
#pragma warning restore CA1054 // URI-like parameters should not be strings
    {
        Assert.IsNotNull(_syntaxTree);
        var link = _syntaxTree.Root.DescendantNodes().OfType<LinkSyntax>().FirstOrDefault();
        Assert.IsNotNull(link);
        Assert.AreEqual(expectedUrl, link.Url);
    }

    private void LinkノードのDisplayTextは(string expectedText)
    {
        Assert.IsNotNull(_syntaxTree);
        var link = _syntaxTree.Root.DescendantNodes().OfType<LinkSyntax>().FirstOrDefault();
        Assert.IsNotNull(link);

        var displayText = link.DisplayText?.ToString();
        Assert.AreEqual(expectedText, displayText);
    }

    private void 構文木に_N個のLinkノードが含まれる(int expectedCount)
    {
        Assert.IsNotNull(_syntaxTree);
        var links = _syntaxTree.Root.DescendantNodes().OfType<LinkSyntax>().ToList();
        Assert.HasCount(expectedCount, links);
    }

#pragma warning disable CA1054 // URI-like parameters should not be strings
    private void N番目のLinkノードのターゲットURLは(int index, string expectedUrl)
#pragma warning restore CA1054 // URI-like parameters should not be strings
    {
        Assert.IsNotNull(_syntaxTree);
        var links = _syntaxTree.Root.DescendantNodes().OfType<LinkSyntax>().ToArray();
        Assert.IsTrue(index > 0 && index <= links.Length, $"Linkノード {index} が存在しません");

        var link = links[index - 1];
        Assert.AreEqual(expectedUrl, link.Url);
    }

    private void 構文木から復元したテキストは元の文書と一致する()
    {
        Assert.IsNotNull(_syntaxTree);
        Assert.IsNotNull(_sourceText);

        var reconstructed = _syntaxTree.Root.ToFullString();
        var original = _sourceText.ToString();

        Assert.AreEqual(original, reconstructed);
    }
}
