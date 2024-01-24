using System;

using AsciiSharp.Syntax;

namespace AsciiSharp.Parsing;

internal class Scanner
{
    public Scanner(
        ReadOnlyMemory<char> source,
        ScanOptions options)
    {
        this._source = source;
        this._options = options;
    }

    public void ScanTokens()
    {
        while (!this.IsAtEnd)
        {
            var token = new TokenInfo();
            this.ScanToken(ref token);
        }
    }

    private void ScanToken(ref TokenInfo info)
    {
        var c = this.GetAndAdvance();

        switch (c)
        {
            case '=':
                this.ScanWhile('=');
                info.Kind = SyntaxKind.SectionHeadingMarkerToken;
                break;
        }
    }

    private bool IsAtEnd
    {
        get
        {
            return this._position >= this._source.Length;
        }
    }

    private char GetAndAdvance()
    {
        var c = this.Peek();

        this.Advance();

        return c;
    }

    private void Advance()
    {
        ++this._position;
    }

    private char Peek()
    {
        return this._source.Span[this._position];
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

        ++this._position;
        return true;
    }

    private void ScanWhile(char expected)
    {
        while (this.IsMatch(expected))
        {
        }
    }

    private ReadOnlyMemory<char> _source;
    private readonly ScanOptions _options;
    private int _start = 0;
    private int _position = 0;
    private int _line = 1;
    private int _column = 1;
}
