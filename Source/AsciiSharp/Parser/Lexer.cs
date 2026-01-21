namespace AsciiSharp.Parser;

using System;
using System.Collections.Generic;
using AsciiSharp.InternalSyntax;
using AsciiSharp.Text;

/// <summary>
/// AsciiDoc 文書の字句解析を行うクラス。
/// </summary>
internal sealed class Lexer
{
    private readonly SourceText _text;
    private int _position;
    private readonly List<InternalTrivia> _leadingTrivia = [];
    private readonly List<InternalTrivia> _trailingTrivia = [];

    /// <summary>
    /// Lexer を作成する。
    /// </summary>
    /// <param name="text">ソーステキスト。</param>
    public Lexer(SourceText text)
    {
        _text = text ?? throw new ArgumentNullException(nameof(text));
        _position = 0;
    }

    /// <summary>
    /// 現在位置の文字。
    /// </summary>
    private char Current => _position < _text.Length ? _text[_position] : '\0';

    /// <summary>
    /// 次の位置の文字。
    /// </summary>
    private char Peek(int offset = 1) => _position + offset < _text.Length ? _text[_position + offset] : '\0';

    /// <summary>
    /// ファイル終端に達したかどうか。
    /// </summary>
    public bool IsAtEnd => _position >= _text.Length;

    /// <summary>
    /// 現在位置。
    /// </summary>
    public int Position => _position;

    /// <summary>
    /// 次のトークンを読み取る。
    /// </summary>
    /// <returns>読み取ったトークン。</returns>
    public InternalToken NextToken()
    {
        _leadingTrivia.Clear();
        _trailingTrivia.Clear();

        // 先行トリビアを収集
        ScanLeadingTrivia();

        var leadingTrivia = _leadingTrivia.Count > 0 ? _leadingTrivia.ToArray() : null;

        // トークンをスキャン
        var token = ScanToken();

        // 後続トリビアを収集
        ScanTrailingTrivia();

        var trailingTrivia = _trailingTrivia.Count > 0 ? _trailingTrivia.ToArray() : null;

        // トリビアを設定したトークンを返す
        return token.WithTrivia(leadingTrivia, trailingTrivia);
    }

    /// <summary>
    /// 先行トリビアをスキャンする。
    /// </summary>
    private void ScanLeadingTrivia()
    {
        while (!IsAtEnd)
        {
            switch (Current)
            {
                case ' ':
                case '\t':
                    ScanWhitespaceTrivia(_leadingTrivia);
                    break;

                case '/':
                    if (Peek() == '/')
                    {
                        ScanCommentTrivia(_leadingTrivia);
                    }
                    else
                    {
                        return;
                    }

                    break;

                default:
                    return;
            }
        }
    }

    /// <summary>
    /// 後続トリビアをスキャンする（改行まで）。
    /// </summary>
    private void ScanTrailingTrivia()
    {
        while (!IsAtEnd)
        {
            switch (Current)
            {
                case ' ':
                case '\t':
                    ScanWhitespaceTrivia(_trailingTrivia);
                    break;

                case '\r':
                case '\n':
                    ScanEndOfLineTrivia(_trailingTrivia);
                    return; // 改行で後続トリビアは終了

                default:
                    return;
            }
        }
    }

    /// <summary>
    /// 空白トリビアをスキャンする。
    /// </summary>
    private void ScanWhitespaceTrivia(List<InternalTrivia> triviaList)
    {
        var start = _position;
        while (!IsAtEnd && (Current == ' ' || Current == '\t'))
        {
            _position++;
        }

        var text = _text.GetText(start, _position - start);
        triviaList.Add(InternalTrivia.Whitespace(text));
    }

    /// <summary>
    /// 行末トリビアをスキャンする。
    /// </summary>
    private void ScanEndOfLineTrivia(List<InternalTrivia> triviaList)
    {
        var start = _position;
        if (Current == '\r')
        {
            _position++;
            if (Current == '\n')
            {
                _position++;
            }
        }
        else if (Current == '\n')
        {
            _position++;
        }

        var text = _text.GetText(start, _position - start);
        triviaList.Add(InternalTrivia.EndOfLine(text));
    }

