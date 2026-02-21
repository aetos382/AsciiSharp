# Feature Specification: インライン要素とブロック要素のセマンティクス定義

**Feature Branch**: `006-element-type-semantics`
**Created**: 2026-02-21
**Status**: Draft
**Input**: User description: "本プロジェクトにおけるインライン要素とブロック要素の意味を整理して明確にする"

## 概要

現在の設計では `BlockSyntax` と `InlineSyntax` の二分法により「すべての構文ノードはブロックかインラインのいずれかである」という前提を置いているが、AsciiDoc 言語仕様における "block" の定義と乖離しており、誤解を招く分類が生じている。

本仕様では以下を行う。

- `BlockSyntax` の適用範囲を AsciiDoc 言語仕様でブロックとされるものに限定する
- ブロックでもインラインでもない構文要素（`SectionTitleSyntax` 等）を `SyntaxNode` 直接派生に変更する
- 内部構造を持つトリビアのための抽象基底クラス `StructuredTriviaSyntax` を導入する

## Clarifications

### Session 2026-02-21

- Q: `StructuredTriviaSyntax` 抽象クラスの名前は何にするか → A: `StructuredTriviaSyntax`
- Q: `SyntaxKind.TextSpan` の enum 値の扱いは → A: 現時点で削除

## 定義

### ブロック要素（BlockSyntax）

AsciiDoc 言語仕様において block として定義される要素を指す。
ブロック要素は行単位（linewise）でソーステキスト内に出現し、垂直方向に積み重なる。

本プロジェクトでは `BlockSyntax` にAsciiDoc 仕様で明示的にブロックとされるもののみを含める。

参照: `submodules/asciidoc-lang/spec/modules/ROOT/pages/block-element.adoc`

### インライン要素（InlineSyntax）

AsciiDoc 言語仕様における inline element に対応する要素を指す。
インライン要素はブロック要素の内容として、テキストのフロー内に出現する。

### ブロックでもインラインでもない構文要素

`SectionTitleSyntax`（heading は section の structural form であり独立したブロックではない）、
`AuthorLineSyntax`、`AttributeEntrySyntax`、`DocumentHeaderSyntax`、`DocumentBodySyntax` は
AsciiDoc 仕様のブロック要素ではない。これらは `SyntaxNode` を直接継承する。

これらに対して公式の分類名や共通の中間基底クラスは設けない。
将来の実装上の必要性が生じた段階で検討する。

### 構造化トリビア（StructuredTriviaSyntax）

トリビアであるが内部に構文構造を持つものの抽象基底クラス。
`////...////` で囲まれた複数行コメントが該当する。
Roslyn の `StructuredTriviaSyntax` に相当する概念。

ドキュメントの内容には影響しないためトリビアとして扱うが、
複数行にわたる構造を持つため `SyntaxNode` を継承する。

## 型階層

### 変更後

```
SyntaxNode (abstract)
├── BlockSyntax (abstract)      ← AsciiDoc 仕様の block のみ
│   ├── DocumentSyntax
│   ├── SectionSyntax
│   └── ParagraphSyntax
├── InlineSyntax (abstract)
│   ├── InlineTextSyntax
│   └── LinkSyntax
├── StructuredTriviaSyntax (abstract) ← 新規
└── （直接派生）
    ├── DocumentHeaderSyntax
    ├── DocumentBodySyntax
    ├── SectionTitleSyntax
    ├── AuthorLineSyntax
    └── AttributeEntrySyntax
```

### 変更前との対照

| クラス | 変更前 | 変更後 | 理由 |
|---|---|---|---|
| `DocumentSyntax` | `BlockSyntax` | `BlockSyntax` | AsciiDoc 仕様でブロック |
| `SectionSyntax` | `BlockSyntax` | `BlockSyntax` | AsciiDoc 仕様でブロック |
| `ParagraphSyntax` | `BlockSyntax` | `BlockSyntax` | AsciiDoc 仕様でブロック |
| `DocumentHeaderSyntax` | `BlockSyntax` | `SyntaxNode` 直接 | AsciiDoc 仕様に登場しない内部概念 |
| `DocumentBodySyntax` | `BlockSyntax` | `SyntaxNode` 直接 | AsciiDoc 仕様に登場しない内部概念 |
| `SectionTitleSyntax` | `BlockSyntax` | `SyntaxNode` 直接 | heading は section の structural form であり独立したブロックではない |
| `AuthorLineSyntax` | `BlockSyntax` | `SyntaxNode` 直接 | AsciiDoc 仕様のブロックではない |
| `AttributeEntrySyntax` | `BlockSyntax` | `SyntaxNode` 直接 | AsciiDoc 仕様のブロックではない |
| `InlineTextSyntax` | `InlineSyntax` | `InlineSyntax` | 変更なし |
| `LinkSyntax` | `InlineSyntax` | `InlineSyntax` | 変更なし |

## User Scenarios & Testing *(mandatory)*

### User Story 1 - BlockSyntax が AsciiDoc 仕様と一致する (Priority: P1)

ライブラリ利用者として、`BlockSyntax` に分類されているノードが
AsciiDoc 言語仕様でブロックとされているものと一致していることを確認できる。

**Why this priority**: `BlockSyntax` の意味が仕様と一致していないと、
LSP 実装やセマンティック解析で誤った判断をする可能性がある。

