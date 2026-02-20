# Tasks: 要素境界における行末トリビアの統一

**Input**: Design documents from `/specs/001-trailing-whitespace-trivia/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md

**Organization**: タスクはユーザーストーリーごとに整理されています。

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 並列実行可能（異なるファイル、依存関係なし）
- **[Story]**: 対応するユーザーストーリー（US1=ASG Span、US2=Trivia 識別、US3=ラウンドトリップ）
- 各タスクに正確なファイルパスを記載

---

## Phase 1: Setup（共有インフラ）

**Purpose**: 変更対象ファイルの把握と準備

- [ ] T001 `ParseSectionTitle()` の現状と変更方針を確認する: `Source/AsciiSharp/Parser/Parser.cs` L302-357
- [ ] T002 `ParseAuthorLine()` の現状と変更方針を確認する: `Source/AsciiSharp/Parser/Parser.cs` L193-210
- [ ] T003 [P] `ParseAttributeEntry()` の実装を参照パターンとして確認する: `Source/AsciiSharp/Parser/Parser.cs` L629-697

---

## Phase 2: Foundational（ブロッキング前提条件）

**Purpose**: Parser の行末処理変更（US2・US3・US1 すべての前提）

**⚠️ CRITICAL**: このフェーズが完了するまでユーザーストーリーの実装は開始できない

- [ ] T004 `ParseSectionTitle()` を変更し、ループ終了後に末尾 `WhitespaceToken` を `WhitespaceTrivia` に変換して最終コンテンツトークンの trailing に付与する: `Source/AsciiSharp/Parser/Parser.cs`
- [ ] T005 `ParseSectionTitle()` を変更し、`NewLineToken` を `EndOfLineTrivia` に変換して最終コンテンツトークンの trailing に付与する（CRLF は 1 つの EndOfLineTrivia として扱う）: `Source/AsciiSharp/Parser/Parser.cs`
- [ ] T006 `ParseAuthorLine()` を変更し、末尾 `WhitespaceToken` を `WhitespaceTrivia`、`NewLineToken` を `EndOfLineTrivia` に変換して最終コンテンツトークンの trailing に付与する（`ParseSectionTitle()` と同パターン）: `Source/AsciiSharp/Parser/Parser.cs`
- [ ] T007 ビルドとテストを実行し、既存テストがすべて通ることを確認する（ラウンドトリップ系テスト 5 件は通過するはず）

**Checkpoint**: Parser 変更完了 — ユーザーストーリーの実装が可能になる

---

## Phase 3: User Story 2 - SyntaxTree における行末トリビアの識別（Priority: P2）

**Goal**: セクションタイトル・著者行の最終コンテンツトークンの後続トリビアに WhitespaceTrivia / EndOfLineTrivia が格納されることを BDD で検証する

**Independent Test**: `dotnet test --project Test/AsciiSharp.Specs --filter "TrailingWhitespaceFeature"` を実行し、すべてのシナリオが通ること（Inconclusive ゼロ）

### Implementation for User Story 2

- [ ] T008 [US2] BDD ステップ `セクションタイトルの最終コンテンツトークンの後続トリビアにWhitespaceTriviaとEndOfLineTriviaが含まれる` を実装する（SectionTitleSyntax の最終コンテンツトークンを取得し trailing trivia を検証）: `Test/AsciiSharp.Specs/Features/TrailingWhitespaceFeature.Steps.cs`
- [ ] T009 [US2] BDD ステップ `セクションタイトルの最終コンテンツトークンの後続トリビアにEndOfLineTriviaのみが含まれる` を実装する（WhitespaceTrivia が含まれないことを検証）: `Test/AsciiSharp.Specs/Features/TrailingWhitespaceFeature.Steps.cs`
- [ ] T010 [US2] BDD ステップ `セクションタイトルの最終コンテンツトークンの後続トリビアにCRLFを含むEndOfLineTriviaが含まれる` を実装する（EndOfLineTrivia のテキストが `"\r\n"` であることを検証）: `Test/AsciiSharp.Specs/Features/TrailingWhitespaceFeature.Steps.cs`
- [ ] T011 [US2] BDD ステップ `著者行の最終コンテンツトークンの後続トリビアにWhitespaceTriviaとEndOfLineTriviaが含まれる` を実装する（AuthorLineSyntax の最終コンテンツトークンを取得し trailing trivia を検証）: `Test/AsciiSharp.Specs/Features/TrailingWhitespaceFeature.Steps.cs`
- [ ] T012 [US2] ビルドとテストを実行し、US2 のすべての BDD シナリオが通ることを確認する

**Checkpoint**: US2 完了 — SyntaxTree における Trivia 識別が検証済み

---

## Phase 4: User Story 1 - ASG での要素終了位置の正確な取得（Priority: P1）

**Goal**: `AsgConverter.GetContentEndOffset()` が Parser 変更後に不要になるか検証し、必要に応じてリファクタリングする

**Independent Test**: `Source/AsciiSharp.Asg` のビルドが通り、TCK テスト（`dotnet test` 全体）が通ること

### Implementation for User Story 1

- [ ] T013 [US1] Parser 変更後に `SectionTitleSyntax.Span.End` が行末空白を除いたコンテンツ末尾を指すことを確認する（trailing trivia は Span に含まれないため）: `Source/AsciiSharp.Asg/AsgConverter.cs`
- [ ] T014 [US1] `AsgConverter.GetContentEndOffset()` で `NewLineToken`・`WhitespaceToken` を除外している処理が不要になった場合、`node.Span.End` を直接使うようにリファクタリングする: `Source/AsciiSharp.Asg/AsgConverter.cs`
- [ ] T015 [US1] ビルドとテストを実行し、Asg プロジェクトのビルドと TCK テストがすべて通ることを確認する

**Checkpoint**: US1 完了 — ASG の Span 計算が正確な要素境界を指すことを確認済み

---

## Phase 5: User Story 3 - 元テキストの完全復元（Priority: P3）

**Goal**: Parser 変更後もラウンドトリップが保証されることを BDD で確認する

**Independent Test**: `dotnet test --project Test/AsciiSharp.Specs --filter "TrailingWhitespaceFeature"` の成功シナリオがすべて通ること

### Implementation for User Story 3

- [ ] T016 [US3] ラウンドトリップ系 BDD シナリオ（5 件）がすべて通ることを確認する（Phase 2 完了後は自動的に通るはず）: `Test/AsciiSharp.Specs/Features/TrailingWhitespaceFeature.Steps.cs`
- [ ] T017 [US3] `文書末尾に改行なしの行末空白のみがある場合` のエッジケースが正しく処理されることを確認する（trailing trivia に EndOfLineTrivia がなく WhitespaceTrivia のみ）

**Checkpoint**: US3 完了 — ラウンドトリップ完全性が検証済み

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: コード品質の確保とドキュメント整備

- [ ] T018 [P] ビルド警告ゼロであることを確認し、警告がある場合は修正または抑制する
- [ ] T019 [P] `ParseSectionTitle()` と `ParseAuthorLine()` の変更後に XML ドキュメント コメントを更新する（変更した箇所のコメントが実装と合っているか確認）: `Source/AsciiSharp/Parser/Parser.cs`
- [ ] T020 すべてのテストを実行し（`dotnet test`）、全テストが通ることを最終確認する
- [ ] T021 [P] quickstart.md を実際の API で検証し、必要に応じて更新する: `specs/001-trailing-whitespace-trivia/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup（Phase 1）**: 依存なし — 即座に開始可能
- **Foundational（Phase 2）**: Phase 1 完了後 — US2・US3・US1 をブロック
- **US2（Phase 3）**: Phase 2 完了後 — US1・US3 に依存しない
- **US1（Phase 4）**: Phase 2 完了後 — US2 に依存しない（並列実行可能）
- **US3（Phase 5）**: Phase 2 完了後 — Phase 2 で自動的に検証されている場合あり
- **Polish（Phase 6）**: すべての US 完了後

