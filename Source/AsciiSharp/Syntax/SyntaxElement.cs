using System;
using System.Collections.Generic;

namespace AsciiSharp.Syntax;

public abstract class SyntaxElement
{
    public abstract SyntaxKind Kind { get; }

    public SyntaxTree? SyntaxTree { get; }

    public bool ContainsDiagnostics { get; }

    public TextSpan Span { get; }

    public TextSpan FullSpan { get; }

    public IEnumerable<Diagnostic> GetDiagnostics()
    {
        throw new NotImplementedException();
    }
}
