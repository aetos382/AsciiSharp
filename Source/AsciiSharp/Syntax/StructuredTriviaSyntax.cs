
using AsciiSharp.InternalSyntax;

namespace AsciiSharp.Syntax;

/// <summary>
/// トリビアであるが内部に構文構造を持つノードの抽象基底クラス。
/// </summary>
/// <remarks>
/// <para>ドキュメントの内容には影響しないためトリビアとして扱うが、
/// 複数行にわたる構造を持つため <see cref="SyntaxNode"/> を継承する。</para>
/// <para>具象クラスとして <c>MultilineCommentTriviaSyntax</c> 等が該当するが、
/// 具象クラスの実装はこのクラスのスコープ外とする。</para>
/// </remarks>
public abstract class StructuredTriviaSyntax : SyntaxNode
{
    /// <summary>
    /// StructuredTriviaSyntax を作成する。
    /// </summary>
    private protected StructuredTriviaSyntax(
        InternalNode internalNode,
        SyntaxNode? parent,
        int position,
        SyntaxTree? syntaxTree)
        : base(internalNode, parent, position, syntaxTree)
    {
    }
}
