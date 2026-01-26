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

### Edge Cases

- 空の文書（ノードがない）を走査した場合、エラーなく完了する
- ネストが深い構文木（多くのレベルの入れ子）を走査した場合、スタックオーバーフローが発生しない
- Visitor のメソッドで例外が発生した場合、その例外が適切に伝播する
- 欠落ノード（IsMissing = true）を含む構文木を走査した場合、そのノードも訪問される

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: システムは、構文木のノードを訪問できる Visitor インターフェイスを提供しなければならない
- **FR-002**: システムは、戻り値なしの Visitor インターフェイス（ISyntaxVisitor）を提供しなければならない
- **FR-003**: システムは、戻り値ありの Visitor インターフェイス（ISyntaxVisitor&lt;TResult&gt;）を提供しなければならない
- **FR-004**: Visitor インターフェイスは各具象ノード型（DocumentSyntax, SectionSyntax など）に対応する VisitXxx メソッドを持たなければならない
- **FR-005**: 各 SyntaxNode 派生クラスは Accept メソッドを持ち、適切な VisitXxx メソッドを呼び出さなければならない

### Key Entities

- **ISyntaxVisitor**: 戻り値なしで構文木を走査するインターフェイス
- **ISyntaxVisitor&lt;TResult&gt;**: 走査結果を返すインターフェイス
- **SyntaxNode.Accept**: 各ノード型が実装するダブルディスパッチ用メソッド

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: すべての既存 SyntaxNode 派生クラス（8 種）に対応する VisitXxx メソッドがインターフェイスで定義される
- **SC-002**: すべての既存 SyntaxNode 派生クラス（8 種）に Accept メソッドが追加される
- **SC-003**: Visitor を使用して文書内のすべてのリンクを抽出する処理を実装できる
- **SC-004**: .NET Standard 2.0 と .NET 10.0 の両方で同等に動作する

## Assumptions

- インターフェイスのみを提供し、デフォルト実装クラスは提供しない
- Visit / DefaultVisit のような汎用メソッドは設けない
- SyntaxToken / SyntaxTrivia は構造体であり、Visitor の対象外とする
- 子ノードの走査ロジックは利用者の実装に委ねる
