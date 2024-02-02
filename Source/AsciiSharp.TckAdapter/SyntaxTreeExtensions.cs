using System;

using AsciiSharp.Syntax;

namespace AsciiSharp.TckAdapter;

internal static class SyntaxTreeExtensions
{
    public static SyntaxGraph ToSyntaxGraph(
        this SyntaxTree syntaxTree)
    {
        ArgumentNullException.ThrowIfNull(syntaxTree);

        return new SyntaxGraph();
    }
}
