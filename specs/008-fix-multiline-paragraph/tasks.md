# Tasks: 複数行パラグラフの SyntaxTree 修正

**Input**: Design documents from `/specs/008-fix-multiline-paragraph/`
**Prerequisites**: [plan.md](plan.md) (completed), [spec.md](spec.md) (user stories)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2)
- Include exact file paths in descriptions

---

## Phase 1: Setup（BDD Red 確認）

**Purpose**: フィーチャー クラスと失敗するステップ スタブを作成し、BDD Red 状態を確認する

- [x] T001 LightBDD フィーチャー クラスとステップ スタブを作成して BDD Red を確認する（`Test/AsciiSharp.Specs/Features/MultipleLinesParagraphFeature.cs`、`Test/AsciiSharp.Specs/Features/MultipleLinesParagraphFeature.Steps.cs`）

---

## Phase 2: Foundational（パーサー修正）

**Purpose**: US1・US2 共通の基盤となる `ParseInlineText()` を修正する

**⚠️ CRITICAL**: US1・US2 の BDD Green ステップ実装はこのフェーズ完了後に行う

- [x] T002 `ParseInlineText()` を複数行対応に修正する（`Source/AsciiSharp/Parser/Parser.cs`）
  - 連続するプレーンテキスト行を1つの `InlineTextSyntax` にまとめる
  - 中間行の改行はコンテンツトークンとして出力する
  - 最終行の改行のみトリビアとして付与する（`ParseSectionTitle()` パターンを踏襲）
  - `ParseParagraph()` との改行消費フラグ（`newLineConsumed`）の調整を行う
- [x] T003 ビルドとテストを実行してリグレッションを確認する（`dotnet build`、`dotnet test`）

**Checkpoint**: パーサー修正完了 — US1・US2 の BDD Green ステップ実装を開始できる

---

## Phase 3: User Story 1 - 複数行パラグラフの正しい解析 (Priority: P1) 🎯 MVP

**Goal**: 複数行にわたるパラグラフが単一の `InlineTextSyntax` ノードとして解析され、テキストが改行で結合されて取得できる

**Independent Test**: BDD シナリオ「複数行パラグラフが単一のInlineTextSyntaxノードとして解析される」「複数行InlineTextSyntaxのテキストが改行で結合される」が Green になること

### Implementation for User Story 1

- [x] T004 [P] [US1] `AsgConverter.VisitInlineText()` で改行文字を `\n` に正規化する（`Source/AsciiSharp.Asg/AsgConverter.cs`）
- [x] T005 [US1] BDD Green — 共通ステップ（`パーサーが初期化されている`・`複数行にわたる以下のパラグラフがある`・`文書を解析する`）を実装する（`Test/AsciiSharp.Specs/Features/MultipleLinesParagraphFeature.Steps.cs`）
- [x] T006 [US1] BDD Green — US1 検証ステップ（`パラグラフのインライン要素が_N個である`・`最初のインライン要素がInlineTextSyntaxである`・`InlineTextSyntaxのTextが`）を実装する（`Test/AsciiSharp.Specs/Features/MultipleLinesParagraphFeature.Steps.cs`）

**Checkpoint**: シナリオ 1・2 が Green — User Story 1 MVP 達成

---

## Phase 4: User Story 2 - パラグラフ位置情報の正確な計算 (Priority: P1)

**Goal**: パラグラフの `Span.End` が改行文字を含まない正確な位置を指すように修正される（パーサー修正 T002 の効果として実現される）

**Independent Test**: BDD シナリオ「複数行InlineTextSyntaxのSpanが最終行の改行を含まない」「単一行パラグラフのSpanが行末の改行を含まない」が Green になること

### Implementation for User Story 2

- [x] T007 [US2] 以下の複数パラグラフ文書がある ステップを実装する（`Test/AsciiSharp.Specs/Features/MultipleLinesParagraphFeature.Steps.cs`）
- [x] T008 [US2] BDD Green — US2 検証ステップ（`InlineTextSyntaxのSpanEndが最終行末尾コンテンツの次の位置である`・`最初のパラグラフのSpanが改行を含まない`・`最後のパラグラフのSpanが改行を含まない`）を実装する（`Test/AsciiSharp.Specs/Features/MultipleLinesParagraphFeature.Steps.cs`）

