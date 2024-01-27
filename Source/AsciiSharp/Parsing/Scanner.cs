using System;
using System.Collections.Generic;
using System.Threading;

using AsciiSharp.Syntax;

namespace AsciiSharp.Parsing;

internal class Scanner
{
    public Scanner(
        ReadOnlyMemory<char> source)
    {
        this._source = source;
    }

    public IEnumerable<SyntaxToken> ScanTokens(
        CancellationToken cancellationToken)
    {
        while (!this.IsAtEnd())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tokenInfo = new TokenInfo();

            var leadingTrivia = this.ScanTrivia();

            this.ScanToken(ref tokenInfo);

            var trailingTrivia = this.ScanTrivia();

            var token = this.CreateToken(tokenInfo, leadingTrivia, trailingTrivia);
            yield return token;
        }
    }

    private SyntaxTriviaList ScanTrivia()
    {
        return new SyntaxTriviaList();
    }

    private SyntaxToken CreateToken(
        in TokenInfo tokenInfo,
        SyntaxTriviaList leadingTrivia,
        SyntaxTriviaList trailingTrivia)
    {
        switch (tokenInfo.Kind)
        {
            case SyntaxKind.SectionHeadingMarkerToken:
                return SyntaxFactory.SectionHeadingMarker(tokenInfo.Text, leadingTrivia, trailingTrivia);
        }

        throw new NotImplementedException();
    }

    private void ScanToken(ref TokenInfo info)
    {
        if (!this.TryGetAndAdvance(out var c))
        {
            info.Kind = SyntaxKind.EndOfSourceToken;
            return;
        }

        switch (c)
        {
            case >= 'a' and <= 'z':
            case >= 'A' and <= 'Z':
                info.Kind = SyntaxKind.IdentifierToken;
                break;

            case '=':
                this.ScanWhile('=');
                // TODO:
                info.Kind = SyntaxKind.SectionHeadingMarkerToken;
                info.Kind = SyntaxKind.ExampleBlockDelimiterToken;
                break;

            case '-':
                info.Kind = SyntaxKind.ListingBlockDelimiterToken;
                break;

            case '.':
                info.Kind = SyntaxKind.LiteralBlockDelimiterToken;
                break;

            case '*':
                info.Kind = SyntaxKind.SidebarBlockDelimiterToken;
                break;

            case '+':
                info.Kind = SyntaxKind.PassBlockDelimiterToken;
                break;

            case '_':
                info.Kind = SyntaxKind.QuoteBlockDelimiterToken;
                break;
            
            case '[':
                info.Kind = SyntaxKind.OpenBracketToken;
                break;

            case ']':
                info.Kind = SyntaxKind.CloseBracketToken;
                break;

            case '/':
                if (!this.IsMatch('/'))
                {
                    break;
                }

                this.ScanWhile(() => !this.IsAtEnd() && !this.IsNewLine);

                break;
        }
    }

    private bool IsAtEnd(
        int skip = 0)
    {
        return this._position + skip >= this._source.Length;
    }

    private bool IsNewLine
    {
        get
        {
            if (!this.TryPeek(out var c))
            {
                return false;
            }

            if (c is '\r' or '\n' or '\u0085' or '\u2028' or '\u2029')
            {
                return true;
            }

            return false;
        }
    }

    private bool TryGetAndAdvance(
        out char result)
    {
        if (!this.TryPeek(out result))
        {
            return false;
        }

        this.Advance();

        return true;
    }

    private void Advance()
    {
        ++this._position;
    }

    private bool TryPeek(
        out char result,
        int skip = 0)
    {
        if (this.IsAtEnd(skip))
        {
            result = default;
            return false;
        }

        result = this._source.Span[this._position + skip];
        return true;
    }

    private bool IsMatch(
        char nextExpected)
    {
        if (this.IsAtEnd())
        {
            return false;
        }

        if (!this.TryPeek(out var c) || c != nextExpected)
        {
            return false;
        }

        ++this._position;
        return true;
    }

    private void ScanWhile(
        char expected)
    {
        while (this.IsMatch(expected))
        {
        }
    }

    private void ScanWhile(
        Func<bool> predicate)
    {
        while (!this.TryPeek(out var c) || !predicate())
        {
        }
    }

    private ReadOnlyMemory<char> _source;
    private int _start = 0;
    private int _position = 0;
    private int _line = 1;
    private int _column = 1;
}
