using System;

using AsciiSharp.Syntax;

namespace AsciiSharp;

internal static class Parser
{
    public static DocumentSyntax Parse(ReadOnlySpan<char> source)
    {
        if (source.IsEmpty)
        {
            return new DocumentSyntax
            {
                Blocks = [],
                Location = new Location(new Position(1, 1), new Position(1, 1))
            };
        }

        var text = new TextSyntax
        {
            Value = source.ToString(),
            Location = new Location(new Position(1, 1), new Position(1, source.Length))
        };

        var paragraph = new ParagraphSyntax
        {
            Inlines = [text],
            Location = new Location(new Position(1, 1), new Position(1, source.Length))
        };

        return new DocumentSyntax
        {
            Blocks = [paragraph],
            Location = new Location(new Position(1, 1), new Position(1, source.Length))
        };
    }
}
