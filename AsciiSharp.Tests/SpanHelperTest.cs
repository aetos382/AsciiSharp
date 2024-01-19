using System;

using Xunit;

namespace AsciiSharp.Tests;

public class SpanHelperTest
{
    [Fact]
    public void GetLine_改行文字で終わるケース()
    {
        var source = "abc\r\nfoobar\r\n".AsSpan();

        source.GetLine(0, out var length, out var next);

        Assert.Equal(3, length);
        Assert.Equal(5, next);

        source.GetLine(next, out length, out next);

        Assert.Equal(6, length);
        Assert.Equal(13, next);

        source.GetLine(next, out length, out next);

        Assert.Equal(0, length);
        Assert.Equal(-1, next);
    }

    [Fact]
    public void GetLine_改行以外の文字で終わるケース()
    {
        var source = "abc\r\nfoobar".AsSpan();

        source.GetLine(0, out var length, out var next);

        Assert.Equal(3, length);
        Assert.Equal(5, next);

        source.GetLine(next, out length, out next);

        Assert.Equal(6, length);
        Assert.Equal(-1, next);
    }

    [Fact]
    public void EnumerateLines_改行文字で終わるケース()
    {
        var source = "abc\r\nfoobar\r\n".AsSpan();

        var e = source.GetLines();

        Assert.True(e.MoveNext());

        var line = e.Current;

        Assert.True("abc".AsSpan().SequenceEqual(line.Line));
        Assert.Equal(0, line.Offset);
        Assert.Equal(3, line.Length);

        Assert.True(e.MoveNext());

        line = e.Current;

        Assert.True("foobar".AsSpan().SequenceEqual(line.Line));
        Assert.Equal(5, line.Offset);
        Assert.Equal(6, line.Length);

        Assert.True(e.MoveNext());

        line = e.Current;

        Assert.True(ReadOnlySpan<char>.Empty.SequenceEqual(line.Line));
        Assert.Equal(13, line.Offset);
        Assert.Equal(0, line.Length);

        Assert.False(e.MoveNext());
    }

    [Fact]
    public void EnumerateLines_改行以外の文字で終わるケース()
    {
        var source = "abc\r\nfoobar".AsSpan();

        var e = source.GetLines();

        Assert.True(e.MoveNext());

        var line = e.Current;

        Assert.True("abc".AsSpan().SequenceEqual(line.Line));
        Assert.Equal(0, line.Offset);
        Assert.Equal(3, line.Length);

        Assert.True(e.MoveNext());

        line = e.Current;

        Assert.True("foobar".AsSpan().SequenceEqual(line.Line));
        Assert.Equal(5, line.Offset);
        Assert.Equal(6, line.Length);

        Assert.False(e.MoveNext());
    }
}
