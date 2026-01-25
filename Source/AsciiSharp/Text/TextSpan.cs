
using System;

namespace AsciiSharp.Text;
/// <summary>
/// ソーステキスト内の位置と長さを表す不変の構造体。
/// </summary>
/// <remarks>
/// <para>Start と Length は非負でなければならない。</para>
/// <para>End は Start + Length として計算される。</para>
/// </remarks>
public readonly struct TextSpan : IEquatable<TextSpan>, IComparable<TextSpan>
{
    /// <summary>
    /// 開始位置。
    /// </summary>
    public int Start { get; }

    /// <summary>
    /// 長さ。
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// 終了位置（Start + Length）。
    /// </summary>
    public int End => this.Start + this.Length;

    /// <summary>
    /// スパンが空かどうか。
    /// </summary>
    public bool IsEmpty => this.Length == 0;

    /// <summary>
    /// 指定された開始位置と長さで TextSpan を作成する。
    /// </summary>
    /// <param name="start">開始位置（0以上）。</param>
    /// <param name="length">長さ（0以上）。</param>
    /// <exception cref="ArgumentOutOfRangeException">start または length が負の場合。</exception>
    public TextSpan(int start, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        this.Start = start;
        this.Length = length;
    }

    /// <summary>
    /// 開始位置と終了位置から TextSpan を作成する。
    /// </summary>
    /// <param name="start">開始位置。</param>
    /// <param name="end">終了位置。</param>
    /// <returns>新しい TextSpan。</returns>
    /// <exception cref="ArgumentOutOfRangeException">start が負、または end が start より小さい場合。</exception>
    public static TextSpan FromBounds(int start, int end)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfLessThan(end, start);

        return new TextSpan(start, end - start);
    }

    /// <summary>
    /// 指定された位置がこのスパン内に含まれるかどうかを返す。
    /// </summary>
    /// <param name="position">位置。</param>
    /// <returns>位置がスパン内に含まれる場合は true。</returns>
    public bool Contains(int position)
    {
        return position >= this.Start && position < this.End;
    }

    /// <summary>
    /// 指定されたスパンがこのスパン内に完全に含まれるかどうかを返す。
    /// </summary>
    /// <param name="span">スパン。</param>
    /// <returns>スパンが完全に含まれる場合は true。</returns>
    public bool Contains(TextSpan span)
    {
        return span.Start >= this.Start && span.End <= this.End;
    }

    /// <summary>
    /// このスパンが指定されたスパンと重なるかどうかを返す。
    /// </summary>
    /// <param name="span">スパン。</param>
    /// <returns>重なる場合は true。</returns>
    public bool OverlapsWith(TextSpan span)
    {
        var overlapStart = Math.Max(this.Start, span.Start);
        var overlapEnd = Math.Min(this.End, span.End);
        return overlapStart < overlapEnd;
    }

    /// <summary>
    /// このスパンと指定されたスパンの重なり部分を返す。
    /// </summary>
    /// <param name="span">スパン。</param>
    /// <returns>重なり部分。重ならない場合は null。</returns>
    public TextSpan? Overlap(TextSpan span)
    {
        var overlapStart = Math.Max(this.Start, span.Start);
        var overlapEnd = Math.Min(this.End, span.End);

        if (overlapStart < overlapEnd)
        {
            return FromBounds(overlapStart, overlapEnd);
        }

        return null;
    }

    /// <summary>
    /// このスパンが指定されたスパンと交差するかどうかを返す（境界での接触を含む）。
    /// </summary>
    /// <param name="span">スパン。</param>
    /// <returns>交差する場合は true。</returns>
    public bool IntersectsWith(TextSpan span)
    {
        return span.Start <= this.End && span.End >= this.Start;
    }

    /// <summary>
    /// このスパンと指定されたスパンの交差部分を返す。
    /// </summary>
    /// <param name="span">スパン。</param>
    /// <returns>交差部分。交差しない場合は null。</returns>
    public TextSpan? Intersection(TextSpan span)
    {
        var intersectStart = Math.Max(this.Start, span.Start);
        var intersectEnd = Math.Min(this.End, span.End);

        if (intersectStart <= intersectEnd)
        {
            return FromBounds(intersectStart, intersectEnd);
        }

        return null;
    }

    /// <inheritdoc />
    public bool Equals(TextSpan other)
    {
        return this.Start == other.Start && this.Length == other.Length;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is TextSpan other && this.Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
#if NETSTANDARD2_0
        unchecked
        {
            return (this.Start * 397) ^ this.Length;
        }
#else
        return HashCode.Combine(this.Start, this.Length);
#endif
    }

    /// <inheritdoc />
    public int CompareTo(TextSpan other)
    {
        var startComparison = this.Start.CompareTo(other.Start);
        return startComparison != 0 ? startComparison : this.Length.CompareTo(other.Length);
    }

    /// <summary>
    /// 2つの TextSpan が等しいかどうかを判定する。
    /// </summary>
    public static bool operator ==(TextSpan left, TextSpan right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// 2つの TextSpan が等しくないかどうかを判定する。
    /// </summary>
    public static bool operator !=(TextSpan left, TextSpan right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"[{this.Start}..{this.End})";
    }

    /// <summary>
    /// 左の TextSpan が右の TextSpan より小さいかどうかを判定する。
    /// </summary>
    public static bool operator <(TextSpan left, TextSpan right)
    {
        return left.CompareTo(right) < 0;
    }

    /// <summary>
    /// 左の TextSpan が右の TextSpan 以下かどうかを判定する。
    /// </summary>
    public static bool operator <=(TextSpan left, TextSpan right)
    {
        return left.CompareTo(right) <= 0;
    }

    /// <summary>
    /// 左の TextSpan が右の TextSpan より大きいかどうかを判定する。
    /// </summary>
    public static bool operator >(TextSpan left, TextSpan right)
    {
        return left.CompareTo(right) > 0;
    }

    /// <summary>
    /// 左の TextSpan が右の TextSpan 以上かどうかを判定する。
    /// </summary>
    public static bool operator >=(TextSpan left, TextSpan right)
    {
        return left.CompareTo(right) >= 0;
    }
}
