
using System;
using System.Collections.Generic;

using AsciiSharp.Diagnostics;
using AsciiSharp.Parser;
using AsciiSharp.Text;

namespace AsciiSharp.InternalSyntax;
/// <summary>
/// ITreeSink を実装し、内部構文木を構築するクラス。
/// </summary>
/// <remarks>
/// <para>パーサーからのイベントを受け取り、ボトムアップで内部構文木を構築する。</para>
/// </remarks>
internal sealed class InternalTreeBuilder : ITreeSink
{
    private readonly Stack<BuilderFrame> _frames;
    private readonly List<Diagnostic> _diagnostics;
    private readonly List<InternalTrivia> _pendingLeadingTrivia;
    private readonly List<InternalTrivia> _pendingTrailingTrivia;
    private int _position;

    /// <summary>
    /// 構築された診断情報のリスト。
    /// </summary>
    public IReadOnlyList<Diagnostic> Diagnostics => this._diagnostics;

    /// <summary>
    /// InternalTreeBuilder を作成する。
    /// </summary>
    public InternalTreeBuilder()
    {
        this._frames = new Stack<BuilderFrame>();
        this._diagnostics = new List<Diagnostic>();
        this._pendingLeadingTrivia = new List<InternalTrivia>();
        this._pendingTrailingTrivia = new List<InternalTrivia>();
        this._position = 0;
    }

    /// <inheritdoc />
    public void StartNode(SyntaxKind kind)
    {
        var frame = new BuilderFrame(kind);
        this._frames.Push(frame);
    }

    /// <inheritdoc />
    public void FinishNode()
    {
        if (this._frames.Count == 0)
        {
            throw new InvalidOperationException("StartNode が呼び出されていません。");
        }

        var frame = this._frames.Pop();
        var children = frame.Children.ToArray();
        var node = new InternalSyntaxNode(frame.Kind, children);

        if (this._frames.Count > 0)
        {
            this._frames.Peek().Children.Add(node);
        }
        else
        {
            // ルートノードの場合は特別に保持
            this._frames.Push(new BuilderFrame(frame.Kind));
            this._frames.Peek().Children.Add(node);
        }
    }

    /// <inheritdoc />
    public void Token(SyntaxKind kind, string text)
    {
        
<<<<<<< TODO: Unmerged change from project 'AsciiSharp(netstandard2.0)', Before:
        var leadingTrivia = _pendingLeadingTrivia.Count > 0
            ? _pendingLeadingTrivia.ToArray()
=======
        var leadingTrivia = this._pendingLeadingTrivia.Count > 0
            ? [.. this._pendingLeadingTrivia]
>>>>>>> After

<<<<<<< TODO: Unmerged change from project 'AsciiSharp(netstandard2.0)', Before:
        _pendingLeadingTrivia.Clear();
=======
        this._pendingLeadingTrivia.Clear();
>>>>>>> After

<<<<<<< TODO: Unmerged change from project 'AsciiSharp(netstandard2.0)', Before:
        var trailingTrivia = _pendingTrailingTrivia.Count > 0
            ? _pendingTrailingTrivia.ToArray()
=======
        var trailingTrivia = this._pendingTrailingTrivia.Count > 0
            ? [.. this._pendingTrailingTrivia]
>>>>>>> After

<<<<<<< TODO: Unmerged change from project 'AsciiSharp(netstandard2.0)', Before:
        _pendingTrailingTrivia.Clear();
=======
        this._pendingTrailingTrivia.Clear();
>>>>>>> After
ArgumentNullException.ThrowIfNull(text);

        var leadingTrivia = this._pendingLeadingTrivia.Count > 0
            ? [.. this._pendingLeadingTrivia]
            : Array.Empty<InternalTrivia>();

<<<<<<< TODO: Unmerged change from project 'AsciiSharp(netstandard2.0)', Before:
        _position += token.FullWidth;
=======
        this._position += token.FullWidth;
>>>>>>> After
        this._pendingLeadingTrivia.Clear();

        var trailingTrivia = this._pendingTrailingTrivia.Count > 0
            ? [.. this._pendingTrailingTrivia]
            : Array.Empty<InternalTrivia>();
        this._pendingTrailingTrivia.Clear();

        var token = new InternalToken(kind, text, leadingTrivia, trailingTrivia);
        this._position += token.FullWidth;

        if (this._frames.Count > 0)
        {
            this._frames.Peek().Children.Add(token);
        }
        else
        {
            throw new InvalidOperationException("Token は StartNode の後に呼び出す必要があります。");
        }
    }

