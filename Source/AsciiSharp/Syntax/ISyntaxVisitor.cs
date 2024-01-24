namespace AsciiSharp.Syntax;

public interface ISyntaxVisitor
{
    void VisitDocument(Document node);

    void VisitDocumentHeader(DocumentHeader node);
}
