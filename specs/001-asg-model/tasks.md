# Tasks: AsciiDoc ASG モデルクラス

**Input**: Design documents from `/specs/001-asg-model/`
**Prerequisites**: plan.md (required), spec.md (required), data-model.md, quickstart.md
**Status**: 実装完了 → BDD テスト追加フェーズ

**Note**: 実装は完了しているため、本タスクリストは BDD テスト（.feature ファイル）の作成と Green/Refactor 確認に焦点を当てています。

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `Source/TckAdapter/AsciiSharp.TckAdapter/`
- **Tests**: `Test/AsciiSharp.Specs/Features/`
- **Step Definitions**: `Test/AsciiSharp.Specs/Steps/`

---

## Phase 1: Setup (BDD テスト基盤)

**Purpose**: BDD テストの基盤整備

- [ ] T001 AsgConversion.feature のディレクトリ構造確認 in Test/AsciiSharp.Specs/Features/
- [ ] T002 [P] Step Definition の基底クラス確認 in Test/AsciiSharp.Specs/Steps/

---

## Phase 2: User Story 1 - SyntaxTree から ASG への変換 (Priority: P1) 🎯 MVP

**Goal**: DocumentSyntax, SectionSyntax, ParagraphSyntax, TextSyntax の ASG 変換を BDD テストで検証

**Independent Test**: AsciiDoc テキストをパースし、ASG JSON に変換して構造を検証

### BDD テスト (Red → Green 確認)

- [ ] T003 [US1] .feature ファイル作成: SyntaxTree から ASG への基本変換 in Test/AsciiSharp.Specs/Features/AsgConversion.feature
  - FR-001: document ノードへの変換
  - FR-002: section ノードへの変換
  - FR-003: paragraph ノードへの変換
  - FR-004: text ノードへの変換
  - **FR-007: section の level プロパティ検証を含める**
  - **FR-008: title 配列の複数要素検証を含める**
- [ ] T004 [US1] Step Definition 作成: ASG 変換の Given/When/Then in Test/AsciiSharp.Specs/Steps/AsgConversionSteps.cs
- [ ] T005 [US1] テスト実行: Red 確認（テストが存在しないため失敗を確認）
- [ ] T006 [US1] テスト実行: Green 確認（実装済みコードでテストが通ることを確認）

**Checkpoint**: User Story 1 の BDD テストが Green

---

## Phase 3: User Story 2 - location 情報の出力 (Priority: P2)

**Goal**: 各 ASG ノードの位置情報（line, col）が正しく出力されることを検証

**Independent Test**: 特定位置のテキストが正しい location を持つことを検証

### BDD テスト (Red → Green 確認)

- [ ] T007 [US2] .feature シナリオ追加: location 情報の検証 in Test/AsciiSharp.Specs/Features/AsgConversion.feature
- [ ] T008 [US2] Step Definition 追加: location 検証の Given/When/Then in Test/AsciiSharp.Specs/Steps/AsgConversionSteps.cs
- [ ] T009 [US2] テスト実行: Green 確認

**Checkpoint**: User Story 2 の BDD テストが Green

---

## Phase 4: User Story 3 - DocumentHeader の ASG 変換 (Priority: P2)

**Goal**: 文書ヘッダー（タイトル）の ASG 変換を検証

**Independent Test**: タイトル付き文書で header.title が正しく出力されることを検証

### BDD テスト (Red → Green 確認)

- [ ] T010 [US3] .feature シナリオ追加: DocumentHeader の変換 in Test/AsciiSharp.Specs/Features/AsgConversion.feature
- [ ] T011 [US3] Step Definition 追加: header 検証の Given/When/Then in Test/AsciiSharp.Specs/Steps/AsgConversionSteps.cs
- [ ] T012 [US3] テスト実行: Green 確認

**Checkpoint**: User Story 3 の BDD テストが Green

---

## Phase 5: Unit Tests (TckAdapter.Tests)

**Purpose**: エッジケースのユニットテスト

### テストプロジェクト作成

- [ ] T013 [P] TckAdapter.Tests プロジェクト作成 in Test/AsciiSharp.TckAdapter.Tests/
- [ ] T014 [P] プロジェクト参照追加: AsciiSharp.TckAdapter への参照

### エッジケーステスト

- [ ] T015 空の DocumentSyntax（Header も Body もない）→ blocks が空配列
- [ ] T016 ネストした SectionSyntax → blocks 内に再帰的に section
- [ ] T017 空文字列の TextSyntax → value が空文字列
- [ ] T018 未対応 SyntaxNode（LinkSyntax, AuthorLineSyntax）→ スキップされる

**Checkpoint**: TckAdapter のエッジケーステストが Green

---

## Phase 6: Polish & Refactor

**Purpose**: コード品質の確認と最終検証

- [ ] T019 全テスト実行: `dotnet test` で全テストが Green であることを確認
- [ ] T020 ビルド警告確認: `dotnet build` で警告ゼロを確認
- [ ] T021 [P] quickstart.md の検証: サンプルコードが実際に動作することを確認

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - 確認作業
- **User Story 1 (Phase 2)**: Depends on Setup
- **User Story 2 (Phase 3)**: Can start after Phase 2 (location は基本変換に依存)
- **User Story 3 (Phase 4)**: Can start after Phase 2 (header は独立)
- **Unit Tests (Phase 5)**: Can start after Phase 2 (TckAdapter の実装に依存)
- **Polish (Phase 6)**: Depends on all user stories and unit tests being complete

### User Story Dependencies

- **User Story 1 (P1)**: 基本変換 - 他のストーリーの前提条件
- **User Story 2 (P2)**: location - US1 完了後に開始可能
- **User Story 3 (P2)**: header - US1 完了後に開始可能（US2 と並行可能）

### Parallel Opportunities

```bash
# Phase 3 と Phase 4 は並行実行可能（異なるシナリオ）
# ただし、同じファイルを編集するため注意が必要
```

---

## Implementation Strategy

### BDD First (Red → Green)

1. Complete Phase 1: Setup 確認
2. Complete Phase 2: User Story 1 の .feature 作成 → Green 確認
3. Complete Phase 3: User Story 2 のシナリオ追加 → Green 確認
4. Complete Phase 4: User Story 3 のシナリオ追加 → Green 確認
5. Complete Phase 5: TckAdapter のユニットテスト追加
6. Complete Phase 6: 全体検証

### 注意事項

- 実装は既に完了しているため、テストは Green になることが期待される
- テストが失敗した場合は、実装のバグまたはテストの誤りを調査
- 各 Phase 完了時にコミットを作成

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- 実装済みコードに対する BDD テストの追加が主目的
- テストが Red のままの場合は実装を修正
- Commit after each task or logical group
- コミットメッセージに `[phase: tasks]` を含める
