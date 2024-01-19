using System;

namespace AsciiSharp;

internal class Parser
{
    public Parser(
        ParseOptions options)
    {
        this.Options = options;
    }

    public Document ParseDocument(
        ReadOnlySpan<char> source)
    {
        foreach (var line in source.GetLines())
        {
            var x = 1;
        }

        throw new NotImplementedException();
    }

    public ParseOptions Options { get; }
}
