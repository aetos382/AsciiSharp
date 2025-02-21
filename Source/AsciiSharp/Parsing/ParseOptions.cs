using System;

namespace AsciiSharp.Parsing;

public class ParseOptions
{
    public Uri? BaseUri { get; set; }

    public static readonly ParseOptions Default = new();
}
