using AsciiSharp.Syntax;

namespace AsciiSharp;

public interface ISyntaxVisitor<TResult, TState>
{
    TResult VisitDocument(DocumentSyntax document, TState state);
    TResult VisitText(TextSyntax text, TState state);
    TResult VisitParagraph(ParagraphSyntax paragraph, TState state);
}
