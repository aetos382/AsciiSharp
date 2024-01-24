namespace AsciiSharp.Syntax;

internal abstract class SyntaxToken :
    SyntaxElement
{
    public bool HasLeadingTrivia { get; }

    public bool HasTrailingTrivia { get; }

    public bool ContainsDiagnostics { get; }

    public SyntaxTriviaList LeadingTrivia { get; }

    public SyntaxTriviaList TrailingTrivia { get; }

    public SyntaxNode? Parent { get; }

    public TextSpan Span { get; }

    public TextSpan FullSpan { get; }

    public SyntaxTree? SyntaxTree { get; }

    public string Text { get; }
}
