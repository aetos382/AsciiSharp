using System;

namespace AsciiSharp.Syntax;

public static class SyntaxNodeExtensions
{
    public static T NormalizeWhitespace<T>(
        this T node)
        where T : SyntaxNode
    {
        ArgumentNullException.ThrowIfNull(node);

        return node;
    }
}
