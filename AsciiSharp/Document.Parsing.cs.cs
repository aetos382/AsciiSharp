using System;

namespace AsciiSharp;

public partial class Document
{
    public static Document Parse(
        ReadOnlySpan<char> source,
        ParseOptions options)
    {
        var parser = new Parser(options);
        return parser.ParseDocument(source);
    }

    public static Document Parse(
        string source,
        ParseOptions options)
    {
        ArgumentNullException.ThrowIfNull(source);

        return Parse(source.AsSpan(), options);
    }
}
