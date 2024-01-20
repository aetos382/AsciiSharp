using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsciiSharp;

public partial class Document
{
    public static ValueTask<Document> ParseAsync(
        ReadOnlySpan<char> source,
        ParseOptions options,
        CancellationToken cancellationToken = default)
    {
        var parser = new Parser(options);
        return parser.ParseDocumentAsync(source, cancellationToken);
    }

    public static ValueTask<Document> ParseAsync(
        string source,
        ParseOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        return ParseAsync(source.AsSpan(), options, cancellationToken);
    }
}
