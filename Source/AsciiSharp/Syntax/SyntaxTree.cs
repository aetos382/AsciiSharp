using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using AsciiSharp.Parsing;

namespace AsciiSharp.Syntax;

public class SyntaxTree
{
    public static ValueTask<SyntaxTree> ParseTextAsync(
        string source,
        ParseOptions? parseOptions,
        string? path = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        return ParseTextAsync(source.AsMemory(), parseOptions, path, cancellationToken);
    }

    public static async ValueTask<SyntaxTree> ParseTextAsync(
        TextReader textReader,
        ParseOptions? parseOptions,
        string? path = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(textReader);

        var source = await textReader
            .ReadToEndAsync(cancellationToken)
            .ConfigureAwait(false);

        var result = await ParseTextAsync(source, parseOptions, path, cancellationToken)
            .ConfigureAwait(false);

        return result;
    }

    public static async ValueTask<SyntaxTree> ParseTextAsync(
        ReadOnlyMemory<char> source,
        ParseOptions? parseOptions,
        string? path = null,
        CancellationToken cancellationToken = default)
    {
        var parser = new Parser(parseOptions);

        var result = await parser
            .ParseAsync(source, path, cancellationToken)
            .ConfigureAwait(false);

        return result;
    }

    public void WriteTo(
        IBufferWriter<char> writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        throw new NotImplementedException();
    }

    // TODO: implement
    public override string ToString()
    {
        return base.ToString();
    }
}
