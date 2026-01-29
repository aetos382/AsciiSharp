using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AsciiSharp.Asg.Models;

/// <summary>
/// ASG の section ブロックを表す。
/// </summary>
public sealed class AsgSection : AsgBlockNode
{
    /// <summary>
    /// ノードの名前。常に "section"。
    /// </summary>
    [JsonPropertyName("name")]
    public string Name => "section";

    /// <summary>
    /// セクションタイトルのインライン要素リスト。
    /// </summary>
    [JsonPropertyName("title")]
    public IReadOnlyList<AsgInlineNode> Title { get; init; } = [];

    /// <summary>
    /// セクションレベル（1-6）。
    /// </summary>
    [JsonPropertyName("level")]
    public int Level { get; init; }

    /// <summary>
    /// 子ブロック要素のリスト。
    /// </summary>
    [JsonPropertyName("blocks")]
    public IReadOnlyList<AsgBlockNode> Blocks { get; init; } = [];
}
