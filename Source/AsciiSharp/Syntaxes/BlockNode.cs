namespace AsciiSharp.Syntaxes;

public abstract class BlockNode : SyntaxNode
{
    public override SyntaxNodeType NodeType => SyntaxNodeType.Block;
}
