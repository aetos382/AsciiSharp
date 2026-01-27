
using System;
using System.Collections.Generic;

using AsciiSharp.Diagnostics;
using AsciiSharp.InternalSyntax;
using AsciiSharp.Text;

namespace AsciiSharp.Syntax;
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
    public SyntaxKind Kind => this.Internal.Kind;

    /// <summary>
    /// ノードの位置（トリビアを除く）。
    /// </summary>
    public TextSpan Span => new(this.Position + this.Internal.LeadingTriviaWidth, this.Internal.Width);

    /// <summary>
    /// ノードの位置（トリビアを含む）。
    /// </summary>
    public TextSpan FullSpan => new(this.Position, this.Internal.FullWidth);

    /// <summary>
    /// このノードが欠落ノード（エラー回復で挿入されたもの）かどうか。
    /// </summary>
    public bool IsMissing => this.Internal.IsMissing;

    /// <summary>
    /// このノードまたは子孫に診断情報が含まれるかどうか。
    /// </summary>
    public bool ContainsDiagnostics => this.Internal.ContainsDiagnostics;

    /// <summary>
    /// 指定された内部ノード、親、位置で SyntaxNode を作成する。
    /// </summary>
    /// <param name="internalNode">対応する内部ノード。</param>
    /// <param name="parent">親ノード。</param>
    /// <param name="position">絶対位置。</param>
    /// <param name="syntaxTree">所属する構文木。</param>
    private protected SyntaxNode(InternalNode internalNode, SyntaxNode? parent, int position, SyntaxTree? syntaxTree)
    {
        ArgumentNullException.ThrowIfNull(internalNode);

        this.Internal = internalNode;
        this.Parent = parent;
        this.Position = position;
        this.SyntaxTree = syntaxTree ?? parent?.SyntaxTree;
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
        foreach (var childOrToken in this.ChildNodesAndTokens())
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
        foreach (var childOrToken in this.ChildNodesAndTokens())
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
        foreach (var childOrToken in this.DescendantNodesAndTokens())
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
        var node = includeSelf ? this : this.Parent;
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
        if (!this.ContainsDiagnostics)
        {
            yield break;
        }

        var tree = this.SyntaxTree;
        if (tree is not null)
        {
            foreach (var diagnostic in tree.GetDiagnosticsForNode(this))
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
        return this.Internal.ToFullString();
    }

    /// <summary>
    /// トリビアを除いたテキストを取得する。
    /// </summary>
    /// <returns>トリビアを除いたテキスト。</returns>
    public override string ToString()
    {
        return this.Internal.ToTrimmedString();
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
        ArgumentNullException.ThrowIfNull(oldNode);

        ArgumentNullException.ThrowIfNull(newNode);

        return this.ReplaceNodeCore(oldNode, newNode);
    }

    /// <summary>
    /// ノードの置換を実装する。
    /// </summary>
    /// <param name="oldNode">置換対象のノード。</param>
    /// <param name="newNode">新しいノード。</param>
    /// <returns>新しい構文木のルートノード。</returns>
    protected abstract SyntaxNode ReplaceNodeCore(SyntaxNode oldNode, SyntaxNode newNode);

    /// <summary>
    /// 内部ノードの子スロットを置換した新しい内部ノードを作成する。
    /// </summary>
    /// <param name="slotIndex">置換するスロットのインデックス。</param>
    /// <param name="newChild">新しい子ノード。</param>
    /// <returns>新しい内部ノード。</returns>
    private protected InternalNode ReplaceInternalSlot(int slotIndex, InternalNode newChild)
    {
        var slotCount = this.Internal.SlotCount;
        var newChildren = new InternalNode?[slotCount];

        for (var i = 0; i < slotCount; i++)
        {
            newChildren[i] = i == slotIndex ? newChild : this.Internal.GetSlot(i);
        }

        return new InternalSyntaxNode(this.Internal.Kind, newChildren);
    }

    /// <summary>
    /// 指定された子ノードが含まれるスロットのインデックスを検索する。
    /// </summary>
    /// <param name="childNode">検索する子ノード。</param>
    /// <returns>スロットのインデックス。見つからない場合は -1。</returns>
    private protected int FindSlotIndex(SyntaxNode childNode)
    {
        ArgumentNullException.ThrowIfNull(childNode);

        var slotCount = this.Internal.SlotCount;

        for (var i = 0; i < slotCount; i++)
        {
            var slot = this.Internal.GetSlot(i);
            if (slot is not null && ReferenceEquals(slot, childNode.Internal))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// 子孫ノードに対する置換を処理する共通ロジック。
    /// </summary>
    /// <param name="oldNode">置換対象のノード。</param>
    /// <param name="newNode">新しいノード。</param>
    /// <param name="createNewNode">新しい内部ノードから構文ノードを作成するファクトリ関数。</param>
    /// <returns>新しい構文木のルートノード。</returns>
    private protected SyntaxNode ReplaceInDescendants(
        SyntaxNode oldNode,
        SyntaxNode newNode,
        Func<InternalNode, SyntaxNode> createNewNode)
    {
        // 直接の子ノードかどうかを確認
        var slotIndex = this.FindSlotIndex(oldNode);
        if (slotIndex >= 0)
        {
            // 直接の子ノードを置換
            var newInternal = this.ReplaceInternalSlot(slotIndex, newNode.Internal);
            return createNewNode(newInternal);
        }

        // 子孫ノードを検索して再帰的に置換
        var slotCount = this.Internal.SlotCount;
        for (var i = 0; i < slotCount; i++)
        {
            var slot = this.Internal.GetSlot(i);
            if (slot is null or InternalToken)
            {
                continue;
            }

            // このスロットに対応する子ノードを探す
            foreach (var childOrToken in this.ChildNodesAndTokens())
            {
                if (childOrToken.IsNode)
                {
                    var childNode = childOrToken.AsNode()!;
                    if (ReferenceEquals(childNode.Internal, slot) && childNode.FullSpan.Contains(oldNode.FullSpan))
                    {
                        // この子ノードに oldNode が含まれる
                        var replacedChild = childNode.ReplaceNode(oldNode, newNode);
                        var newInternal = this.ReplaceInternalSlot(i, replacedChild.Internal);
                        return createNewNode(newInternal);
                    }
                }
            }
        }

        // 見つからない場合は自身を返す（変更なし）
        return this;
    }

    /// <summary>
    /// 指定された Visitor でこのノードを訪問する。
    /// </summary>
    /// <param name="visitor">Visitor。</param>
    public abstract void Accept(ISyntaxVisitor visitor);

    /// <summary>
    /// 指定された Visitor でこのノードを訪問し、結果を返す。
    /// </summary>
    /// <typeparam name="TResult">訪問結果の型。</typeparam>
    /// <param name="visitor">Visitor。</param>
    /// <returns>訪問結果。</returns>
    public abstract TResult Accept<TResult>(ISyntaxVisitor<TResult> visitor);
}
