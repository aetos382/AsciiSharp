namespace AsciiSharp.Syntax;

using System.Collections.Generic;
using AsciiSharp.InternalSyntax;

/// <summary>
/// リンクを表す構文ノード。
/// </summary>
public sealed class LinkSyntax : SyntaxNode
{
    /// <summary>
    /// リンクの URL。
    /// </summary>
    public string? Url { get; }

    /// <summary>
    /// リンクの表示テキスト。
    /// </summary>
    public string? DisplayText { get; }

    /// <summary>
    /// LinkSyntax を作成する。
    /// </summary>
    internal LinkSyntax(InternalNode internalNode, SyntaxNode? parent, int position, SyntaxTree? syntaxTree)
        : base(internalNode, parent, position, syntaxTree)
    {
        // TODO: URL と表示テキストの解析
    }

    /// <inheritdoc />
    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield break;
    }

    /// <inheritdoc />
    protected override SyntaxNode ReplaceNodeCore(SyntaxNode oldNode, SyntaxNode newNode)
    {
        // TODO: 実装
        throw new System.NotImplementedException();
    }
}
