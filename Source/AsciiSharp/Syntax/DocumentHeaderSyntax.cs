
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
    private readonly List<SyntaxNodeOrToken> _children = [];

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

            // トークンの場合は SyntaxToken としてラップ
            if (slot is InternalToken token)
            {
                this._children.Add(new SyntaxToken(token, this, currentPosition, i));
                currentPosition += slot.FullWidth;
                continue;
            }

            // ノードの場合は適切な型に変換
            // IDE0010: SyntaxKind の全ケースを網羅する必要なし - ヘッダーに関連する種別のみ処理
#pragma warning disable IDE0010
            switch (slot.Kind)
            {
                case SyntaxKind.SectionTitle:
                    this.Title = new SectionTitleSyntax(slot, this, currentPosition, syntaxTree);
                    this._children.Add(new SyntaxNodeOrToken(this.Title));
                    break;

                case SyntaxKind.AuthorLine:
                    this.AuthorLine = new AuthorLineSyntax(slot, this, currentPosition, syntaxTree);
                    this._children.Add(new SyntaxNodeOrToken(this.AuthorLine));
                    break;

                default:
                    break;
            }
#pragma warning restore IDE0010

            currentPosition += slot.FullWidth;
        }
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
            internalNode => new DocumentHeaderSyntax(internalNode, null, 0, null));
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
        // リーフノードなので、子孫にターゲットノードは存在しない
        return this;
    }
}
