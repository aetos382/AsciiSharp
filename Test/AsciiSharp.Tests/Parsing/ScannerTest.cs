using System;
using System.Runtime.CompilerServices;

using Xunit;

using FluentAssertions;

using AsciiSharp.Parsing;

namespace AsciiSharp.Tests.Parsing;

public class ScannerTest
{
    [Theory]
    [InlineData("", 0, true)]
    [InlineData("", 1, true)]
    [InlineData("a", 0, false)]
    [InlineData("a", 1, true)]
    [InlineData("a", 2, true)]
    public void IsAtEnd(
        string source,
        int skip,
        bool expectedResult)
    {
        var scanner = new Scanner(source.AsMemory());
        scanner.IsAtEnd(skip).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("", 0, false)]
    [InlineData("a", 0, true, 'a')]
    [InlineData("a", 1, false)]
    [InlineData("ab", 1, true, 'b')]
    public void TryPeek(
        string source,
        int skip,
        bool expectedResult,
        char expectedPeeked = default)
    {
        var scanner = new Scanner(source.AsMemory());
        var actualResult = scanner.TryPeek(out var actualPeeked, skip);

        actualResult.Should().Be(expectedResult);

        if (actualResult)
        {
            actualPeeked.Should().Be(expectedPeeked);
        }
    }

    [Fact]
    public void X()
    {
        var scanner = new Scanner("// abc\r\n".AsMemory());
        var tokenInfo = new TokenInfo();
        
        scanner.ScanToken(ref tokenInfo);
    }
}

internal static class ScannerExtensions
{
    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    public static extern void ScanToken(
        this Scanner scanner,
        ref TokenInfo tokenInfo);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    public static extern bool IsAtEnd(
        this Scanner scanner,
        int skip = 0);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    public static extern bool TryPeek(
        this Scanner scanner,
        out char result,
        int skip = 0);
}
