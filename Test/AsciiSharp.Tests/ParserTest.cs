using System.Threading.Tasks;

using Xunit;

namespace AsciiSharp.Tests;

public class ParserTest
{
    [Fact]
    public async Task Parse()
    {
        const string source = "abc\r\nfoobar\r\n";

        var parser = new Parser(new ParseOptions());

        var document = await parser
            .ParseDocumentAsync(source, default);
    }
}
