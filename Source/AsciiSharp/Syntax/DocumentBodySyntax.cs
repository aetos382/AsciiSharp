namespace AsciiSharp.Syntax;

using System.Collections.Generic;
using AsciiSharp.InternalSyntax;

/// <summary>
/// AsciiDoc 文書の本体部分を表す構文ノード。
/// </summary>
/// <remarks>
/// 本体にはセクション、段落、その他のブロック要素が含まれる。
/// </remarks>
public sealed class DocumentBodySyntax : SyntaxNode
{
    private readonly List<SyntaxNode> _children = [];

    /// <summary>
    /// DocumentBodySyntax を作成する。
    /// </summary>
    internal DocumentBodySyntax(InternalNode internalNode, SyntaxNode? parent, int position, SyntaxTree? syntaxTree)
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

            SyntaxNode? child = slot.Kind switch
            {
                SyntaxKind.Section => new SectionSyntax(slot, this, currentPosition, syntaxTree),
                SyntaxKind.Paragraph => new ParagraphSyntax(slot, this, currentPosition, syntaxTree),
                _ => null
            };

            if (child is not null)
            {
                _children.Add(child);
            }

            currentPosition += slot.FullWidth;
        }
    }

    /// <inheritdoc />
    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        foreach (var child in _children)
        {
            yield return new SyntaxNodeOrToken(child);
        }
    }

    /// <inheritdoc />
    protected override SyntaxNode ReplaceNodeCore(SyntaxNode oldNode, SyntaxNode newNode)
    {
        // TODO: 実装
        throw new System.NotImplementedException();
    }
}
