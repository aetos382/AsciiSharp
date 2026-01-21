namespace AsciiSharp.Syntax;

using System.Collections.Generic;
using AsciiSharp.InternalSyntax;

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
                    Title = new SectionTitleSyntax(slot, this, currentPosition, syntaxTree);
                    break;

                case SyntaxKind.AuthorLine:
                    AuthorLine = new AuthorLineSyntax(slot, this, currentPosition, syntaxTree);
                    break;
            }

            currentPosition += slot.FullWidth;
        }
    }

    /// <inheritdoc />
    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        if (Title is not null)
        {
            yield return new SyntaxNodeOrToken(Title);
        }

        if (AuthorLine is not null)
        {
            yield return new SyntaxNodeOrToken(AuthorLine);
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
    public string Text => Internal.ToFullString();

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
