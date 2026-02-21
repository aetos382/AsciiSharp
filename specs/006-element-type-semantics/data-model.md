# Data Model: インライン要素とブロック要素のセマンティクス定義

## 型階層（変更後）

```
SyntaxNode (abstract)  [private protected コンストラクター]
├── BlockSyntax (abstract)           ← AsciiDoc 仕様の block のみ
│   ├── DocumentSyntax
│   ├── SectionSyntax
│   └── ParagraphSyntax
├── InlineSyntax (abstract)
│   ├── InlineTextSyntax
│   └── LinkSyntax
├── StructuredTriviaSyntax (abstract) ← 新規
└── （直接派生）
    ├── DocumentHeaderSyntax          ← BlockSyntax から変更
    ├── DocumentBodySyntax            ← BlockSyntax から変更
    ├── SectionTitleSyntax            ← BlockSyntax から変更
    ├── AuthorLineSyntax              ← BlockSyntax から変更
    └── AttributeEntrySyntax         ← BlockSyntax から変更
```

## 変更対象クラス一覧

| クラス | 変更前 | 変更後 | ファイル |
|--------|--------|--------|---------|
| `DocumentSyntax` | `BlockSyntax` | `BlockSyntax`（変更なし） | — |
| `SectionSyntax` | `BlockSyntax` | `BlockSyntax`（変更なし） | — |
| `ParagraphSyntax` | `BlockSyntax` | `BlockSyntax`（変更なし） | — |
| `SectionTitleSyntax` | `BlockSyntax` | `SyntaxNode` | `Syntax/SectionTitleSyntax.cs` |
| `DocumentHeaderSyntax` | `BlockSyntax` | `SyntaxNode` | `Syntax/DocumentHeaderSyntax.cs` |
| `DocumentBodySyntax` | `BlockSyntax` | `SyntaxNode` | `Syntax/DocumentBodySyntax.cs` |
| `AuthorLineSyntax` | `BlockSyntax` | `SyntaxNode` | `Syntax/AuthorLineSyntax.cs` |
| `AttributeEntrySyntax` | `BlockSyntax` | `SyntaxNode` | `Syntax/AttributeEntrySyntax.cs` |
| `StructuredTriviaSyntax` | — | `SyntaxNode`（新規） | `Syntax/StructuredTriviaSyntax.cs` |
| `InlineTextSyntax` | `InlineSyntax` | `InlineSyntax`（変更なし） | — |
| `LinkSyntax` | `InlineSyntax` | `InlineSyntax`（変更なし） | — |

## SyntaxKind 変更

| 変更前 | 変更後 | 理由 |
|--------|--------|------|
| `TextSpan = 400` | 削除 | 未使用。今後も Inline 種別追加時は別の値を使用する |
| `InlineText`（暗黙 401） | `InlineText = 401`（明示） | `TextSpan` 削除後も 400 番台を維持するため |

## StructuredTriviaSyntax クラス定義

```csharp
/// <summary>
/// トリビアであるが内部に構文構造を持つノードの抽象基底クラス。
/// </summary>
/// <remarks>
/// <para>ドキュメントの内容には影響しないためトリビアとして扱うが、
/// 複数行にわたる構造を持つため <see cref="SyntaxNode"/> を継承する。</para>
/// <para>具象クラスとして <c>MultilineCommentTriviaSyntax</c> 等が該当するが、
/// 具象クラスの実装はこのクラスのスコープ外とする。</para>
/// </remarks>
public abstract class StructuredTriviaSyntax : SyntaxNode
{
    private protected StructuredTriviaSyntax(
        InternalNode internalNode,
        SyntaxNode? parent,
        int position,
        SyntaxTree? syntaxTree)
        : base(internalNode, parent, position, syntaxTree) { }
}
```

## BlockSyntax XML ドキュメント コメント（更新後）

```csharp
/// <summary>
/// AsciiDoc 言語仕様においてブロックと定義される要素の抽象基底クラス。
/// </summary>
/// <remarks>
/// <para>AsciiDoc 仕様でブロックとされる要素のみが本クラスを継承する。
/// 具体的には <see cref="DocumentSyntax"/>、<see cref="SectionSyntax"/>、
/// <see cref="ParagraphSyntax"/> が該当する。</para>
/// <para>ブロック要素は行単位（linewise）でソーステキスト内に出現し、垂直方向に積み重なる。</para>
/// <para>参照: AsciiDoc 言語仕様 block-element.adoc</para>
/// </remarks>
```

## 各クラスの XML ドキュメント コメント追記内容

各 `SyntaxNode` 直接派生クラスの `<remarks>` に以下を追記:

> AsciiDoc 言語仕様のブロック要素ではないため、`BlockSyntax` を継承しない。

（各クラスの既存 `<remarks>` に `<para>` として追記する形式）
