#if !NET7_0_OR_GREATER

#pragma warning disable CA1064 // 例外は public にする必要がある — Polyfill のため internal

namespace System.Diagnostics;

/// <summary>
/// 到達不能なコードが実行されたときにスローされる例外。
/// </summary>
internal class UnreachableException : Exception
{
    /// <summary>
    /// <see cref="UnreachableException"/> の新しいインスタンスを初期化する。
    /// </summary>
    public UnreachableException()
        : base("The program executed an instruction that was thought to be unreachable.")
    {
    }

    /// <summary>
    /// <see cref="UnreachableException"/> の新しいインスタンスを初期化する。
    /// </summary>
    /// <param name="message">エラーメッセージ。</param>
    public UnreachableException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// <see cref="UnreachableException"/> の新しいインスタンスを初期化する。
    /// </summary>
    /// <param name="message">エラーメッセージ。</param>
    /// <param name="innerException">内部例外。</param>
    public UnreachableException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

#endif
