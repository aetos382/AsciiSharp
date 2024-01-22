using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AsciiSharp;

public class SyntaxTree
{
    public static ValueTask<SyntaxTree> ParseTextAsync(
        string source,
        ParseOptions? parseOptions,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);

        return ParseTextAsync(source.AsMemory(), parseOptions, cancellationToken);
    }

    public static async ValueTask<SyntaxTree> ParseTextAsync(
        TextReader textReader,
        ParseOptions? parseOptions,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(textReader);

        var source = await textReader
            .ReadToEndAsync(cancellationToken)
            .ConfigureAwait(false);

        var result = await ParseTextAsync(source, parseOptions, cancellationToken)
            .ConfigureAwait(false);

        return result;
    }

    public static async ValueTask<SyntaxTree> ParseTextAsync(
        ReadOnlyMemory<char> source,
        ParseOptions? parseOptions,
        CancellationToken cancellationToken)
    {
        var parser = new Parser(parseOptions);

        var result = await parser
            .ParseAsync(source, cancellationToken)
            .ConfigureAwait(false);

        return result;
    }

    public ReadOnlySequence<char> AsSequence()
    {
        return new ReadOnlySequence<char>("SyntaxTree".AsMemory());
    }

    public override string ToString()
    {
        return this.AsSequence().ToString();
    }
}
