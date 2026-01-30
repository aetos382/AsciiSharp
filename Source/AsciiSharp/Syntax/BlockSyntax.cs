
using AsciiSharp.InternalSyntax;

namespace AsciiSharp.Syntax;

/// <summary>
/// ブロックレベル要素を表す構文ノードの抽象基底クラス。
/// </summary>
/// <remarks>
/// ブロック要素は段落、セクション、ドキュメントなどの構造的要素を表す。
/// </remarks>
public abstract class BlockSyntax : SyntaxNode
{
    /// <summary>
    /// BlockSyntax を作成する。
    /// </summary>
    private protected BlockSyntax(
        InternalNode internalNode,
        SyntaxNode? parent,
        int position,
        SyntaxTree? syntaxTree)
        : base(internalNode, parent, position, syntaxTree)
    {
    }
}
