using System.Text.Json;
using System.Text.Json.Serialization;

namespace AsciiSharp.TckAdapter;

[JsonSerializable(typeof(TestInput))]
[JsonSerializable(typeof(SyntaxGraph))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal sealed partial class TestSerializerContext :
    JsonSerializerContext
{
}
