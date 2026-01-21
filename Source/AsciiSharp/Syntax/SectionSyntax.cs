
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

            switch (slot.Kind)
            {
                case SyntaxKind.SectionTitle:
                    this.Title = new SectionTitleSyntax(slot, this, currentPosition, syntaxTree);
                    break;

                case SyntaxKind.Section:
                    this._content.Add(new SectionSyntax(slot, this, currentPosition, syntaxTree));
                    break;

                case SyntaxKind.Paragraph:
                    this._content.Add(new ParagraphSyntax(slot, this, currentPosition, syntaxTree));
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

    /// <summary>
    /// セクションの内容（子セクションと段落）。
    /// </summary>
    public IReadOnlyList<SyntaxNode> Content => this._content;

    /// <inheritdoc />
    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        if (this.Title is not null)
        {
            yield return new SyntaxNodeOrToken(this.Title);
        }

        foreach (var child in this._content)
        {
            yield return new SyntaxNodeOrToken(child);
        }
    }

    /// <inheritdoc />
    protected override SyntaxNode ReplaceNodeCore(SyntaxNode oldNode, SyntaxNode newNode)
    {
        // TODO: 実装
        throw new System.NotImplementedException();
    }
}