### User Story Dependencies

- **User Story 2（P2）**: Phase 2 完了後に開始可能 — 独立してテスト可能
- **User Story 1（P1）**: Phase 2 完了後に開始可能 — US2 に依存しない
- **User Story 3（P3）**: Phase 2 完了後に開始可能 — 実質 Phase 2 完了時点で達成済みの可能性

### Within Each Phase

- T004 → T005（順序依存: 同じメソッド内の変更）
- T004, T005 → T006（参照パターンとして使用）
- T006 → T007（ビルド確認）
- T008〜T011 は並列実行可能（同一ファイルだが独立したメソッド）
- T013 → T014（調査結果を受けてリファクタリング判断）

---

## Parallel Opportunities

```bash
# Phase 1: 全タスク並列実行可能（参照のみ）
Task: T001 ParseSectionTitle 確認
Task: T002 ParseAuthorLine 確認
Task: T003 ParseAttributeEntry 確認（参照）

# Phase 3: BDD ステップ実装（独立したメソッド）
Task: T008 WhitespaceTrivia + EndOfLineTrivia ステップ
Task: T009 EndOfLineTrivia のみステップ
Task: T010 CRLF ステップ
Task: T011 著者行ステップ
```

---

## Implementation Strategy

### MVP（最小検証可能製品）: Phase 1 + Phase 2 + Phase 3

1. Phase 1 完了: 変更対象の把握
2. Phase 2 完了（T004〜T007）: Parser 変更 — **最も重要**
3. Phase 3 完了（T008〜T012）: BDD で US2 検証
4. **STOP and VALIDATE**: `dotnet test` を実行してすべて通ることを確認
5. US1（ASG Span）と US3（ラウンドトリップ）は追加フェーズで対応

### Incremental Delivery

1. Phase 1 + Phase 2 → Parser 変更完了（ラウンドトリップ系は自動的に通過）
2. Phase 3 → US2 の BDD シナリオ通過
3. Phase 4 → US1 の ASG Span 確認・Asg リファクタリング
4. Phase 5 + Phase 6 → US3 確認と最終 polish

---

## Notes

- `ParseAttributeEntry()` が参照実装パターン — 変更の際は必ず参照する
- T007 の「既存テストがすべて通ること」は必須条件（回帰なし）
- `GetContentEndOffset()` のリファクタリング（T014）は「不要になった場合」の条件付き
- BDD 対象はコアライブラリのみ（US1 の ASG Span は Asg プロジェクトの unit test で確認）
- 各タスク完了後にコミットを作成する
