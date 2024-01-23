namespace AsciiSharp.Model;

public abstract class SyntaxNode :
    SyntaxElement
{
    public abstract SyntaxKind Kind { get; }
}
