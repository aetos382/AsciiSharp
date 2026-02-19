# Feature Specification: 行末空白 Trivia の識別と保持

**Feature Branch**: `001-trailing-whitespace-trivia`
**Created**: 2026-02-19
**Status**: Draft
**Input**: 各行の行末の空白文字（改行を含む）は、その他の空白と区別してひとまとめの Trivia として扱うようにする。

## 背景と仕様の摩擦

本機能には、以下の仕様上の摩擦が存在する。

1. **AsciiDoc 正規化仕様との摩擦**: AsciiDoc の正規化仕様では、改行文字を含む行末の連続する空白はパースする前に取り除かれると規定されている。
2. **本ライブラリの目標との摩擦**: 本ライブラリでは、元のバイト列を SyntaxTree から完全に復元可能とすることを目標としている。このため、正規化で取り除かれる行末空白も SyntaxTree に保持する必要がある。

この摩擦に対する本仕様の立場は以下の通りとする。

- SyntaxTree は元のバイト列の完全な表現であり、行末空白を含むすべての Trivia を保持する
- ASG は AsciiDoc 意味論（正規化後の構造）を表現するため、行末空白を無視する
- これにより、ロスレスな元テキスト復元（SyntaxTree レベル）と、正規化に準拠した意味解析（ASG レベル）の両立を図る

## Clarifications

### Session 2026-02-19

- Q: 非改行空白文字のみからなる行（例: `"   \n"`）は、行全体が行末空白 Trivia となるのか、それとも「行頭空白 Trivia + 行末空白 Trivia」に分割されるのか？ → A: 行全体が単一の行末空白 Trivia となる。空白 Trivia の種別は「一般の空白 Trivia」と「行末空白 Trivia」の 2 種類のみとする。なぜなら、行末以外の空白 Trivia は AsciiDoc の正規化の影響を受けないため、特別な区別が不要であるからである。

## User Scenarios & Testing *(mandatory)*

### User Story 1 - 行末空白 Trivia の識別と SyntaxTree への保持 (Priority: P1)

パーサーの利用者（ライブラリ消費者）は、行末に空白文字が含まれるテキストを解析したとき、SyntaxTree において行末の空白文字群（改行文字を含む）が、その他の Trivia とは区別される「行末空白 Trivia」として保持されることを期待する。これにより、ライブラリの利用者は SyntaxTree を解析して行末空白の有無や内容を正確に把握できる。

**Why this priority**: SyntaxTree における Trivia の正確な分類は、この機能の根幹をなす。これが実現されなければ、後続の ASG での無視も元テキストの完全復元も成立しない。

**Independent Test**: 末尾に空白文字を含む AsciiDoc テキストをパースし、SyntaxTree の末尾ノードが「行末空白 Trivia」として識別されることを検証することで、単独でテスト可能。

**Acceptance Scenarios**:

1. **Given** 行末に ASCII 空白（U+0020）と LF（U+000A）を含む行が存在する場合、**When** そのテキストをパースする、**Then** 行末の空白と LF はひとまとめの行末空白 Trivia として SyntaxTree に格納される
2. **Given** 行末に CRLF（U+000D U+000A）のみ存在する行がある場合、**When** そのテキストをパースする、**Then** CRLF は単一の行末空白 Trivia として SyntaxTree に格納される
3. **Given** 行末に空白文字がなく改行文字のみ存在する行がある場合、**When** そのテキストをパースする、**Then** 改行文字のみで構成される行末空白 Trivia が SyntaxTree に格納される
4. **Given** Unicode の非 ASCII 空白文字（例: U+3000 全角スペース）が行末に存在する場合、**When** そのテキストをパースする、**Then** 当該文字も行末空白 Trivia の一部として認識される

---

### User Story 2 - ASG における行末空白の無視 (Priority: P2)

意味解析を行うライブラリ利用者は、ASG（Abstract Semantic Graph）を用いて文書の構造を解析する際、行末空白が要素の意味的な幅や内容に影響を与えないことを期待する。AsciiDoc の正規化仕様に従い、行末空白は可視コンテンツとして扱わず、意味論的には無視する。

