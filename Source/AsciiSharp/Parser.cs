using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsciiSharp;

internal class Parser
{
    public Parser(
        ParseOptions options)
    {
        this.Options = options;
    }

    public ValueTask<Document> ParseDocumentAsync(
        ReadOnlySpan<char> source,
        CancellationToken cancellationToken)
    {
        foreach (var line in source.GetLines())
        {
            var x = 1;
        }

        throw new NotImplementedException();
    }

    public ParseOptions Options { get; }
}
