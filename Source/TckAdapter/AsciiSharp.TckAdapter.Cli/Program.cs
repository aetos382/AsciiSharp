using System;
using System.Text.Json;

using AsciiSharp;
using AsciiSharp.TckAdapter;
using AsciiSharp.TckAdapter.Cli;

var rawInput = Console.ReadLine();
if (string.IsNullOrWhiteSpace(rawInput))
{
    return 1;
}

var input = JsonSerializer.Deserialize(rawInput, TckInputSerializerContext.Default.TckInput);
if (input is null)
{
    return 2;
}

var syntaxTree = SyntaxTree.Parse(input.Contents);
var asgDocument = syntaxTree.ToAsg();

var output = JsonSerializer.Serialize(asgDocument, AsgSerializerContext.Default.Document);

Console.WriteLine(output);

return 0;
