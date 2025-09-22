using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AsciiSharp.TckAdapter;

// Root document class
public class Document
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "document";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "block";

    [JsonPropertyName("attributes")]
    public Dictionary<string, string?> Attributes { get; set; } = new();

    [JsonPropertyName("header")]
    public DocumentHeader? Header { get; set; }

    [JsonPropertyName("blocks")]
    public List<IBlock> Blocks { get; set; } = new();

    [JsonPropertyName("location")]
    public Location? Location { get; set; }
}

// Document header
public class DocumentHeader
{
    [JsonPropertyName("title")]
    public List<IInline> Title { get; set; } = new();

    [JsonPropertyName("authors")]
    public List<Author>? Authors { get; set; }

    [JsonPropertyName("location")]
    public Location? Location { get; set; }
}

// Author information
public class Author
{
    [JsonPropertyName("fullname")]
    public string? Fullname { get; set; }

    [JsonPropertyName("initials")]
    public string? Initials { get; set; }

    [JsonPropertyName("firstname")]
    public string? Firstname { get; set; }

    [JsonPropertyName("middlename")]
    public string? Middlename { get; set; }

    [JsonPropertyName("lastname")]
    public string? Lastname { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }
}

// Location information
public class Location
{
    public LocationBoundary Start { get; set; } = new();
    public LocationBoundary End { get; set; } = new();
}

public class LocationBoundary
{
    [JsonPropertyName("line")]
    public int Line { get; set; }

    [JsonPropertyName("col")]
    public int Column { get; set; }

    [JsonPropertyName("file")]
    public List<string>? File { get; set; }
}

// Block metadata
public class BlockMetadata
{
    [JsonPropertyName("attributes")]
    public Dictionary<string, string> Attributes { get; set; } = new();

    [JsonPropertyName("options")]
    public List<string> Options { get; set; } = new();

    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; } = new();

    [JsonPropertyName("location")]
    public Location? Location { get; set; }
}

// Interfaces and base classes
[JsonPolymorphic(TypeDiscriminatorPropertyName = "name")]
[JsonDerivedType(typeof(Section), "section")]
[JsonDerivedType(typeof(List), "list")]
[JsonDerivedType(typeof(DescriptionList), "dlist")]
[JsonDerivedType(typeof(DiscreteHeading), "heading")]
[JsonDerivedType(typeof(Break), "break")]
[JsonDerivedType(typeof(AudioMacro), "audio")]
[JsonDerivedType(typeof(VideoMacro), "video")]
[JsonDerivedType(typeof(ImageMacro), "image")]
[JsonDerivedType(typeof(TocMacro), "toc")]
[JsonDerivedType(typeof(ListingBlock), "listing")]
[JsonDerivedType(typeof(LiteralBlock), "literal")]
[JsonDerivedType(typeof(ParagraphBlock), "paragraph")]
[JsonDerivedType(typeof(PassBlock), "pass")]
[JsonDerivedType(typeof(StemBlock), "stem")]
[JsonDerivedType(typeof(VerseBlock), "verse")]
[JsonDerivedType(typeof(AdmonitionBlock), "admonition")]
[JsonDerivedType(typeof(ExampleBlock), "example")]
[JsonDerivedType(typeof(SidebarBlock), "sidebar")]
[JsonDerivedType(typeof(OpenBlock), "open")]
[JsonDerivedType(typeof(QuoteBlock), "quote")]
public interface IBlock
{
    string Name { get; set; }
    string Type { get; set; }
    string? Id { get; set; }
    List<IInline>? Title { get; set; }
    List<IInline>? Reftext { get; set; }
    BlockMetadata? Metadata { get; set; }
    Location? Location { get; set; }
}

// Abstract base block
public abstract class AbstractBlock : IBlock
{
    [JsonPropertyName("name")]
    public abstract string Name { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = "block";

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("title")]
    public List<IInline>? Title { get; set; }

    [JsonPropertyName("reftext")]
    public List<IInline>? Reftext { get; set; }

    [JsonPropertyName("metadata")]
    public BlockMetadata? Metadata { get; set; }

    [JsonPropertyName("location")]
    public Location? Location { get; set; }
}

// Abstract heading
public abstract class AbstractHeading : AbstractBlock
{
    [JsonPropertyName("level")]
    public int Level { get; set; }
}

// Section block
public class Section : AbstractHeading
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "section";

    [JsonPropertyName("blocks")]
    public List<IBlock> Blocks { get; set; } = new();
}

// Discrete heading
public class DiscreteHeading : AbstractHeading
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "heading";
}

// List blocks
public enum ListVariant
{
    [JsonStringEnumMemberName("callout")] Callout,
    [JsonStringEnumMemberName("ordered")] Ordered,
    [JsonStringEnumMemberName("unordered")] Unordered
}

public class List : AbstractBlock
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "list";

    [JsonPropertyName("marker")]
    public string Marker { get; set; } = string.Empty;

    [JsonPropertyName("variant")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ListVariant Variant { get; set; }

    [JsonPropertyName("items")]
    public List<ListItem> Items { get; set; } = new();
}

public class DescriptionList : AbstractBlock
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "dlist";

    [JsonPropertyName("marker")]
    public string Marker { get; set; } = string.Empty;

    [JsonPropertyName("items")]
    public List<DescriptionListItem> Items { get; set; } = new();
}

// Abstract list item
public abstract class AbstractListItem : AbstractBlock
{
    [JsonPropertyName("marker")]
    public string Marker { get; set; } = string.Empty;

    [JsonPropertyName("principal")]
    public List<IInline> Principal { get; set; } = new();

