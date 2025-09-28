using AsciiSharp.SyntaxNodes;

namespace AsciiSharp.TckAdapter;

internal sealed class AsgConverter :
    ISyntaxVisitor<IAsgElement, Unit>
{
    public IAsgElement VisitDocument(SyntaxNodes.Document document, Unit state)
    {
        throw new System.NotImplementedException();
    }

    public IAsgElement VisitText(Text text, Unit state)
    {
        throw new System.NotImplementedException();
    }

    public IAsgElement VisitParagraph(Paragraph paragraph, Unit state)
    {
        throw new System.NotImplementedException();
    }
}
