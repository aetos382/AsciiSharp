namespace AsciiSharp.Syntax;

public enum SyntaxKind
{
    None,
    Unknown,
    Error,

    DocumentNode,
    DocumentHeaderNode,
    AuthorLineNode,
    SectionNode,
    AttributeEntryNode,

    IdentifierToken,
    EscapeToken,
    ColonToken,
    DoubleColonToken,
    DotToken,
    CommaToken,
    DoubleQuotationToken,
    HashToken,
    SemicolonToken,
    TildeToken,
    CaretToken,

    AsteriskToken,
    DoubleAsteriskToken,
    UnderscoreToken,
    DoubleUnderscoreToken,
    BacktickToken,
    DoubleBacktickToken,

    CommentDelimiterToken,
    ExampleBlockDelimiterToken,
    ListingBlockDelimiterToken,
    LiteralBlockDelimiterToken,
    OpenBlockDelimiterToken,
    SidebarBlockDelimiterToken,
    TableBlockDelimiterToken,
    PassBlockDelimiterToken,
    QuoteBlockDelimiterToken,

    OpenParenthesisToken,
    CloseParenthesisToken,
    OpenBracketToken,
    CloseBracketToken,
    OpenDoubleBracketToken,
    CloseDoubleBracketToken,
    OpenBraceToken,
    CloseBraceToken,
    ExclamationToken,
    EqualsToken,
    PercentToken,
    LessThanToken,
    GreaterThanToken,

    EqualsSequenceToken,

    EndOfLineTrivia,
    LineCommentTrivia,
    BlockCommentTrivia,

    EndOfSourceToken
}
