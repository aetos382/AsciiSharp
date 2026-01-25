
using System.Collections.Generic;

using AsciiSharp.InternalSyntax;

namespace AsciiSharp.Syntax;
/// <summary>
/// AsciiDoc の段落を表す構文ノード。
/// </summary>
/// <remarks>
/// 段落は空行で区切られたテキストのブロック。
/// </remarks>
public sealed class ParagraphSyntax : SyntaxNode
{
    private readonly List<SyntaxNode> _inlineElements = [];

    /// <summary>
    /// 段落内のインライン要素。
    /// </summary>
    public IReadOnlyList<SyntaxNode> InlineElements => this._inlineElements;

    /// <summary>
    /// ParagraphSyntax を作成する。
    /// </summary>
    internal ParagraphSyntax(InternalNode internalNode, SyntaxNode? parent, int position, SyntaxTree? syntaxTree)
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

            switch (slot.Kind)
            {
                case SyntaxKind.Text:
                    this._inlineElements.Add(new TextSyntax(slot, this, currentPosition, syntaxTree));
                    break;

                case SyntaxKind.Link:
                    this._inlineElements.Add(new LinkSyntax(slot, this, currentPosition, syntaxTree));
                    break;
                case SyntaxKind.None:
                    break;
                case SyntaxKind.MissingToken:
                    break;
                case SyntaxKind.SkippedTokensTrivia:
                    break;
                case SyntaxKind.EndOfFileToken:
                    break;
                case SyntaxKind.NewLineToken:
                    break;
                case SyntaxKind.WhitespaceToken:
                    break;
                case SyntaxKind.TextToken:
                    break;
                case SyntaxKind.EqualsToken:
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
                default:
                    break;
            }

            currentPosition += slot.FullWidth;
        }
    }

    /// <inheritdoc />
    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        foreach (var element in this._inlineElements)
        {
            yield return new SyntaxNodeOrToken(element);
        }
    }

    /// <inheritdoc />
    protected override SyntaxNode ReplaceNodeCore(SyntaxNode oldNode, SyntaxNode newNode)
    {
        return this.ReplaceInDescendants(
            oldNode,
            newNode,
            internalNode => new ParagraphSyntax(internalNode, null, 0, null));
    }
}