    /// <summary>
    /// コメントトリビアをスキャンする。
    /// </summary>
    private void ScanCommentTrivia(List<InternalTrivia> triviaList)
    {
        var start = _position;

        // ブロックコメントか単一行コメントか判定
        if (_position + 3 < _text.Length &&
            _text[_position] == '/' &&
            _text[_position + 1] == '/' &&
            _text[_position + 2] == '/' &&
            _text[_position + 3] == '/')
        {
            // ブロックコメント
            _position += 4;
            while (!IsAtEnd)
            {
                if (_position + 3 < _text.Length &&
                    _text[_position] == '/' &&
                    _text[_position + 1] == '/' &&
                    _text[_position + 2] == '/' &&
                    _text[_position + 3] == '/')
                {
                    _position += 4;
                    break;
                }

                _position++;
            }

            var text = _text.GetText(start, _position - start);
            triviaList.Add(InternalTrivia.MultiLineComment(text));
        }
        else
        {
            // 単一行コメント
            while (!IsAtEnd && Current != '\r' && Current != '\n')
            {
                _position++;
            }

            var text = _text.GetText(start, _position - start);
            triviaList.Add(InternalTrivia.SingleLineComment(text));
        }
    }

    /// <summary>
    /// トークンをスキャンする。
    /// </summary>
    private InternalToken ScanToken()
    {
        if (IsAtEnd)
        {
            return new InternalToken(SyntaxKind.EndOfFileToken, string.Empty);
        }

        var start = _position;
        var kind = SyntaxKind.None;

        switch (Current)
        {
            case '=':
                _position++;
                kind = SyntaxKind.EqualsToken;
                break;

            case ':':
                _position++;
                kind = SyntaxKind.ColonToken;
                break;

            case '/':
                _position++;
                kind = SyntaxKind.SlashToken;
                break;

            case '[':
                _position++;
                kind = SyntaxKind.OpenBracketToken;
                break;

            case ']':
                _position++;
                kind = SyntaxKind.CloseBracketToken;
                break;

            case '{':
                _position++;
                kind = SyntaxKind.OpenBraceToken;
                break;

            case '}':
                _position++;
                kind = SyntaxKind.CloseBraceToken;
                break;

            case '#':
                _position++;
                kind = SyntaxKind.HashToken;
                break;

            case '*':
                _position++;
                kind = SyntaxKind.AsteriskToken;
                break;

            case '_':
                _position++;
                kind = SyntaxKind.UnderscoreToken;
                break;

            case '`':
                _position++;
                kind = SyntaxKind.BacktickToken;
                break;

            case '.':
                _position++;
                kind = SyntaxKind.DotToken;
                break;

            case ',':
                _position++;
                kind = SyntaxKind.CommaToken;
                break;

            case '|':
                _position++;
                kind = SyntaxKind.PipeToken;
                break;

            case '<':
                _position++;
                kind = SyntaxKind.LessThanToken;
                break;

            case '>':
                _position++;
                kind = SyntaxKind.GreaterThanToken;
                break;

            case '\r':
            case '\n':
                ScanNewLine();
                kind = SyntaxKind.NewLineToken;
                break;

            case ' ':
            case '\t':
                ScanWhitespace();
                kind = SyntaxKind.WhitespaceToken;
                break;

            default:
                ScanText();
                kind = SyntaxKind.TextToken;
                break;
        }

        var text = _text.GetText(start, _position - start);
        return new InternalToken(kind, text);
    }

    /// <summary>
    /// 改行をスキャンする。
    /// </summary>
    private void ScanNewLine()
    {
        if (Current == '\r')
        {
            _position++;
            if (Current == '\n')
            {
                _position++;
            }
        }
        else if (Current == '\n')
        {
            _position++;
        }
    }

    /// <summary>
    /// 空白をスキャンする。
    /// </summary>
    private void ScanWhitespace()
    {
        while (!IsAtEnd && (Current == ' ' || Current == '\t'))
        {
            _position++;
        }
    }

    /// <summary>
    /// テキストをスキャンする（特殊文字以外）。
    /// </summary>
    private void ScanText()
    {
        while (!IsAtEnd && !IsSpecialCharacter(Current))
        {
            _position++;
        }
    }

    /// <summary>
    /// 特殊文字かどうかを判定する。
    /// </summary>
    private static bool IsSpecialCharacter(char c)
    {
        return c switch
        {
            '=' or ':' or '/' or '[' or ']' or '{' or '}' or
            '#' or '*' or '_' or '`' or '.' or ',' or '|' or
            '<' or '>' or ' ' or '\t' or '\r' or '\n' => true,
            _ => false
        };
    }
}
