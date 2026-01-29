using System.Text.Json.Serialization;

namespace AsciiSharp.TckAdapter;

/// <summary>
/// TCK から渡される入力データを表す。
/// </summary>
internal sealed class TckInput
{
    /// <summary>
    /// パース対象の AsciiDoc 文書の内容。
    /// </summary>
    [JsonPropertyName("contents")]
    public required string Contents { get; init; }

    /// <summary>
    /// 入力ファイルの仮想パス。
    /// </summary>
    [JsonPropertyName("path")]
    public required string Path { get; init; }

    /// <summary>
    /// パース タイプ。"block" または "inline"。
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }
}
