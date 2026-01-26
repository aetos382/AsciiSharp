
using System;

using AsciiSharp.Text;

namespace AsciiSharp.Syntax;
/// <summary>
/// 構文ノードまたはトークンを表す構造体。
/// </summary>
public readonly struct SyntaxNodeOrToken : IEquatable<SyntaxNodeOrToken>
{
    private readonly SyntaxNode? _node;
    private readonly SyntaxToken _token;
    private readonly bool _isToken;

    /// <summary>
    /// これがノードかどうか。
    /// </summary>
    public bool IsNode => !this._isToken && this._node is not null;

    /// <summary>
    /// これがトークンかどうか。
    /// </summary>
    public bool IsToken => this._isToken;

    /// <summary>
    /// 種別。
    /// </summary>
    public SyntaxKind Kind => this._isToken ? this._token.Kind : (this._node?.Kind ?? SyntaxKind.None);

    /// <summary>
    /// スパン（トリビアを除く）。
    /// </summary>
    public TextSpan Span => this._isToken ? this._token.Span : (this._node?.Span ?? default);

    /// <summary>
    /// フルスパン（トリビアを含む）。
    /// </summary>
    public TextSpan FullSpan => this._isToken ? this._token.FullSpan : (this._node?.FullSpan ?? default);

    /// <summary>
    /// 親ノード。
    /// </summary>
    public SyntaxNode? Parent => this._isToken ? this._token.Parent : this._node?.Parent;

    /// <summary>
    /// 欠落しているかどうか。
    /// </summary>
    public bool IsMissing => this._isToken ? this._token.IsMissing : (this._node?.IsMissing ?? true);

    /// <summary>
    /// ノードから SyntaxNodeOrToken を作成する。
    /// </summary>
    /// <param name="node">ノード。</param>
    public SyntaxNodeOrToken(SyntaxNode node)
    {
        this._node = node;
        this._token = default;
        this._isToken = false;
    }

    /// <summary>
    /// トークンから SyntaxNodeOrToken を作成する。
    /// </summary>
    /// <param name="token">トークン。</param>
    public SyntaxNodeOrToken(SyntaxToken token)
    {
        this._node = null;
        this._token = token;
        this._isToken = true;
    }

    /// <summary>
    /// ノードとして取得する。
    /// </summary>
    /// <returns>ノード。トークンの場合は null。</returns>
    public SyntaxNode? AsNode()
    {
        return this._isToken ? null : this._node;
    }

    /// <summary>
    /// トークンとして取得する。
    /// </summary>
    /// <returns>トークン。ノードの場合は既定値。</returns>
    public SyntaxToken AsToken()
    {
        return this._isToken ? this._token : default;
    }

    /// <summary>
    /// ノードから暗黙的に変換する。
    /// </summary>
    /// <param name="node">ノード。</param>
    public static implicit operator SyntaxNodeOrToken(SyntaxNode node)
    {
        return new SyntaxNodeOrToken(node);
    }

    /// <summary>
    /// トークンから暗黙的に変換する。
    /// </summary>
    /// <param name="token">トークン。</param>
    public static implicit operator SyntaxNodeOrToken(SyntaxToken token)
    {
        return new SyntaxNodeOrToken(token);
    }

    /// <inheritdoc />
    public bool Equals(SyntaxNodeOrToken other)
    {
        if (this._isToken != other._isToken)
        {
            return false;
        }

        if (this._isToken)
        {
            return this._token.Equals(other._token);
        }

        return ReferenceEquals(this._node, other._node);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is SyntaxNodeOrToken other && this.Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        if (this._isToken)
        {
            return this._token.GetHashCode();
        }

        return this._node?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// 2つの SyntaxNodeOrToken が等しいかどうかを判定する。
    /// </summary>
    public static bool operator ==(SyntaxNodeOrToken left, SyntaxNodeOrToken right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// 2つの SyntaxNodeOrToken が等しくないかどうかを判定する。
    /// </summary>
    public static bool operator !=(SyntaxNodeOrToken left, SyntaxNodeOrToken right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (this._isToken)
        {
            return this._token.ToString();
        }

        return this._node?.ToString() ?? string.Empty;
    }

    public SyntaxNodeOrToken ToSyntaxNodeOrToken()
    {
        throw new NotImplementedException();
    }
}
