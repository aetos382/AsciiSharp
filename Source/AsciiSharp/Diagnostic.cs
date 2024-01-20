namespace AsciiSharp;

public class Diagnostic
{
    public Severity Severity { get; set; }

    public string Message { get; set; }

    public string? Path { get; set; }

    public Location? Location { get; set; }

    public Diagnostic[] RelatedDiagnostics { get; set; }
}
