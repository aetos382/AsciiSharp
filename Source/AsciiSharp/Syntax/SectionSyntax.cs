
using System;
using System.Collections.Generic;

using AsciiSharp.InternalSyntax;

namespace AsciiSharp.Syntax;
/// <summary>
/// AsciiDoc セクションを表す構文ノード。
/// </summary>
/// <remarks>
/// セクションはタイトル行（= で始まる）とその内容で構成される。
/// </remarks>
public sealed class SectionSyntax : SyntaxNode
{
    private readonly List<SyntaxNodeOrToken> _children = [];
    private readonly List<SyntaxNode> _content = [];

    /// <summary>
    /// セクションタイトル。
    /// </summary>
    public SectionTitleSyntax? Title { get; }

    /// <summary>
    /// セクションのレベル（1-6）。
    /// </summary>
    public int Level => this.Title?.Level ?? 0;

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

            // トークンの場合は SyntaxToken としてラップ
            if (slot is InternalToken token)
            {
                this._children.Add(new SyntaxToken(token, this, currentPosition, i));
                currentPosition += slot.FullWidth;
                continue;
            }

            // ノードの場合は適切な型に変換
            // IDE0010: SyntaxKind の全ケースを網羅する必要なし - セクションに関連する種別のみ処理
#pragma warning disable IDE0010
            switch (slot.Kind)
            {
                case SyntaxKind.SectionTitle:
                    this.Title = new SectionTitleSyntax(slot, this, currentPosition, syntaxTree);
                    this._children.Add(new SyntaxNodeOrToken(this.Title));
                    break;

                case SyntaxKind.Section:
                    var section = new SectionSyntax(slot, this, currentPosition, syntaxTree);
                    this._content.Add(section);
                    this._children.Add(new SyntaxNodeOrToken(section));
                    break;

                case SyntaxKind.Paragraph:
                    var paragraph = new ParagraphSyntax(slot, this, currentPosition, syntaxTree);
                    this._content.Add(paragraph);
                    this._children.Add(new SyntaxNodeOrToken(paragraph));
                    break;

                default:
                    break;
            }
#pragma warning restore IDE0010

            currentPosition += slot.FullWidth;
        }
    }

    /// <summary>
    /// セクションの内容（子セクションと段落）。
    /// </summary>
    public IReadOnlyList<SyntaxNode> Content => this._content;

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
            internalNode => new SectionSyntax(internalNode, null, 0, null));
    }

    /// <inheritdoc />
    public override void Accept(ISyntaxVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        visitor.VisitSection(this);
    }

    /// <inheritdoc />
    public override TResult Accept<TResult>(ISyntaxVisitor<TResult> visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        return visitor.VisitSection(this);
    }
}
