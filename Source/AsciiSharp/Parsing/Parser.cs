using System;
using System.Threading;
using System.Threading.Tasks;

using AsciiSharp.Syntax;

namespace AsciiSharp.Parsing;

public class Parser
{
    public Parser(
        ParseOptions? options)
    {
        this.Options = options ?? ParseOptions.Default;
    }

    public SyntaxTree Parse(
        ReadOnlyMemory<char> source,
        string? path = null,
        CancellationToken cancellationToken = default)
    {
        var scanner = new Scanner(source);

        return default;
    }

    public ParseOptions Options { get; }
}
