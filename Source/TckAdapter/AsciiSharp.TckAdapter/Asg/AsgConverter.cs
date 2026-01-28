using System;
using System.Collections.Generic;
using System.Linq;

using AsciiSharp.Syntax;
using AsciiSharp.TckAdapter.Asg.Models;
using AsciiSharp.Text;

namespace AsciiSharp.TckAdapter.Asg;

/// <summary>
/// SyntaxTree を ASG に変換するコンバーター。
/// </summary>
/// <remarks>
/// <see cref="ISyntaxVisitor{TResult}"/> を実装し、各 <see cref="SyntaxNode"/> を
/// 対応する ASG ノードに変換する。未対応のノードタイプは <c>null</c> を返す。
/// </remarks>
public sealed class AsgConverter : ISyntaxVisitor<AsgNode?>
{
    private readonly SourceText _sourceText;

    /// <summary>
    /// <see cref="AsgConverter"/> を作成する。
    /// </summary>
    /// <param name="sourceText">位置情報計算に使用するソーステキスト。</param>
    /// <exception cref="ArgumentNullException"><paramref name="sourceText"/> が <c>null</c> の場合。</exception>
    public AsgConverter(SourceText sourceText)
    {
        ArgumentNullException.ThrowIfNull(sourceText);
        this._sourceText = sourceText;
    }

    /// <summary>
    /// <see cref="DocumentSyntax"/> を <see cref="AsgDocument"/> に変換する。
    /// </summary>
    /// <param name="document">変換する <see cref="DocumentSyntax"/>。</param>
    /// <returns>変換された <see cref="AsgDocument"/>。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="document"/> が <c>null</c> の場合。</exception>
    public AsgDocument Convert(DocumentSyntax document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return (AsgDocument)this.VisitDocument(document)!;
    }

    /// <inheritdoc />
    public AsgNode? VisitDocument(DocumentSyntax node)
    {
        ArgumentNullException.ThrowIfNull(node);

        var header = node.Header is not null
            ? this.ConvertHeader(node.Header)
            : null;

        return new AsgDocument
        {
            Header = header,
            Blocks = this.ConvertBlocks(node.Body).ToList(),
            Location = this.GetLocation(node)
        };
    }

    /// <inheritdoc />
    public AsgNode? VisitSection(SectionSyntax node)
    {
        ArgumentNullException.ThrowIfNull(node);

        var title = node.Title is not null
            ? this.ConvertTitleInlines(node.Title)
            : [];

        return new AsgSection
        {
            Title = title,
            Level = node.Level,
            Blocks = this.ConvertSectionContent(node.Content).ToList(),
            Location = this.GetLocation(node)
        };
    }

    /// <inheritdoc />
    public AsgNode? VisitParagraph(ParagraphSyntax node)
    {
        ArgumentNullException.ThrowIfNull(node);

        var inlines = new List<AsgInlineNode>();

        foreach (var element in node.InlineElements)
        {
            var asgNode = element.Accept(this);
            if (asgNode is AsgInlineNode inlineNode)
            {
                inlines.Add(inlineNode);
            }
        }

        // InlineElements が空の場合、パラグラフのテキストを直接取得
        if (inlines.Count == 0)
        {
            var paragraphText = node.ToString().Trim();
            if (!string.IsNullOrEmpty(paragraphText))
            {
                inlines.Add(new AsgText
                {
                    Value = paragraphText,
                    Location = this.GetLocation(node)
                });
            }
        }

        return new AsgParagraph
        {
            Inlines = inlines,
            Location = this.GetLocation(node)
        };
    }

    /// <inheritdoc />
    public AsgNode? VisitText(TextSyntax node)
    {
        ArgumentNullException.ThrowIfNull(node);

        return new AsgText
        {
            Value = node.Text,
            Location = this.GetLocation(node)
        };
    }

    // 以下は直接変換せず、親ノードで処理するか、未対応のため null を返す

    /// <inheritdoc />
    /// <remarks>
    /// <see cref="DocumentHeaderSyntax"/> は直接変換せず、
    /// <see cref="VisitDocument"/> で <see cref="ConvertHeader"/> を通じて処理する。
    /// </remarks>
    public AsgNode? VisitDocumentHeader(DocumentHeaderSyntax node) => null;

    /// <inheritdoc />
    /// <remarks>
    /// <see cref="DocumentBodySyntax"/> は直接変換せず、
    /// <see cref="VisitDocument"/> で <see cref="ConvertBlocks"/> を通じて処理する。
    /// </remarks>
    public AsgNode? VisitDocumentBody(DocumentBodySyntax node) => null;

    /// <inheritdoc />
    /// <remarks>
    /// <see cref="SectionTitleSyntax"/> は直接変換せず、
    /// <see cref="VisitSection"/> で <see cref="ConvertTitleInlines"/> を通じて処理する。
    /// </remarks>
    public AsgNode? VisitSectionTitle(SectionTitleSyntax node) => null;

