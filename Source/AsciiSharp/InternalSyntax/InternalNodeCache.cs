using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AsciiSharp.InternalSyntax;

/// <summary>
/// 同一内容の内部ノードを共有するためのキャッシュ。
/// </summary>
/// <remarks>
/// <para>このキャッシュは WeakReference を使用してメモリ圧迫を回避する。</para>
/// <para>構造共有により、増分解析時のメモリ効率を向上させる。</para>
/// </remarks>
internal sealed class InternalNodeCache
{
    private const int DefaultCapacity = 1024;
    private const int MaxCacheableWidth = 256;

    private readonly object _lock = new();
    private readonly Dictionary<CacheKey, WeakReference<InternalNode>> _cache;

    /// <summary>
    /// 既定の容量で InternalNodeCache を作成する。
    /// </summary>
    public InternalNodeCache()
        : this(DefaultCapacity)
    {
    }

    /// <summary>
    /// 指定された容量で InternalNodeCache を作成する。
    /// </summary>
    /// <param name="capacity">初期容量。</param>
    public InternalNodeCache(int capacity)
    {
        this._cache = new Dictionary<CacheKey, WeakReference<InternalNode>>(capacity);
    }

    /// <summary>
    /// キャッシュからノードを取得する。
    /// </summary>
    /// <param name="kind">ノードの種別。</param>
    /// <param name="hashCode">ノードのハッシュコード。</param>
    /// <param name="node">取得したノード。</param>
    /// <returns>キャッシュにノードが存在した場合は true。</returns>
    public bool TryGetNode(SyntaxKind kind, int hashCode, out InternalNode? node)
    {
        var key = new CacheKey(kind, hashCode);

        lock (this._lock)
        {
            if (this._cache.TryGetValue(key, out var weakRef) && weakRef.TryGetTarget(out var cachedNode))
            {
                node = cachedNode;
                return true;
            }
        }

        node = null;
        return false;
    }

    /// <summary>
    /// ノードをキャッシュに追加する。
    /// </summary>
    /// <param name="node">追加するノード。</param>
    /// <remarks>
    /// 大きすぎるノードや診断情報を含むノードはキャッシュしない。
    /// </remarks>
    public void AddNode(InternalNode node)
    {
        if (node is null)
        {
            return;
        }

        // 大きすぎるノードや診断情報を含むノードはキャッシュしない
        if (node.FullWidth > MaxCacheableWidth || node.ContainsDiagnostics)
        {
            return;
        }

        var hashCode = ComputeHashCode(node);
        var key = new CacheKey(node.Kind, hashCode);

        lock (this._lock)
        {
            // 既存エントリをチェック
            if (this._cache.TryGetValue(key, out var weakRef) && weakRef.TryGetTarget(out _))
            {
                // 既にキャッシュされている
                return;
            }

            this._cache[key] = new WeakReference<InternalNode>(node);
        }
    }

    /// <summary>
    /// キャッシュをクリアする。
    /// </summary>
    public void Clear()
    {
        lock (this._lock)
        {
            this._cache.Clear();
        }
    }

    /// <summary>
    /// ガベージコレクションされたエントリを削除する。
    /// </summary>
    public void Compact()
    {
        lock (this._lock)
        {
            var keysToRemove = new List<CacheKey>();

            foreach (var kvp in this._cache)
            {
                if (!kvp.Value.TryGetTarget(out _))
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                this._cache.Remove(key);
            }
        }
    }

    /// <summary>
    /// ノードのハッシュコードを計算する。
    /// </summary>
    /// <param name="node">ノード。</param>
    /// <returns>ハッシュコード。</returns>
    private static int ComputeHashCode(InternalNode node)
    {
        unchecked
        {
            var hash = (int)node.Kind;
            hash = (hash * 397) ^ node.FullWidth;

            if (node is InternalToken token)
            {
                hash = (hash * 397) ^ (token.Text?.GetHashCode(StringComparison.Ordinal) ?? 0);
            }
            else
            {
                for (var i = 0; i < node.SlotCount; i++)
                {
                    var child = node.GetSlot(i);
                    if (child is not null)
                    {
                        hash = (hash * 397) ^ ComputeHashCode(child);
                    }
                }
            }

            return hash;
        }
    }

    /// <summary>
    /// キャッシュのキー。
    /// </summary>
    private readonly struct CacheKey : IEquatable<CacheKey>
    {
        public SyntaxKind Kind { get; }
        public int HashCode { get; }

        public CacheKey(SyntaxKind kind, int hashCode)
        {
            this.Kind = kind;
            this.HashCode = hashCode;
        }

        public bool Equals(CacheKey other)
        {
            return this.Kind == other.Kind && this.HashCode == other.HashCode;
        }

        public override bool Equals(object? obj)
        {
            return obj is CacheKey other && this.Equals(other);
        }

        public override int GetHashCode()
        {
#if NETSTANDARD2_0
            unchecked
            {
                return ((int)this.Kind * 397) ^ this.HashCode;
            }
#else
            return System.HashCode.Combine(this.Kind, this.HashCode);
#endif
        }
    }
}