**Why this priority**: P1 の SyntaxTree での識別が完了した後に、ASG への影響を定義する必要がある。AsciiDoc 正規化仕様への準拠と意味解析の正確性に直結するため P2 とする。

**Independent Test**: 行末空白あり・なしの同一内容の行を ASG に変換し、両者の要素幅が等しいことを検証することで、単独でテスト可能。

**Acceptance Scenarios**:

1. **Given** 行末に空白文字を含む行が存在する場合、**When** SyntaxTree を ASG に変換する、**Then** ASG における該当要素の幅に行末空白は算入されない
2. **Given** 同一内容で行末空白の有無が異なる 2 つの行がある場合、**When** 両者を ASG に変換する、**Then** 両者の意味的な要素幅は等しい

---

### User Story 3 - 元テキストの完全復元 (Priority: P3)

ロスレスのラウンドトリップを必要とする利用者（例：エディタ、フォーマッタ）は、SyntaxTree からオリジナルのテキストを完全に復元できることを期待する。行末空白 Trivia が SyntaxTree に保持されているため、行末の空白も含めて完全な復元が保証される。

**Why this priority**: 元テキスト復元は SyntaxTree の根本的な品質特性であり、本機能においても維持されなければならない。ただし P1・P2 の実現を前提とするため P3 とする。

**Independent Test**: 行末空白を含む任意のテキストをパースし、SyntaxTree を文字列化したものが元のテキストと完全一致することを検証することで、単独でテスト可能。

**Acceptance Scenarios**:

1. **Given** 行末空白を含む AsciiDoc テキストが存在する場合、**When** パースして SyntaxTree を文字列化する、**Then** 元のテキストと 1 文字も違わず一致する
2. **Given** 複数種類の改行文字（LF、CRLF、NEL 等）が混在するテキストが存在する場合、**When** パースして SyntaxTree を文字列化する、**Then** 各行の改行文字の種類も含めて元のテキストと完全一致する

---

### Edge Cases

- 行末に改行文字が存在しない場合（文書の最終行で改行なし）: 非改行空白文字のみで構成される行末空白 Trivia として扱う
- CRLF シーケンス（U+000D + U+000A）は 1 つの改行として扱い、単一の行末空白 Trivia に含める
- 行末の空白が一切ない場合（行頭から直接改行文字）: 改行文字のみからなる行末空白 Trivia として扱う
- NEL（U+0085）、LS（U+2028）、PS（U+2029）、FF（U+000C）などの非 ASCII 改行文字も改行文字として認識する
- 空行（改行文字のみからなる行）: 改行文字が行末空白 Trivia となる
- 非改行空白文字のみからなる行（例: `"   \n"`）: コンテンツ（非空白文字）が存在しないため「行末」は行頭と一致し、行全体（非改行空白文字 + 改行文字）が単一の行末空白 Trivia となる
- 末尾に空白文字のみがあり改行文字がなく文書が終わる場合: 当該空白文字は行末空白 Trivia として扱う

## Requirements *(mandatory)*

### 機能要件

