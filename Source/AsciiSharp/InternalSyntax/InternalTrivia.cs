
using System;

namespace AsciiSharp.InternalSyntax;
/// <summary>
/// 内部トリビアを表す不変の構造体。
/// </summary>
/// <remarks>
/// <para>トリビアは空白、改行、コメントなど、構文的には意味を持たないがソースの復元に必要なテキストを表す。</para>
/// <para>この構造体は内部実装であり、外部からは SyntaxTrivia を通じてアクセスする。</para>
/// </remarks>
internal readonly struct InternalTrivia : IEquatable<InternalTrivia>
{
    /// <summary>
    /// トリビアの種別。
    /// </summary>
    public SyntaxKind Kind { get; }

    /// <summary>
    /// トリビアのテキスト。
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// トリビアの幅（テキストの長さ）。
    /// </summary>
    public int Width => this.Text.Length;

    /// <summary>
    /// トリビアが空かどうか。
    /// </summary>
    public bool IsEmpty => this.Text.Length == 0;

    /// <summary>
    /// 指定された種別とテキストで InternalTrivia を作成する。
    /// </summary>
    /// <param name="kind">トリビアの種別。</param>
    /// <param name="text">トリビアのテキスト。</param>
    /// <exception cref="ArgumentNullException">text が null の場合。</exception>
    public InternalTrivia(SyntaxKind kind, string text)
    {
        
<<<<<<< TODO: Unmerged change from project 'AsciiSharp(netstandard2.0)', Before:
        Kind = kind;
=======
        this.Kind = kind;
>>>>>>> After
ArgumentNullException.ThrowIfNull(text);

        this.Kind = kind;
        this.Text = text;
    }

    /// <summary>
    /// 空白トリビアを作成する。
    /// </summary>
    /// <param name="text">空白テキスト。</param>
    /// <returns>新しい InternalTrivia。</returns>
    public static InternalTrivia Whitespace(string text)
    {
        return new InternalTrivia(SyntaxKind.WhitespaceTrivia, text);
    }

    /// <summary>
    /// 行末トリビアを作成する。
    /// </summary>
    /// <param name="text">改行テキスト（\n, \r, \r\n）。</param>
    /// <returns>新しい InternalTrivia。</returns>
    public static InternalTrivia EndOfLine(string text)
    {
        return new InternalTrivia(SyntaxKind.EndOfLineTrivia, text);
    }

    /// <summary>
    /// 単一行コメントトリビアを作成する。
    /// </summary>
    /// <param name="text">コメントテキスト（// を含む）。</param>
    /// <returns>新しい InternalTrivia。</returns>
    public static InternalTrivia SingleLineComment(string text)
    {
        return new InternalTrivia(SyntaxKind.SingleLineCommentTrivia, text);
    }

    /// <summary>
    /// 複数行コメントトリビアを作成する。
    /// </summary>
    /// <param name="text">コメントテキスト（//// 区切りを含む）。</param>
    /// <returns>新しい InternalTrivia。</returns>
    public static InternalTrivia MultiLineComment(string text)
    {
        return new InternalTrivia(SyntaxKind.MultiLineCommentTrivia, text);
    }

    /// <inheritdoc />
    public bool Equals(InternalTrivia other)
    {
        return this.Kind == other.Kind && this.Text == other.Text;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is InternalTrivia other && this.Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
#if NETSTANDARD2_0
        unchecked
        {
            return ((int)this.Kind * 397) ^ (this.Text?.GetHashCode() ?? 0);
        }
#else
        return HashCode.Combine(this.Kind, this.Text);
#endif
    }

    /// <summary>
    /// 2つの InternalTrivia が等しいかどうかを判定する。
    /// </summary>
    public static bool operator ==(InternalTrivia left, InternalTrivia right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// 2つの InternalTrivia が等しくないかどうかを判定する。
    /// </summary>
    public static bool operator !=(InternalTrivia left, InternalTrivia right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{this.Kind}: \"{EscapeText(this.Text)}\"";
    }

    private static string EscapeText(string text)
    {
        return text
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t");
    }
}
