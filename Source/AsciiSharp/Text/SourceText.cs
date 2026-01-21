
using System;
using System.Collections.Generic;

namespace AsciiSharp.Text;
/// <summary>
/// ソーステキストを表す抽象基底クラス。
/// </summary>
/// <remarks>
/// <para>このクラスは不変であり、テキストの変更は新しいインスタンスを返す。</para>
/// <para>行・列番号の計算やテキストの部分取得をサポートする。</para>
/// </remarks>
public abstract class SourceText
{
    /// <summary>
    /// テキストの長さ（文字数）。
    /// </summary>
    public abstract int Length { get; }

    /// <summary>
    /// 指定されたインデックスの文字を取得する。
    /// </summary>
    /// <param name="index">インデックス。</param>
    /// <returns>指定されたインデックスの文字。</returns>
    public abstract char this[int index] { get; }

    /// <summary>
    /// 行情報のリスト。
    /// </summary>
    protected abstract IReadOnlyList<TextLine> Lines { get; }

    /// <summary>
    /// 行の数。
    /// </summary>
    public int LineCount => this.Lines.Count;

    /// <summary>
    /// 指定された範囲のテキストを取得する。
    /// </summary>
    /// <param name="span">取得する範囲。</param>
    /// <returns>指定された範囲のテキスト。</returns>
    public abstract string GetText(TextSpan span);

    /// <summary>
    /// 指定された開始位置と長さのテキストを取得する。
    /// </summary>
    /// <param name="start">開始位置。</param>
    /// <param name="length">長さ。</param>
    /// <returns>指定された範囲のテキスト。</returns>
    public string GetText(int start, int length)
    {
        return this.GetText(new TextSpan(start, length));
    }

    /// <summary>
    /// テキスト全体を文字列として取得する。
    /// </summary>
    /// <returns>テキスト全体。</returns>
    public abstract override string ToString();

    /// <summary>
    /// 指定された位置の行・列番号を取得する。
    /// </summary>
    /// <param name="position">位置。</param>
    /// <returns>行番号（0始まり）と列番号（0始まり）のタプル。</returns>
    /// <exception cref="ArgumentOutOfRangeException">位置が範囲外の場合。</exception>
    public (int Line, int Column) GetLineAndColumn(int position)
    {
        if (position < 0 || position > this.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(position), position, "位置がテキストの範囲外です。");
        }

        var lineIndex = this.GetLineIndexFromPosition(position);
        var line = this.Lines[lineIndex];
        var column = position - line.Start;
        return (lineIndex, column);
    }

    /// <summary>
    /// 指定された行番号の TextLine を取得する。
    /// </summary>
    /// <param name="lineNumber">行番号（0始まり）。</param>
    /// <returns>TextLine。</returns>
    /// <exception cref="ArgumentOutOfRangeException">行番号が範囲外の場合。</exception>
    public TextLine GetLine(int lineNumber)
    {
        if (lineNumber < 0 || lineNumber >= this.Lines.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(lineNumber), lineNumber, "行番号が範囲外です。");
        }

        return this.Lines[lineNumber];
    }

    /// <summary>
    /// 指定された位置が含まれる行のインデックスを取得する。
    /// </summary>
    /// <param name="position">位置。</param>
    /// <returns>行インデックス。</returns>
    protected int GetLineIndexFromPosition(int position)
    {
        var lines = this.Lines;
        var low = 0;
        var high = lines.Count - 1;

        while (low <= high)
        {
            var mid = low + ((high - low) / 2);
            var line = lines[mid];

            if (position < line.Start)
            {
                high = mid - 1;
            }
            else if (position >= line.EndIncludingLineBreak)
            {
                low = mid + 1;
            }
            else
            {
                return mid;
            }
        }

        return low;
    }

    /// <summary>
    /// 文字列から SourceText を作成する。
    /// </summary>
    /// <param name="text">ソーステキスト。</param>
    /// <returns>新しい SourceText インスタンス。</returns>
    public static SourceText From(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return new StringText(text);
    }

    /// <summary>
    /// 変更を適用した新しい SourceText を返す。
    /// </summary>
    /// <param name="changes">適用する変更のリスト。</param>
    /// <returns>変更後の新しい SourceText。</returns>
    public abstract SourceText WithChanges(IEnumerable<TextChange> changes);

    /// <summary>
    /// 単一の変更を適用した新しい SourceText を返す。
    /// </summary>
    /// <param name="change">適用する変更。</param>
    /// <returns>変更後の新しい SourceText。</returns>
    public SourceText WithChanges(TextChange change)
    {
        return this.WithChanges([change]);
    }
}

