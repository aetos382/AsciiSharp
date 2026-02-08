
using System;
using System.Collections.Generic;

using AsciiSharp.InternalSyntax;

namespace AsciiSharp.Syntax;
/// <summary>
/// AsciiDoc 文書のヘッダー部分を表す構文ノード。
/// </summary>
/// <remarks>
/// ヘッダーには文書タイトル、著者行などが含まれる。
/// </remarks>
public sealed class DocumentHeaderSyntax : BlockSyntax
{
    private readonly List<SyntaxNodeOrToken> _children = [];

    /// <summary>
    /// 文書タイトル。
    /// </summary>
    public SectionTitleSyntax? Title { get; }

    /// <summary>
    /// 著者行（オプション）。
    /// </summary>
    public AuthorLineSyntax? AuthorLine { get; }

    /// <summary>
    /// DocumentHeaderSyntax を作成する。
    /// </summary>
    internal DocumentHeaderSyntax(InternalNode internalNode, SyntaxNode? parent, int position, SyntaxTree? syntaxTree)
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
            switch (slot.Kind)
            {
                case SyntaxKind.SectionTitle:
                    this.Title = new SectionTitleSyntax(slot, this, currentPosition, syntaxTree);
                    this._children.Add(new SyntaxNodeOrToken(this.Title));
                    break;

                case SyntaxKind.AuthorLine:
                    this.AuthorLine = new AuthorLineSyntax(slot, this, currentPosition, syntaxTree);
                    this._children.Add(new SyntaxNodeOrToken(this.AuthorLine));
                    break;

                default:
                    break;
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
        return this.ReplaceInDescendants(
            oldNode,
            newNode,
            internalNode => new DocumentHeaderSyntax(internalNode, null, 0, null));
    }

    /// <inheritdoc />
    public override void Accept(ISyntaxVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        visitor.VisitDocumentHeader(this);
    }

    /// <inheritdoc />
    public override TResult Accept<TResult>(ISyntaxVisitor<TResult> visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        return visitor.VisitDocumentHeader(this);
    }
}
