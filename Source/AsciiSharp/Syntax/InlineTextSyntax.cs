
using System;
using System.Collections.Generic;

using AsciiSharp.InternalSyntax;

namespace AsciiSharp.Syntax;
/// <summary>
/// インライン テキストを表す構文ノード。
/// 一行のプレーンテキストを表すインライン要素。
/// </summary>
public sealed class InlineTextSyntax : InlineSyntax
{
    private readonly List<SyntaxToken> _tokens = [];

    /// <summary>
    /// テキストの内容（先行・後続トリビアを除く）。
    /// </summary>
    public string Text => this.Internal.ToTrimmedString();

    /// <summary>
    /// InlineTextSyntax を作成する。
    /// </summary>
    internal InlineTextSyntax(InternalNode internalNode, SyntaxNode? parent, int position, SyntaxTree? syntaxTree)
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

            // トークンを収集
            if (slot is InternalToken internalToken)
            {
                this._tokens.Add(new SyntaxToken(internalToken, this, currentPosition, i));
            }

            currentPosition += slot.FullWidth;
        }
    }

    /// <inheritdoc />
    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        foreach (var token in this._tokens)
        {
            yield return new SyntaxNodeOrToken(token);
        }
    }

    /// <inheritdoc />
    protected override SyntaxNode ReplaceNodeCore(SyntaxNode oldNode, SyntaxNode newNode)
    {
        // 子はすべてトークンなので、子孫にターゲットノードは存在しない
        return this;
    }

    /// <inheritdoc />
    public override void Accept(ISyntaxVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        visitor.VisitInlineText(this);
    }

    /// <inheritdoc />
    public override TResult Accept<TResult>(ISyntaxVisitor<TResult> visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        return visitor.VisitInlineText(this);
    }
}