**Independent Test**: `is BlockSyntax` が `true` を返すのが paragraph、section、document
に対応するノードのみであることを確認する。

**Acceptance Scenarios**:

1. **Given** 構文木に `ParagraphSyntax` が含まれる、**When** `is BlockSyntax` でチェックする、**Then** `true` が返される
2. **Given** 構文木に `SectionSyntax` が含まれる、**When** `is BlockSyntax` でチェックする、**Then** `true` が返される
3. **Given** 構文木に `SectionTitleSyntax` が含まれる、**When** `is BlockSyntax` でチェックする、**Then** `false` が返される
4. **Given** 構文木に `DocumentHeaderSyntax` が含まれる、**When** `is BlockSyntax` でチェックする、**Then** `false` が返される
5. **Given** 構文木に `AuthorLineSyntax` が含まれる、**When** `is BlockSyntax` でチェックする、**Then** `false` が返される
6. **Given** 構文木に `AttributeEntrySyntax` が含まれる、**When** `is BlockSyntax` でチェックする、**Then** `false` が返される

---

### User Story 2 - StructuredTriviaSyntax が SyntaxNode として扱える (Priority: P2)

ライブラリ利用者として、`StructuredTriviaSyntax` が `SyntaxNode` を継承していることを確認できる。

**Why this priority**: 複数行コメント等の構造化トリビアを将来実装する際の基盤となる。

**Independent Test**: `StructuredTriviaSyntax` が `SyntaxNode` を継承しており、
`BlockSyntax` でも `InlineSyntax` でもないことを確認する。

**Acceptance Scenarios**:

1. **Given** `StructuredTriviaSyntax` 抽象クラスが存在する、**When** `is SyntaxNode` でチェックする、**Then** `true` が返される
2. **Given** `StructuredTriviaSyntax` 抽象クラスが存在する、**When** `is BlockSyntax` でチェックする、**Then** `false` が返される

---

### Edge Cases

- `DocumentSyntax` は AsciiDoc 仕様で「document はブロックである」と明記されているため `BlockSyntax` に残す
- `SectionTitleSyntax` は heading という structural form であり、section ブロックの開始を定義するが、それ自体は独立したブロックではない
- `StructuredTriviaSyntax` の具象クラス（`MultilineCommentTriviaSyntax` 等）の実装は本仕様のスコープ外

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: `BlockSyntax` を継承するクラスは `DocumentSyntax`、`SectionSyntax`、`ParagraphSyntax` のみとしなければならない
- **FR-002**: `DocumentHeaderSyntax`、`DocumentBodySyntax`、`SectionTitleSyntax`、`AuthorLineSyntax`、`AttributeEntrySyntax` は `BlockSyntax` の継承をやめ、`SyntaxNode` を直接継承しなければならない
- **FR-003**: `StructuredTriviaSyntax` 抽象クラスを新規作成し、`SyntaxNode` を継承させなければならない
- **FR-004**: `BlockSyntax` の XML ドキュメントコメントは、AsciiDoc 仕様でブロックとされる要素のみを含む旨を明記しなければならない
- **FR-005**: `SyntaxNode` を直接継承することになった各クラスの XML ドキュメントコメントは、AsciiDoc 仕様のブロックではない旨を明記しなければならない
- **FR-006**: `StructuredTriviaSyntax` の XML ドキュメントコメントは、トリビアであるが内部に構文構造を持つノードの抽象基底クラスであることを明記しなければならない
- **FR-007**: 既存のすべてのビルドおよびテストがパスしなければならない
- **FR-008**: `SyntaxKind.TextSpan` の enum 値を削除しなければならない

### Key Entities

- **BlockSyntax**: AsciiDoc 仕様のブロック要素の抽象基底クラス
- **InlineSyntax**: AsciiDoc 仕様のインライン要素の抽象基底クラス
- **StructuredTriviaSyntax**: 内部構造を持つトリビアの抽象基底クラス。`SyntaxNode` を継承する

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: `is BlockSyntax` が `true` を返すのが `DocumentSyntax`、`SectionSyntax`、`ParagraphSyntax` のみである
- **SC-002**: `SectionTitleSyntax`、`AuthorLineSyntax`、`AttributeEntrySyntax`、`DocumentHeaderSyntax`、`DocumentBodySyntax` に対して `is BlockSyntax` が `false` を返す
- **SC-003**: `StructuredTriviaSyntax` が `SyntaxNode` を継承している
- **SC-004**: 既存の全てのビルドおよびテストが 100% パスする
- **SC-005**: `BlockSyntax` のコメントに AsciiDoc 仕様との対応が明記されている
- **SC-006**: `SyntaxKind.TextSpan` の enum 値が削除されている

## Assumptions

- AsciiDoc 仕様（`submodules/asciidoc-lang`）における block の定義を基準とする
- `StructuredTriviaSyntax` の具象実装は本仕様のスコープ外とする。基底クラスの定義のみを行う
- `DocumentSyntax` は AsciiDoc 仕様で「document はブロックである」と明記されているため `BlockSyntax` に含める
- `SyntaxNode` を直接継承するクラス群に対して、共通の中間基底クラスは現時点では設けない
- ブロックのコンテント モデルの細分化（compound、simple 等）は本仕様のスコープ外とする
