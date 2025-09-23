using System;

namespace AsciiSharp.Specs;

internal static class SyntaxNodeKindExtensions
{
    extension(SyntaxNodeKind)
    {
        public static SyntaxNodeKind Parse(string value)
        {
            ArgumentNullException.ThrowIfNull(value);

            return value switch
            {
                "document" => SyntaxNodeKind.Document,
                "paragraph" => SyntaxNodeKind.Paragraph,
                "text" => SyntaxNodeKind.Text,
                _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unknown node kind: {value}")
            };
        }
    }
}
