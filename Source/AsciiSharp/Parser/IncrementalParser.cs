
using System;
using System.Collections.Generic;

using AsciiSharp.Diagnostics;
using AsciiSharp.InternalSyntax;
using AsciiSharp.Syntax;
using AsciiSharp.Text;

namespace AsciiSharp.Parser;

/// <summary>
/// 増分解析を行うパーサー。
/// </summary>
/// <remarks>
/// <para>文書の一部が変更された場合、変更されていない部分の内部ノードを再利用しつつ、
/// 変更部分のみを再解析して新しい構文木を生成する。</para>
/// </remarks>
internal sealed class IncrementalParser
{
    private readonly SyntaxTree _oldTree;
    private readonly SourceText _newText;
    private readonly IReadOnlyList<TextChange> _changes;

    /// <summary>
    /// IncrementalParser を作成する。
    /// </summary>
    /// <param name="oldTree">元の構文木。</param>
    /// <param name="newText">変更後のソーステキスト。</param>
    /// <param name="changes">適用された変更のリスト。</param>
    public IncrementalParser(SyntaxTree oldTree, SourceText newText, IReadOnlyList<TextChange> changes)
    {
        ArgumentNullException.ThrowIfNull(oldTree);
        ArgumentNullException.ThrowIfNull(newText);
        ArgumentNullException.ThrowIfNull(changes);

        this._oldTree = oldTree;
        this._newText = newText;
        this._changes = changes;
    }

    /// <summary>
    /// 増分解析を実行し、新しい構文木を返す。
    /// </summary>
    /// <returns>変更後の構文木。</returns>
    public SyntaxTree Parse()
    {
        // 変更がない場合は同じテキストで新しいツリーを作成
        if (this._changes.Count == 0)
        {
            return SyntaxTree.ParseText(this._newText, this._oldTree.FilePath);
        }

        // 影響範囲を計算
        var affectedRange = this.ComputeAffectedRange();

        // 再利用可能なノードを特定
        var reusableNodes = this.FindReusableNodes(affectedRange);

        // 再利用ノードがない場合は全体を再解析
        if (reusableNodes.Count == 0)
        {
            return SyntaxTree.ParseText(this._newText, this._oldTree.FilePath);
        }

        // 再利用可能なノードを使って新しい構文木を構築
        return this.BuildTreeWithReuse(affectedRange, reusableNodes);
    }

    /// <summary>
    /// 変更による影響範囲を計算する。
    /// </summary>
    /// <returns>影響範囲。</returns>
    private AffectedRange ComputeAffectedRange()
    {
        // 全変更の最小開始位置と最大終了位置を計算
        var minStart = int.MaxValue;
        var maxOldEnd = 0;
        var totalDelta = 0;

        foreach (var change in this._changes)
        {
            if (change.Span.Start < minStart)
            {
                minStart = change.Span.Start;
            }

            var oldEnd = change.Span.Start + change.Span.Length;
            if (oldEnd > maxOldEnd)
            {
                maxOldEnd = oldEnd;
            }

            totalDelta += change.NewText.Length - change.Span.Length;
        }

        return new AffectedRange(minStart, maxOldEnd, totalDelta);
    }

    /// <summary>
    /// 影響範囲外で再利用可能なノードを特定する。
    /// </summary>
    /// <param name="affectedRange">影響範囲。</param>
    /// <returns>再利用可能なノードのコレクション。</returns>
    private ReusableNodeCollection FindReusableNodes(AffectedRange affectedRange)
    {
        var reusable = new ReusableNodeCollection();
        var root = this._oldTree.Root;

        // Document 直下の構造を取得
        if (root is not DocumentSyntax document)
        {
            return reusable;
        }

        // DocumentBody 内のセクションを走査
        var body = document.Body;
        if (body is null)
        {
            return reusable;
        }

        // ヘッダーがある場合
        if (document.Header is not null)
        {
            var headerEnd = document.Header.FullSpan.End;
            if (headerEnd <= affectedRange.OldStart)
            {
                // ヘッダーは影響範囲より前なので再利用可能
                reusable.AddBeforeRange(document.Header);
            }
        }

        // Body の子ノードを走査
        foreach (var child in body.ChildNodesAndTokens())
        {
            if (!child.IsNode)
            {
                continue;
            }

            var node = child.AsNode();
            if (node is null)
            {
                continue;
            }

            var nodeStart = node.FullSpan.Start;
            var nodeEnd = node.FullSpan.End;

            if (nodeEnd <= affectedRange.OldStart)
            {
                // ノードは影響範囲より前なので再利用可能
                reusable.AddBeforeRange(node);
            }
            else if (nodeStart >= affectedRange.OldEnd)
            {
                // ノードは影響範囲より後なので再利用可能
                reusable.AddAfterRange(node);
            }
            // else: ノードは影響範囲と重なるので再解析が必要
        }

        return reusable;
    }

