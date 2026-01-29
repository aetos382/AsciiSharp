# Tasks: TCK 統合テスト基盤

**Input**: Design documents from `/specs/001-tck-integration/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

このプロジェクトの構造:
- **Source**: `Source/TckAdapter/` 配下
- **CI**: `.github/workflows/`

---

## Phase 1: Setup

**Purpose**: 基盤モデルと JSON シリアライゼーションの準備

- [ ] T001 TckInput モデルを作成 `Source/TckAdapter/AsciiSharp.TckAdapter/Tck/TckInput.cs`
- [ ] T002 TckInput を AsgJsonContext に追加 `Source/TckAdapter/AsciiSharp.TckAdapter/Asg/Serialization/AsgJsonContext.cs`

**Checkpoint**: TckInput モデルと JSON シリアライゼーションの準備完了

---

## Phase 2: User Story 1 - TCK アダプターによる ASG 変換 (Priority: P1) 🎯 MVP

**Goal**: CLI が標準入力から TCK 形式の JSON を受け取り、ASG 形式の JSON を標準出力に出力する

**Independent Test**: `echo '{"contents": "Hello", "path": "/test.adoc", "type": "block"}' | dotnet run --project Source/TckAdapter/AsciiSharp.TckAdapter.Cli/AsciiSharp.TckAdapter.Cli.csproj`

### Implementation for User Story 1

- [ ] T003 [US1] Program.cs に CLI 処理フローを実装 `Source/TckAdapter/AsciiSharp.TckAdapter.Cli/Program.cs`
- [ ] T004 [US1] ビルドして手動テストで動作確認
- [ ] T005 [US1] 警告をゼロにする

**Checkpoint**: CLI が動作し、手動テストで ASG JSON が出力される

---

## Phase 3: User Story 2 - Docker によるビルドと配布 (Priority: P2)

**Goal**: `docker buildx bake tck` で TCK アダプターの Docker イメージをビルドし、コンテナ内で動作確認する

**Independent Test**: `docker buildx bake tck && docker run --rm asciisharp-tck`

### Implementation for User Story 2

- [ ] T006 [US2] Docker イメージのビルドテスト `docker buildx bake tck`
- [ ] T007 [US2] コンテナ内での TCK 実行確認

**Checkpoint**: Docker イメージがビルドされ、TCK テストが実行される

---

## Phase 4: User Story 3 - GitHub Actions での自動 TCK 実行 (Priority: P3)

**Goal**: プルリクエストやコミット時に GitHub Actions で TCK テストを自動実行する

**Independent Test**: GitHub Actions ワークフローを手動トリガー (`workflow_dispatch`) して TCK テストが実行されることを確認

### Implementation for User Story 3

- [ ] T008 [US3] TCK ワークフローを作成 `.github/workflows/tck.yml`
- [ ] T009 [US3] 手動トリガーまたは PR で動作確認

**Checkpoint**: GitHub Actions で TCK テストが自動実行される

---

## Phase 5: User Story 4 - 失敗したテストのレポート (Priority: P4)

**Goal**: TCK テストで失敗した構文要素を明確に把握し、次の実装候補を特定できる

**Independent Test**: TCK テストを実行し、失敗したテストの一覧が出力されることを確認

### Implementation for User Story 4

- [ ] T010 [US4] TCK テスト結果のサマリー表示を確認

**Checkpoint**: 失敗したテストから次の実装候補が明確になる

---

## Phase 6: Polish

**Purpose**: 最終確認とドキュメント検証

- [ ] T011 quickstart.md のサンプルコマンドを実行して動作確認
- [ ] T012 ビルド警告がゼロであることを確認

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: 即座に開始可能
- **Phase 2 (US1)**: Phase 1 完了後
- **Phase 3 (US2)**: Phase 2 完了後
- **Phase 4 (US3)**: Phase 3 完了後
- **Phase 5 (US4)**: Phase 4 完了後
- **Phase 6 (Polish)**: 全ユーザーストーリー完了後

### User Story Dependencies

```
US1 (P1): TCK アダプター CLI
    ↓
US2 (P2): Docker ビルド
    ↓
US3 (P3): GitHub Actions CI
    ↓
US4 (P4): テストレポート
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Phase 1: Setup 完了
2. Phase 2: US1 実装
3. **STOP and VALIDATE**: CLI が手動で動作することを確認

### Incremental Delivery

1. Setup → 基盤準備完了
2. US1 完了 → CLI 単体で動作（MVP!）
3. US2 完了 → Docker で動作
4. US3 完了 → CI で自動実行
5. US4 完了 → レポート確認
6. Polish → 全体検証

---

## Notes

- 警告ゼロ: 各フェーズ完了時に警告を解消
- コミット: 各フェーズ終了時に必ずコミット
- 検証: TCK テスト自体が CLI の検証となる
