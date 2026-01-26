
using System.Collections.Generic;
using System.Text;

using AsciiSharp.InternalSyntax;

namespace AsciiSharp.Syntax;
/// <summary>
/// リンクを表す構文ノード。
/// </summary>
public sealed class LinkSyntax : SyntaxNode
{
    private readonly List<SyntaxToken> _tokens = [];

    /// <summary>
    /// リンクの URL 文字列。
    /// </summary>
    /// <remarks>
    /// CA1056 を抑制: 構文解析では URL を文字列として保持する。
    /// 不正な URL も解析対象となり得るため、Uri 型への変換は行わない。
    /// </remarks>
#pragma warning disable CA1056
    public string? Url { get; }
#pragma warning restore CA1056

    /// <summary>
    /// リンクの表示テキスト。
    /// </summary>
    public string? DisplayText { get; }

    /// <summary>
    /// LinkSyntax を作成する。
    /// </summary>
    internal LinkSyntax(InternalNode internalNode, SyntaxNode? parent, int position, SyntaxTree? syntaxTree)
        : base(internalNode, parent, position, syntaxTree)
    {
        var currentPosition = position;
        var urlBuilder = new StringBuilder();
        var displayTextBuilder = new StringBuilder();
        var inDisplayText = false;
        var hasDisplayText = false;

        for (var i = 0; i < internalNode.SlotCount; i++)
        {
            var slot = internalNode.GetSlot(i);
            if (slot is not InternalToken internalToken)
            {
                continue;
            }

            var token = new SyntaxToken(internalToken, this, currentPosition, i);
            this._tokens.Add(token);

            // URL と表示テキストの解析
            if (internalToken.Kind == SyntaxKind.OpenBracketToken)
            {
                inDisplayText = true;
                hasDisplayText = true;
            }
            else if (internalToken.Kind == SyntaxKind.CloseBracketToken)
            {
                inDisplayText = false;
            }
            else if (inDisplayText)
            {
                displayTextBuilder.Append(internalToken.Text);
            }
            else
            {
                urlBuilder.Append(internalToken.Text);
            }

            currentPosition += slot.FullWidth;
        }

        this.Url = urlBuilder.Length > 0 ? urlBuilder.ToString() : null;
        this.DisplayText = hasDisplayText ? displayTextBuilder.ToString() : null;
    }

    /// <inheritdoc />
    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        foreach (var token in this._tokens)
        {
            yield return new SyntaxNodeOrToken(token);
        }
    }

    /// <inheritdoc />
    protected override SyntaxNode ReplaceNodeCore(SyntaxNode oldNode, SyntaxNode newNode)
    {
        // リーフノードなので、子孫にターゲットノードは存在しない
        return this;
    }
}
