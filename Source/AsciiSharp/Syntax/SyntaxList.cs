namespace AsciiSharp.Syntax;

using System;
using System.Collections;
using System.Collections.Generic;

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
    public int Count => _nodes?.Length ?? 0;

    /// <summary>
    /// 指定されたインデックスのノードを取得する。
    /// </summary>
    /// <param name="index">インデックス。</param>
    /// <returns>ノード。</returns>
    public TNode this[int index]
    {
        get
        {
            if (_nodes is null || index < 0 || index >= _nodes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return _nodes[index];
        }
    }

    /// <summary>
    /// リストが空かどうか。
    /// </summary>
    public bool IsEmpty => _nodes is null || _nodes.Length == 0;

    /// <summary>
    /// 空の SyntaxList を作成する。
    /// </summary>
    public SyntaxList()
    {
        _nodes = Array.Empty<TNode>();
    }

    /// <summary>
    /// 指定されたノードで SyntaxList を作成する。
    /// </summary>
    /// <param name="node">単一のノード。</param>
    public SyntaxList(TNode node)
    {
        _nodes = node is null ? Array.Empty<TNode>() : new[] { node };
    }

    /// <summary>
    /// 指定されたノード配列で SyntaxList を作成する。
    /// </summary>
    /// <param name="nodes">ノードの配列。</param>
    public SyntaxList(TNode[] nodes)
    {
        _nodes = nodes ?? Array.Empty<TNode>();
    }

    /// <summary>
    /// 指定されたノードコレクションで SyntaxList を作成する。
    /// </summary>
    /// <param name="nodes">ノードのコレクション。</param>
    public SyntaxList(IEnumerable<TNode> nodes)
    {
        if (nodes is null)
        {
            _nodes = Array.Empty<TNode>();
        }
        else
        {
            var list = new List<TNode>(nodes);
            _nodes = list.ToArray();
        }
    }

    /// <summary>
    /// リストに要素があるかどうかを返す。
    /// </summary>
    /// <returns>要素がある場合は true。</returns>
    public bool Any()
    {
        return Count > 0;
    }

    /// <summary>
    /// 最初のノードを取得する。
    /// </summary>
    /// <returns>最初のノード。リストが空の場合は null。</returns>
    public TNode? FirstOrDefault()
    {
        return Count > 0 ? _nodes[0] : default;
    }

    /// <summary>
    /// 最後のノードを取得する。
    /// </summary>
    /// <returns>最後のノード。リストが空の場合は null。</returns>
    public TNode? LastOrDefault()
    {
        return Count > 0 ? _nodes[Count - 1] : default;
    }

    /// <summary>
    /// 指定されたノードのインデックスを取得する。
    /// </summary>
    /// <param name="node">検索するノード。</param>
    /// <returns>インデックス。見つからない場合は -1。</returns>
    public int IndexOf(TNode node)
    {
        if (_nodes is null || node is null)
        {
            return -1;
        }

        return Array.IndexOf(_nodes, node);
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
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc />
    public bool Equals(SyntaxList<TNode> other)
    {
        if (_nodes is null && other._nodes is null)
        {
            return true;
        }

        if (_nodes is null || other._nodes is null)
        {
            return false;
        }

        if (_nodes.Length != other._nodes.Length)
        {
            return false;
        }

        for (var i = 0; i < _nodes.Length; i++)
        {
            if (!ReferenceEquals(_nodes[i], other._nodes[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is SyntaxList<TNode> other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        if (_nodes is null)
        {
            return 0;
        }

        unchecked
        {
            var hash = 17;
            foreach (var node in _nodes)
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
        public TNode Current => _list[_index];

        object IEnumerator.Current => Current;

        internal Enumerator(SyntaxList<TNode> list)
        {
            _list = list;
            _index = -1;
        }

        /// <summary>
        /// 次のノードに移動する。
        /// </summary>
        /// <returns>次のノードが存在する場合は true。</returns>
        public bool MoveNext()
        {
            _index++;
            return _index < _list.Count;
        }

        /// <summary>
        /// 列挙子をリセットする。
        /// </summary>
        public void Reset()
        {
            _index = -1;
        }

        /// <summary>
        /// リソースを解放する。
        /// </summary>
        public void Dispose()
        {
        }
    }
}
