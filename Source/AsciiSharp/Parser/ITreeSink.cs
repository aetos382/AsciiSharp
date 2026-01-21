namespace AsciiSharp.Parser;

/// <summary>
/// パーサーからのイベントを受け取り、構文木を構築するためのインターフェース。
/// </summary>
/// <remarks>
/// <para>このインターフェースは rust-analyzer の Rowan ライブラリのパターンに従い、</para>
/// <para>パーサー本体とツリー構築を分離することで、テスト容易性と柔軟性を確保する。</para>
/// </remarks>
public interface ITreeSink
{
    /// <summary>
    /// 新しいノードの開始を通知する。
    /// </summary>
    /// <param name="kind">ノードの種別。</param>
    void StartNode(SyntaxKind kind);

    /// <summary>
    /// 現在のノードの終了を通知する。
    /// </summary>
    void FinishNode();

    /// <summary>
    /// トークンを追加する。
    /// </summary>
    /// <param name="kind">トークンの種別。</param>
    /// <param name="text">トークンのテキスト。</param>
    void Token(SyntaxKind kind, string text);

    /// <summary>
    /// 先行トリビアを追加する。
    /// </summary>
    /// <param name="kind">トリビアの種別。</param>
    /// <param name="text">トリビアのテキスト。</param>
    void LeadingTrivia(SyntaxKind kind, string text);

    /// <summary>
    /// 後続トリビアを追加する。
    /// </summary>
    /// <param name="kind">トリビアの種別。</param>
    /// <param name="text">トリビアのテキスト。</param>
    void TrailingTrivia(SyntaxKind kind, string text);

    /// <summary>
    /// エラーを報告する。
    /// </summary>
    /// <param name="code">エラーコード。</param>
    /// <param name="message">エラーメッセージ。</param>
    void Error(string code, string message);

    /// <summary>
    /// 欠落トークンを追加する（エラー回復用）。
    /// </summary>
    /// <param name="kind">期待されるトークンの種別。</param>
    void MissingToken(SyntaxKind kind);

    /// <summary>
    /// トリビア付きトークンを追加する。
    /// </summary>
    /// <param name="token">追加するトークン。</param>
    void EmitToken(AsciiSharp.InternalSyntax.InternalToken token);
}
