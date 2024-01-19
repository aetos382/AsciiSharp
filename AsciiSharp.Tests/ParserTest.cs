using System;

using Xunit;

namespace AsciiSharp.Tests;

public class ParserTest
{
    [Fact]
    public void Parse()
    {
        var source = "abc\r\nfoobar\r\n".AsSpan();

        var parser = new Parser(new ParseOptions());

        var document = parser.ParseDocument(source);
    }
}
