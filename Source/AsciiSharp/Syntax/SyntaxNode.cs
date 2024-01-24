namespace AsciiSharp.Syntax;

public abstract class SyntaxNode :
    SyntaxElement
{
    public SyntaxNode? Parent { get; }

    public abstract void Accept(
        ISyntaxVisitor visitor);

    public abstract TResult Accept<TResult>(
        ISyntaxVisitor<TResult> visitor);
}
