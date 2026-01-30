# Feature Specification: BlockSyntax と InlineSyntax 階層の導入

**Feature Branch**: `001-block-inline-syntax`
**Created**: 2026-01-30
**Status**: Draft
**Input**: User description: "SyntaxNode の構造を見直します。まず、BlockSyntax と InlineSyntax という階層を導入してください。"

## 概要

現在、すべての具象構文ノードクラスは `SyntaxNode` 抽象基底クラスを直接継承している。この仕様では、AsciiDoc の構造的特性を反映するため、ブロックレベル要素とインラインレベル要素を区別する中間抽象クラス `BlockSyntax` と `InlineSyntax` を導入する。

## User Scenarios & Testing

### User Story 1 - 型によるブロック要素の識別 (Priority: P1)

開発者として、構文木を走査する際に、型システムを利用してブロックレベル要素を一括で識別・処理したい。

**Why this priority**: ブロック要素とインライン要素の区別は AsciiDoc の基本的な構造概念であり、LSP 実装やセマンティック解析において最も頻繁に必要となる操作である。

**Independent Test**: `is BlockSyntax` パターンマッチングで段落やセクションなどのブロック要素を識別できることを確認する。

**Acceptance Scenarios**:

1. **Given** 構文木にブロック要素（Paragraph、Section 等）が含まれる、**When** `node is BlockSyntax` でチェックする、**Then** ブロック要素に対して true が返される
2. **Given** 構文木にインライン要素（Text、Link 等）が含まれる、**When** `node is BlockSyntax` でチェックする、**Then** インライン要素に対して false が返される

---

### User Story 2 - 型によるインライン要素の識別 (Priority: P1)

開発者として、構文木を走査する際に、型システムを利用してインラインレベル要素を一括で識別・処理したい。

**Why this priority**: インライン要素の識別は、テキスト装飾やリンクの処理、シンタックスハイライトなど、エディタ機能において重要な操作である。

**Independent Test**: `is InlineSyntax` パターンマッチングでテキストやリンクなどのインライン要素を識別できることを確認する。

**Acceptance Scenarios**:

1. **Given** 構文木にインライン要素（Text、Link 等）が含まれる、**When** `node is InlineSyntax` でチェックする、**Then** インライン要素に対して true が返される
2. **Given** 構文木にブロック要素（Paragraph、Section 等）が含まれる、**When** `node is InlineSyntax` でチェックする、**Then** ブロック要素に対して false が返される

---

### User Story 3 - ジェネリック制約での利用 (Priority: P2)

開発者として、ブロック要素のみを受け付けるメソッドや、インライン要素のみを操作するユーティリティを型安全に定義したい。

**Why this priority**: 型安全な API 設計により、コンパイル時にミスを検出でき、コードの保守性が向上する。

**Independent Test**: `BlockSyntax` や `InlineSyntax` をジェネリック制約として使用するメソッドがコンパイルできることを確認する。

**Acceptance Scenarios**:

1. **Given** `where T : BlockSyntax` 制約を持つジェネリックメソッドがある、**When** `ParagraphSyntax` を型引数として渡す、**Then** コンパイルが成功する
2. **Given** `where T : InlineSyntax` 制約を持つジェネリックメソッドがある、**When** `ParagraphSyntax` を型引数として渡す、**Then** コンパイルエラーが発生する

---

### Edge Cases

- `DocumentSyntax` はルートノードとして特殊だが、ブロック要素として扱う
- `SectionTitleSyntax` はセクションの一部だが、独立したブロック要素として扱う
- 将来的に追加されるノード型は、適切な基底クラスを継承する必要がある

## Requirements

### Functional Requirements

- **FR-001**: `BlockSyntax` 抽象クラスは `SyntaxNode` を継承しなければならない
- **FR-002**: `InlineSyntax` 抽象クラスは `SyntaxNode` を継承しなければならない
- **FR-003**: `BlockSyntax` と `InlineSyntax` はともに抽象クラスであり、直接インスタンス化できない
- **FR-004**: 現在の SyntaxKind.Document (300) から SyntaxKind.AuthorLine までのブロックノード（Document, DocumentHeader, DocumentBody, Section, SectionTitle, Paragraph, AuthorLine）は `BlockSyntax` を継承しなければならない
- **FR-005**: 現在の SyntaxKind.TextSpan (400) から SyntaxKind.Link までのインラインノード（TextSpan, Text, Link）は `InlineSyntax` を継承しなければならない
- **FR-006**: 既存の `SyntaxNode` の public API（プロパティ、メソッド）は変更されない
- **FR-007**: `BlockSyntax` と `InlineSyntax` は、現時点では追加のメンバーを持たない（マーカークラスとして機能する）
- **FR-008**: 既存のテストはすべて引き続きパスしなければならない

### Key Entities

- **SyntaxNode**: 既存の構文ノード抽象基底クラス。今後も全ノードの共通基底として機能する
- **BlockSyntax**: ブロックレベル要素を表す中間抽象クラス。段落、セクション、ドキュメントなどの構造的要素
- **InlineSyntax**: インラインレベル要素を表す中間抽象クラス。テキスト、リンク、書式設定などのフロー内要素

## Success Criteria

### Measurable Outcomes

- **SC-001**: すべての既存のブロックノードクラスが `is BlockSyntax` チェックで true を返す
- **SC-002**: すべての既存のインラインノードクラスが `is InlineSyntax` チェックで true を返す
- **SC-003**: 既存のユニットテストおよび BDD テストが 100% パスする
- **SC-004**: `BlockSyntax` と `InlineSyntax` を使用した型パターンマッチングが正常に動作する

## Assumptions

- `SyntaxToken` および `SyntaxTrivia` は `BlockSyntax` / `InlineSyntax` 階層には含めない（これらはノードではなくトークン/トリビアである）
- 将来の拡張で、`BlockSyntax` または `InlineSyntax` に共通の振る舞いが追加される可能性があるが、本仕様ではマーカークラスとしてのみ定義する
- 継承階層の変更は破壊的変更と見なされないものとする（新しい基底クラスの追加は既存のコードの動作に影響しない）
