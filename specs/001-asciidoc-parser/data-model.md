# Data Model: AsciiDoc パーサー

**Feature**: 001-asciidoc-parser
**Date**: 2026-01-18
**Updated**: 2026-01-20 (MVP スコープ縮小)

## 概要

本データモデルは Roslyn スタイルの二層構文木アーキテクチャに基づく。内部構文木（InternalSyntax）と外部構文木（Syntax）の二層で構成される。Roslyn では Red-Green Tree と呼ばれるパターンである。

**MVP スコープ**: ドキュメントヘッダー（タイトル、著者行）、セクション、段落、リンク、コメントのみ。リスト、テーブル、書式マークアップ、マクロは後続イテレーションで実装。

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
│   ├── ColonToken (:)
│   ├── SlashToken (/)
│   ├── OpenBracketToken ([)
│   ├── CloseBracketToken (])
│   └── ... (MVP: リンク解析に必要なトークンのみ)
├── Trivia
│   ├── WhitespaceTrivia
│   ├── EndOfLineTrivia
│   ├── SingleLineCommentTrivia (//)
│   └── MultiLineCommentTrivia (////...////)
├── Nodes (Blocks) - MVP スコープ
│   ├── Document
│   ├── DocumentHeader
│   ├── DocumentBody
│   ├── Section
│   ├── SectionTitle
│   ├── Paragraph
│   ├── (UnorderedList)      # 後続イテレーション
│   ├── (OrderedList)        # 後続イテレーション
│   ├── (ListItem)           # 後続イテレーション
│   ├── (DescriptionList)    # 後続イテレーション
│   ├── (DelimitedBlock)     # 後続イテレーション
│   ├── (Table)              # 後続イテレーション
│   ├── (TableRow)           # 後続イテレーション
│   └── (TableCell)          # 後続イテレーション
└── Nodes (Inlines) - MVP スコープ
    ├── TextSpan
    ├── Link                  # MVP
    ├── (BoldText)           # 後続イテレーション
    ├── (ItalicText)         # 後続イテレーション
    ├── (MonospaceText)      # 後続イテレーション
    ├── (CrossReference)     # 後続イテレーション
    ├── (InlineMacro)        # 後続イテレーション
    └── (AttributeReference) # 後続イテレーション
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

## 2. 内部構文木（InternalSyntax）

ユーザーからは直接アクセスできない内部実装。

### 2.1 InternalNode（内部ノード基底）

```text
InternalNode (abstract class, immutable)
├── Kind: SyntaxKind              # ノード種別
├── Width: int                    # 幅（バイト単位で内部管理可）
├── FullWidth: int                # トリビア含む全幅
├── SlotCount: int                # 子スロット数
├── IsMissing: bool               # 欠落ノードか
├── ContainsDiagnostics: bool     # 診断情報を含むか
└── GetSlot(index: int): InternalNode?  # 子ノード取得

特性:
- 完全に不変（Immutable）
- 親参照を持たない
- 絶対位置を持たない（幅のみ）
```

### 2.2 InternalToken（内部トークン）

```text
InternalToken (class, sealed, immutable)
├── Kind: SyntaxKind
├── Text: string                  # トークンテキスト
├── Width: int                    # テキスト幅
├── LeadingTriviaWidth: int       # 先行トリビア幅
├── TrailingTriviaWidth: int      # 後続トリビア幅
├── LeadingTrivia: InternalTrivia[]  # 先行トリビア配列
└── TrailingTrivia: InternalTrivia[] # 後続トリビア配列
```

### 2.3 InternalTrivia（内部トリビア）

```text
InternalTrivia (struct, immutable)
├── Kind: SyntaxKind              # トリビア種別
├── Text: string                  # トリビアテキスト
└── Width: int                    # 幅
```

### 2.4 InternalNodeCache（ノードキャッシュ）

同一内容のノードを共有するためのキャッシュ。

```text
InternalNodeCache (class)
├── TryGetNode(kind, children): InternalNode?
└── AddNode(node: InternalNode): void

実装詳細:
- WeakReference を使用してメモリ圧迫を回避
- ハッシュベースの検索
```

---

## 3. 外部構文木（Syntax）

ユーザーが操作する公開 API。

### 3.1 SyntaxNode（構文ノード基底）

```text
SyntaxNode (abstract class)
├── [internal] Internal: InternalNode   # 対応する内部ノード
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
├── [internal] Internal: InternalToken
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

> **MVP スコープ注記**: 括弧 `()` で囲まれた項目は後続イテレーションで実装

### 4.1 Document（文書）- MVP

```text
DocumentSyntax (class) : SyntaxNode
├── Header: DocumentHeaderSyntax?
├── Body: DocumentBodySyntax
└── EndOfFileToken: SyntaxToken
```

### 4.2 DocumentHeader（文書ヘッダー）- MVP

```text
DocumentHeaderSyntax (class) : SyntaxNode
├── Title: SectionTitleSyntax?
├── AuthorLine: AuthorLineSyntax?       # MVP
├── (RevisionLine: RevisionLineSyntax?) # 後続イテレーション
└── (Attributes: SyntaxList<AttributeEntrySyntax>) # 後続イテレーション
```

### 4.3 Section（セクション）- MVP

```text
SectionSyntax (class) : BlockSyntax
├── Level: int                    # 0-5
├── Title: SectionTitleSyntax
├── Content: SyntaxList<BlockSyntax>
└── Subsections: SyntaxList<SectionSyntax>
```

### 4.4 Paragraph（段落）- MVP

```text
ParagraphSyntax (class) : BlockSyntax
├── Inlines: SyntaxList<InlineSyntax>
└── TrailingBlankLines: int       # メタ情報として
```

### 4.5 List（リスト）- 後続イテレーション

> **注**: リストは MVP スコープ外

```text
(UnorderedListSyntax) (class) : BlockSyntax
└── Items: SyntaxList<ListItemSyntax>

(OrderedListSyntax) (class) : BlockSyntax
└── Items: SyntaxList<ListItemSyntax>

(ListItemSyntax) (class) : SyntaxNode
├── Marker: SyntaxToken           # *, -, .など
├── Content: SyntaxList<BlockSyntax>
└── NestedList: ListSyntax?
```

### 4.6 DelimitedBlock（区切りブロック）- 後続イテレーション

> **注**: 区切りブロックは MVP スコープ外

```text
(DelimitedBlockSyntax) (class) : BlockSyntax
├── BlockStyle: SyntaxKind        # Listing, Example, Sidebar, etc.
├── OpenDelimiter: SyntaxToken
├── Content: SyntaxList<SyntaxNode>
└── CloseDelimiter: SyntaxToken   # IsMissing の場合あり
```

### 4.7 Table（テーブル）- 後続イテレーション

> **注**: テーブルは MVP スコープ外

```text
(TableSyntax) (class) : BlockSyntax
├── OpenDelimiter: SyntaxToken    # |===
├── Rows: SyntaxList<TableRowSyntax>
└── CloseDelimiter: SyntaxToken

(TableRowSyntax) (class) : SyntaxNode
└── Cells: SyntaxList<TableCellSyntax>

(TableCellSyntax) (class) : SyntaxNode
├── Separator: SyntaxToken        # |
├── Content: SyntaxList<InlineSyntax>
├── ColSpan: int?
└── RowSpan: int?
```

### 4.8 Inline Elements（インライン要素）

```text
InlineSyntax (abstract class) : SyntaxNode

TextSyntax (class) : InlineSyntax      # MVP
└── Text: SyntaxToken

LinkSyntax (class) : InlineSyntax      # MVP
├── Scheme: SyntaxToken?          # https:, mailto:, etc.
├── Target: SyntaxToken
├── OpenBracket: SyntaxToken
├── Text: SyntaxList<InlineSyntax>?
└── CloseBracket: SyntaxToken

# 以下は後続イテレーションで実装

(FormattedTextSyntax) (class) : InlineSyntax
├── Style: SyntaxKind             # Bold, Italic, Monospace
├── OpenMarker: SyntaxToken
├── Content: SyntaxList<InlineSyntax>
└── CloseMarker: SyntaxToken

(MacroSyntax) (class) : InlineSyntax
├── Name: SyntaxToken
├── Colon: SyntaxToken
├── Target: SyntaxToken?
├── OpenBracket: SyntaxToken
├── Attributes: AttributeListSyntax?
└── CloseBracket: SyntaxToken

(AttributeReferenceSyntax) (class) : InlineSyntax
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
[内部構文木]
    ↓ SyntaxTree.Create
[SyntaxTree + 外部構文木ルート]
```

### 6.2 増分更新フロー

```text
[Existing SyntaxTree] + [TextChange]
    ↓ IncrementalParser
[新しい内部構文木] (構造共有)
    ↓ SyntaxTree.WithChanges
[New SyntaxTree] (新しい外部構文木ルート)
```

---

## 7. 検証規則まとめ

| エンティティ | 規則 |
|-------------|------|
| TextSpan | Start >= 0, Length >= 0 |
| InternalNode | Width >= 0, SlotCount >= 0 |
| SyntaxKind | 有効な列挙値のみ |
| Diagnostic | Code と Message は空でない |
| SectionSyntax | Level は 0-5 の範囲 |
| DelimitedBlockSyntax | OpenDelimiter は IsMissing = false |
