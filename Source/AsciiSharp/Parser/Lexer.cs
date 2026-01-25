
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
    private bool _isAtLineStart = true;
    private bool _isAtDocumentStart = true;

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
    private char GetCurrent()
    {
        return this.Position < this._text.Length ? this._text[this.Position] : '\0';
    }

    /// <summary>
    /// 指定されたオフセット位置の文字を取得する。
    /// </summary>
    /// <param name="offset">現在位置からのオフセット。</param>
    /// <returns>指定位置の文字。範囲外の場合は '\0'。</returns>
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

        // 最初のトークンを返した後、文書冒頭フラグをリセット
        // EOF 以外の実際のトークンが返された場合
        if (this._isAtDocumentStart && token.Kind != SyntaxKind.EndOfFileToken)
        {
            this._isAtDocumentStart = false;
        }

        // トリビアを設定したトークンを返す
        return token.WithTrivia(leadingTrivia, trailingTrivia: null);
    }

    /// <summary>
    /// 先行トリビアをスキャンする。
    /// </summary>
    /// <remarks>
    /// <para>AsciiDoc では空白が意味を持つため、空白はトークンとして扱う。</para>
    /// <para>文書冒頭の行頭コメントはトリビアとして収集される。</para>
    /// <para>本文中のコメントはトークンとして処理される。</para>
    /// </remarks>
    private void ScanLeadingTrivia()
    {
        // 文書冒頭の行頭コメントのみトリビアとして収集
        // 本文中のコメントはトークンとして処理（Parser が検出）
        while (this._isAtDocumentStart && this._isAtLineStart && !this.IsAtEnd && this.GetCurrent() == '/' && this.Peek() == '/')
        {
            var start = this.Position;

            // ブロックコメントか単一行コメントか判定
            if (this.Peek(2) == '/' && this.Peek(3) == '/')
            {
                // ブロックコメント (////...////)
                this.ScanBlockCommentAsTrivia(start);
            }
            else
            {
                // 単一行コメント (//)
                this.ScanSingleLineCommentAsTrivia(start);
            }

            // コメントの後の改行もトリビアとして収集
            if (!this.IsAtEnd && (this.GetCurrent() == '\r' || this.GetCurrent() == '\n'))
            {
                this.ScanNewLineAsTrivia();
            }
        }
    }

    /// <summary>
    /// 単一行コメントをトリビアとしてスキャンする。
    /// </summary>
    /// <param name="start">開始位置。</param>
    private void ScanSingleLineCommentAsTrivia(int start)
    {
        // 行末まで読み取る（改行は含まない）
        while (!this.IsAtEnd && this.GetCurrent() != '\r' && this.GetCurrent() != '\n')
        {
            this.Position++;
        }

        var text = this._text.GetText(start, this.Position - start);
        this._leadingTrivia.Add(new InternalTrivia(SyntaxKind.SingleLineCommentTrivia, text));
    }

    /// <summary>
    /// ブロックコメントをトリビアとしてスキャンする。
    /// </summary>
    /// <param name="start">開始位置。</param>
    private void ScanBlockCommentAsTrivia(int start)
    {
        // 開始デリミタ //// を読み飛ばす
        this.Position += 4;

        // 終了デリミタ //// を探す
        while (!this.IsAtEnd)
        {
            // 行頭の //// を検出
            if (this.GetCurrent() == '/' &&
                this.Peek() == '/' &&
                this.Peek(2) == '/' &&
                this.Peek(3) == '/')
            {
                this.Position += 4;
                break;
            }

            this.Position++;
        }

        var text = this._text.GetText(start, this.Position - start);
        this._leadingTrivia.Add(new InternalTrivia(SyntaxKind.MultiLineCommentTrivia, text));
    }

    /// <summary>
    /// 改行をトリビアとしてスキャンする。
    /// </summary>
    private void ScanNewLineAsTrivia()
    {
        var start = this.Position;

        if (this.GetCurrent() == '\r')
        {
            this.Position++;

            if (!this.IsAtEnd && this.GetCurrent() == '\n')
            {
                this.Position++;
            }
        }
        else if (this.GetCurrent() == '\n')
        {
            this.Position++;
        }

        var text = this._text.GetText(start, this.Position - start);
        this._leadingTrivia.Add(new InternalTrivia(SyntaxKind.EndOfLineTrivia, text));

        // 改行後は行頭フラグを設定（次のコメントを検出するため）
        this._isAtLineStart = true;
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
    /// トークンをスキャンする。
    /// </summary>
    private InternalToken ScanToken()
    {
        if (this.IsAtEnd)
        {
            return new InternalToken(SyntaxKind.EndOfFileToken, string.Empty);
        }

        // 行頭でのコメント検出
        if (this._isAtLineStart && this.GetCurrent() == '/' && this.Peek() == '/')
        {
            return this.ScanComment();
        }

        var start = this.Position;
        SyntaxKind kind;
        var isNewLine = false;

        switch (this.GetCurrent())
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
                isNewLine = true;
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

        // 改行の後のみ行頭フラグをセット、それ以外はリセット
        this._isAtLineStart = isNewLine;

        var text = this._text.GetText(start, this.Position - start);
        return new InternalToken(kind, text);
    }

    /// <summary>
    /// コメントをスキャンする。
    /// </summary>
    /// <returns>コメントトークン。</returns>
    private InternalToken ScanComment()
    {
        var start = this.Position;

        // ブロックコメントか単一行コメントか判定
        if (this.Peek(2) == '/' && this.Peek(3) == '/')
        {
            // ブロックコメント (////...////)
            return this.ScanBlockComment(start);
        }
        else
        {
            // 単一行コメント (//)
            return this.ScanSingleLineComment(start);
        }
    }

    /// <summary>
    /// 単一行コメントをスキャンする。
    /// </summary>
    /// <param name="start">開始位置。</param>
    /// <returns>単一行コメントトークン。</returns>
    private InternalToken ScanSingleLineComment(int start)
    {
        // 行末まで読み取る（改行は含まない）
        while (!this.IsAtEnd && this.GetCurrent() != '\r' && this.GetCurrent() != '\n')
        {
            this.Position++;
        }

        this._isAtLineStart = false;

        var text = this._text.GetText(start, this.Position - start);
        return new InternalToken(SyntaxKind.SingleLineCommentToken, text);
    }

    /// <summary>
    /// ブロックコメントをスキャンする。
    /// </summary>
    /// <param name="start">開始位置。</param>
    /// <returns>ブロックコメントトークン。</returns>
    private InternalToken ScanBlockComment(int start)
    {
        // 開始デリミタ //// を読み飛ばす
        this.Position += 4;

        // 終了デリミタ //// を探す
        while (!this.IsAtEnd)
        {
            // 行頭の //// を検出
            if (this.GetCurrent() == '/' &&
                this.Peek() == '/' &&
                this.Peek(2) == '/' &&
                this.Peek(3) == '/')
            {
                this.Position += 4;
                break;
            }

            this.Position++;
        }

        this._isAtLineStart = false;

        var text = this._text.GetText(start, this.Position - start);
        return new InternalToken(SyntaxKind.BlockCommentToken, text);
    }

    /// <summary>
    /// 改行をスキャンする。
    /// </summary>
    private void ScanNewLine()
    {
        if (this.GetCurrent() == '\r')
        {
            this.Position++;

            if (this.GetCurrent() == '\n')
            {
                this.Position++;
            }
        }
        else if (this.GetCurrent() == '\n')
        {
            this.Position++;
        }
    }

    /// <summary>
    /// 空白をスキャンする。
    /// </summary>
    private void ScanWhitespace()
    {
        while (!this.IsAtEnd && (this.GetCurrent() == ' ' || this.GetCurrent() == '\t'))
        {
            this.Position++;
        }
    }

    /// <summary>
    /// テキストをスキャンする（特殊文字以外）。
    /// </summary>
    private void ScanText()
    {
        while (!this.IsAtEnd && !IsSpecialCharacter(this.GetCurrent()))
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
