namespace AsciiSharp.Syntax;

public static class SyntaxFactory
{
    public static SyntaxToken SectionHeadingMarker(
        string text,
        SyntaxTriviaList leading,
        SyntaxTriviaList trailing)
    {
        return new SyntaxToken(
            SyntaxKind.SectionHeadingMarkerToken,
            text,
            leading,
            trailing);
    }
}
