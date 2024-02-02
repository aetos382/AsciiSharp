using System;
using System.Buffers;
using System.IO;
using System.Threading;

using AsciiSharp.Parsing;

namespace AsciiSharp.Syntax;

public class SyntaxTree
{
    public static SyntaxTree ParseText(
        string source,
        ParseOptions? parseOptions = null,
        string? path = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        return ParseText(source.AsMemory(), parseOptions, path, cancellationToken);
    }

    public static SyntaxTree ParseText(
        TextReader textReader,
        ParseOptions? parseOptions = null,
        string? path = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(textReader);

        var source = textReader.ReadToEnd();
        var result = ParseText(source, parseOptions, path, cancellationToken);

        return result;
    }

    public static SyntaxTree ParseText(
        ReadOnlyMemory<char> source,
        ParseOptions? parseOptions = null,
        string? path = null,
        CancellationToken cancellationToken = default)
    {
        var parser = new Parser(parseOptions);

        var result = parser.Parse(source, path, cancellationToken);

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
