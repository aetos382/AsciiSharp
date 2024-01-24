namespace AsciiSharp.Syntax;

public class DocumentHeader :
    SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.DocumentHeader;

    public override void Accept(ISyntaxVisitor visitor)
    {
        visitor.VisitDocumentHeader(this);
    }

    public override TResult Accept<TResult>(ISyntaxVisitor<TResult> visitor)
    {
        return visitor.VisitDocumentHeader(this);
    }
}
