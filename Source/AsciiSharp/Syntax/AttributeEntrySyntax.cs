
using System;
using System.Collections.Generic;

using AsciiSharp.InternalSyntax;

namespace AsciiSharp.Syntax;

/// <summary>
/// AsciiDoc の属性エントリ（<c>:name: value</c>）を表す構文ノード。
/// </summary>
public sealed class AttributeEntrySyntax : BlockSyntax
{
    private readonly List<SyntaxNodeOrToken> _children = [];

    /// <summary>
    /// 開きコロン。
    /// </summary>
    public SyntaxToken OpeningColon { get; }

    /// <summary>
    /// 属性名トークン。
    /// </summary>
    public SyntaxToken NameToken { get; }

    /// <summary>
    /// 閉じコロン。
    /// </summary>
    public SyntaxToken ClosingColon { get; }

    /// <summary>
    /// 属性値トークン（省略可）。
    /// </summary>
    public SyntaxToken? ValueToken { get; }

    /// <summary>
    /// 属性名のテキスト。
    /// </summary>
    public string Name => this.NameToken.Text;

    /// <summary>
    /// 属性値のテキスト。値がない場合は空文字列。
    /// </summary>
    public string Value => this.ValueToken?.Text ?? string.Empty;

    /// <summary>
    /// AttributeEntrySyntax を作成する。
    /// </summary>
    internal AttributeEntrySyntax(InternalNode internalNode, SyntaxNode? parent, int position, SyntaxTree? syntaxTree)
        : base(internalNode, parent, position, syntaxTree)
    {
        var currentPosition = position;
        var slotIndex = 0;

        for (var i = 0; i < internalNode.SlotCount; i++)
        {
            var slot = internalNode.GetSlot(i);
            if (slot is null)
            {
                continue;
            }

            if (slot is InternalToken internalToken)
            {
                var token = new SyntaxToken(internalToken, this, currentPosition, i);
                this._children.Add(new SyntaxNodeOrToken(token));

                switch (slotIndex)
                {
                    case 0:
                        this.OpeningColon = token;
                        break;

                    case 1:
                        this.NameToken = token;
                        break;

                    case 2:
                        this.ClosingColon = token;
                        break;

                    case 3:
                        this.ValueToken = token;
                        break;
                }

                slotIndex++;
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
        // 子はすべてトークンなので、子孫にターゲットノードは存在しない
        return this;
    }

    /// <inheritdoc />
    public override void Accept(ISyntaxVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        visitor.VisitAttributeEntry(this);
    }

    /// <inheritdoc />
    public override TResult Accept<TResult>(ISyntaxVisitor<TResult> visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        return visitor.VisitAttributeEntry(this);
    }
}
