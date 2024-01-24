namespace AsciiSharp.Syntax;

internal abstract class SyntaxToken :
    SyntaxElement
{
    public bool HasLeadingTrivia { get; }

    public bool HasTrailingTrivia { get; }

    public SyntaxTriviaList LeadingTrivia { get; }

    public SyntaxTriviaList TrailingTrivia { get; }

    public SyntaxNode? Parent { get; }

    public string Text { get; }
}