    /// <inheritdoc />
    public void LeadingTrivia(SyntaxKind kind, string text)
    {
        
<<<<<<< TODO: Unmerged change from project 'AsciiSharp(netstandard2.0)', Before:
        _pendingLeadingTrivia.Add(trivia);
=======
        this._pendingLeadingTrivia.Add(trivia);
>>>>>>> After
ArgumentNullException.ThrowIfNull(text);

        var trivia = new InternalTrivia(kind, text);
        this._pendingLeadingTrivia.Add(trivia);
    }

    /// <inheritdoc />
    public void TrailingTrivia(SyntaxKind kind, string text)
    {
        
<<<<<<< TODO: Unmerged change from project 'AsciiSharp(netstandard2.0)', Before:
        _pendingTrailingTrivia.Add(trivia);
=======
        this._pendingTrailingTrivia.Add(trivia);
>>>>>>> After
ArgumentNullException.ThrowIfNull(text);

        var trivia = new InternalTrivia(kind, text);
        this._pendingTrailingTrivia.Add(trivia);
    }

    /// <inheritdoc />
    public void Error(string code, string message)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("エラーコードは空にできません。", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("エラーメッセージは空にできません。", nameof(message));
        }

        var location = new TextSpan(this._position, 0);
        var diagnostic = Diagnostic.Error(code, message, location);
        this._diagnostics.Add(diagnostic);
    }

    /// <inheritdoc />
    public void MissingToken(SyntaxKind kind)
    {
        var token = InternalToken.Missing(kind);

        if (this._frames.Count > 0)
        {
            this._frames.Peek().Children.Add(token);
        }
        else
        {
            throw new InvalidOperationException("MissingToken は StartNode の後に呼び出す必要があります。");
        }
    }

    /// <inheritdoc />
    public void EmitToken(InternalToken token)
    {
        ArgumentNullException.ThrowIfNull(token);

        this._position += token.FullWidth;

        if (this._frames.Count > 0)
        {
            this._frames.Peek().Children.Add(token);
        }
        else
        {
            throw new InvalidOperationException("EmitToken は StartNode の後に呼び出す必要があります。");
        }
    }

    /// <summary>
    /// 構築されたルートノードを取得する。
    /// </summary>
    /// <returns>ルートノード。ノードが構築されていない場合は null。</returns>
    public InternalNode? GetRoot()
    {
        if (this._frames.Count == 0)
        {
            return null;
        }

        var frame = this._frames.Peek();
        if (frame.Children.Count == 0)
        {
            return null;
        }

        return frame.Children[0];
    }

    /// <summary>
    /// 構築されたルートノードを取得する（非 null）。
    /// </summary>
    /// <returns>ルートノード。</returns>
    /// <exception cref="InvalidOperationException">ルートノードが構築されていない場合。</exception>
    public InternalNode BuildRoot()
    {
        var root = this.GetRoot();
        if (root is null)
        {
            throw new InvalidOperationException("ルートノードが構築されていません。ParseDocument を呼び出してください。");
        }

        return root;
    }

    /// <summary>
    /// ビルダーをリセットする。
    /// </summary>
    public void Reset()
    {
        this._frames.Clear();
        this._diagnostics.Clear();
        this._pendingLeadingTrivia.Clear();
        this._pendingTrailingTrivia.Clear();
        this._position = 0;
    }

    /// <summary>
    /// ノード構築中のフレーム。
    /// </summary>
    private sealed class BuilderFrame
    {
        public SyntaxKind Kind { get; }
        public List<InternalNode> Children { get; }

        public BuilderFrame(SyntaxKind kind)
        {
            this.Kind = kind;
            this.Children = new List<InternalNode>();
        }
    }
}
