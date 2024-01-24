namespace AsciiSharp.Syntax;

public class SyntaxToken :
    SyntaxElement
{
    public SyntaxToken(
        SyntaxKind kind,
        string text,
        SyntaxTriviaList leadingTrivia,
        SyntaxTriviaList trailingTrivia)
    {
        this.Kind = kind;
        this.Text = text;
        this.LeadingTrivia = leadingTrivia;
        this.TrailingTrivia = trailingTrivia;
    }

    public override SyntaxKind Kind { get; }

    public bool HasLeadingTrivia { get; }

    public bool HasTrailingTrivia { get; }

    public SyntaxTriviaList LeadingTrivia { get; }

    public SyntaxTriviaList TrailingTrivia { get; }

    public SyntaxNode? Parent { get; }

    public string Text { get; }
}
