namespace AsciiSharp.SyntaxNodes;

public abstract class SyntaxNode
{
    public abstract SyntaxNodeKind Kind { get; }
    public abstract SyntaxNodeType NodeType { get; }
    public Location? Location { get; init; }
}