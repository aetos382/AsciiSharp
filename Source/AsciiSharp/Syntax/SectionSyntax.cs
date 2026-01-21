namespace AsciiSharp.Syntax;

using System.Collections.Generic;
using AsciiSharp.InternalSyntax;

/// <summary>
/// AsciiDoc セクションを表す構文ノード。
/// </summary>
/// <remarks>
/// セクションはタイトル行（= で始まる）とその内容で構成される。
/// </remarks>
public sealed class SectionSyntax : SyntaxNode
{
    private readonly List<SyntaxNode> _content = [];

    /// <summary>
    /// セクションタイトル。
    /// </summary>
    public SectionTitleSyntax? Title { get; }

    /// <summary>
    /// セクションのレベル（1-6）。
    /// </summary>
    public int Level => Title?.Level ?? 0;

    /// <summary>
    /// SectionSyntax を作成する。
    /// </summary>
    internal SectionSyntax(InternalNode internalNode, SyntaxNode? parent, int position, SyntaxTree? syntaxTree)
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
                case SyntaxKind.SectionTitle:
                    Title = new SectionTitleSyntax(slot, this, currentPosition, syntaxTree);
                    break;

                case SyntaxKind.Section:
                    _content.Add(new SectionSyntax(slot, this, currentPosition, syntaxTree));
                    break;

                case SyntaxKind.Paragraph:
                    _content.Add(new ParagraphSyntax(slot, this, currentPosition, syntaxTree));
                    break;
            }

            currentPosition += slot.FullWidth;
        }
    }

    /// <summary>
    /// セクションの内容（子セクションと段落）。
    /// </summary>
    public IReadOnlyList<SyntaxNode> Content => _content;

    /// <inheritdoc />
    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        if (Title is not null)
        {
            yield return new SyntaxNodeOrToken(Title);
        }

        foreach (var child in _content)
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
