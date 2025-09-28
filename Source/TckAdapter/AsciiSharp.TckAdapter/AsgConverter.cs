using AsciiSharp.Syntax;

namespace AsciiSharp.TckAdapter;

internal sealed class AsgConverter :
    ISyntaxVisitor<IAsgElement, Unit>
{
    public IAsgElement VisitDocument(DocumentSyntax document, Unit state)
    {
        throw new System.NotImplementedException();
    }

    public IAsgElement VisitText(TextSyntax text, Unit state)
    {
        throw new System.NotImplementedException();
    }

    public IAsgElement VisitParagraph(ParagraphSyntax paragraph, Unit state)
    {
        throw new System.NotImplementedException();
    }
}
