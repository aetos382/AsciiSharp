
using AsciiSharp.InternalSyntax;

namespace AsciiSharp.Syntax;

/// <summary>
/// AsciiDoc 言語仕様においてブロックと定義される要素の抽象基底クラス。
/// </summary>
/// <remarks>
/// <para>AsciiDoc 仕様でブロックとされる要素のみが本クラスを継承する。
/// 具体的には <see cref="DocumentSyntax"/>、<see cref="SectionSyntax"/>、
/// <see cref="ParagraphSyntax"/> が該当する。</para>
/// <para>ブロック要素は行単位（linewise）でソーステキスト内に出現し、垂直方向に積み重なる。</para>
/// <para>参照: AsciiDoc 言語仕様 block-element.adoc</para>
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
