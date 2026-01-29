using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using AsciiSharp.Asg.Models;

namespace AsciiSharp.Asg.Serialization;

/// <summary>
/// <see cref="AsgLocation"/> を JSON 配列形式でシリアライズするコンバーター。
/// </summary>
/// <remarks>
/// TCK が期待する <c>[{start}, {end}]</c> 形式で出力する。
/// </remarks>
public sealed class AsgLocationJsonConverter : JsonConverter<AsgLocation>
{
    /// <inheritdoc />
    public override AsgLocation? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        throw new NotSupportedException("AsgLocation の読み取りはサポートされていません。");
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        AsgLocation value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteStartArray();
        WritePosition(writer, value.Start);
        WritePosition(writer, value.End);
        writer.WriteEndArray();
    }

    private static void WritePosition(Utf8JsonWriter writer, AsgPosition position)
    {
        writer.WriteStartObject();
        writer.WriteNumber("line", position.Line);
        writer.WriteNumber("col", position.Col);
        writer.WriteEndObject();
    }
}
