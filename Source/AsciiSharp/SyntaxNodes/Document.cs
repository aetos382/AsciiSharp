using System.Collections.Generic;

namespace AsciiSharp.SyntaxNodes;

public sealed class Document : BlockNode
{
    public override SyntaxNodeKind Kind => SyntaxNodeKind.Document;
    public IReadOnlyList<BlockNode> Blocks { get; init; } = [];
}