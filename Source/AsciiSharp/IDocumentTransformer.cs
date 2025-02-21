using AsciiSharp.Syntax;

namespace AsciiSharp;

public interface IDocumentTransformer
{
    Document Transform(Document source);
}
