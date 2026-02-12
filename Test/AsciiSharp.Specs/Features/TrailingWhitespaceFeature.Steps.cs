// <copyright file="TrailingWhitespaceFeature.Steps.cs" company="Takamasa Matsuyama">
// Copyright (c) 2025 Takamasa Matsuyama
// Licensed under the MIT License
// See the LICENSE file in the project root for full license information.
// </copyright>

using AsciiSharp.Syntax;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

/// <summary>
/// TrailingWhitespaceFeature のステップ定義。
/// </summary>
public partial class TrailingWhitespaceFeature
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
}
