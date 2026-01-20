namespace AsciiSharp.Syntax;

using System;
using AsciiSharp.Text;

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
    public bool IsNode => !_isToken && _node is not null;

    /// <summary>
    /// これがトークンかどうか。
    /// </summary>
    public bool IsToken => _isToken;

    /// <summary>
    /// 種別。
    /// </summary>
    public SyntaxKind Kind => _isToken ? _token.Kind : (_node?.Kind ?? SyntaxKind.None);

    /// <summary>
    /// スパン（トリビアを除く）。
    /// </summary>
    public TextSpan Span => _isToken ? _token.Span : (_node?.Span ?? default);

    /// <summary>
    /// フルスパン（トリビアを含む）。
    /// </summary>
    public TextSpan FullSpan => _isToken ? _token.FullSpan : (_node?.FullSpan ?? default);

    /// <summary>
    /// 親ノード。
    /// </summary>
    public SyntaxNode? Parent => _isToken ? _token.Parent : _node?.Parent;

    /// <summary>
    /// 欠落しているかどうか。
    /// </summary>
    public bool IsMissing => _isToken ? _token.IsMissing : (_node?.IsMissing ?? true);

    /// <summary>
    /// ノードから SyntaxNodeOrToken を作成する。
    /// </summary>
    /// <param name="node">ノード。</param>
    public SyntaxNodeOrToken(SyntaxNode node)
    {
        _node = node;
        _token = default;
        _isToken = false;
    }

    /// <summary>
    /// トークンから SyntaxNodeOrToken を作成する。
    /// </summary>
    /// <param name="token">トークン。</param>
    public SyntaxNodeOrToken(SyntaxToken token)
    {
        _node = null;
        _token = token;
        _isToken = true;
    }

    /// <summary>
    /// ノードとして取得する。
    /// </summary>
    /// <returns>ノード。トークンの場合は null。</returns>
    public SyntaxNode? AsNode()
    {
        return _isToken ? null : _node;
    }

    /// <summary>
    /// トークンとして取得する。
    /// </summary>
    /// <returns>トークン。ノードの場合は既定値。</returns>
    public SyntaxToken AsToken()
    {
        return _isToken ? _token : default;
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
        if (_isToken != other._isToken)
        {
            return false;
        }

        if (_isToken)
        {
            return _token.Equals(other._token);
        }

        return ReferenceEquals(_node, other._node);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is SyntaxNodeOrToken other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        if (_isToken)
        {
            return _token.GetHashCode();
        }

        return _node?.GetHashCode() ?? 0;
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
        if (_isToken)
        {
            return _token.ToString();
        }

        return _node?.ToString() ?? string.Empty;
    }
}
