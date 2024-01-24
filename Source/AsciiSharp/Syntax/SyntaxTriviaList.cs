using System;
using System.Collections;
using System.Collections.Generic;

namespace AsciiSharp.Syntax;

public class SyntaxTriviaList :
    IEquatable<SyntaxTriviaList>,
    IReadOnlyList<SyntaxTrivia>
{
    public bool Equals(SyntaxTriviaList? other)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<SyntaxTrivia> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public int Count { get; }

    public SyntaxTrivia this[int index]
    {
        get { throw new NotImplementedException(); }
    }
}
