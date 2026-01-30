
using AsciiSharp.InternalSyntax;

namespace AsciiSharp.Syntax;

/// <summary>
/// インラインレベル要素を表す構文ノードの抽象基底クラス。
/// </summary>
/// <remarks>
/// インライン要素はテキスト、リンク、書式設定などのフロー内要素を表す。
/// </remarks>
public abstract class InlineSyntax : SyntaxNode
{
    /// <summary>
    /// InlineSyntax を作成する。
    /// </summary>
    private protected InlineSyntax(
        InternalNode internalNode,
        SyntaxNode? parent,
        int position,
        SyntaxTree? syntaxTree)
        : base(internalNode, parent, position, syntaxTree)
    {
    }
}
