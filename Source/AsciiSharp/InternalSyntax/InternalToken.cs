
using System;
using System.Collections.Generic;
using System.Text;

namespace AsciiSharp.InternalSyntax;
/// <summary>
/// 内部トークンを表すクラス。
/// </summary>
/// <remarks>
/// <para>トークンは構文木の葉ノードであり、実際のテキストを保持する。</para>
/// <para>トリビア（空白、コメント等）はトークンに付与される。</para>
/// </remarks>
internal sealed class InternalToken : InternalNode
{
    private static readonly InternalTrivia[] EmptyTriviaArray = [];

    /// <summary>
    /// トークンのテキスト。
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// 先行トリビアの配列。
    /// </summary>
    public IReadOnlyList<InternalTrivia> LeadingTrivia { get; }

    /// <summary>
    /// 後続トリビアの配列。
    /// </summary>
    public IReadOnlyList<InternalTrivia> TrailingTrivia { get; }

    private readonly int _leadingTriviaWidth;
    private readonly int _trailingTriviaWidth;
    private readonly bool _isMissing;

    /// <inheritdoc />
    public override int Width => this.Text.Length;

    /// <inheritdoc />
    public override int FullWidth => this._leadingTriviaWidth + this.Text.Length + this._trailingTriviaWidth;

    /// <inheritdoc />
    public override int SlotCount => 0;

    /// <inheritdoc />
    public override bool IsMissing => this._isMissing;

    /// <inheritdoc />
    public override int LeadingTriviaWidth => this._leadingTriviaWidth;

    /// <inheritdoc />
    public override int TrailingTriviaWidth => this._trailingTriviaWidth;

    /// <summary>
    /// 指定された種別とテキストで InternalToken を作成する。
    /// </summary>
    /// <param name="kind">トークンの種別。</param>
    /// <param name="text">トークンのテキスト。</param>
    public InternalToken(SyntaxKind kind, string text)
        : this(kind, text, EmptyTriviaArray, EmptyTriviaArray, isMissing: false, containsDiagnostics: false)
    {
    }

    /// <summary>
    /// 指定された種別、テキスト、トリビアで InternalToken を作成する。
    /// </summary>
    /// <param name="kind">トークンの種別。</param>
    /// <param name="text">トークンのテキスト。</param>
    /// <param name="leadingTrivia">先行トリビアの配列。</param>
    /// <param name="trailingTrivia">後続トリビアの配列。</param>
    /// <param name="isMissing">欠落トークンかどうか。</param>
    /// <param name="containsDiagnostics">診断情報を含むかどうか。</param>
    public InternalToken(
        SyntaxKind kind,
        string text,
        IReadOnlyList<InternalTrivia> leadingTrivia,
        IReadOnlyList<InternalTrivia> trailingTrivia,
        bool isMissing = false,
        bool containsDiagnostics = false)
        : base(kind)
    {
        ArgumentNullException.ThrowIfNull(text);

        this.Text = text;
        this.LeadingTrivia = leadingTrivia ?? EmptyTriviaArray;
        this.TrailingTrivia = trailingTrivia ?? EmptyTriviaArray;
        this._isMissing = isMissing;
        this.ContainsDiagnostics = containsDiagnostics;

        this._leadingTriviaWidth = 0;
        foreach (var trivia in this.LeadingTrivia)
        {
            this._leadingTriviaWidth += trivia.Width;
        }

        this._trailingTriviaWidth = 0;
        foreach (var trivia in this.TrailingTrivia)
        {
            this._trailingTriviaWidth += trivia.Width;
        }
    }

    /// <summary>
    /// 欠落トークンを作成する。
    /// </summary>
    /// <param name="kind">期待されるトークンの種別。</param>
    /// <returns>欠落トークン。</returns>
    public static InternalToken Missing(SyntaxKind kind)
    {
        return new InternalToken(
            kind,
            string.Empty,
            EmptyTriviaArray,
            EmptyTriviaArray,
            isMissing: true,
            containsDiagnostics: true);
    }

    /// <summary>
    /// 新しいトリビアを持つトークンを作成する。
    /// </summary>
    /// <param name="leadingTrivia">新しい先行トリビア。</param>
    /// <param name="trailingTrivia">新しい後続トリビア。</param>
    /// <returns>新しい InternalToken。</returns>
    public InternalToken WithTrivia(IReadOnlyList<InternalTrivia>? leadingTrivia, IReadOnlyList<InternalTrivia>? trailingTrivia)
    {
        return new InternalToken(
            this.Kind,
            this.Text,
            leadingTrivia ?? this.LeadingTrivia,
            trailingTrivia ?? this.TrailingTrivia,
            this._isMissing,
            this.ContainsDiagnostics);
    }

    /// <inheritdoc />
    public override InternalNode? GetSlot(int index)
    {
        // トークンは子ノードを持たない
        return null;
    }

    /// <inheritdoc />
    public override string ToFullString()
    {
        if (this.LeadingTrivia.Count == 0 && this.TrailingTrivia.Count == 0)
        {
            return this.Text;
        }

        var builder = new StringBuilder(this.FullWidth);

        foreach (var trivia in this.LeadingTrivia)
        {
            builder.Append(trivia.Text);
        }

        builder.Append(this.Text);

        foreach (var trivia in this.TrailingTrivia)
        {
            builder.Append(trivia.Text);
        }

        return builder.ToString();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var missing = this._isMissing ? " (missing)" : string.Empty;
        return $"{this.Kind}: \"{EscapeText(this.Text)}\"{missing} [{this.FullWidth}]";
    }

    private static string EscapeText(string text)
    {
#if NETSTANDARD
        return text
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t");
#else
        return text
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\t", "\\t", StringComparison.Ordinal);
#endif
    }
}
