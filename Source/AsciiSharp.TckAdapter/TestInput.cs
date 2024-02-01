namespace AsciiSharp.TckAdapter;

internal readonly record struct TestInput(
    string Contents,
    string Path,
    string Type);
