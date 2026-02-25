
using System;
using System.Collections.Generic;

using AsciiSharp.InternalSyntax;

namespace AsciiSharp.Syntax;
/// <summary>
/// インライン テキストを表す構文ノード。
/// 1 つまたは複数の連続する行からなるプレーンテキストを表すインライン要素。
/// 中間行の改行はコンテンツとして保持され、最終行の改行はトリビアとして付与される。
/// </summary>
public sealed class InlineTextSyntax : InlineSyntax
{
    private readonly List<SyntaxToken> _tokens = [];

    /// <summary>
    /// テキストの内容。
    /// 先頭・末尾のトリビア（空白・改行）を除いたテキスト内容を返す。
    /// 行中の空白および複数行テキストの中間改行はコンテンツとして保持される。
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
