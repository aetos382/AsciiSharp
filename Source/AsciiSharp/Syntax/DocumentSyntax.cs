
using System;
using System.Collections.Generic;

using AsciiSharp.InternalSyntax;

namespace AsciiSharp.Syntax;
/// <summary>
/// AsciiDoc 文書全体を表す構文ノード。
/// </summary>
public sealed class DocumentSyntax : BlockSyntax
{
    private readonly List<SyntaxNodeOrToken> _children = [];

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

            // トークンの場合は SyntaxToken としてラップ
            if (slot is InternalToken token)
            {
                this._children.Add(new SyntaxToken(token, this, currentPosition, i));
                currentPosition += slot.FullWidth;
                continue;
            }

            // ノードの場合は適切な型に変換
            // IDE0010: SyntaxKind の全ケースを網羅する必要なし - 文書に関連する種別のみ処理
#pragma warning disable IDE0010
            switch (slot.Kind)
            {
                case SyntaxKind.DocumentHeader:
                    this.Header = new DocumentHeaderSyntax(slot, this, currentPosition, syntaxTree);
                    this._children.Add(new SyntaxNodeOrToken(this.Header));
                    break;

                case SyntaxKind.DocumentBody:
                    this.Body = new DocumentBodySyntax(slot, this, currentPosition, syntaxTree);
                    this._children.Add(new SyntaxNodeOrToken(this.Body));
                    break;

                default:
                    break;
            }
#pragma warning restore IDE0010

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
            internalNode => new DocumentSyntax(internalNode, null, 0, null));
    }

    /// <inheritdoc />
    public override void Accept(ISyntaxVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        visitor.VisitDocument(this);
    }

    /// <inheritdoc />
    public override TResult Accept<TResult>(ISyntaxVisitor<TResult> visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        return visitor.VisitDocument(this);
    }
}
