using System.Collections.Generic;

namespace AsciiSharp.SyntaxNodes;

public sealed class Paragraph : BlockNode
{
    public override SyntaxNodeKind Kind => SyntaxNodeKind.Paragraph;
    public IReadOnlyList<InlineNode> Inlines { get; init; } = [];
}