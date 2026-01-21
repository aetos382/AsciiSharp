
using System;

using AsciiSharp.Text;

namespace AsciiSharp.Diagnostics;
/// <summary>
/// 診断情報の重大度。
/// </summary>
public enum DiagnosticSeverity
{
    /// <summary>情報メッセージ。</summary>
    Info = 0,

    /// <summary>警告メッセージ。</summary>
    Warning = 1,

    /// <summary>エラーメッセージ。</summary>
    Error = 2,
}

/// <summary>
/// 構文解析中に発生した診断情報（エラー、警告、情報）を表す不変クラス。
/// </summary>
public sealed class Diagnostic : IEquatable<Diagnostic>
{
    /// <summary>
    /// 診断コード（例: "ASD001"）。
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// 診断メッセージ。
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// 診断の重大度。
    /// </summary>
    public DiagnosticSeverity Severity { get; }

    /// <summary>
    /// 診断が発生した位置。
    /// </summary>
    public TextSpan Location { get; }

    /// <summary>
    /// メッセージフォーマット用の追加引数。
    /// </summary>
    public object?[] Arguments { get; }

    /// <summary>
    /// 新しい Diagnostic インスタンスを作成する。
    /// </summary>
    /// <param name="code">診断コード。空でない文字列。</param>
    /// <param name="message">診断メッセージ。空でない文字列。</param>
    /// <param name="severity">重大度。</param>
    /// <param name="location">発生位置。</param>
    /// <param name="arguments">フォーマット引数（オプション）。</param>
    /// <exception cref="ArgumentNullException">code または message が null の場合。</exception>
    /// <exception cref="ArgumentException">code または message が空の場合。</exception>
    public Diagnostic(
        string code,
        string message,
        DiagnosticSeverity severity,
        TextSpan location,
        params object?[] arguments)
    {
        ArgumentNullException.ThrowIfNull(code);

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("診断コードは空にできません。", nameof(code));
        }

        ArgumentNullException.ThrowIfNull(message);

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("診断メッセージは空にできません。", nameof(message));
        }

        this.Code = code;
        this.Message = message;
        this.Severity = severity;
        this.Location = location;
        this.Arguments = arguments ?? [];
    }

    /// <summary>
    /// エラー診断を作成する。
    /// </summary>
    /// <param name="code">診断コード。</param>
    /// <param name="message">診断メッセージ。</param>
    /// <param name="location">発生位置。</param>
    /// <param name="arguments">フォーマット引数（オプション）。</param>
    /// <returns>新しい Diagnostic インスタンス。</returns>
    public static Diagnostic Error(string code, string message, TextSpan location, params object?[] arguments)
    {
        return new Diagnostic(code, message, DiagnosticSeverity.Error, location, arguments);
    }

    /// <summary>
    /// 警告診断を作成する。
    /// </summary>
    /// <param name="code">診断コード。</param>
    /// <param name="message">診断メッセージ。</param>
    /// <param name="location">発生位置。</param>
    /// <param name="arguments">フォーマット引数（オプション）。</param>
    /// <returns>新しい Diagnostic インスタンス。</returns>
    public static Diagnostic Warning(string code, string message, TextSpan location, params object?[] arguments)
    {
        return new Diagnostic(code, message, DiagnosticSeverity.Warning, location, arguments);
    }

    /// <summary>
    /// 情報診断を作成する。
    /// </summary>
    /// <param name="code">診断コード。</param>
    /// <param name="message">診断メッセージ。</param>
    /// <param name="location">発生位置。</param>
    /// <param name="arguments">フォーマット引数（オプション）。</param>
    /// <returns>新しい Diagnostic インスタンス。</returns>
    public static Diagnostic Info(string code, string message, TextSpan location, params object?[] arguments)
    {
        return new Diagnostic(code, message, DiagnosticSeverity.Info, location, arguments);
    }

    /// <inheritdoc />
    public bool Equals(Diagnostic? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return this.Code == other.Code
            && this.Message == other.Message
            && this.Severity == other.Severity
            && this.Location == other.Location;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as Diagnostic);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
#if NETSTANDARD2_0
        unchecked
        {
            var hashCode = this.Code.GetHashCode();
            hashCode = (hashCode * 397) ^ this.Message.GetHashCode();
            hashCode = (hashCode * 397) ^ (int)this.Severity;
            hashCode = (hashCode * 397) ^ this.Location.GetHashCode();
            return hashCode;
        }
#else
        return HashCode.Combine(this.Code, this.Message, this.Severity, this.Location);
#endif
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{this.Severity} {this.Code}: {this.Message} at {this.Location}";
    }
}
