namespace AsciiSharp.Syntax;

/// <summary>
/// 構文木を走査し、結果を返すための Visitor インターフェイス。
/// </summary>
/// <typeparam name="TResult">訪問結果の型。</typeparam>
public interface ISyntaxVisitor<TResult>
{
    /// <summary>
    /// DocumentSyntax ノードを訪問する。
    /// </summary>
    /// <param name="node">訪問するノード。</param>
    /// <returns>訪問結果。</returns>
    TResult VisitDocument(DocumentSyntax node);

    /// <summary>
    /// DocumentHeaderSyntax ノードを訪問する。
    /// </summary>
    /// <param name="node">訪問するノード。</param>
    /// <returns>訪問結果。</returns>
    TResult VisitDocumentHeader(DocumentHeaderSyntax node);

    /// <summary>
    /// AuthorLineSyntax ノードを訪問する。
    /// </summary>
    /// <param name="node">訪問するノード。</param>
    /// <returns>訪問結果。</returns>
    TResult VisitAuthorLine(AuthorLineSyntax node);

    /// <summary>
    /// DocumentBodySyntax ノードを訪問する。
    /// </summary>
    /// <param name="node">訪問するノード。</param>
    /// <returns>訪問結果。</returns>
    TResult VisitDocumentBody(DocumentBodySyntax node);

    /// <summary>
    /// SectionSyntax ノードを訪問する。
    /// </summary>
    /// <param name="node">訪問するノード。</param>
    /// <returns>訪問結果。</returns>
    TResult VisitSection(SectionSyntax node);

    /// <summary>
    /// SectionTitleSyntax ノードを訪問する。
    /// </summary>
    /// <param name="node">訪問するノード。</param>
    /// <returns>訪問結果。</returns>
    TResult VisitSectionTitle(SectionTitleSyntax node);

    /// <summary>
    /// ParagraphSyntax ノードを訪問する。
    /// </summary>
    /// <param name="node">訪問するノード。</param>
    /// <returns>訪問結果。</returns>
    TResult VisitParagraph(ParagraphSyntax node);

    /// <summary>
    /// InlineTextSyntax ノードを訪問する。
    /// </summary>
    /// <param name="node">訪問するノード。</param>
    /// <returns>訪問結果。</returns>
    TResult VisitInlineText(InlineTextSyntax node);

    /// <summary>
    /// AttributeEntrySyntax ノードを訪問する。
    /// </summary>
    /// <param name="node">訪問するノード。</param>
    /// <returns>訪問結果。</returns>
    TResult VisitAttributeEntry(AttributeEntrySyntax node);

    /// <summary>
    /// LinkSyntax ノードを訪問する。
    /// </summary>
    /// <param name="node">訪問するノード。</param>
    /// <returns>訪問結果。</returns>
    TResult VisitLink(LinkSyntax node);
}
