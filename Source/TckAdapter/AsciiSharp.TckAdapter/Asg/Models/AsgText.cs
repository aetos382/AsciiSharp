using System.Text.Json.Serialization;

namespace AsciiSharp.TckAdapter.Asg.Models;

/// <summary>
/// ASG の text インライン要素を表す。
/// </summary>
public sealed class AsgText : AsgInlineNode
{
    /// <summary>
    /// ノードの名前。常に "text"。
    /// </summary>
    [JsonPropertyName("name")]
    public string Name => "text";

    /// <summary>
    /// ノードのタイプ。常に "string"。
    /// </summary>
    [JsonPropertyName("type")]
    public string Type => "string";

    /// <summary>
    /// テキストの内容。
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; init; } = string.Empty;
}
