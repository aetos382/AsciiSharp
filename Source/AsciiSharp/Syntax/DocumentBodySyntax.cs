
using System.Collections.Generic;

using AsciiSharp.InternalSyntax;

namespace AsciiSharp.Syntax;
/// <summary>
/// AsciiDoc 文書の本体部分を表す構文ノード。
/// </summary>
/// <remarks>
/// 本体にはセクション、段落、その他のブロック要素が含まれる。
/// </remarks>
public sealed class DocumentBodySyntax : SyntaxNode
{
    private readonly List<SyntaxNodeOrToken> _children = [];

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

            // トークンの場合は SyntaxToken としてラップ
            if (slot is InternalToken token)
            {
                this._children.Add(new SyntaxToken(token, this, currentPosition, i));
                currentPosition += slot.FullWidth;
                continue;
            }

            // ノードの場合は適切な型に変換
            SyntaxNode? child = slot.Kind switch
            {
                SyntaxKind.Section => new SectionSyntax(slot, this, currentPosition, syntaxTree),
                SyntaxKind.Paragraph => new ParagraphSyntax(slot, this, currentPosition, syntaxTree),
                _ => null
            };

            if (child is not null)
            {
                this._children.Add(new SyntaxNodeOrToken(child));
            }

            currentPosition += slot.FullWidth;
        }
    }

    /// <inheritdoc />
    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        foreach (var child in this._children)
        {
            yield return child;
        }
    }

    /// <inheritdoc />
    protected override SyntaxNode ReplaceNodeCore(SyntaxNode oldNode, SyntaxNode newNode)
    {
        // TODO: 実装
        throw new System.NotImplementedException();
    }
}
