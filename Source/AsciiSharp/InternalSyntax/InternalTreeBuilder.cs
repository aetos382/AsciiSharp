namespace AsciiSharp.InternalSyntax;

using System;
using System.Collections.Generic;
using AsciiSharp.Diagnostics;
using AsciiSharp.Parser;
using AsciiSharp.Text;

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
    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

    /// <summary>
    /// InternalTreeBuilder を作成する。
    /// </summary>
    public InternalTreeBuilder()
    {
        _frames = new Stack<BuilderFrame>();
        _diagnostics = new List<Diagnostic>();
        _pendingLeadingTrivia = new List<InternalTrivia>();
        _pendingTrailingTrivia = new List<InternalTrivia>();
        _position = 0;
    }

    /// <inheritdoc />
    public void StartNode(SyntaxKind kind)
    {
        var frame = new BuilderFrame(kind);
        _frames.Push(frame);
    }

    /// <inheritdoc />
    public void FinishNode()
    {
        if (_frames.Count == 0)
        {
            throw new InvalidOperationException("StartNode が呼び出されていません。");
        }

        var frame = _frames.Pop();
        var children = frame.Children.ToArray();
        var node = new InternalSyntaxNode(frame.Kind, children);

        if (_frames.Count > 0)
        {
            _frames.Peek().Children.Add(node);
        }
        else
        {
            // ルートノードの場合は特別に保持
            _frames.Push(new BuilderFrame(frame.Kind));
            _frames.Peek().Children.Add(node);
        }
    }

    /// <inheritdoc />
    public void Token(SyntaxKind kind, string text)
    {
        if (text is null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        var leadingTrivia = _pendingLeadingTrivia.Count > 0
            ? _pendingLeadingTrivia.ToArray()
            : Array.Empty<InternalTrivia>();
        _pendingLeadingTrivia.Clear();

        var trailingTrivia = _pendingTrailingTrivia.Count > 0
            ? _pendingTrailingTrivia.ToArray()
            : Array.Empty<InternalTrivia>();
        _pendingTrailingTrivia.Clear();

        var token = new InternalToken(kind, text, leadingTrivia, trailingTrivia);
        _position += token.FullWidth;

        if (_frames.Count > 0)
        {
            _frames.Peek().Children.Add(token);
        }
        else
        {
            throw new InvalidOperationException("Token は StartNode の後に呼び出す必要があります。");
        }
    }

    /// <inheritdoc />
    public void LeadingTrivia(SyntaxKind kind, string text)
    {
        if (text is null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        var trivia = new InternalTrivia(kind, text);
        _pendingLeadingTrivia.Add(trivia);
    }

    /// <inheritdoc />
    public void TrailingTrivia(SyntaxKind kind, string text)
    {
        if (text is null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        var trivia = new InternalTrivia(kind, text);
        _pendingTrailingTrivia.Add(trivia);
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

        var location = new TextSpan(_position, 0);
        var diagnostic = Diagnostic.Error(code, message, location);
        _diagnostics.Add(diagnostic);
    }

    /// <inheritdoc />
    public void MissingToken(SyntaxKind kind)
    {
        var token = InternalToken.Missing(kind);

        if (_frames.Count > 0)
        {
            _frames.Peek().Children.Add(token);
        }
        else
        {
            throw new InvalidOperationException("MissingToken は StartNode の後に呼び出す必要があります。");
        }
    }

    /// <summary>
    /// 構築されたルートノードを取得する。
    /// </summary>
    /// <returns>ルートノード。ノードが構築されていない場合は null。</returns>
    public InternalNode? GetRoot()
    {
        if (_frames.Count == 0)
        {
            return null;
        }

        var frame = _frames.Peek();
        if (frame.Children.Count == 0)
        {
            return null;
        }

        return frame.Children[0];
    }

    /// <summary>
    /// ビルダーをリセットする。
    /// </summary>
    public void Reset()
    {
        _frames.Clear();
        _diagnostics.Clear();
        _pendingLeadingTrivia.Clear();
        _pendingTrailingTrivia.Clear();
        _position = 0;
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
            Kind = kind;
            Children = new List<InternalNode>();
        }
    }
}
