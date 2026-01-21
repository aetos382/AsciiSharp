
using System.Collections.Generic;

using AsciiSharp.InternalSyntax;

namespace AsciiSharp.Syntax;
/// <summary>
/// AsciiDoc 文書の本体部分を表す構文ノード。
/// </summary>
/// <remarks>
/// 本体にはセクション、段落、その他のブロック要素が含まれる。
/// </remarks>
public sealed class DocumentBodySyntax : SyntaxNode
{
    private readonly List<SyntaxNode> _children = [];

    /// <summary>
    /// DocumentBodySyntax を作成する。
    /// </summary>
    internal DocumentBodySyntax(InternalNode internalNode, SyntaxNode? parent, int position, SyntaxTree? syntaxTree)
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

            SyntaxNode? child = slot.Kind switch
            {
                SyntaxKind.Section => new SectionSyntax(slot, this, currentPosition, syntaxTree),
                SyntaxKind.Paragraph => new ParagraphSyntax(slot, this, currentPosition, syntaxTree),
                SyntaxKind.None => throw new System.NotImplementedException(),
                SyntaxKind.MissingToken => throw new System.NotImplementedException(),
                SyntaxKind.SkippedTokensTrivia => throw new System.NotImplementedException(),
                SyntaxKind.EndOfFileToken => throw new System.NotImplementedException(),
                SyntaxKind.NewLineToken => throw new System.NotImplementedException(),
                SyntaxKind.WhitespaceToken => throw new System.NotImplementedException(),
                SyntaxKind.TextToken => throw new System.NotImplementedException(),
                SyntaxKind.EqualsToken => throw new System.NotImplementedException(),
                SyntaxKind.ColonToken => throw new System.NotImplementedException(),
                SyntaxKind.SlashToken => throw new System.NotImplementedException(),
                SyntaxKind.OpenBracketToken => throw new System.NotImplementedException(),
                SyntaxKind.CloseBracketToken => throw new System.NotImplementedException(),
                SyntaxKind.OpenBraceToken => throw new System.NotImplementedException(),
                SyntaxKind.CloseBraceToken => throw new System.NotImplementedException(),
                SyntaxKind.HashToken => throw new System.NotImplementedException(),
                SyntaxKind.AsteriskToken => throw new System.NotImplementedException(),
                SyntaxKind.UnderscoreToken => throw new System.NotImplementedException(),
                SyntaxKind.BacktickToken => throw new System.NotImplementedException(),
                SyntaxKind.DotToken => throw new System.NotImplementedException(),
                SyntaxKind.CommaToken => throw new System.NotImplementedException(),
                SyntaxKind.PipeToken => throw new System.NotImplementedException(),
                SyntaxKind.LessThanToken => throw new System.NotImplementedException(),
                SyntaxKind.GreaterThanToken => throw new System.NotImplementedException(),
                SyntaxKind.WhitespaceTrivia => throw new System.NotImplementedException(),
                SyntaxKind.EndOfLineTrivia => throw new System.NotImplementedException(),
                SyntaxKind.SingleLineCommentTrivia => throw new System.NotImplementedException(),
                SyntaxKind.MultiLineCommentTrivia => throw new System.NotImplementedException(),
                SyntaxKind.Document => throw new System.NotImplementedException(),
                SyntaxKind.DocumentHeader => throw new System.NotImplementedException(),
                SyntaxKind.DocumentBody => throw new System.NotImplementedException(),
                SyntaxKind.SectionTitle => throw new System.NotImplementedException(),
                SyntaxKind.AuthorLine => throw new System.NotImplementedException(),
                SyntaxKind.TextSpan => throw new System.NotImplementedException(),
                SyntaxKind.Text => throw new System.NotImplementedException(),
                SyntaxKind.Link => throw new System.NotImplementedException(),
                _ => null
            };

            if (child is not null)
            {
                this._children.Add(child);
            }

            currentPosition += slot.FullWidth;
        }
    }

    /// <inheritdoc />
    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        foreach (var child in this._children)
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
