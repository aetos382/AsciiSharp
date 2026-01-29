using System;
using System.Text.Json;

using AsciiSharp.Syntax;
using AsciiSharp.TckAdapter;
using AsciiSharp.TckAdapter.Cli;
using AsciiSharp.TckAdapter.Serialization;

try
{
    // 1. 標準入力から JSON を読み取る
    var inputJson = Console.In.ReadToEnd();

    // 2. JSON をデシリアライズして TckInput オブジェクトに変換
    var tckInput = JsonSerializer.Deserialize(inputJson, TckJsonContext.Default.TckInput);

    if (tckInput is null)
    {
        Console.Error.WriteLine("Error: Failed to deserialize input JSON.");
        return 1;
    }

    // 3. AsciiSharp パーサーで AsciiDoc 文書をパース
    var syntaxTree = SyntaxTree.ParseText(tckInput.Contents, tckInput.Path);

    // 4. AsgConverter で SyntaxTree を ASG に変換
    var converter = new AsgConverter(syntaxTree);
    var asgDocument = converter.Convert();

    // 5. ASG を JSON シリアライズして標準出力に出力
    var outputJson = JsonSerializer.Serialize(asgDocument, AsgJsonContext.Default.AsgDocument);
    Console.WriteLine(outputJson);

    // 6. 成功時は終了コード 0 を返す
    return 0;
}
catch (JsonException ex)
{
    Console.Error.WriteLine($"Error: Invalid JSON input - {ex.Message}");
    return 1;
}
#pragma warning disable CA1031 // CLI のエントリポイントではすべての例外をキャッチして適切な終了コードを返す必要がある
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}
#pragma warning restore CA1031
