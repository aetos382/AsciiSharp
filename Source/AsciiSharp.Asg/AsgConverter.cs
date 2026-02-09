using System;
using System.Collections.Generic;
using System.Linq;

using AsciiSharp.Asg.Models;
using AsciiSharp.Syntax;

namespace AsciiSharp.Asg;

/// <summary>
/// SyntaxTree を ASG に変換するコンバーター。
/// </summary>
public sealed class AsgConverter
{
    private readonly SyntaxTree _syntaxTree;

    /// <summary>
    /// <see cref="AsgConverter"/> を作成する。
    /// </summary>
    /// <param name="syntaxTree">変換対象の構文木。</param>
    /// <exception cref="ArgumentNullException"><paramref name="syntaxTree"/> が <c>null</c> の場合。</exception>
    public AsgConverter(SyntaxTree syntaxTree)
    {
        ArgumentNullException.ThrowIfNull(syntaxTree);
        this._syntaxTree = syntaxTree;
    }

    /// <summary>
    /// 構文木を <see cref="AsgDocument"/> に変換する。
    /// </summary>
    /// <returns>変換された <see cref="AsgDocument"/>。</returns>
    /// <exception cref="InvalidOperationException">ルートノードが <see cref="DocumentSyntax"/> でない場合。</exception>
    public AsgDocument Convert()
    {
        if (this._syntaxTree.Root is not DocumentSyntax document)
        {
            throw new InvalidOperationException("Root node is not a DocumentSyntax.");
        }

        var visitor = new Visitor(this._syntaxTree);
        return (AsgDocument)document.Accept(visitor)!;
    }

    /// <summary>
    /// 構文木を走査して ASG ノードに変換する内部ビジター。
    /// </summary>
    private sealed class Visitor : ISyntaxVisitor<AsgNode?>
    {
        private readonly SyntaxTree _syntaxTree;

        internal Visitor(SyntaxTree syntaxTree)
        {
            this._syntaxTree = syntaxTree;
        }

        AsgNode? ISyntaxVisitor<AsgNode?>.VisitDocument(DocumentSyntax node)
        {
            ArgumentNullException.ThrowIfNull(node);

            var header = node.Header is not null
                ? this.ConvertHeader(node.Header)
                : null;

            var attributes = ConvertAttributes(node.Header);

            return new AsgDocument
            {
                Attributes = attributes,
                Header = header,
                Blocks = this.ConvertBlocks(node.Body).ToList(),
                Location = this.GetLocation(node)
            };
        }

        AsgNode? ISyntaxVisitor<AsgNode?>.VisitSection(SectionSyntax node)
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

        AsgNode? ISyntaxVisitor<AsgNode?>.VisitParagraph(ParagraphSyntax node)
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

        AsgNode? ISyntaxVisitor<AsgNode?>.VisitInlineText(InlineTextSyntax node)
        {
            ArgumentNullException.ThrowIfNull(node);

            return new AsgText
            {
                Value = node.Text,
                Location = this.GetLocation(node)
            };
        }

        // 以下は直接変換せず、親ノードで処理するか、未対応のため null を返す

        AsgNode? ISyntaxVisitor<AsgNode?>.VisitDocumentHeader(DocumentHeaderSyntax node) => null;

        AsgNode? ISyntaxVisitor<AsgNode?>.VisitDocumentBody(DocumentBodySyntax node) => null;

        AsgNode? ISyntaxVisitor<AsgNode?>.VisitSectionTitle(SectionTitleSyntax node) => null;

        AsgNode? ISyntaxVisitor<AsgNode?>.VisitLink(LinkSyntax node) => null;

        AsgNode? ISyntaxVisitor<AsgNode?>.VisitAuthorLine(AuthorLineSyntax node) => null;

        AsgNode? ISyntaxVisitor<AsgNode?>.VisitAttributeEntry(AttributeEntrySyntax node) => null;

        /// <summary>
        /// <see cref="DocumentHeaderSyntax"/> の属性エントリを辞書に変換する。
        /// </summary>
        /// <param name="header">ドキュメントヘッダー。<c>null</c> の場合は空の辞書を返す。</param>
        /// <returns>属性名をキー、属性値を値とする辞書。値なし属性は空文字列。</returns>
        private static Dictionary<string, string> ConvertAttributes(DocumentHeaderSyntax? header)
        {
            var attributes = new Dictionary<string, string>();

            if (header is null)
            {
                return attributes;
            }

            foreach (var entry in header.AttributeEntries)
            {
                attributes[entry.Name] = entry.Value;
            }

            return attributes;
        }

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
            var titleText = title.GetTitleContent();

            if (string.IsNullOrEmpty(titleText))
            {
                return [];
            }

            // タイトルテキストの位置を計算
            // InlineElements があればその位置を使用、なければタイトル全体の位置
            var location = title.InlineElements.Count > 0
                ? this.GetLocation(title.InlineElements[0])
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
        /// <remarks>
        /// 末尾の改行・空白・EOF トークンを除外したコンテンツ スパンを使用する。
        /// TCK はノードの位置を「意味のあるコンテンツの範囲」として扱うため、
        /// 構文的に必要だが意味を持たない末尾トークンを除外する。
        /// </remarks>
        private AsgLocation? GetLocation(SyntaxNode node)
        {
            var startOffset = node.Span.Start;
            var endOffset = GetContentEndOffset(node);

            if (endOffset <= startOffset)
            {
                return null;
            }

            return this.GetLocationFromSpan(startOffset, endOffset);
        }

        /// <summary>
        /// ノード内の末尾の改行・空白・EOF トークンを除外したコンテンツ終端オフセットを取得する。
        /// </summary>
        private static int GetContentEndOffset(SyntaxNode node)
        {
            var endOffset = node.Span.Start;

            foreach (var token in node.DescendantTokens())
            {
                if (token.Kind != SyntaxKind.NewLineToken &&
                    token.Kind != SyntaxKind.WhitespaceToken &&
                    token.Kind != SyntaxKind.EndOfFileToken &&
                    !token.IsMissing)
                {
                    var tokenEnd = token.Span.End;
                    if (tokenEnd > endOffset)
                    {
                        endOffset = tokenEnd;
                    }
                }
            }

            return endOffset;
        }

        /// <summary>
        /// 開始位置と終了位置から <see cref="AsgLocation"/> を作成する。
        /// </summary>
        /// <param name="startOffset">開始オフセット（0-based、包含）。</param>
        /// <param name="endOffset">終了オフセット（0-based、排他）。</param>
        private AsgLocation? GetLocationFromSpan(int startOffset, int endOffset)
        {
            // 空のスパンの場合は null を返す
            if (this._syntaxTree.Text.Length == 0)
            {
                return null;
            }

            // 開始位置（0-based から 1-based への変換）
            var (startLine, startCol) = this._syntaxTree.Text.GetLineAndColumn(startOffset);
            var start = new AsgPosition(startLine + 1, startCol + 1);

            // 終了位置
            // TCK は終了位置を包含的（最後の文字の位置）として扱うため、endOffset - 1 を使用
            var endPos = endOffset > startOffset ? endOffset - 1 : startOffset;
            var (endLine, endCol) = this._syntaxTree.Text.GetLineAndColumn(endPos);
            var end = new AsgPosition(endLine + 1, endCol + 1);

            return new AsgLocation(start, end);
        }
    }
}
