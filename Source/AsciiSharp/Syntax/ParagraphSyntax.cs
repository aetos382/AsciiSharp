
using System;
using System.Collections.Generic;

using AsciiSharp.InternalSyntax;

namespace AsciiSharp.Syntax;
/// <summary>
/// AsciiDoc の段落を表す構文ノード。
/// </summary>
/// <remarks>
/// 段落は空行で区切られたテキストのブロック。
/// </remarks>
public sealed class ParagraphSyntax : SyntaxNode
{
    private readonly List<SyntaxNode> _inlineElements = [];
    private readonly List<SyntaxNodeOrToken> _children = [];

    /// <summary>
    /// 段落内のインライン要素。
    /// </summary>
    public IReadOnlyList<SyntaxNode> InlineElements => this._inlineElements;

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

            // トークンの場合は SyntaxToken としてラップ
            if (slot is InternalToken token)
            {
                this._children.Add(new SyntaxToken(token, this, currentPosition, i));
                currentPosition += slot.FullWidth;
                continue;
            }

            // ノードの場合は適切な型に変換
            // IDE0072: SyntaxKind の全ケースを網羅する必要なし - 段落に関連する種別のみ処理
#pragma warning disable IDE0072
            SyntaxNode? child = slot.Kind switch
            {
                SyntaxKind.Text => new TextSyntax(slot, this, currentPosition, syntaxTree),
                SyntaxKind.Link => new LinkSyntax(slot, this, currentPosition, syntaxTree),
                _ => null
            };
#pragma warning restore IDE0072

            if (child is not null)
            {
                this._inlineElements.Add(child);
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
        return this.ReplaceInDescendants(
            oldNode,
            newNode,
            internalNode => new ParagraphSyntax(internalNode, null, 0, null));
    }

    /// <inheritdoc />
    public override void Accept(ISyntaxVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        visitor.VisitParagraph(this);
    }

    /// <inheritdoc />
    public override TResult Accept<TResult>(ISyntaxVisitor<TResult> visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        return visitor.VisitParagraph(this);
    }
}
