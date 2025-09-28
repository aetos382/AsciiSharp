using System.Collections.Generic;

namespace AsciiSharp.Syntax;

public sealed class DocumentSyntax : BlockSyntax
{
    public override SyntaxNodeKind Kind => SyntaxNodeKind.Document;
    public IReadOnlyList<BlockSyntax> Blocks { get; init; } = [];

    public override TResult Accept<TResult, TState>(ISyntaxVisitor<TResult, TState> visitor, TState state)
    {
        return visitor.VisitDocument(this, state);
    }
}