    /// <summary>
    /// 再利用可能なノードを使って新しい構文木を構築する。
    /// </summary>
    /// <param name="affectedRange">影響範囲。</param>
    /// <param name="reusableNodes">再利用可能なノードのコレクション。</param>
    /// <returns>新しい構文木。</returns>
    private SyntaxTree BuildTreeWithReuse(AffectedRange affectedRange, ReusableNodeCollection reusableNodes)
    {
        // 新しいテキストを解析してベースとなる構文木を取得
        var fullParsedTree = SyntaxTree.ParseText(this._newText, this._oldTree.FilePath);

        if (fullParsedTree.Root is not DocumentSyntax)
        {
            return fullParsedTree;
        }

        // 新しい構文木の構造を構築（内部ノードを再利用）
        var builder = new IncrementalTreeBuilder(
            reusableNodes,
            affectedRange,
            fullParsedTree);

        return builder.Build();
    }

    /// <summary>
    /// 影響範囲を表す構造体。
    /// </summary>
    private readonly struct AffectedRange
    {
        /// <summary>
        /// 元のテキストでの影響範囲の開始位置。
        /// </summary>
        public int OldStart { get; }

        /// <summary>
        /// 元のテキストでの影響範囲の終了位置。
        /// </summary>
        public int OldEnd { get; }

        /// <summary>
        /// テキスト長の変化量（正の値は増加、負の値は減少）。
        /// </summary>
        public int Delta { get; }

        public AffectedRange(int oldStart, int oldEnd, int delta)
        {
            this.OldStart = oldStart;
            this.OldEnd = oldEnd;
            this.Delta = delta;
        }
    }

    /// <summary>
    /// 再利用可能なノードのコレクション。
    /// </summary>
    private sealed class ReusableNodeCollection
    {
        private readonly List<SyntaxNode> _nodesBeforeRange = [];
        private readonly List<SyntaxNode> _nodesAfterRange = [];

        /// <summary>
        /// 再利用可能なノードの総数。
        /// </summary>
        public int Count => this._nodesBeforeRange.Count + this._nodesAfterRange.Count;

        /// <summary>
        /// 影響範囲より前にあるノードを追加する。
        /// </summary>
        /// <param name="node">再利用可能なノード。</param>
        public void AddBeforeRange(SyntaxNode node)
        {
            this._nodesBeforeRange.Add(node);
        }

        /// <summary>
        /// 影響範囲より後にあるノードを追加する。
        /// </summary>
        /// <param name="node">再利用可能なノード。</param>
        public void AddAfterRange(SyntaxNode node)
        {
            this._nodesAfterRange.Add(node);
        }

        /// <summary>
        /// 影響範囲より前にあるノードを取得する。
        /// </summary>
        public IReadOnlyList<SyntaxNode> NodesBeforeRange => this._nodesBeforeRange;

        /// <summary>
        /// 影響範囲より後にあるノードを取得する。
        /// </summary>
        public IReadOnlyList<SyntaxNode> NodesAfterRange => this._nodesAfterRange;
    }

    /// <summary>
    /// 増分構文木を構築するビルダー。
    /// </summary>
    private sealed class IncrementalTreeBuilder
    {
        private readonly ReusableNodeCollection _reusableNodes;
        private readonly AffectedRange _affectedRange;
        private readonly SyntaxTree _fullParsedTree;

        public IncrementalTreeBuilder(
            ReusableNodeCollection reusableNodes,
            AffectedRange affectedRange,
            SyntaxTree fullParsedTree)
        {
            this._reusableNodes = reusableNodes;
            this._affectedRange = affectedRange;
            this._fullParsedTree = fullParsedTree;
        }

        /// <summary>
        /// 新しい構文木を構築する。
        /// </summary>
        /// <returns>構築された構文木。</returns>
        public SyntaxTree Build()
        {
            if (this._fullParsedTree.Root is not DocumentSyntax newDocument)
            {
                return this._fullParsedTree;
            }

            // Document 構造を再構築
            var newInternalRoot = this.BuildDocument(newDocument);

            return new SyntaxTree(
                newInternalRoot,
                this._fullParsedTree.Text,
                [.. this._fullParsedTree.Diagnostics],
                this._fullParsedTree.FilePath);
        }

