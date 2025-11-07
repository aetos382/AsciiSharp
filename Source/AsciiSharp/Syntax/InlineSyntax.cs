namespace AsciiSharp.Syntax;

public abstract class InlineSyntax : SyntaxNode
{
    public override SyntaxNodeType NodeType => SyntaxNodeType.Inline;
}
