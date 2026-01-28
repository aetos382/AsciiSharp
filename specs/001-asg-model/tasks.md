# Tasks: AsciiDoc ASG モデルクラス

**Input**: Design documents from `/specs/001-asg-model/`
**Prerequisites**: plan.md (required), spec.md (required), data-model.md, quickstart.md
**Status**: 実装完了 → ユニットテスト追加フェーズ

**Note**: 実装は完了しています。BDD はコアプロジェクト（Source/AsciiSharp）のみを対象とするため、TckAdapter のテストは通常のユニットテストで実施します。

## Format: `[ID] [P?] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `Source/TckAdapter/AsciiSharp.TckAdapter/`
- **Tests**: `Test/AsciiSharp.TckAdapter.Tests/`

---

## Phase 1: Unit Tests (TckAdapter.Tests)

**Purpose**: ASG 変換のユニットテスト

### テストプロジェクト作成

- [x] T001 [P] TckAdapter.Tests プロジェクト作成 in Test/AsciiSharp.TckAdapter.Tests/
- [x] T002 プロジェクト参照追加: AsciiSharp.TckAdapter への参照

### User Story 1: 基本変換テスト (FR-001〜FR-004, FR-007, FR-008)

- [ ] T003 document ノードへの変換テスト
- [ ] T004 section ノードへの変換テスト（level プロパティ含む）
- [ ] T005 paragraph ノードへの変換テスト
- [ ] T006 text ノードへの変換テスト

### User Story 2: location テスト (FR-005)

- [ ] T007 位置情報の開始・終了位置テスト
- [ ] T008 複数行にまたがるノードの位置情報テスト

### User Story 3: header テスト (FR-006)

- [ ] T009 タイトル付き文書の header 変換テスト
- [ ] T010 ヘッダーのない文書の header null テスト

### エッジケーステスト

- [ ] T011 空の DocumentSyntax（Header も Body もない）→ blocks が空配列
- [ ] T012 ネストした SectionSyntax → blocks 内に再帰的に section
- [ ] T013 空文字列の TextSyntax → value が空文字列
- [ ] T014 未対応 SyntaxNode（LinkSyntax, AuthorLineSyntax）→ スキップされる

**Checkpoint**: TckAdapter のユニットテストが Green

---

## Phase 2: Polish & Refactor

**Purpose**: コード品質の確認と最終検証

- [ ] T015 全テスト実行: `dotnet test` で全テストが Green であることを確認
- [ ] T016 ビルド警告確認: `dotnet build` で警告ゼロを確認
- [ ] T017 [P] quickstart.md の検証: サンプルコードが実際に動作することを確認

---

## Dependencies & Execution Order

### Phase Dependencies

- **Unit Tests (Phase 1)**: No dependencies - テスト作成
- **Polish (Phase 2)**: Depends on Phase 1 being complete

### Parallel Opportunities

- T001 は独立して実行可能
- T003〜T014 は同じテストファイルを編集するため順次実行

---

## Implementation Strategy

1. Complete Phase 1: TckAdapter.Tests プロジェクト作成 → ユニットテスト追加
2. Complete Phase 2: 全体検証

### 注意事項

- 実装は既に完了しているため、テストは Green になることが期待される
- テストが失敗した場合は、実装のバグまたはテストの誤りを調査
- 各 Phase 完了時にコミットを作成

---

## Notes

- [P] tasks = different files, no dependencies
- 実装済みコードに対するユニットテストの追加が主目的
- テストが失敗した場合は実装を修正
- Commit after each task or logical group
