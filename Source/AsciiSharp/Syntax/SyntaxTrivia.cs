
using System;

using AsciiSharp.InternalSyntax;
using AsciiSharp.Text;

namespace AsciiSharp.Syntax;
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
    public SyntaxKind Kind => this._internal.Kind;

    /// <summary>
    /// トリビアのテキスト。
    /// </summary>
    public string Text => this._internal.Text;

    /// <summary>
    /// 所属するトークン。
    /// </summary>
    public SyntaxToken Token => this._token;

    /// <summary>
    /// トリビアのスパン。
    /// </summary>
    public TextSpan Span => new(this._position, this._internal.Width);

    /// <summary>
    /// トリビアのフルスパン（Span と同じ）。
    /// </summary>
    public TextSpan FullSpan => this.Span;

    /// <summary>
    /// トリビアの幅。
    /// </summary>
    public int Width => this._internal.Width;

    /// <summary>
    /// トリビアリスト内のインデックス。
    /// </summary>
    public int Index => this._index;

    /// <summary>
    /// SyntaxTrivia を作成する。
    /// </summary>
    /// <param name="internalTrivia">内部トリビア。</param>
    /// <param name="token">所属するトークン。</param>
    /// <param name="position">絶対位置。</param>
    /// <param name="index">トリビアリスト内のインデックス。</param>
    internal SyntaxTrivia(InternalTrivia internalTrivia, SyntaxToken token, int position, int index)
    {
        this._internal = internalTrivia;
        this._token = token;
        this._position = position;
        this._index = index;
    }

    /// <summary>
    /// 完全なテキストを取得する。
    /// </summary>
    /// <returns>トリビアのテキスト。</returns>
    public string ToFullString()
    {
        return this._internal.Text;
    }

    /// <inheritdoc />
    public bool Equals(SyntaxTrivia other)
    {
        return this._position == other._position
            && this._internal.Kind == other._internal.Kind
            && this._internal.Text == other._internal.Text;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is SyntaxTrivia other && this.Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
#if NETSTANDARD2_0
        unchecked
        {
            var hash = this._position;
            hash = (hash * 397) ^ (int)this._internal.Kind;
            hash = (hash * 397) ^ (this._internal.Text?.GetHashCode() ?? 0);
            return hash;
        }
#else
        return HashCode.Combine(this._position, this._internal.Kind, this._internal.Text);
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
        return $"{this.Kind}: \"{EscapeText(this._internal.Text)}\"";
    }

    private static string EscapeText(string text)
    {
        return text
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t");
    }
}
