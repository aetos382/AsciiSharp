
using System;

namespace AsciiSharp.InternalSyntax;
/// <summary>
/// 内部構文木のノードを表す抽象基底クラス。
/// </summary>
/// <remarks>
/// <para>このクラスは Roslyn の Green Tree に相当する。</para>
/// <para>特性:</para>
/// <list type="bullet">
///   <item>完全に不変（Immutable）</item>
///   <item>親参照を持たない</item>
///   <item>絶対位置を持たない（幅のみ）</item>
/// </list>
/// </remarks>
internal abstract class InternalNode
{
    /// <summary>
    /// ノードの種別。
    /// </summary>
    public SyntaxKind Kind { get; }

    /// <summary>
    /// ノードの幅（トリビアを除く）。
    /// </summary>
    public abstract int Width { get; }

    /// <summary>
    /// ノードの全幅（トリビアを含む）。
    /// </summary>
    public abstract int FullWidth { get; }

    /// <summary>
    /// 子スロットの数。
    /// </summary>
    public abstract int SlotCount { get; }

    /// <summary>
    /// このノードが欠落ノード（エラー回復で挿入されたもの）かどうか。
    /// </summary>
    public virtual bool IsMissing => false;

    /// <summary>
    /// このノードまたは子孫に診断情報が含まれるかどうか。
    /// </summary>
    public bool ContainsDiagnostics { get; protected set; }

    /// <summary>
    /// 先行トリビアの幅。
    /// </summary>
    public virtual int LeadingTriviaWidth => 0;

    /// <summary>
    /// 後続トリビアの幅。
    /// </summary>
    public virtual int TrailingTriviaWidth => 0;

    /// <summary>
    /// 指定された種別で InternalNode を作成する。
    /// </summary>
    /// <param name="kind">ノードの種別。</param>
    protected InternalNode(SyntaxKind kind)
    {
        this.Kind = kind;
    }

    /// <summary>
    /// 指定されたインデックスの子スロットを取得する。
    /// </summary>
    /// <param name="index">スロットのインデックス。</param>
    /// <returns>子ノード。存在しない場合は null。</returns>
    public abstract InternalNode? GetSlot(int index);

    /// <summary>
    /// 子ノードのテキストを連結した完全なテキストを取得する。
    /// </summary>
    /// <returns>完全なテキスト。</returns>
    public abstract string ToFullString();

    /// <summary>
    /// トリビアを除いたテキストを取得する。
    /// </summary>
    /// <returns>トリビアを除いたテキスト。</returns>
    public virtual string ToTrimmedString()
    {
        var fullString = this.ToFullString();
        var leadingWidth = this.LeadingTriviaWidth;
        var trailingWidth = this.TrailingTriviaWidth;
        var width = fullString.Length - leadingWidth - trailingWidth;

        if (width <= 0)
        {
            return string.Empty;
        }

        return fullString.Substring(leadingWidth, width);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{this.Kind} [{this.FullWidth}]";
    }
}

/// <summary>
/// 子ノードを持つ内部構文ノード。
/// </summary>
internal sealed class InternalSyntaxNode : InternalNode
{
    private readonly InternalNode?[] _children;
    private readonly int _width;
    private readonly int _fullWidth;
    private readonly int _leadingTriviaWidth;
    private readonly int _trailingTriviaWidth;

    /// <inheritdoc />
    public override int Width => this._width;

    /// <inheritdoc />
    public override int FullWidth => this._fullWidth;

    /// <inheritdoc />
    public override int SlotCount => this._children.Length;

    /// <inheritdoc />
    public override int LeadingTriviaWidth => this._leadingTriviaWidth;

    /// <inheritdoc />
    public override int TrailingTriviaWidth => this._trailingTriviaWidth;

    /// <summary>
    /// 指定された種別と子ノードで InternalSyntaxNode を作成する。
    /// </summary>
    /// <param name="kind">ノードの種別。</param>
    /// <param name="children">子ノードの配列。</param>
    public InternalSyntaxNode(SyntaxKind kind, params InternalNode?[] children)
        : base(kind)
    {
        this._children = children ?? [];

        var fullWidth = 0;
        var containsDiagnostics = false;

        foreach (var child in this._children)
        {
            if (child is not null)
            {
                fullWidth += child.FullWidth;

                if (child.ContainsDiagnostics)
                {
                    containsDiagnostics = true;
                }
            }
        }

        this._fullWidth = fullWidth;
        this.ContainsDiagnostics = containsDiagnostics;

        // 先行トリビア幅は最初の非 null 子ノードの先行トリビア幅
        this._leadingTriviaWidth = 0;
        foreach (var child in this._children)
        {
            if (child is not null)
            {
                this._leadingTriviaWidth = child.LeadingTriviaWidth;
                break;
            }
        }

        // 後続トリビア幅は最後の非 null 子ノードの後続トリビア幅
        this._trailingTriviaWidth = 0;
        for (var i = this._children.Length - 1; i >= 0; i--)
        {
            var child = this._children[i];
            if (child is not null)
            {
                this._trailingTriviaWidth = child.TrailingTriviaWidth;
                break;
            }
        }

        this._width = this._fullWidth - this._leadingTriviaWidth - this._trailingTriviaWidth;
    }

    /// <inheritdoc />
    public override InternalNode? GetSlot(int index)
    {
        if (index < 0 || index >= this._children.Length)
        {
            return null;
        }

        return this._children[index];
    }

    /// <inheritdoc />
    public override string ToFullString()
    {
        if (this._children.Length == 0)
        {
            return string.Empty;
        }

        if (this._children.Length == 1 && this._children[0] is not null)
        {
            return this._children[0]!.ToFullString();
        }

        var builder = new System.Text.StringBuilder(this._fullWidth);
        foreach (var child in this._children)
        {
            if (child is not null)
            {
                builder.Append(child.ToFullString());
            }
        }

        return builder.ToString();
    }
}
