
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

            switch (slot.Kind)
            {
                case SyntaxKind.EqualsToken:
                    level++;
                    this.Marker ??= new SyntaxToken((InternalToken)slot, this, currentPosition, i);
                    break;

                case SyntaxKind.WhitespaceToken:
                    if (markerAndSpaceFinished)
                    {
                        // タイトル内の空白
                        var wsToken = (InternalToken)slot;
                        titleBuilder.Append(wsToken.Text);
                    }
                    else
                    {
                        // マーカー後の最初の空白
                        markerAndSpaceFinished = true;
                    }

                    break;

                case SyntaxKind.TextToken:
                    markerAndSpaceFinished = true;
                    var textToken = (InternalToken)slot;
                    titleBuilder.Append(textToken.Text);
                    this.TitleText ??= new SyntaxToken(textToken, this, currentPosition, i);
                    break;

                case SyntaxKind.NewLineToken:
                case SyntaxKind.EndOfFileToken:
                    // 改行やEOFはタイトルに含めない
                    break;
                case SyntaxKind.None:
                    break;
                case SyntaxKind.MissingToken:
                    break;
                case SyntaxKind.SkippedTokensTrivia:
                    break;
                case SyntaxKind.ColonToken:
                    break;
                case SyntaxKind.SlashToken:
                    break;
                case SyntaxKind.OpenBracketToken:
                    break;
                case SyntaxKind.CloseBracketToken:
                    break;
                case SyntaxKind.OpenBraceToken:
                    break;
                case SyntaxKind.CloseBraceToken:
                    break;
                case SyntaxKind.HashToken:
                    break;
                case SyntaxKind.AsteriskToken:
                    break;
                case SyntaxKind.UnderscoreToken:
                    break;
                case SyntaxKind.BacktickToken:
                    break;
                case SyntaxKind.DotToken:
                    break;
                case SyntaxKind.CommaToken:
                    break;
                case SyntaxKind.PipeToken:
                    break;
                case SyntaxKind.LessThanToken:
                    break;
                case SyntaxKind.GreaterThanToken:
                    break;
                case SyntaxKind.WhitespaceTrivia:
                    break;
                case SyntaxKind.EndOfLineTrivia:
                    break;
                case SyntaxKind.SingleLineCommentTrivia:
                    break;
                case SyntaxKind.MultiLineCommentTrivia:
                    break;
                case SyntaxKind.Document:
                    break;
                case SyntaxKind.DocumentHeader:
                    break;
                case SyntaxKind.DocumentBody:
                    break;
                case SyntaxKind.Section:
                    break;
                case SyntaxKind.SectionTitle:
                    break;
                case SyntaxKind.Paragraph:
                    break;
                case SyntaxKind.AuthorLine:
                    break;
                case SyntaxKind.TextSpan:
                    break;
                case SyntaxKind.Text:
                    break;
                case SyntaxKind.Link:
                    break;
                default:
                    // その他のトークンもタイトルの一部として扱う
                    if (markerAndSpaceFinished && slot is InternalToken otherToken)
                    {
                        titleBuilder.Append(otherToken.Text);
                    }

                    break;
            }

            currentPosition += slot.FullWidth;
        }

        this.Level = level;
        this.TitleContent = titleBuilder.ToString().Trim();
    }

    /// <inheritdoc />
    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        if (this.Marker is not null)
        {
            yield return new SyntaxNodeOrToken(this.Marker.Value);
        }

        if (this.TitleText is not null)
        {
            yield return new SyntaxNodeOrToken(this.TitleText.Value);
        }
    }

    /// <inheritdoc />
    protected override SyntaxNode ReplaceNodeCore(SyntaxNode oldNode, SyntaxNode newNode)
    {
        // TODO: 実装
        throw new System.NotImplementedException();
    }
}
