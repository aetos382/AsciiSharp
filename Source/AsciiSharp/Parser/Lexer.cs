
using System;
using System.Collections.Generic;

using AsciiSharp.InternalSyntax;
using AsciiSharp.Text;

namespace AsciiSharp.Parser;
/// <summary>
/// AsciiDoc 文書の字句解析を行うクラス。
/// </summary>
internal sealed class Lexer
{
    private readonly SourceText _text;
    private readonly List<InternalTrivia> _leadingTrivia = [];

    /// <summary>
    /// Lexer を作成する。
    /// </summary>
    /// <param name="text">ソーステキスト。</param>
    public Lexer(SourceText text)
    {
        ArgumentNullException.ThrowIfNull(text);

        this._text = text;
        this.Position = 0;
    }

    /// <summary>
    /// 現在位置の文字。
    /// </summary>
    private char Current => this.Position < this._text.Length ? this._text[this.Position] : '\0';

    /// <summary>
    /// 次の位置の文字。
    /// </summary>
    private char Peek(int offset = 1)
    {
        return this.Position + offset < this._text.Length ? this._text[this.Position + offset] : '\0';
    }

    /// <summary>
    /// ファイル終端に達したかどうか。
    /// </summary>
    public bool IsAtEnd => this.Position >= this._text.Length;

    /// <summary>
    /// 現在位置。
    /// </summary>
    public int Position { get; private set; }

    /// <summary>
    /// 次のトークンを読み取る。
    /// </summary>
    /// <returns>読み取ったトークン。</returns>
    public InternalToken NextToken()
    {
        this._leadingTrivia.Clear();

        // 先行トリビアを収集
        this.ScanLeadingTrivia();

        var leadingTrivia = this._leadingTrivia.Count > 0 ? this._leadingTrivia.ToArray() : null;

        // トークンをスキャン
        var token = this.ScanToken();

        // 後続トリビアを収集
        ScanTrailingTrivia();

        // トリビアを設定したトークンを返す
        return token.WithTrivia(leadingTrivia, trailingTrivia: null);
    }

    /// <summary>
    /// 先行トリビアをスキャンする。
    /// </summary>
    /// <remarks>
    /// AsciiDoc では空白が意味を持つため、空白はトークンとして扱う。
    /// コメントのみを先行トリビアとして扱う。
    /// </remarks>
    private void ScanLeadingTrivia()
    {
        while (!this.IsAtEnd)
        {
            switch (this.Current)
            {
                case '/':
                    if (this.Peek() == '/')
                    {
                        this.ScanCommentTrivia(this._leadingTrivia);
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
    /// 後続トリビアをスキャンする。
    /// </summary>
    /// <remarks>
    /// AsciiDoc では空白と改行が意味を持つため、トークンとして扱う。
    /// 現時点では後続トリビアとして収集するものはない。
    /// </remarks>
    private static void ScanTrailingTrivia()
    {
        // 現時点では後続トリビアとして収集するものはない
    }

    /// <summary>
    /// コメントトリビアをスキャンする。
    /// </summary>
    private void ScanCommentTrivia(List<InternalTrivia> triviaList)
    {
        var start = this.Position;

        // ブロックコメントか単一行コメントか判定
        if (this.Position + 3 < this._text.Length &&
            this._text[this.Position] == '/' &&
            this._text[this.Position + 1] == '/' &&
            this._text[this.Position + 2] == '/' &&
            this._text[this.Position + 3] == '/')
        {
            // ブロックコメント
            this.Position += 4;
            while (!this.IsAtEnd)
            {
                if (this.Position + 3 < this._text.Length &&
                    this._text[this.Position] == '/' &&
                    this._text[this.Position + 1] == '/' &&
                    this._text[this.Position + 2] == '/' &&
                    this._text[this.Position + 3] == '/')
                {
                    this.Position += 4;
                    break;
                }

                this.Position++;
            }

            var text = this._text.GetText(start, this.Position - start);
            triviaList.Add(InternalTrivia.MultiLineComment(text));
        }
        else
        {
            // 単一行コメント
            while (!this.IsAtEnd && this.Current != '\r' && this.Current != '\n')
            {
                this.Position++;
            }

            var text = this._text.GetText(start, this.Position - start);
            triviaList.Add(InternalTrivia.SingleLineComment(text));
        }
    }

    /// <summary>
    /// トークンをスキャンする。
    /// </summary>
    private InternalToken ScanToken()
    {
        if (this.IsAtEnd)
        {
            return new InternalToken(SyntaxKind.EndOfFileToken, string.Empty);
        }

        var start = this.Position;
        SyntaxKind kind;
        switch (this.Current)
        {
            case '=':
                this.Position++;
                kind = SyntaxKind.EqualsToken;
                break;

            case ':':
                this.Position++;
                kind = SyntaxKind.ColonToken;
                break;

            case '/':
                this.Position++;
                kind = SyntaxKind.SlashToken;
                break;

            case '[':
                this.Position++;
                kind = SyntaxKind.OpenBracketToken;
                break;

            case ']':
                this.Position++;
                kind = SyntaxKind.CloseBracketToken;
                break;

            case '{':
                this.Position++;
                kind = SyntaxKind.OpenBraceToken;
                break;

            case '}':
                this.Position++;
                kind = SyntaxKind.CloseBraceToken;
                break;

            case '#':
                this.Position++;
                kind = SyntaxKind.HashToken;
                break;

            case '*':
                this.Position++;
                kind = SyntaxKind.AsteriskToken;
                break;

            case '_':
                this.Position++;
                kind = SyntaxKind.UnderscoreToken;
                break;

            case '`':
                this.Position++;
                kind = SyntaxKind.BacktickToken;
                break;

            case '.':
                this.Position++;
                kind = SyntaxKind.DotToken;
                break;

            case ',':
                this.Position++;
                kind = SyntaxKind.CommaToken;
                break;

            case '|':
                this.Position++;
                kind = SyntaxKind.PipeToken;
                break;

            case '<':
                this.Position++;
                kind = SyntaxKind.LessThanToken;
                break;

            case '>':
                this.Position++;
                kind = SyntaxKind.GreaterThanToken;
                break;

            case '\r':
            case '\n':
                this.ScanNewLine();
                kind = SyntaxKind.NewLineToken;
                break;

            case ' ':
            case '\t':
                this.ScanWhitespace();
                kind = SyntaxKind.WhitespaceToken;
                break;

            default:
                this.ScanText();
                kind = SyntaxKind.TextToken;
                break;
        }

        var text = this._text.GetText(start, this.Position - start);
        return new InternalToken(kind, text);
    }

    /// <summary>
    /// 改行をスキャンする。
    /// </summary>
    private void ScanNewLine()
    {
        if (this.Current == '\r')
        {
            this.Position++;
            if (this.Current == '\n')
            {
                this.Position++;
            }
        }
        else if (this.Current == '\n')
        {
            this.Position++;
        }
    }

    /// <summary>
    /// 空白をスキャンする。
    /// </summary>
    private void ScanWhitespace()
    {
        while (!this.IsAtEnd && (this.Current == ' ' || this.Current == '\t'))
        {
            this.Position++;
        }
    }

    /// <summary>
    /// テキストをスキャンする（特殊文字以外）。
    /// </summary>
    private void ScanText()
    {
        while (!this.IsAtEnd && !IsSpecialCharacter(this.Current))
        {
            this.Position++;
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
