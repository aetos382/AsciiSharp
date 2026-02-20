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
- [ ] T003 [P] `ParseAttributeEntry()` の実装を参照パターンとして確認する（変更なし・参照のみ）: `Source/AsciiSharp/Parser/Parser.cs` L629-697

> **フェーズ終了後**: `/commit-commands:commit` でコミットを作成する（任意 — Setup は軽微のためスキップ可）

---

## Phase 2: Foundational（ブロッキング前提条件）

**Purpose**: Parser の行末処理変更（US2・US3・US1 すべての前提）

**⚠️ CRITICAL**: このフェーズが完了するまでユーザーストーリーの実装は開始できない

- [ ] T004 `ParseSectionTitle()` を変更し、ループ終了後に末尾 `WhitespaceToken` を `WhitespaceTrivia` に変換して最終コンテンツトークンの trailing に付与する: `Source/AsciiSharp/Parser/Parser.cs`
- [ ] T005 `ParseSectionTitle()` を変更し、`NewLineToken` を `EndOfLineTrivia` に変換して最終コンテンツトークンの trailing に付与する（CRLF は `"\r\n"` として 1 つの EndOfLineTrivia になること）: `Source/AsciiSharp/Parser/Parser.cs`
- [ ] T006 `ParseAuthorLine()` を変更し、末尾 `WhitespaceToken` を `WhitespaceTrivia`、`NewLineToken` を `EndOfLineTrivia` に変換して最終コンテンツトークンの trailing に付与する（`ParseAttributeEntry()` と同パターン）: `Source/AsciiSharp/Parser/Parser.cs`
- [ ] T007 ビルドとテストを実行し、以下をすべて確認する（**回帰なし確認**）:
  - 既存のすべてのテストが通ること
  - 段落パーステスト（FR-007: 段落内 NewLineToken 処理が変わっていないこと）が通ること
  - `ParseAttributeEntry` の出力が変更されていないこと（SC-004: 変更不要の確認）

**Checkpoint**: Parser 変更完了 — ユーザーストーリーの実装が可能になる

> **フェーズ終了後**: `/commit-commands:commit` でコミットを作成する

---

## Phase 3: User Story 2 - SyntaxTree における行末トリビアの識別（Priority: P2）

**Goal**: セクションタイトル・著者行の最終コンテンツトークンの後続トリビアに WhitespaceTrivia / EndOfLineTrivia が格納されることを BDD で検証する

**Independent Test**: `dotnet test --project Test/AsciiSharp.Specs --filter "TrailingWhitespaceFeature"` を実行し、すべてのシナリオが通ること（Inconclusive ゼロ）

### Implementation for User Story 2

- [ ] T008 [US2] BDD ステップ `セクションタイトルの最終コンテンツトークンの後続トリビアにWhitespaceTriviaとEndOfLineTriviaが含まれる` を実装する（`SectionTitleSyntax` の最終コンテンツトークンを取得し、trailing trivia に `SyntaxKind.WhitespaceTrivia` と `SyntaxKind.EndOfLineTrivia` が含まれることを検証）: `Test/AsciiSharp.Specs/Features/TrailingWhitespaceFeature.Steps.cs`
- [ ] T009 [US2] BDD ステップ `セクションタイトルの最終コンテンツトークンの後続トリビアにEndOfLineTriviaのみが含まれる` を実装する（`SyntaxKind.WhitespaceTrivia` が含まれず `SyntaxKind.EndOfLineTrivia` のみが含まれることを検証）: `Test/AsciiSharp.Specs/Features/TrailingWhitespaceFeature.Steps.cs`
- [ ] T010 [US2] BDD ステップ `セクションタイトルの最終コンテンツトークンの後続トリビアにCRLFを含むEndOfLineTriviaが含まれる` を実装する（`EndOfLineTrivia` のテキストが `"\r\n"` であることを検証）: `Test/AsciiSharp.Specs/Features/TrailingWhitespaceFeature.Steps.cs`
- [ ] T011 [US2] BDD ステップ `著者行の最終コンテンツトークンの後続トリビアにWhitespaceTriviaとEndOfLineTriviaが含まれる` を実装する（`AuthorLineSyntax` の最終コンテンツトークンを取得し trailing trivia を検証）: `Test/AsciiSharp.Specs/Features/TrailingWhitespaceFeature.Steps.cs`
- [ ] T012 ビルドとテストを実行し、以下を確認する（**US2 BDD シナリオ通過確認**）:
  - `TrailingWhitespaceFeature` の全 9 シナリオが通ること（Inconclusive ゼロ）
  - 既存の他のフィーチャーテストが引き続き通ること

**Checkpoint**: US2 完了 — SyntaxTree における Trivia 識別が検証済み

> **フェーズ終了後**: `/commit-commands:commit` でコミットを作成する

---

## Phase 4: User Story 1 - ASG での要素終了位置の正確な取得（Priority: P1）

