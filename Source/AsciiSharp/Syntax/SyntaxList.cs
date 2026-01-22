
using System;
using System.Collections;
using System.Collections.Generic;

namespace AsciiSharp.Syntax;
/// <summary>
/// 構文ノードのリストを表す不変の構造体。
/// </summary>
/// <typeparam name="TNode">ノードの型。</typeparam>
public readonly struct SyntaxList<TNode> : IReadOnlyList<TNode>, IEquatable<SyntaxList<TNode>>
    where TNode : SyntaxNode
{
    private readonly TNode[] _nodes;

    /// <summary>
    /// リスト内のノード数。
    /// </summary>
    public int Count => this._nodes?.Length ?? 0;

    /// <summary>
    /// 指定されたインデックスのノードを取得する。
    /// </summary>
    /// <param name="index">インデックス。</param>
    /// <returns>ノード。</returns>
    public TNode this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this.Count);

            return this._nodes[index];
        }
    }

    /// <summary>
    /// リストが空かどうか。
    /// </summary>
    public bool IsEmpty => this._nodes is null || this._nodes.Length == 0;

    /// <summary>
    /// 空の SyntaxList を作成する。
    /// </summary>
    public SyntaxList()
    {
        this._nodes = [];
    }

    /// <summary>
    /// 指定されたノードで SyntaxList を作成する。
    /// </summary>
    /// <param name="node">単一のノード。</param>
    public SyntaxList(TNode node)
    {
        this._nodes = node is null ? [] : [node];
    }

    /// <summary>
    /// 指定されたノード配列で SyntaxList を作成する。
    /// </summary>
    /// <param name="nodes">ノードの配列。</param>
    public SyntaxList(TNode[] nodes)
    {
        this._nodes = nodes ?? [];
    }

    /// <summary>
    /// 指定されたノードコレクションで SyntaxList を作成する。
    /// </summary>
    /// <param name="nodes">ノードのコレクション。</param>
    public SyntaxList(IEnumerable<TNode> nodes)
    {
        if (nodes is null)
        {
            this._nodes = [];
        }
        else
        {
            var list = new List<TNode>(nodes);
            this._nodes = [.. list];
        }
    }

    /// <summary>
    /// リストに要素があるかどうかを返す。
    /// </summary>
    /// <returns>要素がある場合は true。</returns>
    public bool Any()
    {
        return this.Count > 0;
    }

    /// <summary>
    /// 最初のノードを取得する。
    /// </summary>
    /// <returns>最初のノード。リストが空の場合は null。</returns>
    public TNode? FirstOrDefault()
    {
        return this.Count > 0 ? this._nodes[0] : default;
    }

    /// <summary>
    /// 最後のノードを取得する。
    /// </summary>
    /// <returns>最後のノード。リストが空の場合は null。</returns>
    public TNode? LastOrDefault()
    {
        return this.Count > 0 ? this._nodes[this.Count - 1] : default;
    }

    /// <summary>
    /// 指定されたノードのインデックスを取得する。
    /// </summary>
    /// <param name="node">検索するノード。</param>
    /// <returns>インデックス。見つからない場合は -1。</returns>
    public int IndexOf(TNode node)
    {
        if (this._nodes is null || node is null)
        {
            return -1;
        }

        return Array.IndexOf(this._nodes, node);
    }

    /// <summary>
    /// ノードを列挙する。
    /// </summary>
    /// <returns>列挙子。</returns>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator<TNode> IEnumerable<TNode>.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    /// <inheritdoc />
    public bool Equals(SyntaxList<TNode> other)
    {
        if (this._nodes is null && other._nodes is null)
        {
            return true;
        }

        if (this._nodes is null || other._nodes is null)
        {
            return false;
        }

        if (this._nodes.Length != other._nodes.Length)
        {
            return false;
        }

        for (var i = 0; i < this._nodes.Length; i++)
        {
            if (!ReferenceEquals(this._nodes[i], other._nodes[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is SyntaxList<TNode> other && this.Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        if (this._nodes is null)
        {
            return 0;
        }

        unchecked
        {
            var hash = 17;
            foreach (var node in this._nodes)
            {
                hash = (hash * 31) + (node?.GetHashCode() ?? 0);
            }

            return hash;
        }
    }

    /// <summary>
    /// 2つの SyntaxList が等しいかどうかを判定する。
    /// </summary>
    public static bool operator ==(SyntaxList<TNode> left, SyntaxList<TNode> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// 2つの SyntaxList が等しくないかどうかを判定する。
    /// </summary>
    public static bool operator !=(SyntaxList<TNode> left, SyntaxList<TNode> right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// SyntaxList の列挙子。
    /// </summary>
    public struct Enumerator : IEnumerator<TNode>
    {
        private readonly SyntaxList<TNode> _list;
        private int _index;

        /// <summary>
        /// 現在のノード。
        /// </summary>
        public readonly TNode Current => this._list[this._index];

        readonly object IEnumerator.Current => this.Current;

        internal Enumerator(SyntaxList<TNode> list)
        {
            this._list = list;
            this._index = -1;
        }

        /// <summary>
        /// 次のノードに移動する。
        /// </summary>
        /// <returns>次のノードが存在する場合は true。</returns>
        public bool MoveNext()
        {
            this._index++;
            return this._index < this._list.Count;
        }

        /// <summary>
        /// 列挙子をリセットする。
        /// </summary>
        public void Reset()
        {
            this._index = -1;
        }

        /// <summary>
        /// リソースを解放する。
        /// </summary>
        public readonly void Dispose()
        {
        }
    }
}
