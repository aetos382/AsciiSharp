using System.Collections.Generic;

namespace AsciiSharp.Syntaxes;

public sealed class Document : BlockNode
{
    public override SyntaxNodeKind Kind => SyntaxNodeKind.Document;
    public IReadOnlyList<BlockNode> Blocks { get; init; } = [];

    public override TResult Accept<TResult, TState>(ISyntaxVisitor<TResult, TState> visitor, TState state)
    {
        return visitor.VisitDocument(this, state);
    }
}
