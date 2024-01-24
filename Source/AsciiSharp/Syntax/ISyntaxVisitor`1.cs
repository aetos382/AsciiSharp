namespace AsciiSharp.Syntax;

public interface ISyntaxVisitor<T>
{
    T VisitDocument(Document document);
}
