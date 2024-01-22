namespace AsciiSharp.Parsing;

public class ParseOptions
{
    public ParseOptions()
    {
        this.FileResolver = new FileResolver(this);
    }

    public IFileResolver? FileResolver { get; set; }

    public bool PreprocessDocument { get; set; }

    public IDocumentPreprocessor[]? Preprocessors { get; set; }

    public static readonly ParseOptions Default = new();
}
