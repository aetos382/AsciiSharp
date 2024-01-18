using System;

using Xunit;

namespace AsciiSharp.Tests;

public class SpanHelpersTest
{
    [Theory]
    [InlineData("ABC\r", true, 3, 4)]
    [InlineData("ABC\r\n", true, 3, 5)]
    [InlineData("ABC\n", true, 3, 4)]
    [InlineData("ABC\f", true, 3, 4)]
    [InlineData("ABC\u0085", true, 3, 4)]
    [InlineData("ABC\u2028", true, 3, 4)]
    [InlineData("ABC\u2029", true, 3, 4)]
    [InlineData("ABC", false, 0, 0)]
    [InlineData("ABC\v", false, 0, 0)]
    public void TryFindNewLineTest(
        string value,
        bool result,
        int start,
        int end)
    {
        var actualResult = value.AsSpan().TryFindNewLine(out var range);

        Assert.Equal(result, actualResult);
        Assert.Equal(start..end, range);
    }
}
