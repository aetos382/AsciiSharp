namespace AsciiSharp.Model;

public partial class Document :
    Block
{
    public override SyntaxKind Kind => SyntaxKind.Document;
}
