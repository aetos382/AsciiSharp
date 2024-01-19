using System;
using System.Buffers;

namespace AsciiSharp;

internal static class SpanHelper
{
    private static readonly SearchValues<char> NewLineChars =
        SearchValues.Create(['\r', '\n', '\f', '\u0085', '\u2028', '\u2029']);

    public static void GetLine(
        this ReadOnlySpan<char> source,
        out int length,
        out int next)
    {
        GetLine(source, 0, out length, out next);
    }

    public static void GetLine(
        this ReadOnlySpan<char> source,
        int start,
        out int length,
        out int next)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(start, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(start, source.Length);

        if (start > 0)
        {
            source = source[start..];
        }

        var index = source.IndexOfAny(NewLineChars);
        if (index < 0)
        {
            length = source.Length;
            next = -1;

            return;
        }

        var stride = 1;

        if (source[index] == '\r' && index < source.Length - 1 && source[index + 1] == '\n')
        {
            stride = 2;
        }

        length = index;
        next = start + index + stride;
    }

    public static LineEnumerator GetLines(
        this ReadOnlySpan<char> source)
    {
        return new LineEnumerator(source);
    }

    public ref struct LineEnumerator
    {
        internal LineEnumerator(
            ReadOnlySpan<char> source)
        {
            this._source = source;
        }

        public LineEnumerator GetEnumerator()
        {
            return this;
        }

        public SpanLine Current { get; private set; }

        public bool MoveNext()
        {
            if (!this._isEnumeratorActive)
            {
                return false;
            }

            var source = this._source;
            var start = this._start;

            source.GetLine(start, out var length, out var next);

            var range = start..(start + length);

            this.Current = new SpanLine(source[range], range, source.Length);
            this._start = next;

            if (next == -1)
            {
                this._isEnumeratorActive = false;
            }

            return true;
        }

        private readonly ReadOnlySpan<char> _source;

        private int _start = 0;

        private bool _isEnumeratorActive = true;
    }

    public readonly ref struct SpanLine
    {
        public SpanLine(
            ReadOnlySpan<char> line,
            Range range,
            int totalLength)
        {
            this.Line = line;
            this.Range = range;

            var (offset, length) = range.GetOffsetAndLength(totalLength);

            this.Offset = offset;
            this.Length = length;
        }

        public ReadOnlySpan<char> Line { get; }

        public Range Range { get; }

        public int Offset { get; }

        public int Length { get; }
    }
}