    /// <inheritdoc />
    /// <remarks>
    /// <see cref="LinkSyntax"/> は現時点でスコープ外のため、<c>null</c> を返す。
    /// </remarks>
    public AsgNode? VisitLink(LinkSyntax node) => null;

    /// <inheritdoc />
    /// <remarks>
    /// <see cref="AuthorLineSyntax"/> は現時点でスコープ外のため、<c>null</c> を返す。
    /// </remarks>
    public AsgNode? VisitAuthorLine(AuthorLineSyntax node) => null;

    /// <summary>
    /// <see cref="DocumentHeaderSyntax"/> を <see cref="AsgHeader"/> に変換する。
    /// </summary>
    private AsgHeader ConvertHeader(DocumentHeaderSyntax header)
    {
        var title = header.Title is not null
            ? this.ConvertTitleInlines(header.Title)
            : [];

        return new AsgHeader
        {
            Title = title,
            Location = this.GetLocation(header)
        };
    }

    /// <summary>
    /// <see cref="SectionTitleSyntax"/> のインライン要素を <see cref="AsgInlineNode"/> のリストに変換する。
    /// </summary>
    private IReadOnlyList<AsgInlineNode> ConvertTitleInlines(SectionTitleSyntax title)
    {
        var titleText = title.TitleContent;

        if (string.IsNullOrEmpty(titleText))
        {
            return [];
        }

        // タイトルテキストの位置を計算
        // TitleText トークンの位置を使用（なければタイトル全体の位置）
        var location = title.TitleText is { } titleToken
            ? this.GetTokenLocation(titleToken)
            : this.GetLocation(title);

        return
        [
            new AsgText
            {
                Value = titleText,
                Location = location
            }
        ];
    }

    /// <summary>
    /// <see cref="DocumentBodySyntax"/> の子ノードを <see cref="AsgBlockNode"/> のシーケンスに変換する。
    /// </summary>
    private IEnumerable<AsgBlockNode> ConvertBlocks(DocumentBodySyntax? body)
    {
        if (body is null)
        {
            yield break;
        }

        foreach (var childOrToken in body.ChildNodesAndTokens())
        {
            if (!childOrToken.IsNode)
            {
                continue;
            }

            var node = childOrToken.AsNode()!;
            var asgNode = node.Accept(this);

            if (asgNode is AsgBlockNode blockNode)
            {
                yield return blockNode;
            }
        }
    }

    /// <summary>
    /// <see cref="SectionSyntax.Content"/> を <see cref="AsgBlockNode"/> のシーケンスに変換する。
    /// </summary>
    private IEnumerable<AsgBlockNode> ConvertSectionContent(IReadOnlyList<SyntaxNode> content)
    {
        foreach (var node in content)
        {
            var asgNode = node.Accept(this);

            if (asgNode is AsgBlockNode blockNode)
            {
                yield return blockNode;
            }
        }
    }

    /// <summary>
    /// <see cref="SyntaxNode"/> の位置情報を <see cref="AsgLocation"/> に変換する。
    /// </summary>
    private AsgLocation? GetLocation(SyntaxNode node)
    {
        var span = node.Span;
        return this.GetLocationFromSpan(span.Start, span.End);
    }

    /// <summary>
    /// <see cref="SyntaxToken"/> の位置情報を <see cref="AsgLocation"/> に変換する。
    /// </summary>
    private AsgLocation? GetTokenLocation(SyntaxToken token)
    {
        var span = token.Span;
        return this.GetLocationFromSpan(span.Start, span.End);
    }

    /// <summary>
    /// 開始位置と終了位置から <see cref="AsgLocation"/> を作成する。
    /// </summary>
    /// <param name="startOffset">開始オフセット（0-based、包含）。</param>
    /// <param name="endOffset">終了オフセット（0-based、排他）。</param>
    private AsgLocation? GetLocationFromSpan(int startOffset, int endOffset)
    {
        // 空のスパンの場合は null を返す
        if (this._sourceText.Length == 0)
        {
            return null;
        }

        // 開始位置（0-based から 1-based への変換）
        var (startLine, startCol) = this._sourceText.GetLineAndColumn(startOffset);
        var start = new AsgPosition(startLine + 1, startCol + 1);

        // 終了位置
        // TCK は終了位置を包含的（最後の文字の位置）として扱うため、endOffset - 1 を使用
        var endPos = endOffset > startOffset ? endOffset - 1 : startOffset;
        var (endLine, endCol) = this._sourceText.GetLineAndColumn(endPos);
        var end = new AsgPosition(endLine + 1, endCol + 1);

        return new AsgLocation(start, end);
    }
}
