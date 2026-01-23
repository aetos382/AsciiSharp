
using System;
using System.Collections.Generic;

namespace AsciiSharp.Syntax;

/// <summary>
/// 構文ノードの LINQ 拡張メソッドを提供する。
/// </summary>
public static class SyntaxExtensions
{
    /// <summary>
    /// 指定された種類のノードのみをフィルタリングする。
    /// </summary>
    /// <param name="nodes">フィルタリング対象のノードシーケンス。</param>
    /// <param name="kind">フィルタリングする種類。</param>
    /// <returns>指定された種類のノードのみを含むシーケンス。</returns>
    public static IEnumerable<SyntaxNode> OfKind(this IEnumerable<SyntaxNode> nodes, SyntaxKind kind)
    {
        ArgumentNullException.ThrowIfNull(nodes);

        foreach (var node in nodes)
        {
            if (node.Kind == kind)
            {
                yield return node;
            }
        }
    }

    /// <summary>
    /// 指定された型の最初の祖先ノードを取得する。
    /// </summary>
    /// <typeparam name="TNode">検索する祖先ノードの型。</typeparam>
    /// <param name="node">開始ノード。</param>
    /// <returns>指定された型の最初の祖先ノード。見つからない場合は null。</returns>
    public static TNode? FirstAncestor<TNode>(this SyntaxNode node)
        where TNode : SyntaxNode
    {
        ArgumentNullException.ThrowIfNull(node);

        var current = node.Parent;
        while (current is not null)
        {
            if (current is TNode result)
            {
                return result;
            }

            current = current.Parent;
        }

        return null;
    }

    /// <summary>
    /// 自身を含め、指定された型の最初の祖先ノードを取得する。
    /// </summary>
    /// <typeparam name="TNode">検索する祖先ノードの型。</typeparam>
    /// <param name="node">開始ノード。</param>
    /// <returns>指定された型の最初の祖先ノード（自身を含む）。見つからない場合は null。</returns>
    public static TNode? FirstAncestorOrSelf<TNode>(this SyntaxNode node)
        where TNode : SyntaxNode
    {
        ArgumentNullException.ThrowIfNull(node);

        var current = node;
        while (current is not null)
        {
            if (current is TNode result)
            {
                return result;
            }

            current = current.Parent;
        }

        return null;
    }
}
