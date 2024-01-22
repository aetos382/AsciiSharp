using System;
using System.Threading;
using System.Threading.Tasks;

using AsciiSharp.Model;

namespace AsciiSharp.Parsing;

public class Parser
{
    public Parser(
        ParseOptions? options)
    {
        this.Options = options ?? ParseOptions.Default;
    }

    public ValueTask<SyntaxTree> ParseAsync(
        ReadOnlyMemory<char> source,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public ParseOptions Options { get; }
}
