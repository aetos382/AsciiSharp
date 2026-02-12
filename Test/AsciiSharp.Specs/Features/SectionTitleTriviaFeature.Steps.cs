// <copyright file="SectionTitleTriviaFeature.Steps.cs" company="Takamasa Matsuyama">
// Copyright (c) 2025 Takamasa Matsuyama
// Licensed under the MIT License
// See the LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;

using AsciiSharp.Syntax;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

/// <summary>
/// SectionTitleTriviaFeature のステップ定義。
/// </summary>
public partial class SectionTitleTriviaFeature
{
    private string? _sourceText;
    private SyntaxTree? _syntaxTree;
    private string? _reconstructedText;

    /// <summary>
    /// パーサーが初期化されている状態を設定する。
    /// </summary>
    private void パーサーが初期化されている()
    {
        // 特に初期化処理は不要
    }

    /// <summary>
    /// 指定された AsciiDoc 文書を設定する。
    /// </summary>
    /// <param name="text">AsciiDoc 文書のテキスト。</param>
    private void 以下のAsciiDoc文書がある(string text)
    {
        this._sourceText = text;
    }

    /// <summary>
    /// 文書を解析する。
    /// </summary>
    private void 文書を解析する()
    {
        Assert.IsNotNull(this._sourceText);
        this._syntaxTree = SyntaxTree.ParseText(this._sourceText);
    }

    /// <summary>
    /// 構文木から完全なテキストを取得する。
    /// </summary>
    private void 構文木から完全なテキストを取得する()
    {
        Assert.IsNotNull(this._syntaxTree);
        this._reconstructedText = this._syntaxTree.Root.ToFullString();
    }

    /// <summary>
    /// 再構築されたテキストが元の文書と一致することを検証する。
    /// </summary>
    private void 再構築されたテキストは元の文書と一致する()
    {
        Assert.IsNotNull(this._sourceText);
        Assert.IsNotNull(this._reconstructedText);
        Assert.AreEqual(this._sourceText, this._reconstructedText);
    }

    /// <summary>
    /// セクションタイトルのマーカーが TrailingTrivia に空白を持つことを検証する。
    /// </summary>
    private void セクションタイトルのマーカーはTrailingTriviaに空白を持つ()
    {
        var tree = this._syntaxTree;
        Assert.IsNotNull(tree);
        var document = tree.Root as DocumentSyntax;
        Assert.IsNotNull(document);

        // 最初のセクションタイトルを取得（ヘッダーまたは本文の最初のセクション）
        SectionTitleSyntax? sectionTitle = document.Header?.Title;
        if (sectionTitle is null)
        {
            var firstSection = document.Body?.ChildNodesAndTokens()
                .Where(c => c.IsNode && c.AsNode()?.Kind == SyntaxKind.Section)
                .Select(c => c.AsNode() as SectionSyntax)
                .FirstOrDefault();
            sectionTitle = firstSection?.Title;
        }

        Assert.IsNotNull(sectionTitle);
        Assert.IsNotNull(sectionTitle.Marker);
        var marker = sectionTitle.Marker.Value;
        var hasWhitespaceTrivia = marker.TrailingTrivia
            .Any(t => t.Kind == SyntaxKind.WhitespaceTrivia);
        Assert.IsTrue(hasWhitespaceTrivia, "マーカーの TrailingTrivia に空白がありません。");
    }
}
