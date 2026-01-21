namespace AsciiSharp.Syntax;

using System.Collections.Generic;
using AsciiSharp.InternalSyntax;

/// <summary>
/// AsciiDoc 文書全体を表す構文ノード。
/// </summary>
public sealed class DocumentSyntax : SyntaxNode
{
    /// <summary>
    /// 文書ヘッダー。
    /// </summary>
    public DocumentHeaderSyntax? Header { get; }

    /// <summary>
    /// 文書本体。
    /// </summary>
    public DocumentBodySyntax? Body { get; }

    /// <summary>
    /// DocumentSyntax を作成する。
    /// </summary>
    internal DocumentSyntax(InternalNode internalNode, SyntaxNode? parent, int position, SyntaxTree? syntaxTree)
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
                case SyntaxKind.DocumentHeader:
                    Header = new DocumentHeaderSyntax(slot, this, currentPosition, syntaxTree);
                    break;

                case SyntaxKind.DocumentBody:
                    Body = new DocumentBodySyntax(slot, this, currentPosition, syntaxTree);
                    break;
            }

            currentPosition += slot.FullWidth;
        }
    }

    /// <inheritdoc />
    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        if (Header is not null)
        {
            yield return new SyntaxNodeOrToken(Header);
        }

        if (Body is not null)
        {
            yield return new SyntaxNodeOrToken(Body);
        }
    }

    /// <inheritdoc />
    protected override SyntaxNode ReplaceNodeCore(SyntaxNode oldNode, SyntaxNode newNode)
    {
        // TODO: 実装
        throw new System.NotImplementedException();
    }
}
