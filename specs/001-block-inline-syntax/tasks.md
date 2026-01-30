# Tasks: BlockSyntax と InlineSyntax 階層の導入

**Input**: Design documents from `/specs/001-block-inline-syntax/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅

**Tests**: BDD テストは plan フェーズで .feature ファイルとして作成済み。ステップ定義の実装が必要。

**Organization**: タスクはユーザーストーリーごとにグループ化され、独立した実装とテストが可能。

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 並列実行可能（異なるファイル、依存関係なし）
- **[Story]**: タスクが属するユーザーストーリー（US1, US2, US3）
- ファイルパスを含む具体的な説明

## Path Conventions

```text
Source/AsciiSharp/Syntax/    # ソースコード
Test/AsciiSharp.Specs/       # BDD テスト
```

---

## Phase 1: Setup

**Purpose**: 本機能は既存プロジェクトへのリファクタリングのため、セットアップタスクなし

✅ セットアップ完了 - 既存プロジェクト構造を使用

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: すべてのユーザーストーリーに先立って完了が必要な基盤タスク

**⚠️ CRITICAL**: このフェーズが完了するまでユーザーストーリーの作業は開始できない

- [ ] T001 [P] BlockSyntax 抽象クラスを作成 in Source/AsciiSharp/Syntax/BlockSyntax.cs
- [ ] T002 [P] InlineSyntax 抽象クラスを作成 in Source/AsciiSharp/Syntax/InlineSyntax.cs
- [ ] T003 SyntaxHierarchySteps クラスを作成 in Test/AsciiSharp.Specs/StepDefinitions/SyntaxHierarchySteps.cs

**Checkpoint**: 基盤完了 - ユーザーストーリー実装を開始可能

---

## Phase 3: User Story 1 & 2 - ブロック/インライン要素の型識別 (Priority: P1) 🎯 MVP

**Goal**: 構文ノードを BlockSyntax / InlineSyntax として型レベルで識別可能にする

**Independent Test**: `is BlockSyntax` および `is InlineSyntax` パターンマッチングで要素を識別できる

**Note**: US1 と US2 は相互依存（BlockSyntax のテストには InlineSyntax の否定テストが含まれる）のため、同一フェーズで実装

### Block ノードの継承元変更

- [ ] T004 [P] [US1] DocumentSyntax の継承元を BlockSyntax に変更 in Source/AsciiSharp/Syntax/DocumentSyntax.cs
- [ ] T005 [P] [US1] DocumentHeaderSyntax の継承元を BlockSyntax に変更 in Source/AsciiSharp/Syntax/DocumentHeaderSyntax.cs
- [ ] T006 [P] [US1] DocumentBodySyntax の継承元を BlockSyntax に変更 in Source/AsciiSharp/Syntax/DocumentBodySyntax.cs
- [ ] T007 [P] [US1] SectionSyntax の継承元を BlockSyntax に変更 in Source/AsciiSharp/Syntax/SectionSyntax.cs
- [ ] T008 [P] [US1] SectionTitleSyntax の継承元を BlockSyntax に変更 in Source/AsciiSharp/Syntax/SectionTitleSyntax.cs
- [ ] T009 [P] [US1] ParagraphSyntax の継承元を BlockSyntax に変更 in Source/AsciiSharp/Syntax/ParagraphSyntax.cs
- [ ] T010 [P] [US1] AuthorLineSyntax の継承元を BlockSyntax に変更 in Source/AsciiSharp/Syntax/AuthorLineSyntax.cs

### Inline ノードの継承元変更

- [ ] T011 [P] [US2] TextSyntax の継承元を InlineSyntax に変更 in Source/AsciiSharp/Syntax/TextSyntax.cs
- [ ] T012 [P] [US2] LinkSyntax の継承元を InlineSyntax に変更 in Source/AsciiSharp/Syntax/LinkSyntax.cs

### BDD ステップ定義の実装 (型識別)

- [ ] T013 [US1] 型識別ステップ定義を実装（BlockSyntax 判定）in Test/AsciiSharp.Specs/StepDefinitions/SyntaxHierarchySteps.cs
- [ ] T014 [US2] 型識別ステップ定義を実装（InlineSyntax 判定）in Test/AsciiSharp.Specs/StepDefinitions/SyntaxHierarchySteps.cs

**Checkpoint**: US1 & US2 完了 - BlockSyntax/InlineSyntax による型識別が機能

---

## Phase 4: User Story 3 - ジェネリック制約での利用 (Priority: P2)

**Goal**: BlockSyntax / InlineSyntax をジェネリック制約として使用可能にする

**Independent Test**: `where T : BlockSyntax` 制約を持つメソッドがコンパイルでき、適切な型のみを受け入れる

**Note**: 型階層が完成した時点でジェネリック制約は自動的に機能する。このフェーズでは一括クエリのステップ定義を追加

### BDD ステップ定義の実装 (クエリ)

- [ ] T015 [US3] 一括クエリステップ定義を実装（BlockSyntax ノードのクエリ）in Test/AsciiSharp.Specs/StepDefinitions/SyntaxHierarchySteps.cs
- [ ] T016 [US3] 一括クエリステップ定義を実装（InlineSyntax ノードのクエリ）in Test/AsciiSharp.Specs/StepDefinitions/SyntaxHierarchySteps.cs
- [ ] T017 [US3] 結果検証ステップ定義を実装（ノードの包含/非包含チェック）in Test/AsciiSharp.Specs/StepDefinitions/SyntaxHierarchySteps.cs

**Checkpoint**: US3 完了 - ジェネリック制約とクエリが機能

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: 品質保証と最終検証

- [ ] T018 ビルド実行と警告確認 (dotnet build)
- [ ] T019 全テスト実行と結果確認 (dotnet test)
- [ ] T020 警告があれば解消または無効化

**Checkpoint**: 実装完了 - すべてのテストがパス、警告ゼロ

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: なし - 既存プロジェクト
- **Phase 2 (Foundational)**: すべてのユーザーストーリーをブロック
- **Phase 3 (US1 & US2)**: Phase 2 完了後に開始可能
- **Phase 4 (US3)**: Phase 3 完了後に開始可能
- **Phase 5 (Polish)**: Phase 4 完了後に開始可能

### User Story Dependencies

```
Phase 2 (Foundational)
    ↓
