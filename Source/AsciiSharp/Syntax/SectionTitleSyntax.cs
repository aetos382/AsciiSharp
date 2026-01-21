namespace AsciiSharp.Syntax;

using System.Collections.Generic;
using AsciiSharp.InternalSyntax;

/// <summary>
/// セクションタイトルを表す構文ノード。
/// </summary>
/// <remarks>
/// セクションタイトルは = で始まる行で、= の数がセクションレベルを示す。
/// </remarks>
public sealed class SectionTitleSyntax : SyntaxNode
{
    /// <summary>
    /// セクションレベル（= の数、1-6）。
    /// </summary>
    public int Level { get; }

    /// <summary>
    /// タイトルのテキスト部分。
    /// </summary>
    public SyntaxToken? TitleText { get; }

    /// <summary>
    /// セクションマーカー（= の並び）。
    /// </summary>
    public SyntaxToken? Marker { get; }

    /// <summary>
    /// SectionTitleSyntax を作成する。
    /// </summary>
    internal SectionTitleSyntax(InternalNode internalNode, SyntaxNode? parent, int position, SyntaxTree? syntaxTree)
        : base(internalNode, parent, position, syntaxTree)
    {
        var currentPosition = position;
        var level = 0;

        for (var i = 0; i < internalNode.SlotCount; i++)
        {
            var slot = internalNode.GetSlot(i);
            if (slot is null)
            {
                continue;
            }

            switch (slot.Kind)
            {
                case SyntaxKind.EqualsToken:
                    level++;
                    if (Marker is null)
                    {
                        Marker = new SyntaxToken((InternalToken)slot, this, currentPosition, i);
                    }

                    break;

                case SyntaxKind.TextToken:
                    TitleText = new SyntaxToken((InternalToken)slot, this, currentPosition, i);
                    break;
            }

            currentPosition += slot.FullWidth;
        }

        Level = level;
    }

    /// <inheritdoc />
    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        if (Marker is not null)
        {
            yield return new SyntaxNodeOrToken(Marker.Value);
        }

        if (TitleText is not null)
        {
            yield return new SyntaxNodeOrToken(TitleText.Value);
        }
    }

    /// <inheritdoc />
    protected override SyntaxNode ReplaceNodeCore(SyntaxNode oldNode, SyntaxNode newNode)
    {
        // TODO: 実装
        throw new System.NotImplementedException();
    }
}