**Checkpoint**: 全 4 シナリオが Green — User Story 1・2 ともに達成

---

## Phase 5: Regression & Validation

**Purpose**: 既存テストのリグレッション修正と TCK 検証

- [x] T009 [P] 既存の段落関連テストを確認し、`InlineElements` 構造の変更に追従して修正する（`Test/AsciiSharp.Tests/`）
- [x] T010 TCK を実行して SC-001〜SC-006 をすべて確認する（`docker buildx bake tck && docker run --rm asciisharp-tck`）

---

## Phase 6: Refactor & Polish

**Purpose**: 警告ゼロポリシーの適用とコード整理

- [x] T011 ビルド警告を解消する（`Source/AsciiSharp/Parser/Parser.cs`、`Source/AsciiSharp.Asg/AsgConverter.cs`）
- [x] T012 修正箇所のコードコメント・XML ドキュメントを日本語で整備する（`Source/AsciiSharp/Parser/Parser.cs`）
- [x] T013 最終ビルドとテストを実行して警告ゼロを確認する（`dotnet build`、`dotnet test`）

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: 完了済み — BDD Red 確認済み
- **Foundational (Phase 2)**: T002（パーサー修正）は US1・US2 の BDD Green を BLOCK する
- **US1 (Phase 3)**: T002 完了後に開始可能。T004 と T005〜T006 は並行実行可能（異なるファイル）
- **US2 (Phase 4)**: T002 完了後に開始可能。US1 と実質的に独立（Steps.cs は同一ファイルのため順次処理推奨）
- **Regression (Phase 5)**: Phase 3・4 完了後に実行する
- **Refactor (Phase 6)**: Phase 5 完了後に実行する

### User Story Dependencies

- **US1 (P1)**: Foundational（T002）完了後 — 独立してテスト可能
- **US2 (P1)**: Foundational（T002）完了後 — US1 と独立してテスト可能（同じパーサー修正が基盤）

### Parallel Opportunities

- T004（AsgConverter 修正）は T002（Parser 修正）と同時進行可能（異なるファイル）
- Phase 3 の T004 と T005〜T006 は並行実行可能

---

## Parallel Example: Phase 2-3

```bash
# Phase 2（T002）と Phase 3 初期（T004）を並行実行:
Task A: "ParseInlineText() を複数行対応に修正する" (Source/AsciiSharp/Parser/Parser.cs)
Task B: "AsgConverter.VisitInlineText() で改行正規化" (Source/AsciiSharp.Asg/AsgConverter.cs)
```

---

## Implementation Strategy

### MVP First (User Story 1 のみ)

1. Phase 2: T002 — パーサー修正（複数行を1つの `InlineTextSyntax` にまとめる）
2. Phase 3: T004〜T006 — AsgConverter 修正 + BDD Green（シナリオ 1・2）
3. **STOP and VALIDATE**: BDD シナリオ 1・2 が Green であることを確認
4. TCK `block/paragraph/multiple-lines` を実行して SC-001 を確認

### Incremental Delivery

1. Phase 2: T002 → T003（パーサー基盤修正）→ Phase 3: T004〜T006（US1 Green）→ MVP 達成
2. Phase 4: T007〜T008（US2 Green）→ 位置情報修正確認
3. Phase 5: T009〜T010（リグレッション修正 + TCK 確認）
4. Phase 6: T011〜T013（Refactor・警告ゼロ）

---

## Notes

- [P] tasks = 異なるファイル、依存関係なし（並行実行可能）
- [Story] ラベルはタスクとユーザーストーリーのトレーサビリティを示す
- 各フェーズ完了後に `dotnet build && dotnet test` を実行してリグレッションを確認する
- BDD シナリオ Green 後は必ずコミットする
- T002（パーサー修正）は最重要タスク。`ParseSectionTitle()` パターンを参照すること
- T001 は完了済み（BDD Red 確認済み）
