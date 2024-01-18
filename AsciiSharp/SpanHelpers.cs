using System;
using System.Buffers;

namespace AsciiSharp;

internal static class SpanHelpers
{
    private static readonly SearchValues<char> NewLineChars =
        SearchValues.Create(['\r', '\n', '\f', '\u0085', '\u2028', '\u2029']);

    public static bool TryFindNewLine(
        this ReadOnlySpan<char> chars,
        out Range range)
    {
        var index = chars.IndexOfAny(NewLineChars);
        if (index < 0)
        {
            range = default;
            return false;
        }

        var stride = 1;

        if (chars[index] == '\r' &&
            index < chars.Length - 1 &&
            chars[index + 1] == '\n')
        {
            stride = 2;
        }

        range = new Range(index, index + stride);
        return true;
    }
}
