using System.Text.Json.Serialization;

namespace AsciiSharp.TckAdapter.Models;

/// <summary>
/// ブロック要素の基底クラス。
/// </summary>
/// <remarks>
/// ブロック要素は <c>Type</c> が "block" となる。
/// </remarks>
[JsonDerivedType(typeof(AsgDocument))]
[JsonDerivedType(typeof(AsgSection))]
[JsonDerivedType(typeof(AsgParagraph))]
public abstract class AsgBlockNode : AsgNode
{
    /// <summary>
    /// ノードのタイプ。ブロック要素は常に "block"。
    /// </summary>
    [JsonPropertyName("type")]
    public string Type => "block";
}
