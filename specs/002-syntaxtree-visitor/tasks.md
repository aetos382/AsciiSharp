# Tasks: SyntaxTree Visitor パターン

**Input**: Design documents from `/specs/002-syntaxtree-visitor/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md

**BDD**: Constitution により BDD 必須。Red-Green-Refactor サイクルに従う。

**Organization**: タスクはユーザーストーリー別に整理し、各ストーリーを独立して実装・テスト可能にする。

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 並列実行可能（異なるファイル、依存関係なし）
- **[Story]**: このタスクが属するユーザーストーリー（US1, US2）
- 説明には正確なファイルパスを含める

## Path Conventions

```text
Source/AsciiSharp/Syntax/     # インターフェイスと Accept メソッド
Test/AsciiSharp.Specs/        # BDD テスト（Reqnroll）
```

---

## Phase 1: Foundational（基盤）

**Purpose**: すべてのユーザーストーリーに必要なインターフェイスと抽象メソッドを作成

**⚠️ CRITICAL**: このフェーズが完了するまでユーザーストーリーの実装は開始できない

- [x] T001 ISyntaxVisitor インターフェイスを Source/AsciiSharp/Syntax/ISyntaxVisitor.cs に作成
- [x] T002 [P] ISyntaxVisitor&lt;TResult&gt; インターフェイスを Source/AsciiSharp/Syntax/ISyntaxVisitorOfT.cs に作成
- [x] T003 SyntaxNode に抽象 Accept メソッドを追加（Source/AsciiSharp/Syntax/SyntaxNode.cs）

**Checkpoint**: 基盤完了 - ユーザーストーリーの実装開始可能

---

## Phase 2: User Story 1 - 構文木の走査 (Priority: P1) 🎯 MVP

**Goal**: ISyntaxVisitor を使用して構文木を走査し、各ノードを訪問できるようにする

**Independent Test**: Visitor で構文木を走査し、すべてのノードが訪問されることを確認できる

### BDD テスト（Red）

- [x] T004 [US1] BDD feature ファイルを Test/AsciiSharp.Specs/Features/Visitor/SyntaxVisitor.feature に作成（エッジケース含む: 空文書、例外伝播、欠落ノード）

### Accept メソッド実装（Green）

- [x] T005 [P] [US1] DocumentSyntax に Accept メソッドを追加（Source/AsciiSharp/Syntax/DocumentSyntax.cs）
- [x] T006 [P] [US1] DocumentHeaderSyntax に Accept メソッドを追加（Source/AsciiSharp/Syntax/DocumentHeaderSyntax.cs）
- [x] T007 [P] [US1] DocumentBodySyntax に Accept メソッドを追加（Source/AsciiSharp/Syntax/DocumentBodySyntax.cs）
- [x] T008 [P] [US1] SectionSyntax に Accept メソッドを追加（Source/AsciiSharp/Syntax/SectionSyntax.cs）
- [x] T009 [P] [US1] SectionTitleSyntax に Accept メソッドを追加（Source/AsciiSharp/Syntax/SectionTitleSyntax.cs）
- [x] T010 [P] [US1] ParagraphSyntax に Accept メソッドを追加（Source/AsciiSharp/Syntax/ParagraphSyntax.cs）
- [x] T011 [P] [US1] TextSyntax に Accept メソッドを追加（Source/AsciiSharp/Syntax/TextSyntax.cs）
- [x] T012 [P] [US1] LinkSyntax に Accept メソッドを追加（Source/AsciiSharp/Syntax/LinkSyntax.cs）

### ステップ定義（Green 完了）

- [x] T013 [US1] ステップ定義を Test/AsciiSharp.Specs/StepDefinitions/VisitorSteps.cs に作成
- [x] T014 [US1] ビルドとテストを実行し、Green を確認

**Checkpoint**: User Story 1 完了 - ISyntaxVisitor による構文木走査が動作する

---

## Phase 3: User Story 2 - 結果を返す走査 (Priority: P2)

**Goal**: ISyntaxVisitor&lt;TResult&gt; を使用して構文木を走査し、結果を返せるようにする

**Independent Test**: Visitor で構文木を走査し、計算結果が正しく返されることを確認できる

### BDD テスト（Red）

- [x] T015 [US2] BDD feature ファイルに US2 シナリオを追加（Test/AsciiSharp.Specs/Features/Visitor/SyntaxVisitor.feature）

### ステップ定義（Green）

- [x] T016 [US2] US2 用ステップ定義を Test/AsciiSharp.Specs/StepDefinitions/VisitorSteps.cs に追加
- [x] T017 [US2] ビルドとテストを実行し、Green を確認

**Checkpoint**: User Story 2 完了 - ISyntaxVisitor&lt;TResult&gt; による結果返却走査が動作する

---

## Phase 4: Polish & Cross-Cutting Concerns

**Purpose**: 品質保証と最終確認

- [x] T018 ビルドを実行し、警告ゼロを確認
- [x] T019 [P] コード整形（dotnet format）を実行
- [x] T020 quickstart.md の使用例が動作することを確認（Visitor パターンは既存の quickstart.md に記載なし、新規機能のため省略）

---

## Dependencies & Execution Order

### Phase Dependencies

- **Foundational (Phase 1)**: 依存なし - 即座に開始可能
- **User Story 1 (Phase 2)**: Foundational 完了に依存
- **User Story 2 (Phase 3)**: User Story 1 完了に依存
- **Polish (Phase 4)**: すべてのユーザーストーリー完了に依存

### User Story Dependencies

- **User Story 1 (P1)**: Foundational 完了後に開始可能 - 他ストーリーへの依存なし
- **User Story 2 (P2)**: User Story 1 完了後に開始可能 - Accept&lt;TResult&gt; は US1 の T005-T012 で実装される

### Within Each User Story

- BDD feature ファイル（Red）を先に作成
- 実装タスク（Green）
- テスト実行で Green 確認
- 次のストーリーへ

### Parallel Opportunities

- T001 と T002 は並列実行可能（異なるファイル）
- T005-T012 はすべて並列実行可能（異なるファイル）
- US1 と US2 は Foundational 完了後に並列開始可能

---

## Parallel Example: User Story 1

```bash
# Accept メソッド実装を並列で実行:
Task: "DocumentSyntax に Accept メソッドを追加"
Task: "DocumentHeaderSyntax に Accept メソッドを追加"
Task: "DocumentBodySyntax に Accept メソッドを追加"
Task: "SectionSyntax に Accept メソッドを追加"
Task: "SectionTitleSyntax に Accept メソッドを追加"
Task: "ParagraphSyntax に Accept メソッドを追加"
Task: "TextSyntax に Accept メソッドを追加"
Task: "LinkSyntax に Accept メソッドを追加"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Phase 1: Foundational を完了
2. Phase 2: User Story 1 を完了
3. **STOP and VALIDATE**: US1 が独立して動作することを確認
4. 必要に応じてデプロイ/デモ

### Incremental Delivery

1. Foundational 完了 → 基盤準備完了
2. User Story 1 追加 → テスト → MVP!
3. User Story 2 追加 → テスト → 完全版
4. 各ストーリーは前のストーリーを壊さずに価値を追加

---

## Notes

- [P] タスク = 異なるファイル、依存関係なし
- [Story] ラベルはタスクを特定のユーザーストーリーにマッピング
- 各ユーザーストーリーは独立して完了・テスト可能
- タスク完了ごとにコミット（`/commit-commands:commit` 使用）
- 任意のチェックポイントで停止してストーリーを独立検証可能
