
using System;
using System.Collections.Generic;

using AsciiSharp.InternalSyntax;

namespace AsciiSharp.Syntax;

/// <summary>
/// 著者行を表す構文ノード。
/// </summary>
/// <remarks>
/// AsciiDoc 言語仕様のブロック要素ではないため、<see cref="BlockSyntax"/> を継承しない。
/// </remarks>
public sealed class AuthorLineSyntax : SyntaxNode
{
    private readonly List<SyntaxToken> _tokens = [];

    /// <summary>
    /// 著者行のテキスト。
    /// 行頭・行末のトリビア（空白・改行）を除いたテキスト内容を返す。行中の空白は保持される。
    /// </summary>
    public string Text => this.Internal.ToTrimmedString();

    /// <summary>
    /// AuthorLineSyntax を作成する。
    /// </summary>
    internal AuthorLineSyntax(InternalNode internalNode, SyntaxNode? parent, int position, SyntaxTree? syntaxTree)
        : base(internalNode, parent, position, syntaxTree) // base は SyntaxNode
    {
        var currentPosition = position;

        for (var i = 0; i < internalNode.SlotCount; i++)
        {
            var slot = internalNode.GetSlot(i);
            if (slot is null)
            {
                continue;
            }

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
        visitor.VisitAuthorLine(this);
    }

    /// <inheritdoc />
    public override TResult Accept<TResult>(ISyntaxVisitor<TResult> visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        return visitor.VisitAuthorLine(this);
    }
}
