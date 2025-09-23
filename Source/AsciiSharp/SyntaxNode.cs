using System;
using System.Collections.Generic;

namespace AsciiSharp;

public abstract class SyntaxNode
{
    public abstract string Name { get; }
    public abstract string Type { get; }
    public Location? Location { get; init; }
}

public abstract class BlockNode : SyntaxNode
{
    public override string Type => "block";
}

public abstract class InlineNode : SyntaxNode
{
    public override string Type => "inline";
}

public sealed class Document : BlockNode
{
    public override string Name => "document";
    public IReadOnlyList<BlockNode> Blocks { get; init; } = Array.Empty<BlockNode>();
}

public sealed class Paragraph : BlockNode
{
    public override string Name => "paragraph";
    public IReadOnlyList<InlineNode> Inlines { get; init; } = Array.Empty<InlineNode>();
}

public sealed class Text : InlineNode
{
    public override string Name => "text";
    public override string Type => "string";
    public string Value { get; init; } = string.Empty;
}