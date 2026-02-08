# Feature Specification: TCK header-body-output テスト対応

**Feature Branch**: `004-tck-header-body`
**Created**: 2026-02-08
**Status**: Draft
**Input**: User description: "TCK の header-body-output が通るようにする"

## Clarifications

### Session 2026-02-08

- Q: 属性エントリ (`:name: value`) を ASG レベルのみで対応するか、SyntaxTree レベルでも対応するか？ → A: SyntaxTree + ASG の両レベルで対応する（方針 B）。パーサーで属性エントリ構文を認識し、構文木ノードとして保持した上で ASG に変換する。

## User Scenarios & Testing *(mandatory)*

### User Story 1 - ヘッダーとボディを持つドキュメントの ASG 変換 (Priority: P1)

AsciiDoc パーサーの利用者（ツール開発者）は、ドキュメント タイトル（ヘッダー）と本文（ボディ）を含む AsciiDoc 文書をパースし、ASG (Abstract Semantic Graph) 形式に変換した結果が、AsciiDoc TCK (Technology Compatibility Kit) の `header-body-output` テスト ケースの期待出力と一致することを求める。

**Why this priority**: TCK 準拠は AsciiDoc パーサーとしての互換性を証明する基盤であり、最も基本的なドキュメント構造（ヘッダー + ボディ）を正しく変換できることが前提条件となる。

**Independent Test**: TCK の `tests/block/document/header-body-input.adoc` を入力として TckAdapter に渡し、出力が `header-body-output.json` と一致することで検証できる。

**Acceptance Scenarios**:

1. **Given** `= Document Title` の後に空行と `body` テキストが続く AsciiDoc 文書、**When** パースして ASG に変換する、**Then** ドキュメント ノードに `header` と `blocks` が含まれる
2. **Given** 同上の文書、**When** ASG に変換する、**Then** `header.title` にはテキスト `"Document Title"` が含まれ、その位置は行1 列3〜行1 列16 である
3. **Given** 同上の文書、**When** ASG に変換する、**Then** `header.location` は行1 列1〜行1 列16 である
4. **Given** 同上の文書、**When** ASG に変換する、**Then** `blocks` には段落ノードが1つ含まれ、そのインライン テキストは `"body"`（行3 列1〜行3 列4）である
5. **Given** 同上の文書、**When** ASG に変換する、**Then** ドキュメント全体の `location` は行1 列1〜行3 列4 である
6. **Given** 同上の文書、**When** ASG に変換する、**Then** ドキュメントの `attributes` は空のオブジェクト `{}` である

---

### User Story 2 - 属性エントリの SyntaxTree レベルでのパース (Priority: P2)

AsciiDoc パーサーの利用者は、ドキュメント ヘッダー内の属性エントリ（`:name: value` 形式）が構文木ノードとして保持されることを期待する。これにより、属性エントリのテキスト情報（コロン、名前、値）が元文書の完全な復元に使用でき、LSP やエディタ連携での補完・診断にも活用できる。

**Why this priority**: SyntaxTree レベルでの属性エントリの認識は、ASG での `attributes` 出力の前提となり、かつ将来のエディタ連携機能の基盤となる。

**Independent Test**: `:icons: font` を含むドキュメントをパースし、構文木に属性エントリ ノードが含まれること、および元文書が完全に復元できることで検証できる。

**Acceptance Scenarios**:

1. **Given** ヘッダー内に `:icons: font` を含む AsciiDoc 文書、**When** パースする、**Then** 構文木のドキュメント ヘッダー ノード内に属性エントリ ノードが存在する
2. **Given** 同上の文書、**When** パースする、**Then** 属性エントリ ノードから属性名 `icons` と属性値 `font` が取得できる
3. **Given** ヘッダー内に `:toc:` を含む AsciiDoc 文書（値なし）、**When** パースする、**Then** 属性エントリ ノードから属性名 `toc` と空の属性値が取得できる
4. **Given** 属性エントリを含む文書、**When** 構文木の全トークンを連結する、**Then** 元のテキストが完全に復元される

---

### User Story 3 - 属性エントリの ASG 変換 (Priority: P3)

AsciiDoc パーサーの利用者は、ドキュメント ヘッダー内の属性エントリが ASG の `attributes` フィールドにキー・値ペアとして出力されることを期待する。

**Why this priority**: ASG 変換は TCK テスト合格の直接的な要件であるが、SyntaxTree レベルのパース（P2）が前提条件となる。

**Independent Test**: `:icons: font` と `:toc:` を含むドキュメントを ASG に変換し、`"attributes": { "icons": "font", "toc": "" }` が出力されることで検証できる。

**Acceptance Scenarios**:

1. **Given** ヘッダー内に `:icons: font` と `:toc:` を含む AsciiDoc 文書、**When** ASG に変換する、**Then** `attributes` は `{ "icons": "font", "toc": "" }` となる
2. **Given** 属性エントリを含まない AsciiDoc 文書、**When** ASG に変換する、**Then** `attributes` は空のオブジェクト `{}` となる

---

### Edge Cases