Phase 3 (US1 & US2) ← MVP
    ↓
Phase 4 (US3)
    ↓
Phase 5 (Polish)
```

### Within Each Phase

- [P] マークのタスクは並列実行可能
- T001, T002 → T003 (基盤クラス作成後にステップ定義骨格を作成)
- T004-T012 は並列実行可能（すべて異なるファイル）
- T013, T014 は T004-T012 完了後に実行

### Parallel Opportunities

```
Phase 2:
  並列: T001, T002
  順次: → T003

Phase 3:
  並列: T004, T005, T006, T007, T008, T009, T010, T011, T012
  順次: → T013, T014

Phase 4:
  順次: T015 → T016 → T017

Phase 5:
  順次: T018 → T019 → T020
```

---

## Parallel Example: Phase 3

```bash
# ブロックノードの継承元変更を並列実行:
Task: "DocumentSyntax の継承元を BlockSyntax に変更 in Source/AsciiSharp/Syntax/DocumentSyntax.cs"
Task: "DocumentHeaderSyntax の継承元を BlockSyntax に変更 in Source/AsciiSharp/Syntax/DocumentHeaderSyntax.cs"
Task: "DocumentBodySyntax の継承元を BlockSyntax に変更 in Source/AsciiSharp/Syntax/DocumentBodySyntax.cs"
Task: "SectionSyntax の継承元を BlockSyntax に変更 in Source/AsciiSharp/Syntax/SectionSyntax.cs"
Task: "SectionTitleSyntax の継承元を BlockSyntax に変更 in Source/AsciiSharp/Syntax/SectionTitleSyntax.cs"
Task: "ParagraphSyntax の継承元を BlockSyntax に変更 in Source/AsciiSharp/Syntax/ParagraphSyntax.cs"
Task: "AuthorLineSyntax の継承元を BlockSyntax に変更 in Source/AsciiSharp/Syntax/AuthorLineSyntax.cs"

# インラインノードの継承元変更を並列実行:
Task: "TextSyntax の継承元を InlineSyntax に変更 in Source/AsciiSharp/Syntax/TextSyntax.cs"
Task: "LinkSyntax の継承元を InlineSyntax に変更 in Source/AsciiSharp/Syntax/LinkSyntax.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 & 2)

1. Complete Phase 2: Foundational (BlockSyntax, InlineSyntax 作成)
2. Complete Phase 3: US1 & US2 (継承元変更 + ステップ定義)
3. **STOP and VALIDATE**: BDD テストが Green になることを確認
4. コミットしてレビュー可能な状態にする

### Incremental Delivery

1. Phase 2 完了 → 基盤クラス作成済み
2. Phase 3 完了 → 型識別が機能（MVP!）
3. Phase 4 完了 → クエリ機能も追加
4. Phase 5 完了 → 品質保証完了

---

## Notes

- [P] タスク = 異なるファイル、依存関係なし
- [Story] ラベルでタスクをユーザーストーリーに紐付け
- BDD テスト (.feature) は plan フェーズで作成済み - ステップ定義の実装が必要
- 各タスク完了後にコミット
- Constitution に従い、Green 後に必ずビルドとテストを実行
