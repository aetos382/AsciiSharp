using System;
using System.Diagnostics;
using System.Text.Json;

using AsciiSharp.Syntax;
using AsciiSharp.TckAdapter;

if (IsDebug())
{
    Console.WriteLine("Debug Mode");
    Debugger.Launch();
}

if (!Console.IsInputRedirected)
{
    Console.Error.WriteLine("No input.");
    return -1;
}

var inputString = Console.In.ReadToEnd();

var input = JsonSerializer.Deserialize(inputString, TestSerializerContext.Default.TestInput);

var tree = SyntaxTree.ParseText(input.Contents);
var graph = tree.ToSyntaxGraph();

var result = JsonSerializer.Serialize(graph);

Console.WriteLine(result);

return 0;

static bool IsDebug()
{
    return bool.TryParse(Environment.GetEnvironmentVariable("DEBUG_TCK_ADAPTER"), out var debug) && debug;
}
