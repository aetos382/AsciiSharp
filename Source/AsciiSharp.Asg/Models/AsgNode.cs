using System.Text.Json.Serialization;

namespace AsciiSharp.Asg.Models;

/// <summary>
/// すべての ASG ノードの基底クラス。
/// </summary>
/// <remarks>
/// 各派生クラスは <c>Name</c> と <c>Type</c> プロパティを独自に定義する。
/// </remarks>
public abstract class AsgNode
{
    /// <summary>
    /// ソース文書内の位置情報。
    /// </summary>
    [JsonPropertyName("location")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AsgLocation? Location { get; init; }
}
