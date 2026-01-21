
using System;
using System.Collections.Generic;
using System.Text;

namespace AsciiSharp.Text;
/// <summary>
/// 文字列ベースの SourceText 実装。
/// </summary>
internal sealed class StringText : SourceText
{
    private readonly string _text;
    private readonly IReadOnlyList<TextLine> _lines;

    /// <inheritdoc />
    public override int Length => this._text.Length;

    /// <inheritdoc />
    public override char this[int index]
    {
        get
        {
            if (index < 0 || index >= this._text.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "インデックスがテキストの範囲外です。");
            }

            return this._text[index];
        }
    }

    /// <inheritdoc />
    protected override IReadOnlyList<TextLine> Lines => this._lines;

    /// <summary>
    /// 文字列から StringText を作成する。
    /// </summary>
    /// <param name="text">ソーステキスト。</param>
    public StringText(string text)
    {
        this._text = text ?? throw new ArgumentNullException(nameof(text));
        this._lines = ParseLines(text);
    }

    /// <inheritdoc />
    public override string GetText(TextSpan span)
    {
        if (span.Start < 0 || span.End > this._text.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(span), span, "スパンがテキストの範囲外です。");
        }

        return this._text.Substring(span.Start, span.Length);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return this._text;
    }

    /// <inheritdoc />
    public override SourceText WithChanges(IEnumerable<TextChange> changes)
    {
        ArgumentNullException.ThrowIfNull(changes);

        // 変更を位置の降順でソートして適用（後ろから適用することで位置のずれを防ぐ）
        var sortedChanges = new List<TextChange>(changes);
        sortedChanges.Sort((a, b) => b.Span.Start.CompareTo(a.Span.Start));

        var builder = new StringBuilder(this._text);

        foreach (var change in sortedChanges)
        {
            if (change.Span.Start < 0 || change.Span.End > builder.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(changes), "変更範囲がテキストの範囲外です。");
            }

            builder.Remove(change.Span.Start, change.Span.Length);
            builder.Insert(change.Span.Start, change.NewText);
        }

        return new StringText(builder.ToString());
    }

    /// <summary>
    /// テキストから行情報を解析する。
    /// </summary>
    private static IReadOnlyList<TextLine> ParseLines(string text)
    {
        var lines = new List<TextLine>();
        var position = 0;
        var lineStart = 0;

        while (position < text.Length)
        {
            var c = text[position];

            if (c == '\r')
            {
                var lineBreakLength = 1;

                // CRLF の場合
                if (position + 1 < text.Length && text[position + 1] == '\n')
                {
                    lineBreakLength = 2;
                }

                lines.Add(new TextLine(lineStart, position - lineStart, lineBreakLength));
                position += lineBreakLength;
                lineStart = position;
            }
            else if (c == '\n')
            {
                lines.Add(new TextLine(lineStart, position - lineStart, 1));
                position++;
                lineStart = position;
            }
            else
            {
                position++;
            }
        }

        // 最後の行（改行で終わっていない場合も含む）
        lines.Add(new TextLine(lineStart, text.Length - lineStart, 0));

        return lines;
    }
}
