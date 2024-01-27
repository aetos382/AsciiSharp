using System;

namespace AsciiSharp;

public readonly struct TextSpan :
    IEquatable<TextSpan>
{
    public TextSpan(
        int start,
        int end)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(start, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(end, start);

        this.Start = start;
        this.End = end;
    }

    public int Start { get; }

    public int End { get; }

    public int Width
    {
        get
        {
            return this.End - this.Start;
        }
    }

    public bool IsEmpty
    {
        get
        {
            return this.Start == this.End;
        }
    }

    public bool Equals(TextSpan other)
    {
        return
            this.Start == other.Start &&
            this.End == other.End;
    }

    public override bool Equals(object? obj)
    {
        return
            obj is TextSpan other &&
            this.Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Start, this.End);
    }

    public static bool operator ==(
        TextSpan left,
        TextSpan right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(
        TextSpan left,
        TextSpan right)
    {
        return !(left == right);
    }
}
