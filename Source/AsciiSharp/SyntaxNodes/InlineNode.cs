namespace AsciiSharp.SyntaxNodes;

public abstract class InlineNode : SyntaxNode
{
    public override SyntaxNodeType NodeType => SyntaxNodeType.Inline;
}