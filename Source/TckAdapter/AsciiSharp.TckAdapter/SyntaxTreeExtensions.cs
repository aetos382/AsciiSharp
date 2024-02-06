using System;

using AsciiSharp.Syntax;

namespace AsciiSharp.TckAdapter;

public static class SyntaxTreeExtensions
{
    public static SyntaxGraph ToSyntaxGraph(
        this SyntaxTree syntaxTree)
    {
        ArgumentNullException.ThrowIfNull(syntaxTree);

        return new SyntaxGraph();
    }
}
