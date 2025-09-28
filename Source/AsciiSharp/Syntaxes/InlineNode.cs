namespace AsciiSharp.Syntaxes;

public abstract class InlineNode : SyntaxNode
{
    public override SyntaxNodeType NodeType => SyntaxNodeType.Inline;
}
