using System.Text.Json.Serialization;

namespace AsciiSharp.TckAdapter.Cli;

/// <summary>
/// TCK 入力の JSON シリアライズ用コンテキスト。
/// </summary>
/// <remarks>
/// AOT コンパイルに対応するため、ソース生成を使用する。
/// </remarks>
[JsonSerializable(typeof(TckInput))]
internal partial class TckJsonContext : JsonSerializerContext
{
}