        /// <summary>
        /// Document ノードを構築する。
        /// </summary>
        /// <param name="newDocument">新しくパースされた Document。</param>
        /// <returns>再利用ノードを含む内部ノード。</returns>
        private InternalSyntaxNode BuildDocument(DocumentSyntax newDocument)
        {
            var children = new List<InternalNode?>();

            // ヘッダーを処理
            if (newDocument.Header is not null)
            {
                var reuseHeader = TryReuseNode(newDocument.Header, this._reusableNodes.NodesBeforeRange);
                children.Add(reuseHeader ?? newDocument.Header.Internal);
            }

            // Body を処理
            if (newDocument.Body is not null)
            {
                var bodyChildren = new List<InternalNode?>();

                foreach (var child in newDocument.Body.ChildNodesAndTokens())
                {
                    if (child.IsToken)
                    {
                        bodyChildren.Add(child.AsToken().Internal);
                        continue;
                    }

                    var node = child.AsNode();
                    if (node is null)
                    {
                        continue;
                    }

                    // 影響範囲より後のノードで再利用可能なものを探す
                    var reusedNode = this.TryReuseNodeByPosition(node);
                    bodyChildren.Add(reusedNode ?? node.Internal);
                }

                var bodyInternal = new InternalSyntaxNode(SyntaxKind.DocumentBody, [.. bodyChildren]);
                children.Add(bodyInternal);
            }

            // EOF トークンを内部ノードから取得
            var eofToken = FindEofToken(newDocument.Internal);
            if (eofToken is not null)
            {
                children.Add(eofToken);
            }

            return new InternalSyntaxNode(SyntaxKind.Document, [.. children]);
        }

        /// <summary>
        /// 内部ノードから EOF トークンを検索する。
        /// </summary>
        /// <param name="internalNode">内部ノード。</param>
        /// <returns>EOF トークン。見つからない場合は null。</returns>
        private static InternalNode? FindEofToken(InternalNode internalNode)
        {
            for (var i = internalNode.SlotCount - 1; i >= 0; i--)
            {
                var slot = internalNode.GetSlot(i);
                if (slot is not null && slot.Kind == SyntaxKind.EndOfFileToken)
                {
                    return slot;
                }
            }

            return null;
        }

        /// <summary>
        /// 再利用可能なノードリストから対応するノードを探す。
        /// </summary>
        /// <param name="newNode">新しいノード。</param>
        /// <param name="candidates">再利用候補ノードのリスト。</param>
        /// <returns>再利用可能なノードの内部ノード。見つからない場合は null。</returns>
        private static InternalNode? TryReuseNode(SyntaxNode newNode, IReadOnlyList<SyntaxNode> candidates)
        {
            // 同じ種類で同じテキスト内容を持つノードを探す
            foreach (var candidate in candidates)
            {
                if (candidate.Kind == newNode.Kind &&
                    candidate.Internal.FullWidth == newNode.Internal.FullWidth &&
                    string.Equals(candidate.ToFullString(), newNode.ToFullString(), StringComparison.Ordinal))
                {
                    // 内部ノードの参照を再利用
                    return candidate.Internal;
                }
            }

            return null;
        }

        /// <summary>
        /// ノードの位置に基づいて再利用可能なノードを探す。
        /// </summary>
        /// <param name="newNode">新しいノード。</param>
        /// <returns>再利用可能なノードの内部ノード。見つからない場合は null。</returns>
        private InternalNode? TryReuseNodeByPosition(SyntaxNode newNode)
        {
            var newNodeStart = newNode.FullSpan.Start;

            // 影響範囲より前のノード
            if (newNodeStart < this._affectedRange.OldStart)
            {
                return TryReuseNode(newNode, this._reusableNodes.NodesBeforeRange);
            }

            // 影響範囲より後のノード（位置がずれているので調整が必要）
            var adjustedOldStart = newNodeStart - this._affectedRange.Delta;
            if (adjustedOldStart >= this._affectedRange.OldEnd)
            {
                // 影響範囲より後にある可能性が高い
                foreach (var candidate in this._reusableNodes.NodesAfterRange)
                {
                    if (candidate.Kind == newNode.Kind &&
                        candidate.Internal.FullWidth == newNode.Internal.FullWidth)
                    {
                        // 内部ノードの参照を再利用
                        return candidate.Internal;
                    }
                }
            }

            return null;
        }
    }
}
