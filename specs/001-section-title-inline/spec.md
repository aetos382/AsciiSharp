# Feature Specification: SectionTitleSyntax の構成改定と TextSyntax のリネーム

**Feature Branch**: `001-section-title-inline`
**Created**: 2026-02-05
**Status**: Draft
**Input**: User description: "SectionTitleSyntax は「= トークン + 空白トリビア + InlineSyntax コレクション」の構成にする。現状の空白区切りによるタイトル分割をやめる。当面は単一の TextSyntax で十分だが将来的にインラインマークアップを含む拡張を見据える。TextSyntax を InlineTextSyntax にリネームし「一行の文字列」という意味を明確にする。"

## Clarifications

### Session 2026-02-05

- Q: `TitleContent` プロパティの廃止について → A: `TitleContent` を削除し、全参照箇所を更新する
- Q: `SyntaxKind.Text` の名前変更について → A: `SyntaxKind.Text` も `SyntaxKind.InlineText` にリネームし、全参照箇所を更新する
- Q: レベル 6 を超える `=` の扱い → A: `=` の数をそのままカウントし、上限を制限しない（Level の上限は別フィーチャーで扱う）

## User Scenarios & Testing *(mandatory)*

### User Story 1 - セクションタイトルの構文木からインライン要素を取得する（P1）

開発者がセクションタイトルを含む AsciiDoc ドキュメントをパースし、タイトルの内容を構成するインライン要素のコレクションとして走査できる。現在は `TitleContent` という単一の文字列プロパティで取得していたが、今後インラインマークアップ（リンクなど）を混在させるために、インライン要素の配列として取得できるようにする。

**Why this priority**: コア ライブラリの構文木の正確さに直結する変更で、LSP やエディタ連携の基盤となる。この変更なしに将来のインラインマークアップ対応が不可能になる。

**Independent Test**: `== タイトルテキスト` という入力をパースし、得られる SectionTitleSyntax のインライン要素コレクションが単一の InlineTextSyntax を含むことで検証できる。

**Acceptance Scenarios**:

1. **Given** `== Hello` という AsciiDoc を入力、**When** パースして SectionTitleSyntax を取得する、**Then** インライン要素コレクションに1つの InlineTextSyntax が含まれ、そのテキスト内容は `Hello` である
2. **Given** `== Hello World` という AsciiDoc を入力（空白を含むタイトル）、**When** パースして SectionTitleSyntax を取得する、**Then** インライン要素コレクションに1つの InlineTextSyntax が含まれ、そのテキスト内容は `Hello World`（空白を含む一つの要素）である
3. **Given** `=== Deep Section` という AsciiDoc を入力、**When** パースして SectionTitleSyntax を取得する、**Then** Level が 3 であり、インライン要素コレクションに1つの InlineTextSyntax が含まれる

---

### User Story 2 - TextSyntax が InlineTextSyntax として参照される（P2）

開発者がインライン要素を扱うコード中で、プレーンテキストを表すクラスを参照する際に、「一行の文字列」という意味が名前から明確に読み取れる。

**Why this priority**: 将来インラインマークアップの種類が増えるにつれ、テキスト要素としての意味が曖昧になりやすい。名前の明確化は開発者体験の向上に貢献する。

**Independent Test**: ParagraphSyntax やコア ライブラリ内でプレーンテキストを表す箇所がすべて `InlineTextSyntax` の型名を用いることで検証できる。

**Acceptance Scenarios**:

1. **Given** 既存コード中の `TextSyntax` への参照がある、**When** リネームを適用する、**Then** すべての参照が `InlineTextSyntax` に更新され、コンパイルが成功する
2. **Given** `ISyntaxVisitor` インターフェースに `VisitText` メソッドがある、**When** リネームを適用する、**Then** メソッド名も `VisitInlineText` へ更新され、すべての実装と呼び出し箇所が一貫する

---

### User Story 3 - SectionTitleSyntax の空白トリビアが適切に保持される（P3）

開発者がセクションタイトルの構文木を再構築したテキストに変換したとき、マーカー（`=`）とタイトル本文の間の空白が元と同じ通り忠実に復元される。

**Why this priority**: 構文木のイミュタビリティと完全な復元可能性はこのプロジェクトの仕様要件であり、空白情報の喪失は仕様違反になる。

