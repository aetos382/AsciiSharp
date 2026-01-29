using System;
using System.Text.Json.Serialization;

using AsciiSharp.TckAdapter.Serialization;

namespace AsciiSharp.TckAdapter.Models;

/// <summary>
/// ASG における位置情報（開始位置と終了位置）を表す。
/// </summary>
/// <remarks>
/// JSON では <c>[{start}, {end}]</c> 形式の配列としてシリアライズされる。
/// </remarks>
[JsonConverter(typeof(AsgLocationJsonConverter))]
public sealed class AsgLocation
{
    /// <summary>
    /// 開始位置。
    /// </summary>
    public AsgPosition Start { get; }

    /// <summary>
    /// 終了位置。
    /// </summary>
    public AsgPosition End { get; }

    /// <summary>
    /// AsgLocation を作成する。
    /// </summary>
    /// <param name="start">開始位置。</param>
    /// <param name="end">終了位置。</param>
    /// <exception cref="ArgumentNullException">start または end が null の場合。</exception>
    public AsgLocation(AsgPosition start, AsgPosition end)
    {
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(end);

        this.Start = start;
        this.End = end;
    }
}
