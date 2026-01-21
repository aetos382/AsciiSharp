namespace AsciiSharp.Syntax;

using System.Collections.Generic;
using AsciiSharp.InternalSyntax;

/// <summary>
/// AsciiDoc の段落を表す構文ノード。
/// </summary>
/// <remarks>
/// 段落は空行で区切られたテキストのブロック。
/// </remarks>
public sealed class ParagraphSyntax : SyntaxNode
{
    private readonly List<SyntaxNode> _inlineElements = [];

    /// <summary>
    /// 段落内のインライン要素。
    /// </summary>
    public IReadOnlyList<SyntaxNode> InlineElements => _inlineElements;

    /// <summary>
    /// ParagraphSyntax を作成する。
    /// </summary>
    internal ParagraphSyntax(InternalNode internalNode, SyntaxNode? parent, int position, SyntaxTree? syntaxTree)
        : base(internalNode, parent, position, syntaxTree)
    {
        var currentPosition = position;

        for (var i = 0; i < internalNode.SlotCount; i++)
        {
            var slot = internalNode.GetSlot(i);
            if (slot is null)
            {
                continue;
            }

            switch (slot.Kind)
            {
                case SyntaxKind.Text:
                    _inlineElements.Add(new TextSyntax(slot, this, currentPosition, syntaxTree));
                    break;

                case SyntaxKind.Link:
                    _inlineElements.Add(new LinkSyntax(slot, this, currentPosition, syntaxTree));
                    break;
            }

            currentPosition += slot.FullWidth;
        }
    }

    /// <inheritdoc />
    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        foreach (var element in _inlineElements)
        {
            yield return new SyntaxNodeOrToken(element);
        }
    }

    /// <inheritdoc />
    protected override SyntaxNode ReplaceNodeCore(SyntaxNode oldNode, SyntaxNode newNode)
    {
        // TODO: 実装
        throw new System.NotImplementedException();
    }
}