**Goal**: Parser 変更後に `SyntaxNode.Span.End` が行末空白を除いた正確な境界を指すことを確認し、`AsgConverter.GetContentEndOffset()` を `node.Span.End` を直接使う形にリファクタリングする

**Independent Test**: `dotnet test` 全体が通ること（特に TCK テスト）

### Implementation for User Story 1

- [ ] T013 [US1] `AsgConverter` の Span 計算を検証・リファクタリングする:
  1. **SC-003 検証**: `"== Title   \n"` と `"== Title\n"` をそれぞれパースし `AsgConverter` で変換後、両者の `SectionTitle.Location.End` が等しいことを確認する（行末空白あり・なしで Span が同一）
  2. **US1 受け入れ確認**: `"== Title   \n"` の変換結果で `Location.End` が `e` の直後を指すことを確認する
  3. **FR-005 リファクタリング**: `GetContentEndOffset()` が `NewLineToken`/`WhitespaceToken` を手動除外している処理が不要になっていることを確認し、`node.Span.End` を直接使うように変更する（不要になった `GetContentEndOffset()` を削除可）: `Source/AsciiSharp.Asg/AsgConverter.cs`
- [ ] T014 ビルドとテストを実行し、以下を確認する（**US1・Asg リファクタリング確認**）:
  - `Source/AsciiSharp.Asg` のビルドが通ること
  - `dotnet test` 全体（TCK テストを含む）が通ること

**Checkpoint**: US1 完了 — ASG の Span 計算が正確な要素境界を指すことを確認済み

> **フェーズ終了後**: `/commit-commands:commit` でコミットを作成する

---

## Phase 5: User Story 3 - 元テキストの完全復元（Priority: P3）

**Goal**: Parser 変更後もラウンドトリップが保証されることを BDD で確認する

**Independent Test**: `dotnet test --project Test/AsciiSharp.Specs --filter "TrailingWhitespaceFeature"` のラウンドトリップ系シナリオ（5 件）が通ること

### Implementation for User Story 3

- [ ] T015 [US3] ラウンドトリップ系 BDD シナリオ（5 件）がすべて通ることを確認する（Phase 2 完了後に自動的に通るはずだが、念のため個別に確認）: `Test/AsciiSharp.Specs/Features/TrailingWhitespaceFeature.Steps.cs`
- [ ] T016 [US3] エッジケース「文書末尾に改行なしの行末空白のみがある場合（`"== タイトル   "`）」を確認する — trailing trivia に `EndOfLineTrivia` がなく `WhitespaceTrivia` のみになること、かつラウンドトリップが保証されること

**Checkpoint**: US3 完了 — ラウンドトリップ完全性が検証済み

> **フェーズ終了後**: `/commit-commands:commit` でコミットを作成する

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: コード品質の確保とドキュメント整備

- [ ] T017 [P] ビルド警告ゼロであることを確認し、警告がある場合は修正または正当な理由でコメント付き抑制する（**警告ゼロ確認**）
- [ ] T018 [P] `ParseSectionTitle()` と `ParseAuthorLine()` の変更後に XML ドキュメント コメントが実装と合っているか確認し、必要に応じて更新する: `Source/AsciiSharp/Parser/Parser.cs`
- [ ] T019 すべてのテストを実行し（`dotnet test`）、全テストが通ることを最終確認する（**最終回帰確認**）
- [ ] T020 [P] `quickstart.md` のコードサンプルとシナリオ対応表を実際の実装と照合して検証する: `specs/001-trailing-whitespace-trivia/quickstart.md`

> **フェーズ終了後**: `/commit-commands:commit` でコミットを作成する

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
- T013（検証・リファクタリング）→ T014（ビルド確認）

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
4. **STOP and VALIDATE**: `dotnet test --project Test/AsciiSharp.Specs` を実行してすべて通ることを確認
5. US1（ASG Span）と US3（ラウンドトリップ）は追加フェーズで対応

### Incremental Delivery

1. Phase 1 + Phase 2 → Parser 変更完了（ラウンドトリップ系は自動的に通過）
2. Phase 3 → US2 の BDD シナリオ通過
3. Phase 4 → US1 の ASG Span 確認・Asg リファクタリング（GetContentEndOffset 削除）
4. Phase 5 + Phase 6 → US3 確認と最終 polish

---

## Notes

- `ParseAttributeEntry()` が参照実装パターン — 変更の際は必ず参照する（`Source/AsciiSharp/Parser/Parser.cs` L629-697）
- T007 の「既存テストがすべて通ること」は必須条件（回帰なし）
- T013 の SC-003 検証: 行末空白の有無で `AsgConverter` の出力 Span が変わらないことを確認すること
- BDD 対象はコアライブラリのみ（US1 の ASG Span は Asg プロジェクトの手動確認で対応）
- **各フェーズ終了後に必ず `/commit-commands:commit` でコミットを作成すること**（constitution VI 準拠）
