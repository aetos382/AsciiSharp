using System;

using AsciiSharp.Syntax;

namespace AsciiSharp;

public class SyntaxTree
{
    public DocumentSyntax Root { get; init; }

    private SyntaxTree(DocumentSyntax root)
    {
        this.Root = root;
    }

    public static SyntaxTree Parse(ReadOnlySpan<char> source)
    {
        var document = Parser.Parse(source);
        return new SyntaxTree(document);
    }
}
