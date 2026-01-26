#if !NET7_0_OR_GREATER

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System;

public static class ArgumentExceptionExtensions
{
    extension (ArgumentException)
    {
        public static void ThrowIfNullOrEmpty(
            [NotNull] string? argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (string.IsNullOrEmpty(argument))
            {
                Throw(paramName);
            }

            Debug.Assert(argument is not null);
        }

        public static void ThrowIfNullOrWhiteSpace(
            [NotNull] string? argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                Throw(paramName);
            }

            Debug.Assert(argument is not null);
        }
    }

    [DoesNotReturn]
    private static void Throw(string? paramName)
    {
        throw new ArgumentException(paramName);
    }
}
#endif
