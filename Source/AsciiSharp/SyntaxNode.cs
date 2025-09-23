using System;
using System.Collections.Generic;

namespace AsciiSharp;

public abstract class SyntaxNode
{
    public abstract SyntaxNodeKind Kind { get; }
    public abstract SyntaxNodeType NodeType { get; }
    public Location? Location { get; init; }
}

public abstract class BlockNode : SyntaxNode
{
    public override SyntaxNodeType NodeType => SyntaxNodeType.Block;
}

public abstract class InlineNode : SyntaxNode
{
    public override SyntaxNodeType NodeType => SyntaxNodeType.Inline;
}

public sealed class Document : BlockNode
{
    public override SyntaxNodeKind Kind => SyntaxNodeKind.Document;
    public IReadOnlyList<BlockNode> Blocks { get; init; } = [];
}

public sealed class Paragraph : BlockNode
{
    public override SyntaxNodeKind Kind => SyntaxNodeKind.Paragraph;
    public IReadOnlyList<InlineNode> Inlines { get; init; } = [];
}

public sealed class Text : InlineNode
{
    public override SyntaxNodeKind Kind => SyntaxNodeKind.Text;
    public override SyntaxNodeType NodeType => SyntaxNodeType.String;
    public string Value { get; init; } = string.Empty;
}