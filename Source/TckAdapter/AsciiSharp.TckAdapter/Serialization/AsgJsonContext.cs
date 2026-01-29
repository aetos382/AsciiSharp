using System.Collections.Generic;
using System.Text.Json.Serialization;

using AsciiSharp.TckAdapter.Models;

namespace AsciiSharp.TckAdapter.Serialization;

/// <summary>
/// ASG の JSON シリアライズ用コンテキスト。
/// </summary>
/// <remarks>
/// AOT コンパイルに対応するため、ソース生成を使用する。
/// </remarks>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(AsgDocument))]
[JsonSerializable(typeof(AsgSection))]
[JsonSerializable(typeof(AsgParagraph))]
[JsonSerializable(typeof(AsgText))]
[JsonSerializable(typeof(AsgHeader))]
[JsonSerializable(typeof(AsgLocation))]
[JsonSerializable(typeof(AsgPosition))]
[JsonSerializable(typeof(IReadOnlyList<AsgBlockNode>))]
[JsonSerializable(typeof(IReadOnlyList<AsgInlineNode>))]
public partial class AsgJsonContext : JsonSerializerContext
{
}
