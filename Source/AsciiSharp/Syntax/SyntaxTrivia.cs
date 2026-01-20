namespace AsciiSharp.Syntax;

using System;
using AsciiSharp.InternalSyntax;
using AsciiSharp.Text;

/// <summary>
/// 外部構文木のトリビア（空白、コメント等）を表す構造体。
/// </summary>
public readonly struct SyntaxTrivia : IEquatable<SyntaxTrivia>
{
    private readonly InternalTrivia _internal;
    private readonly SyntaxToken _token;
    private readonly int _position;
    private readonly int _index;

    /// <summary>
    /// トリビアの種別。
    /// </summary>
    public SyntaxKind Kind => _internal.Kind;

    /// <summary>
    /// トリビアのテキスト。
    /// </summary>
    public string Text => _internal.Text;

    /// <summary>
    /// 所属するトークン。
    /// </summary>
    public SyntaxToken Token => _token;

    /// <summary>
    /// トリビアのスパン。
    /// </summary>
    public TextSpan Span => new TextSpan(_position, _internal.Width);

    /// <summary>
    /// トリビアのフルスパン（Span と同じ）。
    /// </summary>
    public TextSpan FullSpan => Span;

    /// <summary>
    /// トリビアの幅。
    /// </summary>
    public int Width => _internal.Width;

    /// <summary>
    /// トリビアリスト内のインデックス。
    /// </summary>
    public int Index => _index;

    /// <summary>
    /// SyntaxTrivia を作成する。
    /// </summary>
    /// <param name="internalTrivia">内部トリビア。</param>
    /// <param name="token">所属するトークン。</param>
    /// <param name="position">絶対位置。</param>
    /// <param name="index">トリビアリスト内のインデックス。</param>
    internal SyntaxTrivia(InternalTrivia internalTrivia, SyntaxToken token, int position, int index)
    {
        _internal = internalTrivia;
        _token = token;
        _position = position;
        _index = index;
    }

    /// <summary>
    /// 完全なテキストを取得する。
    /// </summary>
    /// <returns>トリビアのテキスト。</returns>
    public string ToFullString()
    {
        return _internal.Text;
    }

    /// <inheritdoc />
    public bool Equals(SyntaxTrivia other)
    {
        return _position == other._position
            && _internal.Kind == other._internal.Kind
            && _internal.Text == other._internal.Text;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is SyntaxTrivia other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
#if NETSTANDARD2_0
        unchecked
        {
            var hash = _position;
            hash = (hash * 397) ^ (int)_internal.Kind;
            hash = (hash * 397) ^ (_internal.Text?.GetHashCode() ?? 0);
            return hash;
        }
#else
        return HashCode.Combine(_position, _internal.Kind, _internal.Text);
#endif
    }

    /// <summary>
    /// 2つの SyntaxTrivia が等しいかどうかを判定する。
    /// </summary>
    public static bool operator ==(SyntaxTrivia left, SyntaxTrivia right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// 2つの SyntaxTrivia が等しくないかどうかを判定する。
    /// </summary>
    public static bool operator !=(SyntaxTrivia left, SyntaxTrivia right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Kind}: \"{EscapeText(_internal.Text)}\"";
    }

    private static string EscapeText(string text)
    {
        return text
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t");
    }
}
