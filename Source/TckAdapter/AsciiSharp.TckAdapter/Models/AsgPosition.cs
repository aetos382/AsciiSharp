using System;
using System.Text.Json.Serialization;

namespace AsciiSharp.TckAdapter.Models;

/// <summary>
/// ASG における単一の位置情報（行・列）を表す。
/// </summary>
public sealed class AsgPosition
{
    /// <summary>
    /// 行番号（1-based）。
    /// </summary>
    [JsonPropertyName("line")]
    public int Line { get; }

    /// <summary>
    /// 列番号（1-based）。
    /// </summary>
    [JsonPropertyName("col")]
    public int Col { get; }

    /// <summary>
    /// AsgPosition を作成する。
    /// </summary>
    /// <param name="line">行番号（1-based）。</param>
    /// <param name="col">列番号（1-based）。</param>
    /// <exception cref="ArgumentOutOfRangeException">line または col が 1 未満の場合。</exception>
    public AsgPosition(int line, int col)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(line, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(col, 1);

        this.Line = line;
        this.Col = col;
    }
}
