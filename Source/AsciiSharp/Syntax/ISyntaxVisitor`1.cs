namespace AsciiSharp.Syntax;

public interface ISyntaxVisitor<out T>
{
    T VisitDocument(Document node);

    T VisitDocumentHeader(DocumentHeader node);

    T VisitSection(Section node);
}
