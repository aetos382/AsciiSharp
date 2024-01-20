namespace AsciiSharp;

public class ParseOptions
{
    public ParseOptions()
    {
        this.FileResolver = new FileResolver(this);
    }

    public IFileResolver? FileResolver { get; set; }



    public static readonly ParseOptions Default = new();
}
