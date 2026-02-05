
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using AsciiSharp.InternalSyntax;

namespace AsciiSharp.Syntax;
/// <summary>
/// セクションタイトルを表す構文ノード。
/// </summary>
/// <remarks>
/// セクションタイトルは = で始まる行で、= の数がセクションレベルを示す。
/// </remarks>
public sealed class SectionTitleSyntax : BlockSyntax
{
    private readonly List<SyntaxToken> _tokens = [];

    /// <summary>
    /// セクションレベル（= の数、1-6）。
    /// </summary>
    public int Level { get; }

    /// <summary>
    /// セクションマーカー（= の並び）。
    /// マーカー後の空白は TrailingTrivia として保持される。
    /// </summary>
    public SyntaxToken? Marker { get; }

    /// <summary>
    /// タイトルを構成するインライン要素のコレクション。
    /// 構文上の出現順に並ぶ（各要素の Position は前の要素以上）。
    /// </summary>
    public ImmutableArray<InlineSyntax> InlineElements { get; }

    /// <summary>
    /// SectionTitleSyntax を作成する。
    /// </summary>
    internal SectionTitleSyntax(InternalNode internalNode, SyntaxNode? parent, int position, SyntaxTree? syntaxTree)
        : base(internalNode, parent, position, syntaxTree)
    {
        var currentPosition = position;
        var level = 0;
        var inlineElementsBuilder = ImmutableArray.CreateBuilder<InlineSyntax>();
        var titleBuilder = new StringBuilder();
        var markerFinished = false;

        for (var i = 0; i < internalNode.SlotCount; i++)
        {
            var slot = internalNode.GetSlot(i);
            if (slot is null)
            {
                continue;
            }

            // トークンの処理
            if (slot is InternalToken internalToken)
            {
                var token = new SyntaxToken(internalToken, this, currentPosition, i);
                this._tokens.Add(token);

                // 種別ごとの処理
                // IDE0010: SyntaxKind の全ケースを網羅する必要なし - タイトルに関連する種別のみ処理
#pragma warning disable IDE0010
                switch (slot.Kind)
                {
                    case SyntaxKind.EqualsToken:
                        level++;
                        this.Marker ??= token;
                        break;

                    case SyntaxKind.WhitespaceToken:
                        if (markerFinished)
                        {
                            // マーカー後の空白はタイトルの一部
                            titleBuilder.Append(internalToken.Text);
                        }
                        else
                        {
                            // マーカー直後の空白（スキップしてマーカー終了）
                            markerFinished = true;
                        }

                        break;

                    case SyntaxKind.TextToken:
                        markerFinished = true;
                        titleBuilder.Append(internalToken.Text);
                        break;

                    default:
                        // その他のトークンもタイトルの一部として扱う
                        if (markerFinished)
                        {
                            titleBuilder.Append(internalToken.Text);
                        }

                        break;
                }
#pragma warning restore IDE0010
            }
            else if (slot.Kind == SyntaxKind.InlineText)
            {
                // InlineSyntax ノードの処理
                markerFinished = true;
                var inlineText = new InlineTextSyntax(slot, this, currentPosition, syntaxTree);
                inlineElementsBuilder.Add(inlineText);
                titleBuilder.Append(inlineText.Text);
            }

            currentPosition += slot.FullWidth;
        }

        this.Level = level;
        this.InlineElements = inlineElementsBuilder.ToImmutable();
    }

    /// <summary>
    /// タイトルの内容を取得する（InlineElements から構築）。
    /// </summary>
    /// <returns>タイトルのテキスト内容。</returns>
    public string GetTitleContent()
    {
        if (this.InlineElements.IsEmpty)
        {
            // トークンからタイトルを構築（レガシーパス）
            var titleBuilder = new StringBuilder();
            var markerFinished = false;

            foreach (var token in this._tokens)
            {
                switch (token.Kind)
                {
                    case SyntaxKind.EqualsToken:
                        break;

                    case SyntaxKind.WhitespaceToken:
                        if (markerFinished)
                        {
                            titleBuilder.Append(token.Text);
                        }
                        else
                        {
                            markerFinished = true;
                        }

                        break;

                    case SyntaxKind.TextToken:
                        markerFinished = true;
                        titleBuilder.Append(token.Text);
                        break;

                    default:
                        if (markerFinished)
                        {
                            titleBuilder.Append(token.Text);
                        }

                        break;
                }
            }

            return titleBuilder.ToString().Trim();
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
        foreach (var token in this._tokens)
        {
            yield return new SyntaxNodeOrToken(token);
        }

        // InlineElements も子ノードとして返す
        foreach (var element in this.InlineElements)
        {
            yield return new SyntaxNodeOrToken(element);
        }
    }

    /// <inheritdoc />
    protected override SyntaxNode ReplaceNodeCore(SyntaxNode oldNode, SyntaxNode newNode)
    {
        // リーフノードなので、子孫にターゲットノードは存在しない
        return this;
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
