using System.Collections.Generic;

namespace AsciiSharp.SyntaxNodes;

public sealed class Paragraph : BlockNode
{
    public override SyntaxNodeKind Kind => SyntaxNodeKind.Paragraph;
    public override TResult Accept<TResult, TState>(ISyntaxVisitor<TResult, TState> visitor, TState state)
    {
        return visitor.VisitParagraph(this, state);
    }

    public IReadOnlyList<InlineNode> Inlines { get; init; } = [];
}
