
using System;
using System.Collections.Generic;

using AsciiSharp.InternalSyntax;
using AsciiSharp.Text;

namespace AsciiSharp.Syntax;
/// <summary>
/// 外部構文木のトークンを表す構造体。
/// </summary>
public readonly struct SyntaxToken : IEquatable<SyntaxToken>
{
    /// <summary>
    /// 対応する内部トークン。
    /// </summary>
    internal InternalToken? Internal { get; }

    /// <summary>
    /// 親ノード。
    /// </summary>
    public SyntaxNode? Parent { get; }

    /// <summary>
    /// トークンの絶対位置（先行トリビアを含む）。
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// 親ノード内のスロットインデックス。
    /// </summary>
    internal int Index { get; }

    /// <summary>
    /// トークンの種別。
    /// </summary>
    public SyntaxKind Kind => this.Internal?.Kind ?? SyntaxKind.None;

    /// <summary>
    /// トークンのテキスト。
    /// </summary>
    public string Text => this.Internal?.Text ?? string.Empty;

    /// <summary>
    /// トークンのスパン（トリビアを除く）。
    /// </summary>
    public TextSpan Span
    {
        get
        {
            if (this.Internal is null)
            {
                return new TextSpan(this.Position, 0);
            }

            return new TextSpan(this.Position + this.Internal.LeadingTriviaWidth, this.Internal.Width);
        }
    }

    /// <summary>
    /// トークンのフルスパン（トリビアを含む）。
    /// </summary>
    public TextSpan FullSpan
    {
        get
        {
            if (this.Internal is null)
            {
                return new TextSpan(this.Position, 0);
            }

            return new TextSpan(this.Position, this.Internal.FullWidth);
        }
    }

    /// <summary>
    /// トークンの幅（トリビアを除く）。
    /// </summary>
    public int Width => this.Internal?.Width ?? 0;

    /// <summary>
    /// トークンの全幅（トリビアを含む）。
    /// </summary>
    public int FullWidth => this.Internal?.FullWidth ?? 0;

    /// <summary>
    /// このトークンが欠落トークン（エラー回復で挿入されたもの）かどうか。
    /// </summary>
    public bool IsMissing => this.Internal?.IsMissing ?? true;

    /// <summary>
    /// このトークンに診断情報が含まれるかどうか。
    /// </summary>
    public bool ContainsDiagnostics => this.Internal?.ContainsDiagnostics ?? false;

    /// <summary>
    /// 先行トリビアのリスト。
    /// </summary>
    public SyntaxTriviaList LeadingTrivia => new(this, isLeading: true);

    /// <summary>
    /// 後続トリビアのリスト。
    /// </summary>
    public SyntaxTriviaList TrailingTrivia => new(this, isLeading: false);

    /// <summary>
    /// SyntaxToken を作成する。
    /// </summary>
    /// <param name="internalToken">内部トークン。</param>
    /// <param name="parent">親ノード。</param>
    /// <param name="position">絶対位置。</param>
    /// <param name="index">スロットインデックス。</param>
    internal SyntaxToken(InternalToken? internalToken, SyntaxNode? parent, int position, int index)
    {
        this.Internal = internalToken;
        this.Parent = parent;
        this.Position = position;
        this.Index = index;
    }

    /// <summary>
    /// 完全なテキストを取得する。
    /// </summary>
    /// <returns>トリビアを含む完全なテキスト。</returns>
    public string ToFullString()
    {
        return this.Internal?.ToFullString() ?? string.Empty;
    }

    /// <inheritdoc />
    public bool Equals(SyntaxToken other)
    {
        return this.Position == other.Position
            && ReferenceEquals(this.Internal, other.Internal);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is SyntaxToken other && this.Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
#if NETSTANDARD2_0
        unchecked
        {
            return (this.Position * 397) ^ (this.Internal?.GetHashCode() ?? 0);
        }
#else
        return HashCode.Combine(this.Position, this.Internal);
#endif
    }

    /// <summary>
    /// 2つの SyntaxToken が等しいかどうかを判定する。
    /// </summary>
    public static bool operator ==(SyntaxToken left, SyntaxToken right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// 2つの SyntaxToken が等しくないかどうかを判定する。
    /// </summary>
    public static bool operator !=(SyntaxToken left, SyntaxToken right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var text = this.Internal?.Text ?? string.Empty;
        var missing = this.IsMissing ? " (missing)" : string.Empty;
        return $"{this.Kind}: \"{EscapeText(text)}\"{missing}";
    }

    private static string EscapeText(string text)
    {
#if NETSTANDARD
        return text
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t");
#else
        return text
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\t", "\\t", StringComparison.Ordinal);
#endif
    }
}

/// <summary>
/// トリビアのリストを表す構造体。
/// </summary>
public readonly struct SyntaxTriviaList : IReadOnlyList<SyntaxTrivia>
{
    private readonly SyntaxToken _token;
    private readonly bool _isLeading;

    /// <summary>
    /// トリビアの数。
    /// </summary>
    public int Count
    {
        get
        {
            if (this._token.Internal is null)
            {
                return 0;
            }

            return this._isLeading
                ? this._token.Internal.LeadingTrivia.Count
                : this._token.Internal.TrailingTrivia.Count;
        }
    }

    /// <summary>
    /// 指定されたインデックスのトリビアを取得する。
    /// </summary>
    /// <param name="index">インデックス。</param>
    /// <returns>トリビア。</returns>
    public SyntaxTrivia this[int index]
    {
        get
        {
            if (this._token.Internal is null || index < 0 || index >= this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var internalTrivia = this._isLeading
                ? this._token.Internal.LeadingTrivia[index]
                : this._token.Internal.TrailingTrivia[index];

            var position = this.CalculatePosition(index);
            return new SyntaxTrivia(internalTrivia, this._token, position, index);
        }
    }

    /// <summary>
    /// SyntaxTriviaList を作成する。
    /// </summary>
    /// <param name="token">所属するトークン。</param>
    /// <param name="isLeading">先行トリビアかどうか。</param>
    internal SyntaxTriviaList(SyntaxToken token, bool isLeading)
    {
        this._token = token;
        this._isLeading = isLeading;
    }

    private int CalculatePosition(int index)
    {
        if (this._token.Internal is null)
        {
            return this._token.Position;
        }

        if (this._isLeading)
        {
            var position = this._token.Position;
            for (var i = 0; i < index; i++)
            {
                position += this._token.Internal.LeadingTrivia[i].Width;
            }

            return position;
        }
        else
        {
            var position = this._token.Position + this._token.Internal.LeadingTriviaWidth + this._token.Internal.Width;
            for (var i = 0; i < index; i++)
            {
                position += this._token.Internal.TrailingTrivia[i].Width;
            }

            return position;
        }
    }

    /// <summary>
    /// トリビアを列挙する。
    /// </summary>
    /// <returns>列挙子。</returns>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator<SyntaxTrivia> IEnumerable<SyntaxTrivia>.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    /// <summary>
    /// SyntaxTriviaList の列挙子。
    /// </summary>
    public struct Enumerator : IEnumerator<SyntaxTrivia>
    {
        private readonly SyntaxTriviaList _list;
        private int _index;

        /// <summary>
        /// 現在のトリビア。
        /// </summary>
        public readonly SyntaxTrivia Current => this._list[this._index];

        readonly object System.Collections.IEnumerator.Current => this.Current;

        internal Enumerator(SyntaxTriviaList list)
        {
            this._list = list;
            this._index = -1;
        }

        /// <summary>
        /// 次のトリビアに移動する。
        /// </summary>
        /// <returns>次のトリビアが存在する場合は true。</returns>
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

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }

    public static bool operator ==(SyntaxTriviaList left, SyntaxTriviaList right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SyntaxTriviaList left, SyntaxTriviaList right)
    {
        return !(left == right);
    }
}
