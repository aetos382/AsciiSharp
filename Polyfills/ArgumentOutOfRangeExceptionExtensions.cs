#if !NET8_0_OR_GREATER

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using SR = AsciiSharp.Properties.Resources;

namespace System;

internal static class ArgumentOutOfRangeExceptionExtensions
{
    extension(ArgumentOutOfRangeException)
    {
        public static void ThrowIfEqual<T>(
            T value,
            T other,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (EqualityComparer<T>.Default.Equals(value, other))
            {
                ThrowEqual(paramName);
            }
        }

        public static void ThrowIfGreaterThan<T>(
            T value,
            T other,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : IComparable<T>
        {
            if (value.CompareTo(other) > 0)
            {
                ThrowGreaterThan(paramName);
            }
        }

        public static void ThrowIfGreaterThanOrEqual<T>(
            T value,
            T other,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : IComparable<T>
        {
            if (value.CompareTo(other) >= 0)
            {
                ThrowGreaterThanOrEqual(paramName);
            }
        }

        public static void ThrowIfLessThan<T>(
            T value,
            T other,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : IComparable<T>
        {
            if (value.CompareTo(other) < 0)
            {
                ThrowLessThan(paramName);
            }
        }

        public static void ThrowIfLessThanOrEqual<T>(
            T value,
            T other,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : IComparable<T>
        {
            if (value.CompareTo(other) <= 0)
            {
                ThrowLessThanOrEqual(paramName);
            }
        }

        public static void ThrowIfNotEqual<T>(
            T value,
            T other,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(value, other))
            {
                ThrowNotEqual(paramName);
            }
        }

        public static void ThrowIfNegative(
            int value,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value < 0)
            {
                ThrowNegative(paramName);
            }
        }

        public static void ThrowIfNegativeOrZero(
            int value,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value <= 0)
            {
                ThrowNegativeOrZero(paramName);
            }
        }

        public static void ThrowIfZero(
            int value,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value == 0)
            {
                ThrowZero(paramName);
            }
        }
    }

    [DoesNotReturn]
    private static void ThrowEqual(
        string? paramName)
    {
        throw new ArgumentOutOfRangeException(
            paramName,
            SR.Error_ArgumentEqual);
    }

    [DoesNotReturn]
    private static void ThrowGreaterThan(
        string? paramName)
    {
        throw new ArgumentOutOfRangeException(
            paramName,
            SR.Error_ArgumentGreaterThan);
    }

    [DoesNotReturn]
    private static void ThrowGreaterThanOrEqual(
        string? paramName)
    {
        throw new ArgumentOutOfRangeException(
            paramName,
            SR.Error_ArgumentGreaterThanOrEqual);
    }

    [DoesNotReturn]
    private static void ThrowLessThan(
        string? paramName)
    {
        throw new ArgumentOutOfRangeException(
            paramName,
            SR.Error_ArgumentLessThan);
    }

    [DoesNotReturn]
    private static void ThrowLessThanOrEqual(
        string? paramName)
    {
        throw new ArgumentOutOfRangeException(
            paramName,
            SR.Error_ArgumentLessThanOrEqual);
    }

    [DoesNotReturn]
    private static void ThrowNotEqual(
        string? paramName)
    {
        throw new ArgumentOutOfRangeException(
            paramName,
            SR.Error_ArgumentNotEqual);
    }

    [DoesNotReturn]
    private static void ThrowNegative(
        string? paramName)
    {
        throw new ArgumentOutOfRangeException(
            paramName,
            SR.Error_ArgumentNegative);
    }

    [DoesNotReturn]
    private static void ThrowNegativeOrZero(
        string? paramName)
    {
        throw new ArgumentOutOfRangeException(
            paramName,
            SR.Error_ArgumentNegativeOrZero);
    }

    [DoesNotReturn]
    private static void ThrowZero(
        string? paramName)
    {
        throw new ArgumentOutOfRangeException(
            paramName,
            SR.Error_ArgumentZero);
    }
}

#endif
