namespace AsciiSharp.Syntax;

public partial class Document :
    Block
{
    public override SyntaxKind Kind => SyntaxKind.Document;
}
