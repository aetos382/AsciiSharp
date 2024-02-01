using System;
using System.Diagnostics;
using System.Text.Json;

using AsciiSharp.TckAdapter;

if (IsDebug())
{
    Debugger.Break();
}

var inputString = Console.In.ReadToEnd();

var input = JsonSerializer.Deserialize(inputString, TestSerializerContext.Default.TestInput);

Console.WriteLine("{}");

static bool IsDebug()
{
    return bool.TryParse(Environment.GetEnvironmentVariable("DEBUG_TCK_ADAPTER"), out var debug) && debug;
}