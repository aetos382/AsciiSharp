#if !NET7_0_OR_GREATER

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System;

public static class ArgumentExceptionExtensions
{
    // false positive
    // https://github.com/dotnet/sdk/issues/51681
#pragma warning disable CA1034, CS8777
    extension(ArgumentException)
    {
        public static void ThrowIfNullOrEmpty(
            [NotNull] string? argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (string.IsNullOrEmpty(argument))
            {
                ThrowNullOrEmpty(paramName);
            }
        }

        public static void ThrowIfNullOrWhiteSpace(
            [NotNull] string? argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                ThrowNullOrWhitespace(paramName);
            }
        }
    }
#pragma warning restore

    [DoesNotReturn]
    private static void ThrowNullOrEmpty(string? paramName)
    {
        throw new ArgumentException(
            "Value cannot be null or empty.",
            paramName);
    }

    [DoesNotReturn]
    private static void ThrowNullOrWhitespace(string? paramName)
    {
        throw new ArgumentException(
            "Value cannot be null or white space.",
            paramName);
    }
}

#endif
