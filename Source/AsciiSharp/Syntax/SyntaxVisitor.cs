using System;

namespace AsciiSharp.Syntax;

public class SyntaxVisitor :
    ISyntaxVisitor
{
    public virtual void VisitDocument(
        Document node)
    {
        ArgumentNullException.ThrowIfNull(node);

        node.Header.Accept(this);
    }

    public virtual void VisitDocumentHeader(
        DocumentHeader node)
    {
        ArgumentNullException.ThrowIfNull(node);
    }

    public virtual void VisitSection(
        Section node)
    {
        ArgumentNullException.ThrowIfNull(node);
    }
}