    [JsonPropertyName("blocks")]
    public List<IBlock> Blocks { get; set; } = new();
}

public class ListItem : AbstractListItem
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "listItem";
}

public class DescriptionListItem : AbstractListItem
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "dlistItem";

    [JsonPropertyName("terms")]
    public List<List<IInline>> Terms { get; set; } = new();
}

// Break block
public enum BreakVariant
{
    [JsonStringEnumMemberName("page")] Page,
    [JsonStringEnumMemberName("thematic")] Thematic
}

public class Break : AbstractBlock
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "break";

    [JsonPropertyName("variant")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BreakVariant Variant { get; set; }
}

// Block macros
public abstract class BlockMacro : AbstractBlock
{
    [JsonPropertyName("form")]
    public string Form { get; set; } = "macro";

    [JsonPropertyName("target")]
    public string? Target { get; set; }
}

public class AudioMacro : BlockMacro
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "audio";
}

public class VideoMacro : BlockMacro
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "video";
}

public class ImageMacro : BlockMacro
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "image";
}

public class TocMacro : BlockMacro
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "toc";
}

// Leaf blocks
public enum BlockForm
{
    [JsonStringEnumMemberName("delimited")] Delimited,
    [JsonStringEnumMemberName("indented")] Indented,
    [JsonStringEnumMemberName("paragraph")] Paragraph
}

public abstract class LeafBlock : AbstractBlock
{
    [JsonPropertyName("form")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BlockForm? Form { get; set; }

    [JsonPropertyName("delimiter")]
    public string? Delimiter { get; set; }

    [JsonPropertyName("inlines")]
    public List<IInline> Inlines { get; set; } = new();
}

public class ListingBlock : LeafBlock
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "listing";
}

public class LiteralBlock : LeafBlock
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "literal";
}

public class ParagraphBlock : LeafBlock
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "paragraph";
}

public class PassBlock : LeafBlock
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "pass";
}

public class StemBlock : LeafBlock
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "stem";
}

public class VerseBlock : LeafBlock
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "verse";
}

// Parent blocks
public abstract class ParentBlock : AbstractBlock
{
    [JsonPropertyName("form")]
    public string Form { get; set; } = "delimited";

    [JsonPropertyName("delimiter")]
    public string Delimiter { get; set; } = string.Empty;

    [JsonPropertyName("blocks")]
    public List<IBlock> Blocks { get; set; } = new();
}

public enum AdmonitionVariant
{
    [JsonStringEnumMemberName("caution")] Caution,
    [JsonStringEnumMemberName("important")] Important,
    [JsonStringEnumMemberName("note")] Note,
    [JsonStringEnumMemberName("tip")] Tip,
    [JsonStringEnumMemberName("warning")] Warning
}

public class AdmonitionBlock : ParentBlock
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "admonition";

    [JsonPropertyName("variant")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AdmonitionVariant Variant { get; set; }
}

public class ExampleBlock : ParentBlock
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "example";
}

public class SidebarBlock : ParentBlock
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "sidebar";
}

public class OpenBlock : ParentBlock
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "open";
}

public class QuoteBlock : ParentBlock
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "quote";
}

// Inline elements
[JsonPolymorphic(TypeDiscriminatorPropertyName = "name")]
[JsonDerivedType(typeof(InlineSpan), "span")]
[JsonDerivedType(typeof(InlineRef), "ref")]
[JsonDerivedType(typeof(InlineText), "text")]
[JsonDerivedType(typeof(InlineCharRef), "charref")]
[JsonDerivedType(typeof(InlineRaw), "raw")]
public interface IInline
{
    string Name { get; set; }
    string Type { get; set; }
    Location? Location { get; set; }
}

// Abstract parent inline
public abstract class AbstractParentInline : IInline
{
    [JsonPropertyName("name")]
    public abstract string Name { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = "inline";

    [JsonPropertyName("inlines")]
    public List<IInline> Inlines { get; set; } = new();

    [JsonPropertyName("location")]
    public Location? Location { get; set; }
}

// Inline span variants
public enum SpanVariant
{
    [JsonStringEnumMemberName("strong")] Strong,
    [JsonStringEnumMemberName("emphasis")] Emphasis,
    [JsonStringEnumMemberName("code")] Code,
    [JsonStringEnumMemberName("mark")] Mark
}

public enum SpanForm
{
    [JsonStringEnumMemberName("constrained")] Constrained,
    [JsonStringEnumMemberName("unconstrained")] Unconstrained
}

public class InlineSpan : AbstractParentInline
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "span";

    [JsonPropertyName("variant")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SpanVariant Variant { get; set; }

    [JsonPropertyName("form")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SpanForm Form { get; set; }
}

// Inline reference variants
public enum RefVariant
{
    [JsonStringEnumMemberName("link")] Link,
    [JsonStringEnumMemberName("xref")] Xref
}

public class InlineRef : AbstractParentInline
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "ref";

    [JsonPropertyName("variant")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RefVariant Variant { get; set; }

    [JsonPropertyName("target")]
    public string Target { get; set; } = string.Empty;
}

// Inline literal elements
public abstract class InlineLiteral : IInline
{
    [JsonPropertyName("name")]
    public abstract string Name { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = "string";

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public Location? Location { get; set; }
}

public class InlineText : InlineLiteral
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "text";
}

public class InlineCharRef : InlineLiteral
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "charref";
}

public class InlineRaw : InlineLiteral
{
    [JsonPropertyName("name")]
    public override string Name { get; set; } = "raw";
}