# Tasks: BDD フレームワークの LightBDD 移行

**Input**: Design documents from `/specs/005-lightbdd-migration/`
**Prerequisites**: plan.md (required), spec.md (required), research.md

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup

**Purpose**: LightBDD パッケージの導入とプロジェクト基盤の準備

- [ ] T001 `dotnet add Test/AsciiSharp.Specs package LightBDD.MsTest4` を実行して LightBDD.MsTest4 パッケージを追加する
- [ ] T002 `dotnet remove Test/AsciiSharp.Specs package Reqnroll.MsTest` を実行して Reqnroll パッケージ参照を削除する
- [ ] T003 Test/AsciiSharp.Specs/ConfiguredLightBddScope.cs を作成し LightBDD の初期化・クリーンアップと HTML レポート出力を設定する

**Checkpoint**: LightBDD パッケージが参照され、初期化クラスが作成されている

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: 旧 Reqnroll ファイルの削除。ビッグバン移行のためすべての旧ファイルを先に除去する

**⚠️ CRITICAL**: この Phase が完了するまで US タスクに進まない

- [ ] T004 Test/AsciiSharp.Specs/reqnroll.json を削除する
- [ ] T005 Test/AsciiSharp.Specs/StepDefinitions/ ディレクトリとその中の全 .cs ファイルを削除する
- [ ] T006 Test/AsciiSharp.Specs/Features/ ディレクトリ内の全 .feature ファイル（サブディレクトリ含む）を削除する
- [ ] T007 Test/AsciiSharp.Specs/Assembly.cs を LightBDD 用に更新する（Reqnroll 関連の属性を削除、[assembly: Parallelize] を維持）

**Checkpoint**: プロジェクトがビルドでき、テストが 0 件の状態

---

## Phase 3: User Story 1 - BDD テストを C# で記述・管理する (Priority: P1) 🎯 MVP

**Goal**: LightBDD の partial class パターンで最初のフィーチャー（BasicParsing、11 シナリオ）を変換し、パターンを確立する

**Independent Test**: `dotnet test Test/AsciiSharp.Specs/` で BasicParsing の 11 シナリオが全て成功する

### Implementation for User Story 1

- [ ] T008 [US1] BasicParsing.feature を変換: Test/AsciiSharp.Specs/Features/BasicParsingFeature.cs（シナリオ定義）と Test/AsciiSharp.Specs/Features/BasicParsingFeature.Steps.cs（ステップ実装）を作成する（11 シナリオ）

**Checkpoint**: BasicParsing の 11 シナリオが LightBDD で実行・成功し、HTML レポートに表示される。このパターンが以降のフィーチャー変換のテンプレートとなる

---

## Phase 4: User Story 2 - 既存テストカバレッジの維持 (Priority: P1)

**Goal**: 残り 13 フィーチャー（57 シナリオ）をすべて LightBDD に変換し、全 68 シナリオが成功する状態にする

**Independent Test**: `dotnet test Test/AsciiSharp.Specs/` で 68 シナリオが全て成功する

### Implementation for User Story 2

- [ ] T009 [P] [US2] SectionTitleRecognition.feature を変換: Test/AsciiSharp.Specs/Features/SectionTitleRecognitionFeature.cs + .Steps.cs を作成する（7 シナリオ）
- [ ] T010 [P] [US2] CommentParsing.feature を変換: Test/AsciiSharp.Specs/Features/CommentParsingFeature.cs + .Steps.cs を作成する（7 シナリオ）
- [ ] T011 [P] [US2] AttributeEntryParsing.feature を変換: Test/AsciiSharp.Specs/Features/AttributeEntryParsingFeature.cs + .Steps.cs を作成する（7 シナリオ）
- [ ] T012 [P] [US2] SyntaxVisitor.feature を変換: Test/AsciiSharp.Specs/Features/SyntaxVisitorFeature.cs + .Steps.cs を作成する（8 シナリオ）
- [ ] T013 [P] [US2] ErrorRecovery.feature を変換: Test/AsciiSharp.Specs/Features/ErrorRecoveryFeature.cs + .Steps.cs を作成する（5 シナリオ、1 件 @ignore → [Ignore] 属性）
- [ ] T014 [P] [US2] IncrementalParsing.feature を変換: Test/AsciiSharp.Specs/Features/IncrementalParsingFeature.cs + .Steps.cs を作成する（5 シナリオ）
- [ ] T015 [P] [US2] SectionTitleInlineElements.feature を変換: Test/AsciiSharp.Specs/Features/SectionTitleInlineElementsFeature.cs + .Steps.cs を作成する（5 シナリオ）
- [ ] T016 [P] [US2] SectionTitleTrivia.feature を変換: Test/AsciiSharp.Specs/Features/SectionTitleTriviaFeature.cs + .Steps.cs を作成する（5 シナリオ）
- [ ] T017 [P] [US2] BlockInlineSyntax.feature を変換: Test/AsciiSharp.Specs/Features/BlockInlineSyntaxFeature.cs + .Steps.cs を作成する（5 シナリオ）
- [ ] T018 [P] [US2] LinkParsing.feature を変換: Test/AsciiSharp.Specs/Features/LinkParsingFeature.cs + .Steps.cs を作成する（4 シナリオ）
- [ ] T019 [P] [US2] TrailingWhitespace.feature を変換: Test/AsciiSharp.Specs/Features/TrailingWhitespaceFeature.cs + .Steps.cs を作成する（4 シナリオ）
- [ ] T020 [P] [US2] Immutability.feature を変換: Test/AsciiSharp.Specs/Features/ImmutabilityFeature.cs + .Steps.cs を作成する（3 シナリオ）
- [ ] T021 [P] [US2] InlineTextSyntaxRename.feature を変換: Test/AsciiSharp.Specs/Features/InlineTextSyntaxRenameFeature.cs + .Steps.cs を作成する（3 シナリオ）

