namespace AsciiSharp;

/// <summary>
/// 構文ノード、トークン、トリビアの種別を表す列挙型。
/// </summary>
public enum SyntaxKind
{
    /// <summary>未定義または不明な種別。</summary>
    None = 0,

    // ========================================
    // 特殊ノード（番号を固定）
    // ========================================

    /// <summary>欠落ノード（エラー回復用）。</summary>
    MissingToken = 1,

    /// <summary>スキップされたトークン（エラー回復用）。</summary>
    SkippedTokensTrivia = 2,

    // ========================================
    // Tokens
    // ========================================

    /// <summary>ファイル終端トークン。</summary>
    EndOfFileToken = 100,

    /// <summary>改行トークン。</summary>
    NewLineToken,

    /// <summary>空白トークン。</summary>
    WhitespaceToken,

    /// <summary>テキストトークン。</summary>
    TextToken,

    /// <summary>等号トークン (=)。</summary>
    EqualsToken,

    /// <summary>コロントークン (:)。</summary>
    ColonToken,

    /// <summary>スラッシュトークン (/)。</summary>
    SlashToken,

    /// <summary>開き角括弧トークン ([)。</summary>
    OpenBracketToken,

    /// <summary>閉じ角括弧トークン (])。</summary>
    CloseBracketToken,

    /// <summary>開き波括弧トークン ({)。</summary>
    OpenBraceToken,

    /// <summary>閉じ波括弧トークン (})。</summary>
    CloseBraceToken,

    /// <summary>ハッシュトークン (#)。</summary>
    HashToken,

    /// <summary>アスタリスクトークン (*)。</summary>
    AsteriskToken,

    /// <summary>アンダースコアトークン (_)。</summary>
    UnderscoreToken,

    /// <summary>バッククォートトークン (`)。</summary>
    BacktickToken,

    /// <summary>ピリオドトークン (.)。</summary>
    DotToken,

    /// <summary>カンマトークン (,)。</summary>
    CommaToken,

    /// <summary>パイプトークン (|)。</summary>
    PipeToken,

    /// <summary>小なりトークン (&lt;)。</summary>
    LessThanToken,

    /// <summary>大なりトークン (&gt;)。</summary>
    GreaterThanToken,

    // ========================================
    // Trivia
    // ========================================

    /// <summary>空白トリビア。</summary>
    WhitespaceTrivia = 200,

    /// <summary>行末トリビア。</summary>
    EndOfLineTrivia,

    /// <summary>単一行コメントトリビア (//)。</summary>
    SingleLineCommentTrivia,

    /// <summary>複数行コメントトリビア (////...////)。</summary>
    MultiLineCommentTrivia,

    // ========================================
    // Nodes (Blocks) - MVP スコープ
    // ========================================

    /// <summary>ドキュメントノード。</summary>
    Document = 300,

    /// <summary>ドキュメントヘッダーノード。</summary>
    DocumentHeader,

    /// <summary>ドキュメントボディノード。</summary>
    DocumentBody,

    /// <summary>セクションノード。</summary>
    Section,

    /// <summary>セクションタイトルノード。</summary>
    SectionTitle,

    /// <summary>段落ノード。</summary>
    Paragraph,

    /// <summary>著者行ノード。</summary>
    AuthorLine,

    // ========================================
    // Nodes (Inlines) - MVP スコープ
    // ========================================

    /// <summary>テキストスパンノード。</summary>
    TextSpan = 400,

    /// <summary>インライン テキストノード。</summary>
    InlineText,

    /// <summary>リンクノード。</summary>
    Link,
}