- ヘッダー行のみでボディが空の場合、`blocks` は空配列 `[]` となるか？
- ボディのみでヘッダーがない場合（`body-only` テスト ケースに該当、本機能のスコープ外）
- ドキュメント タイトルの前後に余分な空白がある場合の位置計算
- 改行コードが CRLF の場合の位置計算への影響
- 属性名にハイフンを含む場合（例: `:my-attr: value`）
- 属性値の前後の空白の扱い（値の前の空白はトリミングされるか？）
- 同じ属性名が複数回定義された場合（後勝ちか？ — 本機能のスコープでは AsciiDoc 仕様の標準に従う）
- 属性エントリがヘッダーではなくボディに記述された場合（本機能のスコープ外。ヘッダー内のみ対象）

## Requirements *(mandatory)*

### Functional Requirements

#### ドキュメント構造

- **FR-001**: パーサーは `= Document Title` 形式のドキュメント ヘッダーを認識し、構文木にドキュメント ヘッダー ノードとして含めなければならない (MUST)
- **FR-002**: ASG コンバーターは、ドキュメント ヘッダーのタイトル テキストを正しく抽出し、`= ` マーカーを除いたテキスト部分のみを `header.title` の `value` として出力しなければならない (MUST)
- **FR-003**: ASG コンバーターは、タイトル テキストの位置情報を正確に計算しなければならない (MUST)。位置は 1 ベースの行番号・列番号で、終端は包含的（最後の文字の位置）とする
- **FR-004**: ASG コンバーターは、ヘッダー全体の位置情報（`= ` マーカーを含む開始位置からヘッダー末尾まで）を `header.location` として出力しなければならない (MUST)
- **FR-005**: ASG コンバーターは、ドキュメント ボディ内の段落をインライン テキスト要素を含む段落ノードとして出力しなければならない (MUST)
- **FR-006**: ASG コンバーターは、段落内のテキストの位置情報を正確に計算しなければならない (MUST)
- **FR-007**: ASG コンバーターは、ドキュメント全体の位置情報（最初の要素の開始位置から最後の要素の終了位置まで）を `location` として出力しなければならない (MUST)

#### 属性エントリ（SyntaxTree レベル）

- **FR-008**: パーサーは、ドキュメント ヘッダー内の `:name: value` 形式の属性エントリ行を認識し、構文木に属性エントリ ノードとして含めなければならない (MUST)
- **FR-009**: 属性エントリ ノードは、属性名と属性値を構造的に保持しなければならない (MUST)。開きコロン、属性名、閉じコロン、空白、値のテキストを個別のトークンとして保持する
- **FR-010**: パーサーは、値のない属性エントリ（`:name:` 形式）も認識し、値が空の属性エントリ ノードとして保持しなければならない (MUST)
- **FR-011**: 属性エントリを含むドキュメントの構文木から、全トークンを連結して元テキストを完全に復元できなければならない (MUST)

#### 属性エントリ（ASG レベル）

- **FR-012**: ASG コンバーターは、ドキュメント ノードに `attributes` フィールドを含めなければならない (MUST)
- **FR-013**: ASG コンバーターは、ドキュメント ヘッダー内の属性エントリを `attributes` フィールドにキー・値ペアとして出力しなければならない (MUST)。値のない属性エントリは空文字列を値とする
- **FR-014**: 属性エントリがない場合、`attributes` は空のオブジェクト `{}` として出力しなければならない (MUST)

### Key Entities

- **ドキュメント (Document)**: AsciiDoc 文書全体を表すルート エンティティ。ヘッダー、ボディ（ブロック群）、属性、位置情報を持つ
- **ヘッダー (Header)**: ドキュメントのタイトル情報と属性エントリを持つエンティティ。タイトル（インライン要素のリスト）、属性エントリのリスト、位置情報を持つ
- **属性エントリ (Attribute Entry)**: `:name: value` 形式で宣言されるドキュメント属性。属性名（文字列）と属性値（文字列、空可）を持つ。ヘッダー内で宣言される
- **段落 (Paragraph)**: ボディ内のテキスト ブロックを表すエンティティ。インライン要素のリストと位置情報を持つ
- **テキスト (Text)**: インライン要素の一種。テキスト値と位置情報を持つ

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: TCK の `tests/block/document/header-body-input.adoc` を入力として TckAdapter を実行した結果が、`header-body-output.json` の期待出力と完全に一致する
- **SC-002**: 既存の全テスト（BDD テスト含む）がパスし続ける（回帰なし）
- **SC-003**: CI 環境での TCK テスト実行において、`header-body-output` テスト ケースが PASS となる
- **SC-004**: 属性エントリを含むドキュメントをパースした際、構文木から元テキストが完全に復元できる

## Assumptions

- 既存のパーサーはドキュメント ヘッダー（`= Title` 形式）と段落テキストのパースを既にサポートしている
- 属性エントリの構文認識は、現時点ではドキュメント ヘッダー内のみを対象とする（ボディ内の属性エントリは将来対応）
- 属性エントリの値に対する属性参照の展開（`{url-org}` のような置換）は本機能のスコープ外とする
- TCK テストの入力・出力形式は `submodules/asciidoc-tck` で定義されたものに従う
- JSON シリアライズの形式（プロパティの順序、空白の扱いなど）が TCK の期待と一致する必要がある
