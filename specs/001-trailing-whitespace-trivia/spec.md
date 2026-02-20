# Feature Specification: 要素境界における行末トリビアの統一

**Feature Branch**: `001-trailing-whitespace-trivia`
**Created**: 2026-02-19
**Revised**: 2026-02-20
**Status**: Draft
**Input**: セクションタイトル・属性エントリ・著者行などの要素終端で、行末の空白文字と改行文字を既存の WhitespaceTrivia および EndOfLineTrivia として後続トリビアに付与し、ASG での要素境界を正確に表現できるようにする。

## 背景と動機

### 現状の問題

セクションタイトルや著者行などの要素は、末尾の `NewLineToken` をトークンとして構文木に含んでいる。このため ASG が要素の終了位置（Span）を求めると、改行文字が含まれてしまい正確な境界を表現できない。

属性エントリ（`:attr: value`）は例外で、すでに Parser が末尾の空白と改行を `WhitespaceTrivia` / `EndOfLineTrivia` として処理している。他の要素でもこれと同じ扱いにする必要がある。

### 本仕様の立場

- SyntaxTree は元のバイト列の完全な表現であり、行末の空白・改行も Trivia として保持する
- ASG は AsciiDoc 意味論を表現するため、要素の Span から行末空白（Trivia）を除外する
- 既存の `WhitespaceTrivia`（SyntaxKind = 200）と `EndOfLineTrivia`（SyntaxKind = 201）を使用する。新しい SyntaxKind は追加しない。

## Clarifications

### Session 2026-02-19

- Q: 非改行空白文字のみからなる行（例: `"   \n"`）は、行全体が行末空白 Trivia となるのか、それとも「行頭空白 Trivia + 行末空白 Trivia」に分割されるのか？ → A: 今回の対象は要素境界のみ。空白のみの行（段落間の空行）は別途検討する。

### Session 2026-02-20

- Q: 新たな SyntaxKind `TrailingWhitespaceTrivia` を追加するか？ → A: 追加しない。既存の `WhitespaceTrivia` と `EndOfLineTrivia` の組み合わせを要素境界で一貫して使用する。
- Q: 段落内の改行や `[...]` 内の改行の扱いは？ → A: 今回のスコープ外。別フィーチャーで対応する。

## User Scenarios & Testing *(mandatory)*

### User Story 1 - ASG での要素終了位置の正確な取得 (Priority: P1)

ASG を使って文書を解析するライブラリ利用者は、セクションタイトルや属性エントリなどの要素の終了位置（Span）を取得したとき、行末の空白や改行を除いた正確な位置が返ることを期待する。

**Why this priority**: ASG での要素境界の正確性は、LSP 実装・補完・コード操作など後続機能の基盤となるため最優先。

**Independent Test**: 行末に空白を含むセクションタイトルをパースし、ASG における SectionTitle の終了位置が、タイトルテキストの最後の文字の直後を指すことを検証する。

**Acceptance Scenarios**:

1. **Given** `== Title   \n` という行が存在する場合、**When** SyntaxTree から ASG に変換する、**Then** SectionTitle の Span の終端は `e` の直後（trailing の空白・改行を含まない）である
2. **Given** `:attr: value   \n` という属性エントリが存在する場合、**When** SyntaxTree から ASG に変換する、**Then** AttributeEntry の Span の終端は `e` の直後である
3. **Given** `Author Name\n` という著者行が存在する場合、**When** SyntaxTree から ASG に変換する、**Then** AuthorLine の Span の終端は `e` の直後である

---

### User Story 2 - SyntaxTree における行末トリビアの識別 (Priority: P2)

ライブラリ利用者は、セクションタイトル・属性エントリ・著者行の最終トークンの後続トリビアを検査することで、行末の空白文字と改行文字を確認できる。

**Why this priority**: SyntaxTree レベルの Trivia アクセスは、フォーマッタや lint ツールが行末空白を検出・修正するために必要。P1 の ASG 実装が前提。

**Independent Test**: 行末に空白を含むセクションタイトルをパースし、タイトルの最終トークンの TrailingTrivia に `WhitespaceTrivia` と `EndOfLineTrivia` が含まれることを検証する。

**Acceptance Scenarios**:

1. **Given** `== Title   \n` という行がある場合、**When** パースする、**Then** SectionTitle の最終コンテンツトークンの後続トリビアに `WhitespaceTrivia("   ")` と `EndOfLineTrivia("\n")` が含まれる
2. **Given** `== Title\n` という行末空白のない行がある場合、**When** パースする、**Then** 最終コンテンツトークンの後続トリビアに `EndOfLineTrivia("\n")` が含まれる（`WhitespaceTrivia` はなし）
3. **Given** `== Title\r\n` という CRLF の行がある場合、**When** パースする、**Then** 後続トリビアに `EndOfLineTrivia("\r\n")` が含まれる

