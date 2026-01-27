
using System;
using System.Collections.Generic;
using System.Linq;

using AsciiSharp.Diagnostics;
using AsciiSharp.InternalSyntax;
using AsciiSharp.Parser;
using AsciiSharp.Text;

namespace AsciiSharp.Syntax;
/// <summary>
/// AsciiDoc 文書の構文木を表すクラス。
/// </summary>
public sealed class SyntaxTree
{
    private readonly InternalNode _internalRoot;
    private readonly Diagnostic[] _diagnostics;

    /// <summary>
    /// ソーステキスト。
    /// </summary>
    public SourceText Text { get; }

    /// <summary>
    /// ファイルパス（オプション）。
    /// </summary>
    public string? FilePath { get; }

    /// <summary>
    /// 構文木のルートノード。
    /// </summary>
    public SyntaxNode Root
    {
        get
        {
            field ??= CreateRootNode(this._internalRoot, this);

            return field;
        }
    }

    /// <summary>
    /// 診断情報のリスト。
    /// </summary>
    public IReadOnlyList<Diagnostic> Diagnostics => this._diagnostics;

    /// <summary>
    /// 構文エラーがあるかどうか。
    /// </summary>
    public bool HasErrors
    {
        get
        {
            foreach (var diagnostic in this._diagnostics)
            {
                if (diagnostic.Severity == DiagnosticSeverity.Error)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// 内部ルートノード、ソーステキスト、診断情報から SyntaxTree を作成する。
    /// </summary>
    /// <param name="internalRoot">内部ルートノード。</param>
    /// <param name="text">ソーステキスト。</param>
    /// <param name="diagnostics">診断情報のリスト。</param>
    /// <param name="filePath">ファイルパス（オプション）。</param>
    internal SyntaxTree(
        InternalNode internalRoot,
        SourceText text,
        IEnumerable<Diagnostic> diagnostics,
        string? filePath = null)
    {
        ArgumentNullException.ThrowIfNull(internalRoot);
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(diagnostics);

        this._internalRoot = internalRoot;
        this.Text = text;
        this._diagnostics = diagnostics.ToArray();
        this.FilePath = filePath;
    }

    /// <summary>
    /// 文字列から構文木を解析する。
    /// </summary>
    /// <param name="text">ソーステキスト。</param>
    /// <param name="filePath">ファイルパス（オプション）。</param>
    /// <returns>解析された構文木。</returns>
    public static SyntaxTree ParseText(string text, string? filePath = null)
    {
        ArgumentNullException.ThrowIfNull(text);

        return ParseText(SourceText.From(text), filePath);
    }

    /// <summary>
    /// SourceText から構文木を解析する。
    /// </summary>
    /// <param name="text">ソーステキスト。</param>
    /// <param name="filePath">ファイルパス（オプション）。</param>
    /// <returns>解析された構文木。</returns>
    public static SyntaxTree ParseText(SourceText text, string? filePath = null)
    {
        ArgumentNullException.ThrowIfNull(text);

        var lexer = new Lexer(text);
        var treeBuilder = new InternalTreeBuilder();
        var parser = new AsciiDocParser(lexer, treeBuilder);
        parser.ParseDocument();

        var internalRoot = treeBuilder.BuildRoot();

        return new SyntaxTree(internalRoot, text, parser.Diagnostics, filePath);
    }

    /// <summary>
    /// 指定されたノードの診断情報を取得する。
    /// </summary>
    /// <param name="node">ノード。</param>
    /// <returns>診断情報のシーケンス。</returns>
    public IEnumerable<Diagnostic> GetDiagnosticsForNode(SyntaxNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        var span = node.FullSpan;
        foreach (var diagnostic in this._diagnostics)
        {
            if (span.Contains(diagnostic.Location))
            {
                yield return diagnostic;
            }
        }
    }

    /// <summary>
    /// 変更を適用した新しい構文木を返す。
    /// </summary>
    /// <param name="changes">適用する変更のリスト。</param>
    /// <returns>変更後の新しい構文木。</returns>
    /// <remarks>
    /// <para>増分解析を使用し、変更されていない部分の内部ノードを再利用する。</para>
    /// </remarks>
    public SyntaxTree WithChanges(IEnumerable<TextChange> changes)
    {
        ArgumentNullException.ThrowIfNull(changes);

        var changeList = changes as IReadOnlyList<TextChange> ?? [.. changes];

        // 変更がない場合は同じツリーを返す
        if (changeList.Count == 0)
        {
            return this;
        }

        var newText = this.Text.WithChanges(changeList);

        // 増分解析を使用
        var incrementalParser = new IncrementalParser(this, newText, changeList);
        return incrementalParser.Parse();
    }

    /// <summary>
    /// 単一の変更を適用した新しい構文木を返す。
    /// </summary>
    /// <param name="change">適用する変更。</param>
    /// <returns>変更後の新しい構文木。</returns>
    public SyntaxTree WithChanges(TextChange change)
    {
        return this.WithChanges([change]);
    }

    /// <summary>
    /// 新しいルートノードを持つ構文木を作成する。
    /// </summary>
    /// <param name="root">新しいルートノード。</param>
    /// <returns>新しい構文木。</returns>
    public SyntaxTree WithRootAndOptions(SyntaxNode root)
    {
        ArgumentNullException.ThrowIfNull(root);

        return new SyntaxTree(root.Internal, this.Text, this._diagnostics, this.FilePath);
    }

    /// <summary>
    /// 構文木から元のテキストを復元する。
    /// </summary>
    /// <returns>構文木から復元されたテキスト。</returns>
    /// <remarks>
    /// このメソッドは構文木のノードからテキストを再構築し、BOM があった場合は BOM も含めて返す。
    /// </remarks>
    public string ToOriginalString()
    {
        var fullString = this.Root.ToFullString();

        if (this.Text.HasBom)
        {
            return SourceText.ByteOrderMark + fullString;
        }

        return fullString;
    }

    /// <summary>
    /// 内部ルートノードから外部ルートノードを作成する。
    /// </summary>
    /// <param name="internalRoot">内部ルートノード。</param>
    /// <param name="tree">構文木。</param>
    /// <returns>外部ルートノード。</returns>
    private static SyntaxNode CreateRootNode(InternalNode internalRoot, SyntaxTree tree)
    {
        if (internalRoot.Kind == SyntaxKind.Document)
        {
            return new DocumentSyntax(internalRoot, null, 0, tree);
        }

        // フォールバック: プレースホルダーを使用
        return new PlaceholderSyntax(internalRoot, null, 0, tree);
    }

    /// <summary>
    /// プレースホルダー構文ノード（DocumentSyntax 実装前の一時的なクラス）。
    /// </summary>
    private sealed class PlaceholderSyntax : SyntaxNode
    {
        public PlaceholderSyntax(InternalNode internalNode, SyntaxNode? parent, int position, SyntaxTree tree)
            : base(internalNode, parent, position, tree)
        {
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            for (var i = 0; i < this.Internal.SlotCount; i++)
            {
                var child = this.Internal.GetSlot(i);
                if (child is InternalToken token)
                {
                    yield return new SyntaxToken(token, this, this.CalculateChildPosition(i), i);
                }
                else if (child is not null)
                {
                    yield return new PlaceholderSyntax(child, this, this.CalculateChildPosition(i), this.SyntaxTree!);
                }
            }
        }

        protected override SyntaxNode ReplaceNodeCore(SyntaxNode oldNode, SyntaxNode newNode)
        {
            // 置換ロジックは後で実装
            throw new NotImplementedException("ノードの置換は未実装です。");
        }

        public override void Accept(ISyntaxVisitor visitor)
        {
            // PlaceholderSyntax は内部実装用の一時的なクラスであり、Visitor パターンでの訪問は想定されていない
            throw new NotSupportedException("PlaceholderSyntax は Visitor パターンをサポートしません。");
        }

        public override TResult Accept<TResult>(ISyntaxVisitor<TResult> visitor)
        {
            // PlaceholderSyntax は内部実装用の一時的なクラスであり、Visitor パターンでの訪問は想定されていない
            throw new NotSupportedException("PlaceholderSyntax は Visitor パターンをサポートしません。");
        }

        private int CalculateChildPosition(int index)
        {
            var position = this.Position;
            for (var i = 0; i < index; i++)
            {
                var child = this.Internal.GetSlot(i);
                if (child is not null)
                {
                    position += child.FullWidth;
                }
            }

            return position;
        }
    }
}