- **FR-001**: パーサーは各行の末尾にある空白文字群（改行文字を含む）を、他の Trivia 種別とは区別される単一の「行末空白 Trivia」として認識しなければならない
- **FR-009**: 空白 Trivia の種別は「一般の空白 Trivia」と「行末空白 Trivia」の 2 種類のみとする。行末以外の空白 Trivia は AsciiDoc の正規化の影響を受けないため、特別な区別を設けない
- **FR-002**: 「空白文字」とは Unicode の White_Space 特性を持つ文字と定義する（具体的な文字一覧は本仕様末尾の「参照バージョン」を参照）
- **FR-003**: 「改行文字」とは Unicode Core Spec 5.8 Newline Guidelines R4 に列挙された文字と定義する（具体的な文字一覧は本仕様末尾の「参照バージョン」を参照）
- **FR-004**: 行末空白 Trivia は「行末の 0 個以上の非改行空白文字」と「それに続く 1 個の改行文字（またはファイル末尾）」を合わせて 1 つの Trivia として構成する
- **FR-005**: CRLF（U+000D + U+000A）は 1 つの改行文字として扱い、行末空白 Trivia の構成要素として分割しない
- **FR-006**: SyntaxTree は行末空白 Trivia を保持し、SyntaxTree から元のテキストを完全に復元できなければならない
- **FR-007**: ASG への変換において、行末空白 Trivia は無視され、要素の意味的な幅に算入されない
- **FR-008**: 本仕様は AsciiDoc 言語の正式な形式的定義が制定されるまでの暫定仕様であり、正式な定義が策定された場合にはそちらが優先される

### Key Entities

- **一般の空白 Trivia (General Whitespace Trivia)**: 行末以外の位置に出現する空白文字群を表す Trivia ノード。行末空白 Trivia とは区別される別種別。AsciiDoc の正規化の影響を受けないため、さらに細分化しない。SyntaxTree に保持される。
- **行末空白 Trivia (Trailing Whitespace Trivia)**: 行末に出現する空白文字群（非改行空白 + 改行文字、またはそれらの部分集合）をひとまとめに表す Trivia ノード。コンテンツ（非空白文字）のない行では行全体が行末空白 Trivia となる。SyntaxTree に保持され、ASG では無視される。
- **非改行空白文字 (Non-Newline Whitespace)**: White_Space 特性を持つ文字のうち、改行文字に該当しないもの
- **改行文字 (Newline Character)**: Unicode Core Spec 5.8 R4 に定義された文字またはシーケンス（CR、LF、CRLF、NEL、LS、PS、FF）

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 行末空白を含む任意の AsciiDoc テキストをパースして SyntaxTree から文字列化したとき、元のテキストと 100% 一致する（ラウンドトリップ完全性）
- **SC-002**: 定義されたすべての White_Space 文字および改行文字を含むテスト入力に対して、行末空白 Trivia が正しく識別される
- **SC-003**: 行末空白あり・なしの同一内容の行を ASG に変換したとき、両者の意味的な要素幅が等しい
- **SC-004**: 行末空白 Trivia が、一般の空白 Trivia などの他の Trivia 種別と、SyntaxTree 上で明確に区別できる

## Assumptions

- CRLF は常に 1 つの改行として扱う（CR のみ・LF のみは各々別の改行として扱う）
- 行末空白 Trivia の検出は、行単位の走査を前提とする（複数行をまたぐ Trivia は対象外）
- 行末空白 Trivia の「行末」の判定は、改行文字（または文書末尾）によって決定される
- 空白文字のリストは .NET BCL の `MemoryExtensions.SplitAny` で使用される既定のセパレータ文字集合と一致する（参考情報）
- 改行文字のリストは .NET BCL の `MemoryExtensions.EnumerateLines` で使用される行区切り文字集合と一致する（参考情報）

## 参照バージョンと改訂方針

本仕様における空白文字および改行文字の定義は、以下の仕様バージョンに基づく。

**現時点の参照バージョン**: Unicode 17.0

- **空白文字** (White_Space 特性): U+0009〜U+000D、U+0020、U+0085、U+00A0、U+1680、U+2000〜U+200A、U+2028、U+2029、U+202F、U+205F、U+3000
- **改行文字** (Unicode Core Spec 5.8 Newline Guidelines R4): CR (U+000D)、LF (U+000A)、CRLF (U+000D U+000A)、NEL (U+0085)、LS (U+2028)、PS (U+2029)、FF (U+000C)

**改訂方針**: AsciiDoc 言語の正式な形式的言語仕様が策定される前に Unicode の新しいバージョンがリリースされ、空白文字または改行文字の定義が変更された場合、次のリリースからそれに従う。AsciiDoc の正式仕様が策定された場合には、そちらを優先する。
