# Feature Specification: 複数行パラグラフの SyntaxTree 修正

**Feature Branch**: `008-fix-multiline-paragraph`
**Created**: 2026-02-24
**Status**: Draft
**Input**: User description: "GitHub issues の tck bug の中から１つ選んで、それをパスするための修正プランを立てて"

## 背景

TCK テスト `block/paragraph/multiple-lines` が失敗している。また、関連する `block/paragraph/sibling-paragraphs` および `block/paragraph/paragraph-empty-lines-paragraph` も失敗している。

ASG はできるだけ SyntaxTree の構造を素直に反映させる方針をとる。複数の `InlineTextSyntax` ノードを ASG 側でマージするのではなく、SyntaxTree（パーサー）側を正しい構造に修正する。

### 対象 TCK テスト

| テスト名 | 状態 | 関連 Issue |
|---------|------|-----------|
| `block/paragraph/multiple-lines` | FAIL | #153, #143 |
| `block/paragraph/sibling-paragraphs` | FAIL | #154, #145 |
| `block/paragraph/paragraph-empty-lines-paragraph` | FAIL | #155, #144 |

### 問題の概要

複数行パラグラフを AsciiDoc パーサーで解析した際に、以下の2つの問題がある。

**問題 1: 複数行テキストが別々の `InlineTextSyntax` ノードに分割される**

入力:
```asciidoc
This paragraph has multiple lines that wrap after reaching the 72
character limit set by the text editor.
```

現在の SyntaxTree 構造（不正）:
```
ParagraphSyntax
  InlineTextSyntax  ← 1行目のみ
  NewLineToken
  InlineTextSyntax  ← 2行目のみ
  NewLineToken
```

期待される SyntaxTree 構造:
```
ParagraphSyntax
  InlineTextSyntax  ← 複数行を1つのノードとして保持（中間行の改行はコンテンツ、最終行の改行はトリビア）
```

**問題 2: パラグラフの `location.end` が 1 列多い**

行末の改行が `ParagraphSyntax.Span` に含まれているため、`location.end.col` が実際のコンテンツ末尾より1多くなっている。

### 根本原因の分析

1. **`ParseParagraph()` の問題**: 段落内の各行のテキストを1行ごとに別々の `InlineText` ノードとして生成している。プレーンテキストの連続行は1つの `InlineTextSyntax` にまとめるべきである。

2. **`ParseInlineText()` の問題**: 行末の改行を通常のトークンとして `ParseParagraph()` 側で処理しているため、改行が `ParagraphSyntax.Span` に含まれてしまう。最終行の改行のみ、最後のコンテンツトークンのトリビアとして付与する必要がある（`ParseSectionTitle()` と同じパターン）。

## User Scenarios & Testing *(mandatory)*

### User Story 1 - 複数行パラグラフの正しい ASG 出力 (Priority: P1)

複数行にわたるパラグラフを含む AsciiDoc 文書を解析したとき、改行で連結された単一のテキストインラインノードとして ASG に出力される。

**Why this priority**: TCK の準拠テストに直接影響する最も重要な修正であり、他のパラグラフテストの基盤となる。

**Independent Test**: TCK テスト `block/paragraph/multiple-lines` を実行し、期待される JSON 出力と一致することで独立してテスト可能。

**Acceptance Scenarios**:

1. **Given** 2行にわたるパラグラフを含む AsciiDoc 文書が与えられる, **When** ASG に変換する, **Then** `inlines` に1つの `text` ノードが含まれ、その `value` は `\n` で2行を結合した文字列である
2. **Given** 2行にわたるパラグラフを含む AsciiDoc 文書が与えられる, **When** ASG に変換する, **Then** `text` ノードの `location` は1行目の先頭から2行目の末尾まで（改行文字を除く）
3. **Given** 2行にわたるパラグラフを含む AsciiDoc 文書が与えられる, **When** ASG に変換する, **Then** パラグラフの `location.end.col` が末尾文字の列番号と一致する（改行を含まない）

---

### User Story 2 - パラグラフの位置情報の正確な計算 (Priority: P1)

