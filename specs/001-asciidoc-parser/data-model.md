# Data Model: AsciiDoc パーサー

**Feature**: 001-asciidoc-parser
**Date**: 2026-01-18

## 概要

本データモデルは Roslyn スタイルの赤緑木（Red-Green Tree）アーキテクチャに基づく。内部構造（Green Tree）と外部 API（Red Tree）の二層で構成される。

---

## 1. 基盤型

### 1.1 SyntaxKind（構文種別）

すべてのノードとトークンの種別を識別する列挙型。

```text
SyntaxKind (enum)
├── Tokens
│   ├── EndOfFile
│   ├── NewLine
│   ├── Whitespace
│   ├── Text
│   ├── EqualsToken (=)
│   ├── AsteriskToken (*)
│   ├── UnderscoreToken (_)
│   ├── BacktickToken (`)
│   ├── PipeToken (|)
│   ├── ColonToken (:)
│   ├── OpenBracketToken ([)
│   ├── CloseBracketToken (])
│   └── ...
├── Trivia
│   ├── WhitespaceTrivia
│   ├── EndOfLineTrivia
│   ├── SingleLineCommentTrivia (//)
│   └── MultiLineCommentTrivia (////...////)
├── Nodes (Blocks)
│   ├── Document
│   ├── DocumentHeader
│   ├── DocumentBody
│   ├── Section
│   ├── SectionTitle
│   ├── Paragraph
│   ├── UnorderedList
│   ├── OrderedList
│   ├── ListItem
│   ├── DescriptionList
│   ├── DelimitedBlock
│   ├── Table
│   ├── TableRow
│   ├── TableCell
│   └── ...
└── Nodes (Inlines)
    ├── TextSpan
    ├── BoldText
    ├── ItalicText
    ├── MonospaceText
    ├── Link
    ├── CrossReference
    ├── InlineMacro
    ├── AttributeReference
    └── ...
```

### 1.2 TextSpan（テキスト範囲）

ソーステキスト内の位置と長さを表す。

```text
TextSpan (struct)
├── Start: int          # 開始位置（Unicode コードポイント単位）
├── Length: int         # 長さ
└── End: int            # 終了位置（計算プロパティ: Start + Length）

検証規則:
- Start >= 0
- Length >= 0
```

### 1.3 Diagnostic（診断情報）

構文エラーや警告を表す。

```text
Diagnostic (class, immutable)
├── Severity: DiagnosticSeverity (Error | Warning | Info)
├── Code: string                  # エラーコード（例: "ASD001"）
├── Message: string               # エラーメッセージ
├── Location: TextSpan            # エラー位置
└── Arguments: object[]?          # メッセージ引数（フォーマット用）

検証規則:
- Code は空でない
- Message は空でない
```

---

## 2. Green Tree（内部構造）

ユーザーからは直接アクセスできない内部実装。

### 2.1 GreenNode（内部ノード基底）

```text
GreenNode (abstract class, immutable)
├── Kind: SyntaxKind              # ノード種別
├── Width: int                    # 幅（バイト単位で内部管理可）
├── FullWidth: int                # トリビア含む全幅
├── SlotCount: int                # 子スロット数
├── IsMissing: bool               # 欠落ノードか
├── ContainsDiagnostics: bool     # 診断情報を含むか
└── GetSlot(index: int): GreenNode?  # 子ノード取得

特性:
- 完全に不変（Immutable）
- 親参照を持たない
- 絶対位置を持たない（幅のみ）
```

### 2.2 GreenToken（内部トークン）

```text
GreenToken (class, sealed, immutable)
├── Kind: SyntaxKind
├── Text: string                  # トークンテキスト
├── Width: int                    # テキスト幅
├── LeadingTriviaWidth: int       # 先行トリビア幅
├── TrailingTriviaWidth: int      # 後続トリビア幅
├── LeadingTrivia: GreenTrivia[]  # 先行トリビア配列
└── TrailingTrivia: GreenTrivia[] # 後続トリビア配列
```

### 2.3 GreenTrivia（内部トリビア）

```text
GreenTrivia (struct, immutable)
├── Kind: SyntaxKind              # トリビア種別
├── Text: string                  # トリビアテキスト
└── Width: int                    # 幅
```

### 2.4 GreenNodeCache（ノードキャッシュ）

同一内容のノードを共有するためのキャッシュ。

```text
GreenNodeCache (class)
├── TryGetNode(kind, children): GreenNode?
└── AddNode(node: GreenNode): void

実装詳細:
- WeakReference を使用してメモリ圧迫を回避
- ハッシュベースの検索
```

---

## 3. Red Tree（外部 API）

ユーザーが操作する公開 API。

### 3.1 SyntaxNode（構文ノード基底）

```text
SyntaxNode (abstract class)
├── [internal] Green: GreenNode   # 対応する Green ノード
├── Parent: SyntaxNode?           # 親ノード（オンデマンド計算）
├── SyntaxTree: SyntaxTree?       # 所属する構文木
├── Kind: SyntaxKind              # ノード種別
├── Span: TextSpan                # 位置（トリビア除く）
├── FullSpan: TextSpan            # 位置（トリビア含む）
├── IsMissing: bool               # 欠落ノードか
├── ContainsDiagnostics: bool     # 診断情報を含むか
├── ChildNodesAndTokens(): IEnumerable<SyntaxNodeOrToken>
├── DescendantNodes(): IEnumerable<SyntaxNode>
├── GetDiagnostics(): IEnumerable<Diagnostic>
├── ToFullString(): string        # トリビア含む完全テキスト
└── ToString(): string            # トリビア除くテキスト

位置計算:
- Position は親からの累積計算で算出
- Span.Start = Position + LeadingTriviaWidth
- FullSpan.Start = Position
```

### 3.2 SyntaxToken（構文トークン）

```text
SyntaxToken (struct)
├── [internal] Green: GreenToken
├── Parent: SyntaxNode?
├── Kind: SyntaxKind
├── Text: string
├── Value: object?                # リテラル値（該当する場合）
├── Span: TextSpan
├── FullSpan: TextSpan
├── LeadingTrivia: SyntaxTriviaList
├── TrailingTrivia: SyntaxTriviaList
├── IsMissing: bool
└── ContainsDiagnostics: bool
```

### 3.3 SyntaxTrivia（構文トリビア）

```text
SyntaxTrivia (struct)
├── Kind: SyntaxKind
├── Token: SyntaxToken            # 所属トークン
├── Span: TextSpan
├── FullSpan: TextSpan
└── ToFullString(): string
```

### 3.4 SyntaxTree（構文木）

```text
SyntaxTree (class)
├── Root: DocumentSyntax          # ルートノード
├── Text: SourceText              # ソーステキスト
├── FilePath: string?             # ファイルパス（オプション）
├── Diagnostics: IEnumerable<Diagnostic>
├── GetRoot(): DocumentSyntax
├── GetText(): SourceText
└── WithChanges(changes): SyntaxTree  # 増分更新
```

---

## 4. AsciiDoc ドメインノード

### 4.1 Document（文書）

```text
DocumentSyntax (class) : SyntaxNode
├── Header: DocumentHeaderSyntax?
├── Body: DocumentBodySyntax
└── EndOfFileToken: SyntaxToken
```

### 4.2 DocumentHeader（文書ヘッダー）

```text
DocumentHeaderSyntax (class) : SyntaxNode
├── Title: SectionTitleSyntax?
├── AuthorLine: AuthorLineSyntax?
├── RevisionLine: RevisionLineSyntax?
└── Attributes: SyntaxList<AttributeEntrySyntax>
```

### 4.3 Section（セクション）

```text
SectionSyntax (class) : BlockSyntax
├── Level: int                    # 0-5
├── Title: SectionTitleSyntax
├── Content: SyntaxList<BlockSyntax>
└── Subsections: SyntaxList<SectionSyntax>
```

### 4.4 Paragraph（段落）

```text
ParagraphSyntax (class) : BlockSyntax
├── Inlines: SyntaxList<InlineSyntax>
└── TrailingBlankLines: int       # メタ情報として
```

### 4.5 List（リスト）

```text
UnorderedListSyntax (class) : BlockSyntax
└── Items: SyntaxList<ListItemSyntax>

OrderedListSyntax (class) : BlockSyntax
└── Items: SyntaxList<ListItemSyntax>

ListItemSyntax (class) : SyntaxNode
├── Marker: SyntaxToken           # *, -, .など
├── Content: SyntaxList<BlockSyntax>
└── NestedList: ListSyntax?
```

### 4.6 DelimitedBlock（区切りブロック）

```text
DelimitedBlockSyntax (class) : BlockSyntax
├── BlockStyle: SyntaxKind        # Listing, Example, Sidebar, etc.
├── OpenDelimiter: SyntaxToken
├── Content: SyntaxList<SyntaxNode>
└── CloseDelimiter: SyntaxToken   # IsMissing の場合あり
```

### 4.7 Table（テーブル）

```text
TableSyntax (class) : BlockSyntax
├── OpenDelimiter: SyntaxToken    # |===
├── Rows: SyntaxList<TableRowSyntax>
└── CloseDelimiter: SyntaxToken

TableRowSyntax (class) : SyntaxNode
└── Cells: SyntaxList<TableCellSyntax>

TableCellSyntax (class) : SyntaxNode
├── Separator: SyntaxToken        # |
├── Content: SyntaxList<InlineSyntax>
├── ColSpan: int?
└── RowSpan: int?
```

### 4.8 Inline Elements（インライン要素）

```text
InlineSyntax (abstract class) : SyntaxNode

TextSyntax (class) : InlineSyntax
└── Text: SyntaxToken

FormattedTextSyntax (class) : InlineSyntax
├── Style: SyntaxKind             # Bold, Italic, Monospace
├── OpenMarker: SyntaxToken
├── Content: SyntaxList<InlineSyntax>
└── CloseMarker: SyntaxToken

LinkSyntax (class) : InlineSyntax
├── Scheme: SyntaxToken?          # https:, mailto:, etc.
├── Target: SyntaxToken
├── OpenBracket: SyntaxToken
├── Text: SyntaxList<InlineSyntax>?
└── CloseBracket: SyntaxToken

MacroSyntax (class) : InlineSyntax
├── Name: SyntaxToken
├── Colon: SyntaxToken
├── Target: SyntaxToken?
├── OpenBracket: SyntaxToken
├── Attributes: AttributeListSyntax?
└── CloseBracket: SyntaxToken

AttributeReferenceSyntax (class) : InlineSyntax
├── OpenBrace: SyntaxToken        # {
├── Name: SyntaxToken
└── CloseBrace: SyntaxToken       # }
```

---

## 5. ヘルパー型

### 5.1 SyntaxList<T>

```text
SyntaxList<T> (struct) where T : SyntaxNode
├── Count: int
├── this[int index]: T
├── GetEnumerator(): Enumerator
└── Any(): bool

特性:
- イミュータブル
- 値型（スタック割り当て）
```

### 5.2 SyntaxNodeOrToken

```text
SyntaxNodeOrToken (struct)
├── IsNode: bool
├── IsToken: bool
├── AsNode(): SyntaxNode?
└── AsToken(): SyntaxToken
```

### 5.3 SourceText

```text
SourceText (abstract class)
├── Length: int
├── this[int index]: char
├── GetText(TextSpan): string
├── GetLineAndColumn(position): (int line, int column)
└── WithChanges(changes): SourceText

実装:
- StringText: 文字列ベース
- LargeText: 大きなテキスト用（チャンク分割）
```

---

## 6. 状態遷移

### 6.1 パース処理フロー

```text
[SourceText]
    ↓ Lexer
[Token Stream]
    ↓ Parser
[Green Tree]
    ↓ SyntaxTree.Create
[SyntaxTree + Red Tree Root]
```

### 6.2 増分更新フロー

```text
[Existing SyntaxTree] + [TextChange]
    ↓ IncrementalParser
[New Green Tree] (構造共有)
    ↓ SyntaxTree.WithChanges
[New SyntaxTree] (新しい Red Tree Root)
```

---

## 7. 検証規則まとめ

| エンティティ | 規則 |
|-------------|------|
| TextSpan | Start >= 0, Length >= 0 |
| GreenNode | Width >= 0, SlotCount >= 0 |
| SyntaxKind | 有効な列挙値のみ |
| Diagnostic | Code と Message は空でない |
| SectionSyntax | Level は 0-5 の範囲 |
| DelimitedBlockSyntax | OpenDelimiter は IsMissing = false |
