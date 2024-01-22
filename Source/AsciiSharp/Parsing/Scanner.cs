using System;

namespace AsciiSharp.Parsing;

public class Scanner
{
    public Scanner(
        ReadOnlyMemory<char> source)
    {
        this._source = source;
    }

    public void ScanTokens()
    {

    }

    private void ScanToken()
    {
        var c = this.GetAndAdvance();

        switch (c)
        {

        }
    }

    private bool IsAtEnd
    {
        get
        {
            return this._current >= this._source.Length;
        }
    }

    private char GetAndAdvance()
    {
        return this._source.Span[this._current++];
    }

    private char Peek()
    {
        return this._source.Span[this._current];
    }

    private bool IsMatch(char nextExpected)
    {
        if (this.IsAtEnd)
        {
            return false;
        }

        if (this.Peek() != nextExpected)
        {
            return false;
        }

        ++this._current;
        return true;
    }

    private ReadOnlyMemory<char> _source;
    private int _start = 0;
    private int _current = 0;
    private int _line = 1;
    private int _column = 1;
}
