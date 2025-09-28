using System;
using System.Collections.Generic;

using AsciiSharp.Syntaxes;

namespace AsciiSharp;

internal static class Parser
{
    public static Document Parse(ReadOnlySpan<char> source)
    {
        if (source.IsEmpty)
        {
            return new Document
            {
                Blocks = [],
                Location = new Location(new Position(1, 1), new Position(1, 1))
            };
        }

        var text = new Text
        {
            Value = source.ToString(),
            Location = new Location(new Position(1, 1), new Position(1, source.Length))
        };

        var paragraph = new Paragraph
        {
            Inlines = [text],
            Location = new Location(new Position(1, 1), new Position(1, source.Length))
        };

        return new Document
        {
            Blocks = [paragraph],
            Location = new Location(new Position(1, 1), new Position(1, source.Length))
        };
    }
}
