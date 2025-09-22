using System.Text.Json.Serialization;

namespace AsciiSharp.TckAdapter.Cli;

[JsonSerializable(typeof(TckInput))]
internal sealed partial class TckInputSerializerContext : JsonSerializerContext;

[JsonSerializable(typeof(Document))]
internal sealed partial class AsgSerializerContext : JsonSerializerContext;
