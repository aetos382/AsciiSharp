namespace AsciiSharp.Syntaxes;

public abstract class BlockSyntax : SyntaxNode
{
    public override SyntaxNodeType NodeType => SyntaxNodeType.Block;
}
