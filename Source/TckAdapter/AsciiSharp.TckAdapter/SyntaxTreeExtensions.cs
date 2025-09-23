using AsciiSharp.SyntaxNodes;

namespace AsciiSharp.TckAdapter;

public static class SyntaxTreeExtensions
{
    extension(SyntaxTree syntaxTree)
    {
        public Document ToDocument()
        {
            return new Document();
        }
    }
}
