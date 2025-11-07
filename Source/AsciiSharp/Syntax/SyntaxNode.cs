namespace AsciiSharp.Syntax;

public abstract class SyntaxNode
{
    public abstract SyntaxNodeKind Kind { get; }
    public abstract SyntaxNodeType NodeType { get; }

    public SyntaxNode? ParentNode { get; }

    public Location? Location { get; init; }

    public abstract TResult Accept<TResult, TState>(ISyntaxVisitor<TResult, TState> visitor, TState state);
}
