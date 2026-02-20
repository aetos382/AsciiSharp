using System;
using System.Collections.Generic;
using System.Linq;

using AsciiSharp.InternalSyntax;
using AsciiSharp.Syntax;
using AsciiSharp.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

public partial class IncrementalParsingFeature
{
    private SourceText? _sourceText;
    private SyntaxTree? _syntaxTree;
    private SyntaxTree? _incrementalSyntaxTree;
    private string? _modifiedSourceText;
    private string? _reconstructedText;
    private readonly Dictionary<int, InternalNode> _sectionInternalNodesByIndex = [];
    private readonly Dictionary<string, InternalNode> _sectionInternalNodesByName = [];

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

    private void すべてのセクションの内部ノードへの参照を保持する()
    {
        Assert.IsNotNull(_syntaxTree);
        _sectionInternalNodesByIndex.Clear();

        var sections = _syntaxTree.Root.DescendantNodes().OfType<SectionSyntax>().ToArray();
        for (int i = 0; i < sections.Length; i++)
        {
            _sectionInternalNodesByIndex[i + 1] = sections[i].Internal;
        }
    }

    private void 名前のセクションの内部ノードへの参照を保持する(string sectionName)
    {
        Assert.IsNotNull(_syntaxTree);
        _sectionInternalNodesByName.Clear();

        var sections = _syntaxTree.Root.DescendantNodes().OfType<SectionSyntax>();
        foreach (var section in sections)
        {
            var titleText = section.Title?.InlineElements
                .OfType<InlineTextSyntax>()
                .Select(t => t.Text)
                .FirstOrDefault();

            if (titleText == sectionName)
            {
                _sectionInternalNodesByName[sectionName] = section.Internal;
                break;
            }
        }
    }

    private void テキストを変更して増分解析する(string oldText, string newText)
    {
        Assert.IsNotNull(_sourceText);
        Assert.IsNotNull(_syntaxTree);

        var originalText = _sourceText.ToString();
        var startIndex = originalText.IndexOf(oldText, StringComparison.Ordinal);
        Assert.IsTrue(startIndex >= 0, $"テキスト '{oldText}' が見つかりませんでした");

        var span = new TextSpan(startIndex, oldText.Length);
        var change = new TextChange(span, newText);

        _incrementalSyntaxTree = _syntaxTree.WithChanges(change);
        Assert.IsNotNull(_incrementalSyntaxTree);

        _modifiedSourceText = originalText.Remove(startIndex, oldText.Length).Insert(startIndex, newText);
    }

    private void 増分解析後の構文木から完全なテキストを取得する()
    {
        Assert.IsNotNull(_incrementalSyntaxTree);
        _reconstructedText = _incrementalSyntaxTree.Root.ToFullString();
    }

    private void インデックスのセクションの内部ノードは再利用されている(int sectionIndex)
    {
        Assert.IsNotNull(_incrementalSyntaxTree);
        Assert.IsTrue(_sectionInternalNodesByIndex.ContainsKey(sectionIndex),
            $"セクション {sectionIndex} の参照が保持されていません");

        var sections = _incrementalSyntaxTree.Root.DescendantNodes().OfType<SectionSyntax>().ToArray();
        Assert.IsTrue(sectionIndex > 0 && sectionIndex <= sections.Length,
            $"セクション {sectionIndex} が存在しません");

        var storedInternal = _sectionInternalNodesByIndex[sectionIndex];
        var currentInternal = sections[sectionIndex - 1].Internal;

        Assert.AreSame(storedInternal, currentInternal,
            $"セクション {sectionIndex} の内部ノードが再利用されていません");
    }

    private void 名前のセクションの内部ノードは再利用されている(string sectionName)
    {
        Assert.IsNotNull(_incrementalSyntaxTree);
        Assert.IsTrue(_sectionInternalNodesByName.ContainsKey(sectionName),
            $"セクション '{sectionName}' の参照が保持されていません");

        var sections = _incrementalSyntaxTree.Root.DescendantNodes().OfType<SectionSyntax>();
        InternalNode? currentInternal = null;

        foreach (var section in sections)
        {
            var titleText = section.Title?.InlineElements
                .OfType<InlineTextSyntax>()
                .Select(t => t.Text)
                .FirstOrDefault();

            if (titleText == sectionName)
            {
                currentInternal = section.Internal;
                break;
            }
        }

        Assert.IsNotNull(currentInternal, $"セクション '{sectionName}' が見つかりませんでした");

        var storedInternal = _sectionInternalNodesByName[sectionName];
        Assert.AreSame(storedInternal, currentInternal,
            $"セクション '{sectionName}' の内部ノードが再利用されていません");
    }

    private void 再構築されたテキストは変更後の文書と一致する()
    {
        Assert.IsNotNull(_modifiedSourceText);
        Assert.IsNotNull(_reconstructedText);
        Assert.AreEqual(_modifiedSourceText, _reconstructedText);
    }
}
