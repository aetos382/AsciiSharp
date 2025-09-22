using System;

namespace AsciiSharp;

public class SyntaxTree
{
    public static SyntaxTree Parse(
        ReadOnlySpan<char> source)
    {
        return new SyntaxTree();
    }
}