**Checkpoint**: 全 68 シナリオ（67 成功 + 1 Ignore）が LightBDD で実行される

---

## Phase 5: User Story 3 - Reqnroll 依存の完全除去 (Priority: P2)

**Goal**: Reqnroll への参照をプロジェクト全体から完全に除去し、ドキュメントを LightBDD に更新する

**Independent Test**: リポジトリ内で "Reqnroll" を grep して 0 件（specs/ 内のドキュメントを除く）。ビルドが成功する

### Implementation for User Story 3

- [ ] T022 [P] [US3] Directory.Packages.props から Reqnroll.MsTest の PackageVersion エントリを手作業で削除する（`dotnet remove package` は csproj のみ削除するため）
- [ ] T023 [P] [US3] CLAUDE.md 内の Reqnroll・.feature ファイル関連の記述を LightBDD に更新する
- [ ] T024 [P] [US3] .specify/memory/constitution.md 内の「.feature ファイル」を「フィーチャ定義」に一般化する

**Checkpoint**: プロジェクト内に Reqnroll への参照が存在しない（specs/ ドキュメント内の履歴的言及を除く）

---

## Phase 6: User Story 4 - テスト実行レポートの生成 (Priority: P3)

**Goal**: LightBDD の HTML レポートが正しく生成され、シナリオ結果が一覧表示される

**Independent Test**: `dotnet test` 実行後に Reports/FeaturesReport.html が生成され、日本語のシナリオ名が正しく表示される

### Implementation for User Story 4

- [ ] T025 [US4] テスト実行後に HTML レポートが生成されることを確認し、レポート出力先を .gitignore に追加する（Test/AsciiSharp.Specs/.gitignore または既存の .gitignore）

**Checkpoint**: HTML レポートが正しく生成される

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: 最終検証と品質確認

- [ ] T026 全テスト（AsciiSharp.Specs + AsciiSharp.Tests + AsciiSharp.Asg.Tests）が成功し、ビルド警告がゼロであることを確認する

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **US1 (Phase 3)**: Depends on Foundational - パターン確立のため US2 より先に完了する
- **US2 (Phase 4)**: Depends on US1 completion (US1 で確立したパターンをテンプレートとして使用)
- **US3 (Phase 5)**: Depends on US2 completion (全シナリオ移行後にクリーンアップ)
- **US4 (Phase 6)**: Depends on US1 completion (レポート検証は US2 完了前でも可能だが、全シナリオ揃った後が望ましい)
- **Polish (Phase 7)**: Depends on all user stories being complete

### Within User Story 2

- T009〜T021 はすべて [P]（並列実行可能）。各フィーチャーは独立したファイルを生成し、相互依存なし
- US1 の BasicParsingFeature をテンプレートとしてコピー＆修正する

### Parallel Opportunities

- Phase 4 の 13 タスク（T009〜T021）はすべて並列実行可能
- Phase 5 の 3 タスク（T022〜T024）はすべて並列実行可能

---

## Parallel Example: User Story 2

```text
# 13 フィーチャーの変換はすべて同時に開始可能:
Task T009: SectionTitleRecognitionFeature.cs + .Steps.cs
Task T010: CommentParsingFeature.cs + .Steps.cs
Task T011: AttributeEntryParsingFeature.cs + .Steps.cs
... (T012〜T021 も同時)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup → LightBDD パッケージ導入
2. Complete Phase 2: Foundational → 旧 Reqnroll ファイル削除
3. Complete Phase 3: User Story 1 → BasicParsing の 11 シナリオが LightBDD で成功
4. **STOP and VALIDATE**: パターンが正しいことを確認

### Incremental Delivery

1. Setup + Foundational → 基盤完了
2. US1 (BasicParsing) → パターン確立、HTML レポート動作確認
3. US2 (残り 13 フィーチャー) → 全 68 シナリオ移行完了
4. US3 (Reqnroll 除去) → クリーンアップ完了
5. US4 (レポート検証) + Polish → 最終品質確認

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- 各フィーチャークラスは自己完結型（ステップ定義の再利用なし）
- 変換元の .feature ファイルと StepDefinitions/*.cs は Phase 2 で削除済みのため、git の履歴から参照する
- パッケージの追加・削除は `dotnet add package` / `dotnet remove package` コマンドで行う。dotnet コマンドがサポートしていない操作（Directory.Packages.props の不要エントリ削除等）のみ手作業で行う
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
