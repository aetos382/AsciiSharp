namespace AsciiSharp.Syntaxes;

public abstract class InlineSyntax : SyntaxNode
{
    public override SyntaxNodeType NodeType => SyntaxNodeType.Inline;
}
