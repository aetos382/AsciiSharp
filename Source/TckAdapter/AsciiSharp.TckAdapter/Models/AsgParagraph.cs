using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AsciiSharp.TckAdapter.Models;

/// <summary>
/// ASG の paragraph ブロックを表す。
/// </summary>
public sealed class AsgParagraph : AsgBlockNode
{
    /// <summary>
    /// ノードの名前。常に "paragraph"。
    /// </summary>
    [JsonPropertyName("name")]
    public string Name => "paragraph";

    /// <summary>
    /// インライン要素のリスト。
    /// </summary>
    [JsonPropertyName("inlines")]
    public IReadOnlyList<AsgInlineNode> Inlines { get; init; } = [];
}
