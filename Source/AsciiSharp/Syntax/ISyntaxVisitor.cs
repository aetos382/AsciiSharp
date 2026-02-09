namespace AsciiSharp.Syntax;

/// <summary>
/// 構文木を走査するための Visitor インターフェイス。
/// </summary>
public interface ISyntaxVisitor
{
    /// <summary>
    /// DocumentSyntax ノードを訪問する。
    /// </summary>
    /// <param name="node">訪問するノード。</param>
    void VisitDocument(DocumentSyntax node);

    /// <summary>
    /// DocumentHeaderSyntax ノードを訪問する。
    /// </summary>
    /// <param name="node">訪問するノード。</param>
    void VisitDocumentHeader(DocumentHeaderSyntax node);

    /// <summary>
    /// AuthorLineSyntax ノードを訪問する。
    /// </summary>
    /// <param name="node">訪問するノード。</param>
    void VisitAuthorLine(AuthorLineSyntax node);

    /// <summary>
    /// DocumentBodySyntax ノードを訪問する。
    /// </summary>
    /// <param name="node">訪問するノード。</param>
    void VisitDocumentBody(DocumentBodySyntax node);

    /// <summary>
    /// SectionSyntax ノードを訪問する。
    /// </summary>
    /// <param name="node">訪問するノード。</param>
    void VisitSection(SectionSyntax node);

    /// <summary>
    /// SectionTitleSyntax ノードを訪問する。
    /// </summary>
    /// <param name="node">訪問するノード。</param>
    void VisitSectionTitle(SectionTitleSyntax node);

    /// <summary>
    /// ParagraphSyntax ノードを訪問する。
    /// </summary>
    /// <param name="node">訪問するノード。</param>
    void VisitParagraph(ParagraphSyntax node);

    /// <summary>
    /// InlineTextSyntax ノードを訪問する。
    /// </summary>
    /// <param name="node">訪問するノード。</param>
    void VisitInlineText(InlineTextSyntax node);

    /// <summary>
    /// AttributeEntrySyntax ノードを訪問する。
    /// </summary>
    /// <param name="node">訪問するノード。</param>
    void VisitAttributeEntry(AttributeEntrySyntax node);

    /// <summary>
    /// LinkSyntax ノードを訪問する。
    /// </summary>
    /// <param name="node">訪問するノード。</param>
    void VisitLink(LinkSyntax node);
}
