namespace AsciiSharp.SyntaxNodes;

public sealed class Text : InlineNode
{
    public override SyntaxNodeKind Kind => SyntaxNodeKind.Text;
    public override SyntaxNodeType NodeType => SyntaxNodeType.String;
    public string Value { get; init; } = string.Empty;
}