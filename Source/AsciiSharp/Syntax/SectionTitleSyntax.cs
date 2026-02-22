
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using AsciiSharp.InternalSyntax;

namespace AsciiSharp.Syntax;
/// <summary>
/// セクションタイトルを表す構文ノード。
/// </summary>
/// <remarks>
/// <para>セクションタイトルは = で始まる行で、= の数がセクションレベルを示す。</para>
/// <para>AsciiDoc 言語仕様において heading は section の structural form であり、
/// 独立したブロック要素ではないため、<see cref="BlockSyntax"/> を継承しない。</para>
/// </remarks>
public sealed class SectionTitleSyntax : SyntaxNode
{
    private readonly List<SyntaxNodeOrToken> _children = [];

    /// <summary>
    /// セクションレベル（マーカー内の = の文字数）。
    /// </summary>
    public int Level { get; }

    /// <summary>
    /// セクションマーカー（連続する = をまとめた単一トークン、例: "==", "==="）。
    /// マーカー後の空白はこのトークンの TrailingTrivia として保持される。
    /// </summary>
    public SyntaxToken? Marker { get; }

    /// <summary>
    /// タイトルを構成するインライン要素のコレクション。
    /// 構文上の出現順に並ぶ（各要素の Position は前の要素以上）。
    /// </summary>
    public SyntaxList<InlineSyntax> InlineElements { get; }

    /// <summary>
    /// SectionTitleSyntax を作成する。
    /// </summary>
    internal SectionTitleSyntax(InternalNode internalNode, SyntaxNode? parent, int position, SyntaxTree? syntaxTree)
        : base(internalNode, parent, position, syntaxTree)
    {
        var currentPosition = position;
        SyntaxToken? marker = null;
        var inlineElementsList = new List<InlineSyntax>();

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

                if (slot.Kind == SyntaxKind.EqualsToken)
                {
                    marker = token;
                }
            }
            else if (slot.Kind == SyntaxKind.InlineText)
            {
                var inlineText = new InlineTextSyntax(slot, this, currentPosition, syntaxTree);
                inlineElementsList.Add(inlineText);
                this._children.Add(new SyntaxNodeOrToken(inlineText));
            }
            else
            {
                Debug.Fail($"SectionTitleSyntax: 未知のスロット種別 {slot.Kind}");
            }

            currentPosition += slot.FullWidth;
        }

        this.Level = marker?.Text.Length ?? 0;
        this.Marker = marker;
        this.InlineElements = new SyntaxList<InlineSyntax>(inlineElementsList);
    }

    /// <summary>
    /// タイトルの内容を取得する（InlineElements から構築）。
    /// </summary>
    /// <returns>タイトルのテキスト内容。</returns>
    public string GetTitleContent()
    {
        if (this.InlineElements.IsEmpty)
        {
            return string.Empty;
        }

        // InlineElements からタイトルを構築
        var sb = new StringBuilder();
        foreach (var element in this.InlineElements)
        {
            sb.Append(element.ToFullString());
        }

        return sb.ToString().Trim();
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
            internalNode => new SectionTitleSyntax(internalNode, null, 0, null));
    }

    /// <inheritdoc />
    public override void Accept(ISyntaxVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        visitor.VisitSectionTitle(this);
    }

    /// <inheritdoc />
    public override TResult Accept<TResult>(ISyntaxVisitor<TResult> visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        return visitor.VisitSectionTitle(this);
    }
}
