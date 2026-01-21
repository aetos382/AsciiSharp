
using System.Collections.Generic;

using AsciiSharp.InternalSyntax;

namespace AsciiSharp.Syntax;
/// <summary>
/// AsciiDoc 文書のヘッダー部分を表す構文ノード。
/// </summary>
/// <remarks>
/// ヘッダーには文書タイトル、著者行などが含まれる。
/// </remarks>
public sealed class DocumentHeaderSyntax : SyntaxNode
{
    /// <summary>
    /// 文書タイトル。
    /// </summary>
    public SectionTitleSyntax? Title { get; }

    /// <summary>
    /// 著者行（オプション）。
    /// </summary>
    public AuthorLineSyntax? AuthorLine { get; }

    /// <summary>
    /// DocumentHeaderSyntax を作成する。
    /// </summary>
    internal DocumentHeaderSyntax(InternalNode internalNode, SyntaxNode? parent, int position, SyntaxTree? syntaxTree)
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
                case SyntaxKind.SectionTitle:
                    this.Title = new SectionTitleSyntax(slot, this, currentPosition, syntaxTree);
                    break;

                case SyntaxKind.AuthorLine:
                    this.AuthorLine = new AuthorLineSyntax(slot, this, currentPosition, syntaxTree);
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
                case SyntaxKind.Paragraph:
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
        if (this.Title is not null)
        {
            yield return new SyntaxNodeOrToken(this.Title);
        }

        if (this.AuthorLine is not null)
        {
            yield return new SyntaxNodeOrToken(this.AuthorLine);
        }
    }

    /// <inheritdoc />
    protected override SyntaxNode ReplaceNodeCore(SyntaxNode oldNode, SyntaxNode newNode)
    {
        // TODO: 実装
        throw new System.NotImplementedException();
    }
}

/// <summary>
/// 著者行を表す構文ノード。
/// </summary>
public sealed class AuthorLineSyntax : SyntaxNode
{
    /// <summary>
    /// 著者行のテキスト。
    /// </summary>
    public string Text => this.Internal.ToFullString();

    /// <summary>
    /// AuthorLineSyntax を作成する。
    /// </summary>
    internal AuthorLineSyntax(InternalNode internalNode, SyntaxNode? parent, int position, SyntaxTree? syntaxTree)
        : base(internalNode, parent, position, syntaxTree)
    {
    }

    /// <inheritdoc />
    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield break;
    }

    /// <inheritdoc />
    protected override SyntaxNode ReplaceNodeCore(SyntaxNode oldNode, SyntaxNode newNode)
    {
        throw new System.NotImplementedException();
    }
}
