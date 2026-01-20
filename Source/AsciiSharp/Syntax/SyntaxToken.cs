namespace AsciiSharp.Syntax;

using System;
using System.Collections.Generic;
using AsciiSharp.InternalSyntax;
using AsciiSharp.Text;

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
    public SyntaxKind Kind => Internal?.Kind ?? SyntaxKind.None;

    /// <summary>
    /// トークンのテキスト。
    /// </summary>
    public string Text => Internal?.Text ?? string.Empty;

    /// <summary>
    /// トークンのスパン（トリビアを除く）。
    /// </summary>
    public TextSpan Span
    {
        get
        {
            if (Internal is null)
            {
                return new TextSpan(Position, 0);
            }

            return new TextSpan(Position + Internal.LeadingTriviaWidth, Internal.Width);
        }
    }

    /// <summary>
    /// トークンのフルスパン（トリビアを含む）。
    /// </summary>
    public TextSpan FullSpan
    {
        get
        {
            if (Internal is null)
            {
                return new TextSpan(Position, 0);
            }

            return new TextSpan(Position, Internal.FullWidth);
        }
    }

    /// <summary>
    /// トークンの幅（トリビアを除く）。
    /// </summary>
    public int Width => Internal?.Width ?? 0;

    /// <summary>
    /// トークンの全幅（トリビアを含む）。
    /// </summary>
    public int FullWidth => Internal?.FullWidth ?? 0;

    /// <summary>
    /// このトークンが欠落トークン（エラー回復で挿入されたもの）かどうか。
    /// </summary>
    public bool IsMissing => Internal?.IsMissing ?? true;

    /// <summary>
    /// このトークンに診断情報が含まれるかどうか。
    /// </summary>
    public bool ContainsDiagnostics => Internal?.ContainsDiagnostics ?? false;

    /// <summary>
    /// 先行トリビアのリスト。
    /// </summary>
    public SyntaxTriviaList LeadingTrivia => new SyntaxTriviaList(this, isLeading: true);

    /// <summary>
    /// 後続トリビアのリスト。
    /// </summary>
    public SyntaxTriviaList TrailingTrivia => new SyntaxTriviaList(this, isLeading: false);

    /// <summary>
    /// SyntaxToken を作成する。
    /// </summary>
    /// <param name="internalToken">内部トークン。</param>
    /// <param name="parent">親ノード。</param>
    /// <param name="position">絶対位置。</param>
    /// <param name="index">スロットインデックス。</param>
    internal SyntaxToken(InternalToken? internalToken, SyntaxNode? parent, int position, int index)
    {
        Internal = internalToken;
        Parent = parent;
        Position = position;
        Index = index;
    }

    /// <summary>
    /// 完全なテキストを取得する。
    /// </summary>
    /// <returns>トリビアを含む完全なテキスト。</returns>
    public string ToFullString()
    {
        return Internal?.ToFullString() ?? string.Empty;
    }

    /// <inheritdoc />
    public bool Equals(SyntaxToken other)
    {
        return Position == other.Position
            && ReferenceEquals(Internal, other.Internal);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is SyntaxToken other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
#if NETSTANDARD2_0
        unchecked
        {
            return (Position * 397) ^ (Internal?.GetHashCode() ?? 0);
        }
#else
        return HashCode.Combine(Position, Internal);
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
        var text = Internal?.Text ?? string.Empty;
        var missing = IsMissing ? " (missing)" : string.Empty;
        return $"{Kind}: \"{EscapeText(text)}\"{missing}";
    }

    private static string EscapeText(string text)
    {
        return text
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t");
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
            if (_token.Internal is null)
            {
                return 0;
            }

            return _isLeading
                ? _token.Internal.LeadingTrivia.Count
                : _token.Internal.TrailingTrivia.Count;
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
            if (_token.Internal is null || index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var internalTrivia = _isLeading
                ? _token.Internal.LeadingTrivia[index]
                : _token.Internal.TrailingTrivia[index];

            var position = CalculatePosition(index);
            return new SyntaxTrivia(internalTrivia, _token, position, index);
        }
    }

    /// <summary>
    /// SyntaxTriviaList を作成する。
    /// </summary>
    /// <param name="token">所属するトークン。</param>
    /// <param name="isLeading">先行トリビアかどうか。</param>
    internal SyntaxTriviaList(SyntaxToken token, bool isLeading)
    {
        _token = token;
        _isLeading = isLeading;
    }

    private int CalculatePosition(int index)
    {
        if (_token.Internal is null)
        {
            return _token.Position;
        }

        if (_isLeading)
        {
            var position = _token.Position;
            for (var i = 0; i < index; i++)
            {
                position += _token.Internal.LeadingTrivia[i].Width;
            }

            return position;
        }
        else
        {
            var position = _token.Position + _token.Internal.LeadingTriviaWidth + _token.Internal.Width;
            for (var i = 0; i < index; i++)
            {
                position += _token.Internal.TrailingTrivia[i].Width;
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
        return GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
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
        public SyntaxTrivia Current => _list[_index];

        object System.Collections.IEnumerator.Current => Current;

        internal Enumerator(SyntaxTriviaList list)
        {
            _list = list;
            _index = -1;
        }

        /// <summary>
        /// 次のトリビアに移動する。
        /// </summary>
        /// <returns>次のトリビアが存在する場合は true。</returns>
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