---

### User Story 3 - 元テキストの完全復元 (Priority: P3)

ロスレスのラウンドトリップを必要とする利用者は、SyntaxTree からオリジナルのテキストを完全に復元できることを期待する。

**Why this priority**: ラウンドトリップは SyntaxTree の根本的な品質特性。ただし P1・P2 の実現を前提とするため P3 とする。

**Independent Test**: 行末空白を含む AsciiDoc テキストをパースし、SyntaxTree を文字列化したものが元のテキストと完全一致することを検証する。

**Acceptance Scenarios**:

1. **Given** 行末空白を含む AsciiDoc テキストが存在する場合、**When** パースして SyntaxTree を文字列化する、**Then** 元のテキストと 1 文字も違わず一致する
2. **Given** 複数種類の改行文字（LF、CRLF）が混在するテキストが存在する場合、**When** パースして SyntaxTree を文字列化する、**Then** 各行の改行文字の種類も含めて元のテキストと完全一致する

---

### Edge Cases

- 行末に空白がなく改行文字のみ存在する行: `EndOfLineTrivia("\n")` のみが後続トリビアとなる
- CRLF（U+000D + U+000A）は 1 つの改行として `EndOfLineTrivia("\r\n")` とする
- 文書の最終行で改行なし・末尾空白あり: `WhitespaceTrivia` のみが後続トリビアとなる（`EndOfLineTrivia` なし）
- 文書の最終行で改行なし・末尾空白なし: 後続トリビアなし

## Requirements *(mandatory)*

### 機能要件

- **FR-001**: Parser は、セクションタイトル・属性エントリ・著者行の末尾において、行末の非改行空白トークン（`WhitespaceToken`）を `WhitespaceTrivia` に変換し、最終コンテンツトークンの後続トリビアとして付与しなければならない
- **FR-002**: Parser は、セクションタイトル・属性エントリ・著者行の末尾において、改行トークン（`NewLineToken`）を `EndOfLineTrivia` に変換し、最終コンテンツトークンの後続トリビアとして付与しなければならない
- **FR-003**: CRLF（U+000D + U+000A）は 1 つの `EndOfLineTrivia` として扱い、分割しない
- **FR-004**: SyntaxTree は行末空白・改行を Trivia として保持し、SyntaxTree から元のテキストを完全に復元できなければならない
- **FR-005**: ASG への変換において、後続トリビア（`WhitespaceTrivia`・`EndOfLineTrivia`）は要素の Span に算入しない
- **FR-006**: 新たな SyntaxKind は追加しない。既存の `WhitespaceTrivia`（200）と `EndOfLineTrivia`（201）を使用する
- **FR-007**: 段落内の改行、リンクの `[...]` 内の改行は本仕様のスコープ外とする
- **FR-008**: 本仕様は AsciiDoc 言語の正式な形式的定義が制定されるまでの暫定仕様であり、正式な定義が策定された場合にはそちらが優先される

### Key Entities

- **WhitespaceTrivia** (SyntaxKind = 200): 行末の非改行空白文字群（スペース・タブ等）を表す Trivia。要素境界では最終コンテンツトークンの後続トリビアとして付与される。
- **EndOfLineTrivia** (SyntaxKind = 201): 改行文字（LF・CRLF 等）を表す Trivia。要素境界では最終コンテンツトークンの後続トリビアとして付与される。
- **要素境界**: セクションタイトル（`SectionTitle`）・属性エントリ（`AttributeEntry`）・著者行（`AuthorLine`）の末尾位置。これらは「行末 = 要素の終端」となる構造要素。

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 行末空白を含む任意の AsciiDoc テキストをパースして SyntaxTree から文字列化したとき、元のテキストと 100% 一致する（ラウンドトリップ完全性）
- **SC-002**: セクションタイトル・属性エントリ・著者行の最終コンテンツトークンに、行末空白が `WhitespaceTrivia` と `EndOfLineTrivia` として後続トリビアで付与される
- **SC-003**: 行末空白あり・なしの同一内容の要素を ASG に変換したとき、両者の意味的な要素 Span が等しい
- **SC-004**: 属性エントリ（`ParseAttributeEntry`）・セクションタイトル（`ParseSectionTitle`）・著者行（`ParseAuthorLine`）のすべてで、要素境界の行末トリビア処理が統一されている

## Assumptions

- CRLF は常に 1 つの改行として扱う（CR のみ・LF のみは各々別の改行として扱う）
- 行末の判定は改行文字（または文書末尾）によって決定される
- 段落内の行折り返しと `[...]` 内の改行は別フィーチャーで対応する
