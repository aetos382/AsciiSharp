using System;

using AsciiSharp.SyntaxNodes;

namespace AsciiSharp;

public class SyntaxTree
{
    public Document Root { get; init; }

    private SyntaxTree(Document root)
    {
        this.Root = root;
    }

    public static SyntaxTree Parse(ReadOnlySpan<char> source)
    {
        var document = Parser.Parse(source);
        return new SyntaxTree(document);
    }
}
