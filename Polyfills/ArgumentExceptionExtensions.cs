#if !NET7_0_OR_GREATER

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using SR = AsciiSharp.Properties.Resources;

namespace System;

public static class ArgumentExceptionExtensions
{
#pragma warning disable CA1034, CS8777 // false positive
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
                ThrowNullOrWhitespance(paramName);
            }
        }
    }
#pragma warning restore

    [DoesNotReturn]
    private static void ThrowNullOrEmpty(string? paramName)
    {
        throw new ArgumentException(
            SR.Error_ArgumentNullOrEmpty,
            paramName);
    }

    [DoesNotReturn]
    private static void ThrowNullOrWhitespance(string? paramName)
    {
        throw new ArgumentException(
            SR.Error_ArgumentNullOrWhiteSpace,
            paramName);
    }
}

#endif
