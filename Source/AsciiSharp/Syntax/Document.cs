using System;

namespace AsciiSharp.Syntax;

public partial class Document :
    Block
{
    public override SyntaxKind Kind => SyntaxKind.Document;

    public override void Accept(
        ISyntaxVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);

        visitor.VisitDocument(this);
    }

    public override TResult Accept<TResult>(
        ISyntaxVisitor<TResult> visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);

        return visitor.VisitDocument(this);
    }
}
