
using System.Collections.Generic;

using AsciiSharp.InternalSyntax;

namespace AsciiSharp.Syntax;
/// <summary>
/// AsciiDoc 文書全体を表す構文ノード。
/// </summary>
public sealed class DocumentSyntax : SyntaxNode
{
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

            switch (slot.Kind)
            {
                case SyntaxKind.DocumentHeader:
                    this.Header = new DocumentHeaderSyntax(slot, this, currentPosition, syntaxTree);
                    break;

                case SyntaxKind.DocumentBody:
                    this.Body = new DocumentBodySyntax(slot, this, currentPosition, syntaxTree);
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
                    break;
            }

            currentPosition += slot.FullWidth;
        }
    }

    /// <inheritdoc />
    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        if (this.Header is not null)
        {
            yield return new SyntaxNodeOrToken(this.Header);
        }

        if (this.Body is not null)
        {
            yield return new SyntaxNodeOrToken(this.Body);
        }
    }

    /// <inheritdoc />
    protected override SyntaxNode ReplaceNodeCore(SyntaxNode oldNode, SyntaxNode newNode)
    {
        // TODO: 実装
        throw new System.NotImplementedException();
    }
}