**Independent Test**: `==  タイトル`（2つの半角スペース）をパースし、`ToFullString()` で復元したテキストが入力と完全一致することで検証できる。

**Acceptance Scenarios**:

1. **Given** `== タイトル` という入力、**When** パースして `ToFullString()` で復元する、**Then** 出力が入力と完全に一致する
2. **Given** `==タイトル`（空白なし）という入力、**When** パースして SectionTitleSyntax を取得する、**Then** インライン要素コレクションに InlineTextSyntax が含まれ、テキスト復元も一致する

---

### Edge Cases

- マーカー（`=`）のみで本文テキストがない場合（例: `==`）、インライン要素コレクションは空になる
- マーカーと本文の間に空白が複数あある場合、それらすべてがトリビアとして保持される
- レベル 6（`======`）を超える `=` の数の場合、Level はそのまま `=` の数となる（上限は制限しない；上限の強制は別フィーチャーで対応）
- タイトル本文に末尾の空白がある場合：AsciiDoc 正規化仕様に従い、改行前の末尾空白は削除される（参照: https://docs.asciidoctor.org/asciidoc/latest/normalization/）

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: SectionTitleSyntax は「1つ以上の `=` トークン」「0つ以上の空白トリビア」「0つ以上の InlineSyntax のコレクション」の3つの要素で構成される
- **FR-002**: インライン要素コレクションは空白で分割せず、要素の種類ごとに区切られる（現時点では単一の InlineTextSyntax のみ）
- **FR-003**: `TextSyntax` クラスは `InlineTextSyntax` にリネームされ、すべての参照箇所が更新される
- **FR-004**: `ISyntaxVisitor` と `ISyntaxVisitor<TResult>` のメソッド名も `VisitText` から `VisitInlineText` へ更新される
- **FR-005**: リネーム後も ParagraphSyntax やその他のインライン要素を生成する箇所で正しく動作する
- **FR-006**: 構文木の完全な文字列復元（`ToFullString()`）が変更前と同じ結果を返す（イミュタビリティと復元可能性の維持）
- **FR-007**: SectionTitleSyntax の `Level` プロパティは `=` トークンの数のカウントで決まる動作を維持する
- **FR-008**: `TitleContent` プロパティは削除され、タイトル文字列の取得はインライン要素コレクションから行う
- **FR-009**: `TitleContent`・`TitleText` を参照している箇所（テスト StepDefinitions・Asg converter など）も同じフィーチャー内で更新する
- **FR-010**: `SyntaxKind.Text` は `SyntaxKind.InlineText` にリネームされ、すべての参照箇所が更新される

### Key Entities

- **SectionTitleSyntax**: セクションタイトルを表すブロック要素。マーカートークン、トリビア、インライン要素コレクションを保持する。`TitleContent`（文字列）プロパティは削除される
- **InlineTextSyntax**（旧 TextSyntax）: 一行のプレーンテキストを表すインライン要素。インラインマークアップの種類の一つとして扱われる
- **InlineSyntax**: インライン要素の抽象基底クラス。将来的に LinkSyntax やその他のインライン要素の基底となる

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: すべてのビルドが警告ゼロで成功し、既存テスト一貫率が100%を維持する
- **SC-002**: SectionTitleSyntax のインライン要素コレクションが、空白を含むタイトルを1つの要素として正確に表現できる（「Hello World」が2つの要素に分割されないこと）
- **SC-003**: `TextSyntax` への参照がコード全体で0件になり、すべて `InlineTextSyntax` に統一される。同様に `SyntaxKind.Text` への参照も0件になり、`SyntaxKind.InlineText` に統一される
- **SC-004**: 任意の AsciiDoc セクションタイトルについて、パース → `ToFullString()` による復元が入力と完全一致する

## Assumptions

- 現時点のインライン要素は `InlineTextSyntax`（単一種類）のみで十分とし、インラインマークアップの種類を増やすことは別のフィーチャーで対応する
- マーカーとタイトル本文の間の空白はトリビアとして `SyntaxToken` に付随し、別途のノードとして管理しない
- パーサー側（内部構文木の構築）の変更は、外部API（構文木の構成と復元可能性）が維持される限り、実装の詳細として扱う
- `Level` は `=` の数をそのままカウントし、上限を制限しない。レベル上限の強制については別のフィーチャーで対応する
