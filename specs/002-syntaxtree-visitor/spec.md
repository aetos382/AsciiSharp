# Feature Specification: SyntaxTree Visitor パターン

**Feature Branch**: `002-syntaxtree-visitor`
**Created**: 2026-01-26
**Status**: Draft
**Input**: User description: "VisitorパターンによってSyntaxTreeを処理する仕組みを作る"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - 構文木の走査 (Priority: P1)

ライブラリ利用者が、AsciiDoc 文書の構文木を走査し、各ノードを訪問して処理を行いたい。例えば、文書内のすべてのリンクを抽出したり、すべてのセクションタイトルを収集したりする。

**Why this priority**: 構文木を走査して情報を抽出することは、AsciiDoc パーサーの最も基本的なユースケースである。LSP 実装やドキュメント分析ツールにおいて必須の機能。

**Independent Test**: 構文木を走査してすべての特定種別のノードを収集するテストを実行し、期待される結果が得られることを確認できる。

**Acceptance Scenarios**:

1. **Given** AsciiDoc 文書の構文木がある, **When** Visitor で全ノードを走査する, **Then** すべてのノードが訪問される
2. **Given** リンクを含む AsciiDoc 文書がある, **When** Visitor でリンクノードのみを処理する, **Then** すべてのリンクが収集される
3. **Given** 複数セクションを持つ文書がある, **When** Visitor でセクションタイトルを収集する, **Then** すべてのセクションタイトルが順番に取得される

---

### User Story 2 - 結果を返す走査 (Priority: P2)

ライブラリ利用者が、構文木を走査しながら各ノードから値を計算し、最終的な結果を返したい。例えば、文書の見出し階層を解析して目次データ構造を構築する。

**Why this priority**: 情報を単に収集するだけでなく、構造化されたデータに変換する処理は、ドキュメント変換や分析において重要な機能。

**Independent Test**: 構文木から目次構造を生成するテストを実行し、正しい階層構造が得られることを確認できる。

**Acceptance Scenarios**:

1. **Given** 階層化されたセクションを持つ文書がある, **When** 結果を返す Visitor で目次を生成する, **Then** セクション階層を反映した目次データが返される
2. **Given** インライン要素を含む段落がある, **When** 結果を返す Visitor でテキストを抽出する, **Then** プレーンテキストが返される

---

### User Story 3 - 構文木の変換 (Priority: P3)

ライブラリ利用者が、構文木を変換して新しい構文木を生成したい。例えば、特定のノードを別のノードに置換したり、ノードを削除したりする。

**Why this priority**: 構文木の変換は、リファクタリングツールやドキュメント変換ツールにおいて必要な機能だが、読み取りのみのユースケースより優先度は低い。

**Independent Test**: 構文木内のリンクテキストを変換するテストを実行し、新しい構文木が正しく生成されることを確認できる。

**Acceptance Scenarios**:

1. **Given** リンクを含む文書がある, **When** Rewriter でリンクの URL を変換する, **Then** 新しい構文木が生成され、元の構文木は変更されない
2. **Given** 複数のセクションを持つ文書がある, **When** Rewriter で特定のセクションを削除する, **Then** そのセクションを含まない新しい構文木が返される

---

### Edge Cases

- 空の文書（ノードがない）を走査した場合、エラーなく完了する
- ネストが深い構文木（多くのレベルの入れ子）を走査した場合、スタックオーバーフローが発生しない
- Visitor のメソッドで例外が発生した場合、その例外が適切に伝播する
- 欠落ノード（IsMissing = true）を含む構文木を走査した場合、そのノードも訪問される

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: システムは、構文木のすべてのノードを深さ優先順序で訪問できる Visitor を提供しなければならない
- **FR-002**: システムは、戻り値なしの Visitor（SyntaxVisitor）を提供しなければならない
- **FR-003**: システムは、戻り値ありの Visitor（SyntaxVisitor&lt;TResult&gt;）を提供しなければならない
- **FR-004**: システムは、構文木を変換する Rewriter（SyntaxRewriter）を提供しなければならない
- **FR-005**: Visitor は各具象ノード型（DocumentSyntax, SectionSyntax など）に対応する Visit メソッドを持たなければならない
- **FR-006**: Visitor は SyntaxToken を訪問できなければならない
- **FR-007**: Visitor は SyntaxTrivia を訪問できなければならない
- **FR-008**: 利用者は Visit メソッドをオーバーライドして特定のノード型の処理をカスタマイズできなければならない
- **FR-009**: デフォルトの Visit メソッドは子ノードを再帰的に訪問しなければならない
- **FR-010**: SyntaxRewriter は、ノードを変換して新しいイミュータブルな構文木を返さなければならない

### Key Entities

- **SyntaxVisitor**: 戻り値なしで構文木を走査する抽象基底クラス
- **SyntaxVisitor&lt;TResult&gt;**: 走査結果を返す抽象基底クラス
- **SyntaxRewriter**: 構文木を変換して新しい構文木を返すクラス（SyntaxVisitor&lt;SyntaxNode?&gt; から派生）

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: すべての既存 SyntaxNode 派生クラスに対応する Visit メソッドが提供される
- **SC-002**: 1000 ノードを持つ構文木を走査できる
- **SC-003**: Visitor を使用して文書内のすべてのリンクを抽出する処理を実装できる
- **SC-004**: SyntaxRewriter を使用してノードを置換した新しい構文木を生成できる
- **SC-005**: .NET Standard 2.0 と .NET 10.0 の両方で同等に動作する

## Assumptions

- Roslyn の CSharpSyntaxVisitor / CSharpSyntaxRewriter パターンに従う
- Visit メソッドは仮想メソッドとし、派生クラスでオーバーライド可能とする
- 構文木はイミュータブルであり、Rewriter は常に新しい構文木を返す
- トリビアの訪問はオプショナルとし、デフォルトでは訪問しない
