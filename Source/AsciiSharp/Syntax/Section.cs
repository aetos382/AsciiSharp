using System;

namespace AsciiSharp.Syntax;

public class Section :
    Block
{
    public override SyntaxKind Kind => SyntaxKind.Section;

    public override void Accept(
        ISyntaxVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);

        visitor.VisitSection(this);
    }

    public override TResult Accept<TResult>(
        ISyntaxVisitor<TResult> visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);

        return visitor.VisitSection(this);
    }
}
