using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AsciiSharp.Asg.Models;

/// <summary>
/// ASG の document ブロックを表す。
/// </summary>
/// <remarks>
/// 文書全体のルートノード。
/// </remarks>
public sealed class AsgDocument : AsgBlockNode
{
    /// <summary>
    /// ノードの名前。常に "document"。
    /// </summary>
    [JsonPropertyName("name")]
    public string Name => "document";

    /// <summary>
    /// ドキュメント属性。属性エントリがない場合は空の辞書。
    /// </summary>
    [JsonPropertyName("attributes")]
    public IReadOnlyDictionary<string, string> Attributes { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// 文書ヘッダー（オプション）。
    /// </summary>
    [JsonPropertyName("header")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AsgHeader? Header { get; init; }

    /// <summary>
    /// 子ブロック要素のリスト。
    /// </summary>
    [JsonPropertyName("blocks")]
    public IReadOnlyList<AsgBlockNode> Blocks { get; init; } = [];
}
