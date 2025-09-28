namespace AsciiSharp.Syntaxes;

public sealed class TextSyntax : InlineSyntax
{
    public override SyntaxNodeKind Kind => SyntaxNodeKind.Text;
    public override SyntaxNodeType NodeType => SyntaxNodeType.String;
    public string Value { get; init; } = string.Empty;

    public override TResult Accept<TResult, TState>(ISyntaxVisitor<TResult, TState> visitor, TState state)
    {
        return visitor.VisitText(this, state);
    }
}