パラグラフの末尾位置が改行文字を除いたコンテンツ終端を正確に指すように計算される。

**Why this priority**: 複数行テストと同様に重要で、`sibling-paragraphs` および `paragraph-empty-lines-paragraph` テストにも影響する。

**Independent Test**: 単一行・複数パラグラフを含む文書で、各パラグラフの `location.end.col` が最後のコンテンツ文字の列番号と一致することを確認する。

**Acceptance Scenarios**:

1. **Given** 38文字のパラグラフがある, **When** ASG に変換する, **Then** パラグラフの `location.end.col` は `38` である（`39` ではない）
2. **Given** 複数のパラグラフを持つ文書がある, **When** ASG に変換する, **Then** 各パラグラフの `location.end` は末尾コンテンツ文字の位置である
3. **Given** 複数のパラグラフを持つ文書がある, **When** ASG に変換する, **Then** ドキュメントの `location.end` は最後のパラグラフの最終コンテンツ文字の位置である

---

### Edge Cases

- 3行以上にわたるパラグラフが正しく1つの `InlineTextSyntax` にまとめられる
- リンクを含む行が出現した時点でプレーンテキストのまとまりが分断される（リンクは別のノード種別のため境界になる）
- 空白のみからなる行は段落終端として解釈される（同一 `InlineTextSyntax` に取り込まない）
- 文末に空行なしで文書が終わる場合の位置計算

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: パーサーは段落内の連続するプレーンテキスト行（リンクを含まない）を、行をまたいで1つの `InlineTextSyntax` ノードとして生成しなければならない
- **FR-002**: 複数行にわたる `InlineTextSyntax` では、中間行の改行トークンはコンテンツとして内部に保持し、最終行の改行のみ最後のコンテンツトークンのトリビアとして付与する
- **FR-003**: 複数行にわたる `InlineTextSyntax` の `Span.End` は、最終行の末尾コンテンツ文字の次の位置を指さなければならない（最終行の改行を含まない）
- **FR-004**: ASG 変換時、`InlineTextSyntax` の内容を `AsgText.value` に変換する際、改行文字は `\n` に正規化する（これは ASG に許容される変形である）
- **FR-005**: ASG は `InlineTextSyntax` ノードを1対1で `AsgText` に変換する。複数ノードのマージは行わない

### Key Entities

- **`InlineTextSyntax`**: パラグラフ内のプレーンテキスト部分を表す構文ノード。リンク等の特殊要素を含まない連続するテキスト行全体を1つのノードとして表現する。中間行の改行はコンテンツトークンとして、最終行の改行はトリビアとして内部に保持される。
- **`ParagraphSyntax`**: 段落全体を表す構文ノード。`InlineElements` には `InlineTextSyntax`（プレーンテキスト連続行）とリンク等の要素が含まれる。
- **`AsgText`**: ASG のテキストインラインノード。`InlineTextSyntax` の内容をもとに生成し、改行文字を `\n` に正規化した `value` を持つ。

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: TCK テスト `block/paragraph/multiple-lines` がパスする
- **SC-002**: TCK テスト `block/paragraph/sibling-paragraphs` がパスする
- **SC-003**: TCK テスト `block/paragraph/paragraph-empty-lines-paragraph` がパスする
- **SC-004**: 現在パスしている TCK テスト `block/paragraph/single-line` が引き続きパスする（リグレッションなし）
- **SC-005**: 既存のユニットテストがすべて引き続きパスする（リグレッションなし）
- **SC-006**: 現在パスしている TCK テスト `block/document/body-only` が引き続きパスする

## Assumptions

- 段落内でリンクを含む行が出現した場合、その前後でプレーンテキストの `InlineTextSyntax` は分断される（今回の修正で同一ノードにまとめるのはプレーンテキスト連続行のみ）
- 3種類のパラグラフ TCK テストを同時に対象にすることで、変更の影響範囲を包括的に検証できる
- 既存の `ParseSectionTitle()` での「最後のコンテンツトークンに改行をトリビアとして付与する」パターンを踏襲する
