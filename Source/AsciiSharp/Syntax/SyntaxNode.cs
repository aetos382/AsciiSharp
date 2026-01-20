namespace AsciiSharp.Syntax;

using System;
using System.Collections.Generic;
using AsciiSharp.Diagnostics;
using AsciiSharp.InternalSyntax;
using AsciiSharp.Text;

/// <summary>
/// 外部構文木のノードを表す抽象基底クラス。
/// </summary>
/// <remarks>
/// <para>このクラスは Roslyn の Red Tree に相当する。</para>
/// <para>特性:</para>
/// <list type="bullet">
///   <item>内部ノード（InternalNode）をラップする</item>
///   <item>親参照を持つ（オンデマンド計算）</item>
///   <item>絶対位置を持つ（親からの累積計算で算出）</item>
/// </list>
/// </remarks>
public abstract class SyntaxNode
{
    /// <summary>
    /// 対応する内部ノード。
    /// </summary>
    internal InternalNode Internal { get; }

    /// <summary>
    /// 親ノード。
    /// </summary>
    public SyntaxNode? Parent { get; }

    /// <summary>
    /// 所属する構文木。
    /// </summary>
    public SyntaxTree? SyntaxTree { get; }

    /// <summary>
    /// ノードの絶対位置（トリビアを含む開始位置）。
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// ノードの種別。
    /// </summary>
    public SyntaxKind Kind => Internal.Kind;

    /// <summary>
    /// ノードの位置（トリビアを除く）。
    /// </summary>
    public TextSpan Span => new TextSpan(Position + Internal.LeadingTriviaWidth, Internal.Width);

    /// <summary>
    /// ノードの位置（トリビアを含む）。
    /// </summary>
    public TextSpan FullSpan => new TextSpan(Position, Internal.FullWidth);

    /// <summary>
    /// このノードが欠落ノード（エラー回復で挿入されたもの）かどうか。
    /// </summary>
    public bool IsMissing => Internal.IsMissing;

    /// <summary>
    /// このノードまたは子孫に診断情報が含まれるかどうか。
    /// </summary>
    public bool ContainsDiagnostics => Internal.ContainsDiagnostics;

    /// <summary>
    /// 指定された内部ノード、親、位置で SyntaxNode を作成する。
    /// </summary>
    /// <param name="internalNode">対応する内部ノード。</param>
    /// <param name="parent">親ノード。</param>
    /// <param name="position">絶対位置。</param>
    /// <param name="syntaxTree">所属する構文木。</param>
    protected SyntaxNode(InternalNode internalNode, SyntaxNode? parent, int position, SyntaxTree? syntaxTree)
    {
        Internal = internalNode ?? throw new ArgumentNullException(nameof(internalNode));
        Parent = parent;
        Position = position;
        SyntaxTree = syntaxTree ?? parent?.SyntaxTree;
    }

    /// <summary>
    /// 子ノードとトークンを列挙する。
    /// </summary>
    /// <returns>子ノードとトークンのシーケンス。</returns>
    public abstract IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens();

    /// <summary>
    /// 子孫ノードを列挙する。
    /// </summary>
    /// <param name="descendIntoChildren">子ノードに降下するかどうかを決定する関数。</param>
    /// <returns>子孫ノードのシーケンス。</returns>
    public IEnumerable<SyntaxNode> DescendantNodes(Func<SyntaxNode, bool>? descendIntoChildren = null)
    {
        foreach (var childOrToken in ChildNodesAndTokens())
        {
            if (childOrToken.IsNode)
            {
                var node = childOrToken.AsNode()!;
                yield return node;

                if (descendIntoChildren is null || descendIntoChildren(node))
                {
                    foreach (var descendant in node.DescendantNodes(descendIntoChildren))
                    {
                        yield return descendant;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 子孫ノードとトークンを列挙する。
    /// </summary>
    /// <param name="descendIntoChildren">子ノードに降下するかどうかを決定する関数。</param>
    /// <returns>子孫ノードとトークンのシーケンス。</returns>
    public IEnumerable<SyntaxNodeOrToken> DescendantNodesAndTokens(Func<SyntaxNode, bool>? descendIntoChildren = null)
    {
        foreach (var childOrToken in ChildNodesAndTokens())
        {
            yield return childOrToken;

            if (childOrToken.IsNode)
            {
                var node = childOrToken.AsNode()!;

                if (descendIntoChildren is null || descendIntoChildren(node))
                {
                    foreach (var descendant in node.DescendantNodesAndTokens(descendIntoChildren))
                    {
                        yield return descendant;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 子孫トークンを列挙する。
    /// </summary>
    /// <returns>子孫トークンのシーケンス。</returns>
    public IEnumerable<SyntaxToken> DescendantTokens()
    {
        foreach (var childOrToken in DescendantNodesAndTokens())
        {
            if (childOrToken.IsToken)
            {
                yield return childOrToken.AsToken();
            }
        }
    }

    /// <summary>
    /// 祖先ノードを列挙する。
    /// </summary>
    /// <param name="includeSelf">自身を含めるかどうか。</param>
    /// <returns>祖先ノードのシーケンス。</returns>
    public IEnumerable<SyntaxNode> Ancestors(bool includeSelf = false)
    {
        var node = includeSelf ? this : Parent;
        while (node is not null)
        {
            yield return node;
            node = node.Parent;
        }
    }

    /// <summary>
    /// このノードの診断情報を取得する。
    /// </summary>
    /// <returns>診断情報のシーケンス。</returns>
    public IEnumerable<Diagnostic> GetDiagnostics()
    {
        if (!ContainsDiagnostics)
        {
            yield break;
        }

        var tree = SyntaxTree;
        if (tree is not null)
        {
            foreach (var diagnostic in tree.GetDiagnostics(this))
            {
                yield return diagnostic;
            }
        }
    }

    /// <summary>
    /// トリビアを含む完全なテキストを取得する。
    /// </summary>
    /// <returns>完全なテキスト。</returns>
    public string ToFullString()
    {
        return Internal.ToFullString();
    }

    /// <summary>
    /// トリビアを除いたテキストを取得する。
    /// </summary>
    /// <returns>トリビアを除いたテキスト。</returns>
    public override string ToString()
    {
        return Internal.ToTrimmedString();
    }

    /// <summary>
    /// 指定されたノードを新しいノードで置換した新しい構文木を返す。
    /// </summary>
    /// <typeparam name="TNode">置換対象のノードの型。</typeparam>
    /// <param name="oldNode">置換対象のノード。</param>
    /// <param name="newNode">新しいノード。</param>
    /// <returns>新しい構文木のルートノード。</returns>
    public SyntaxNode ReplaceNode<TNode>(TNode oldNode, TNode newNode)
        where TNode : SyntaxNode
    {
        if (oldNode is null)
        {
            throw new ArgumentNullException(nameof(oldNode));
        }

        if (newNode is null)
        {
            throw new ArgumentNullException(nameof(newNode));
        }

        return ReplaceNodeCore(oldNode, newNode);
    }

    /// <summary>
    /// ノードの置換を実装する。
    /// </summary>
    /// <param name="oldNode">置換対象のノード。</param>
    /// <param name="newNode">新しいノード。</param>
    /// <returns>新しい構文木のルートノード。</returns>
    protected abstract SyntaxNode ReplaceNodeCore(SyntaxNode oldNode, SyntaxNode newNode);
}
