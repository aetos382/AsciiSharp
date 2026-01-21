
using System.Collections.Generic;
using System.Text;

using AsciiSharp.InternalSyntax;

namespace AsciiSharp.Syntax;
/// <summary>
/// セクションタイトルを表す構文ノード。
/// </summary>
/// <remarks>
/// セクションタイトルは = で始まる行で、= の数がセクションレベルを示す。
/// </remarks>
public sealed class SectionTitleSyntax : SyntaxNode
{
    private readonly List<SyntaxToken> _tokens = [];

    /// <summary>
    /// セクションレベル（= の数、1-6）。
    /// </summary>
    public int Level { get; }

    /// <summary>
    /// タイトルのテキスト部分（最初のテキストトークン）。
    /// </summary>
    public SyntaxToken? TitleText { get; }

    /// <summary>
    /// セクションマーカー（= の並び）。
    /// </summary>
    public SyntaxToken? Marker { get; }

    /// <summary>
    /// タイトルの内容（マーカーと空白を除いた全テキスト）。
    /// </summary>
    public string TitleContent { get; }

    /// <summary>
    /// SectionTitleSyntax を作成する。
    /// </summary>
    internal SectionTitleSyntax(InternalNode internalNode, SyntaxNode? parent, int position, SyntaxTree? syntaxTree)
        : base(internalNode, parent, position, syntaxTree)
    {
        var currentPosition = position;
        var level = 0;
        var titleBuilder = new StringBuilder();
        var markerAndSpaceFinished = false;

        for (var i = 0; i < internalNode.SlotCount; i++)
        {
            var slot = internalNode.GetSlot(i);
            if (slot is null)
            {
                continue;
            }

            // すべてのトークンをリストに追加
            if (slot is InternalToken internalToken)
            {
                var token = new SyntaxToken(internalToken, this, currentPosition, i);
                this._tokens.Add(token);

                // 種別ごとの処理
                switch (slot.Kind)
                {
                    case SyntaxKind.EqualsToken:
                        level++;
                        this.Marker ??= token;
                        break;

                    case SyntaxKind.WhitespaceToken:
                        if (markerAndSpaceFinished)
                        {
                            titleBuilder.Append(internalToken.Text);
                        }
                        else
                        {
                            markerAndSpaceFinished = true;
                        }

                        break;

                    case SyntaxKind.TextToken:
                        markerAndSpaceFinished = true;
                        titleBuilder.Append(internalToken.Text);
                        this.TitleText ??= token;
                        break;

                    default:
                        // その他のトークンもタイトルの一部として扱う
                        if (markerAndSpaceFinished)
                        {
                            titleBuilder.Append(internalToken.Text);
                        }

                        break;
                }
            }

            currentPosition += slot.FullWidth;
        }

        this.Level = level;
        this.TitleContent = titleBuilder.ToString().Trim();
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
        // TODO: 実装
        throw new System.NotImplementedException();
    }
}
