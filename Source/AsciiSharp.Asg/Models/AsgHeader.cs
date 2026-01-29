using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AsciiSharp.Asg.Models;

/// <summary>
/// ASG の文書ヘッダーを表す。
/// </summary>
/// <remarks>
/// ヘッダーは <see cref="AsgNode"/> を継承しない独立したクラス。
/// document ノードの header プロパティとして出力される。
/// </remarks>
public sealed class AsgHeader
{
    /// <summary>
    /// タイトルのインライン要素リスト。
    /// </summary>
    [JsonPropertyName("title")]
    public IReadOnlyList<AsgInlineNode> Title { get; init; } = [];

    /// <summary>
    /// ヘッダーの位置情報。
    /// </summary>
    [JsonPropertyName("location")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AsgLocation? Location { get; init; }
}