/// <summary>
/// テキスト内の1行を表す構造体。
/// </summary>
public readonly struct TextLine : IEquatable<TextLine>
{
    /// <summary>
    /// 行の開始位置。
    /// </summary>
    public int Start { get; }

    /// <summary>
    /// 行の長さ（改行を除く）。
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// 行末の改行の長さ。
    /// </summary>
    public int LineBreakLength { get; }

    /// <summary>
    /// 行の終了位置（改行を除く）。
    /// </summary>
    public int End => this.Start + this.Length;

    /// <summary>
    /// 行の終了位置（改行を含む）。
    /// </summary>
    public int EndIncludingLineBreak => this.Start + this.Length + this.LineBreakLength;

    /// <summary>
    /// 行のスパン（改行を除く）。
    /// </summary>
    public TextSpan Span => new(this.Start, this.Length);

    /// <summary>
    /// 行のスパン（改行を含む）。
    /// </summary>
    public TextSpan SpanIncludingLineBreak => new(this.Start, this.Length + this.LineBreakLength);

    /// <summary>
    /// TextLine を作成する。
    /// </summary>
    /// <param name="start">行の開始位置。</param>
    /// <param name="length">行の長さ（改行を除く）。</param>
    /// <param name="lineBreakLength">改行の長さ。</param>
    public TextLine(int start, int length, int lineBreakLength)
    {
        this.Start = start;
        this.Length = length;
        this.LineBreakLength = lineBreakLength;
    }

    /// <inheritdoc />
    public bool Equals(TextLine other)
    {
        return this.Start == other.Start
            && this.Length == other.Length
            && this.LineBreakLength == other.LineBreakLength;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is TextLine other && this.Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
#if NETSTANDARD2_0
        unchecked
        {
            var hashCode = this.Start;
            hashCode = (hashCode * 397) ^ this.Length;
            hashCode = (hashCode * 397) ^ this.LineBreakLength;
            return hashCode;
        }
#else
        return HashCode.Combine(this.Start, this.Length, this.LineBreakLength);
#endif
    }

    /// <summary>
    /// 2つの TextLine が等しいかどうかを判定する。
    /// </summary>
    public static bool operator ==(TextLine left, TextLine right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// 2つの TextLine が等しくないかどうかを判定する。
    /// </summary>
    public static bool operator !=(TextLine left, TextLine right)
    {
        return !left.Equals(right);
    }
}

/// <summary>
/// テキストの変更を表す構造体。
/// </summary>
public readonly struct TextChange : IEquatable<TextChange>
{
    /// <summary>
    /// 変更対象の範囲。
    /// </summary>
    public TextSpan Span { get; }

    /// <summary>
    /// 置換後の新しいテキスト。
    /// </summary>
    public string NewText { get; }

    /// <summary>
    /// TextChange を作成する。
    /// </summary>
    /// <param name="span">変更対象の範囲。</param>
    /// <param name="newText">置換後のテキスト。</param>
    /// <exception cref="ArgumentNullException">newText が null の場合。</exception>
    public TextChange(TextSpan span, string newText)
    {
        ArgumentNullException.ThrowIfNull(newText);

        this.Span = span;
        this.NewText = newText;
    }

    /// <inheritdoc />
    public bool Equals(TextChange other)
    {
        return this.Span == other.Span && this.NewText == other.NewText;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is TextChange other && this.Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
#if NETSTANDARD2_0
        unchecked
        {
            return (this.Span.GetHashCode() * 397) ^ (this.NewText?.GetHashCode() ?? 0);
        }
#else
        return HashCode.Combine(this.Span, this.NewText);
#endif
    }

    /// <summary>
    /// 2つの TextChange が等しいかどうかを判定する。
    /// </summary>
    public static bool operator ==(TextChange left, TextChange right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// 2つの TextChange が等しくないかどうかを判定する。
    /// </summary>
    public static bool operator !=(TextChange left, TextChange right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"TextChange({this.Span}, \"{this.NewText}\")";
    }
}
