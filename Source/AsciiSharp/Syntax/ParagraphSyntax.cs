using System.Collections.Generic;

namespace AsciiSharp.Syntax;

public sealed class ParagraphSyntax : BlockSyntax
{
    public override SyntaxNodeKind Kind => SyntaxNodeKind.Paragraph;
    public override TResult Accept<TResult, TState>(ISyntaxVisitor<TResult, TState> visitor, TState state)
    {
        return visitor.VisitParagraph(this, state);
    }

    public IReadOnlyList<InlineSyntax> Inlines { get; init; } = [];
}
