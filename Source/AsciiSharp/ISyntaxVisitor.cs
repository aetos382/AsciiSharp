using AsciiSharp.SyntaxNodes;

namespace AsciiSharp;

public interface ISyntaxVisitor<TResult, TState>
{
    TResult VisitDocument(Document document, TState state);
    TResult VisitText(Text text, TState state);
    TResult VisitParagraph(Paragraph paragraph, TState state);
}
