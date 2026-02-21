# Tasks: インライン要素とブロック要素のセマンティクス定義

**Input**: Design documents from `/specs/006-element-type-semantics/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 並行実行可能（他と異なるファイル、未完了タスクに依存しない）
- **[Story]**: 対応するユーザーストーリー（US1、US2）

---

## Phase 1: 前提クリーンアップ（基盤整備）

**目的**: 型階層変更に先立つクリーンアップ。US1・US2 どちらの実装にも影響しない独立した変更。

- [ ] T001 `SyntaxKind.TextSpan = 400` を削除し、`InlineText = 401` を明示的に付与する（FR-008） → `Source/AsciiSharp/SyntaxKind.cs`
- [ ] T002 `BlockSyntax` の XML ドキュメントコメントを「AsciiDoc 言語仕様のブロックとされる要素のみが継承する」旨に更新する（FR-004） → `Source/AsciiSharp/Syntax/BlockSyntax.cs`

**Checkpoint**: T001・T002 完了後、US1・US2 を並行して実装可能

---

## Phase 2: User Story 1 - BlockSyntax が AsciiDoc 仕様と一致する（Priority: P1）🎯 MVP

**Goal**: `SectionTitleSyntax`・`DocumentHeaderSyntax`・`AuthorLineSyntax`・`AttributeEntrySyntax`・`DocumentBodySyntax` が `BlockSyntax` でなく `SyntaxNode` を直接継承するよう変更し、既存 BDD テストと新規 BDD テストがすべて通る。

**Independent Test**:
```
dotnet test --project Test/AsciiSharp.Specs/AsciiSharp.Specs.csproj
```
`AsciiDoc仕様のブロックとされないノードはBlockSyntaxではない` シナリオが Passed になること。

### Implementation for User Story 1

- [ ] T003 [P] [US1] `SectionTitleSyntax` の基底クラスを `BlockSyntax` → `SyntaxNode` に変更し、AsciiDoc 仕様のブロックではない旨の `<remarks>` を追記する（FR-002、FR-005） → `Source/AsciiSharp/Syntax/SectionTitleSyntax.cs`
- [ ] T004 [P] [US1] `AuthorLineSyntax` の基底クラスを `BlockSyntax` → `SyntaxNode` に変更し、AsciiDoc 仕様のブロックではない旨の `<remarks>` を追記する（FR-002、FR-005） → `Source/AsciiSharp/Syntax/AuthorLineSyntax.cs`
- [ ] T005 [P] [US1] `AttributeEntrySyntax` の基底クラスを `BlockSyntax` → `SyntaxNode` に変更し、AsciiDoc 仕様のブロックではない旨の `<remarks>` を追記する（FR-002、FR-005） → `Source/AsciiSharp/Syntax/AttributeEntrySyntax.cs`
- [ ] T006 [P] [US1] `DocumentHeaderSyntax` の基底クラスを `BlockSyntax` → `SyntaxNode` に変更し、AsciiDoc 仕様に登場しない内部概念である旨の `<remarks>` を追記する（FR-002、FR-005） → `Source/AsciiSharp/Syntax/DocumentHeaderSyntax.cs`
- [ ] T007 [P] [US1] `DocumentBodySyntax` の基底クラスを `BlockSyntax` → `SyntaxNode` に変更し、AsciiDoc 仕様に登場しない内部概念である旨の `<remarks>` を追記する（FR-002、FR-005） → `Source/AsciiSharp/Syntax/DocumentBodySyntax.cs`
- [ ] T008 [US1] `BlockInlineSyntaxFeature` の `セクション関連ノードはBlockSyntaxとして識別できる()` シナリオを更新する（`SectionTitleノードはBlockSyntax()` を `SectionTitleノードはBlockSyntaxではない()` に変更し、シナリオ名も変更する） → `Test/AsciiSharp.Specs/Features/BlockInlineSyntaxFeature.cs` + `Test/AsciiSharp.Specs/Features/BlockInlineSyntaxFeature.Steps.cs`
- [ ] T009 [US1] `ElementTypeSemantics006Feature.Steps.cs` の US1 ステップを実装する（`SectionTitleSyntaxはBlockSyntaxではない`・`DocumentHeaderSyntaxはBlockSyntaxではない`・`AuthorLineSyntaxはBlockSyntaxではない`・`AttributeEntrySyntaxはBlockSyntaxではない` の各 `Assert.Inconclusive` を実際のアサーションに置き換える） → `Test/AsciiSharp.Specs/Features/ElementTypeSemantics006Feature.Steps.cs`

**Checkpoint**: US1 完了。`is BlockSyntax` が `SectionTitleSyntax` 等に対して `false` を返すことが BDD テストで検証済み。

---

## Phase 3: User Story 2 - StructuredTriviaSyntax が SyntaxNode として扱える（Priority: P2）

**Goal**: `StructuredTriviaSyntax` 抽象クラスを新規作成し、`SyntaxNode` を継承していることが BDD テストで確認できる。

**Independent Test**:
```
dotnet test --project Test/AsciiSharp.Specs/AsciiSharp.Specs.csproj
```
`StructuredTriviaSyntaxはSyntaxNodeを継承している` シナリオが Passed になること。

### Implementation for User Story 2

- [ ] T010 [US2] `StructuredTriviaSyntax` 抽象クラスを新規作成する。`SyntaxNode` を継承し、「トリビアであるが内部に構文構造を持つノードの抽象基底クラスである」旨の XML ドキュメントコメントを記述する（FR-003、FR-006） → `Source/AsciiSharp/Syntax/StructuredTriviaSyntax.cs`
- [ ] T011 [US2] `ElementTypeSemantics006Feature.Steps.cs` の US2 ステップを実装する（`StructuredTriviaSyntaxはSyntaxNodeのサブクラスである`・`StructuredTriviaSyntaxはBlockSyntaxのサブクラスではない`・`StructuredTriviaSyntaxはInlineSyntaxのサブクラスではない` の各 `Assert.Inconclusive` をリフレクションを用いたアサーションに置き換える） → `Test/AsciiSharp.Specs/Features/ElementTypeSemantics006Feature.Steps.cs`

**Checkpoint**: US2 完了。`StructuredTriviaSyntax` が `SyntaxNode` を継承し `BlockSyntax` でも `InlineSyntax` でもないことが BDD テストで検証済み。

---

## Phase 4: ポリッシュ

**目的**: ビルド・テストの最終確認、品質ゲートのクリア。

- [ ] T012 ビルドと全テストを実行し、ビルド警告ゼロ・全テスト通過を確認する（SC-001〜SC-006、FR-007） → ソリューション全体

---

## 依存関係グラフ

```
T001 ──┬── T003 ─┐
T002 ──┘   T004 ─┤
           T005 ─┤
           T006 ─┤
           T007 ─┤
                 └── T008 ── T009 ──┬── T012
                                    │
T010 ── T011 ──────────────────────┘
```

- T001・T002 は独立して並行実行可能
- T003〜T007 は T001・T002 完了後、互いに並行実行可能
- T008 は T003 完了（`SectionTitleSyntax` の基底クラス変更）を要する
- T009 は T003〜T007・T008 完了を要する
- T010 は T001 完了後に独立して実行可能
- T011 は T010 完了を要する
- T012 は T009・T011 完了後

## 並行実行の例

**US1 Phase（T003〜T007 を並行）**:
```bash
# 同時に実行可能なタスク
T003: SectionTitleSyntax 変更
T004: AuthorLineSyntax 変更
T005: AttributeEntrySyntax 変更
T006: DocumentHeaderSyntax 変更
T007: DocumentBodySyntax 変更
```

## 実装戦略

### MVP スコープ（US1 のみ）

Phase 1 → Phase 2（US1）を完了することで、`BlockSyntax` の意味が AsciiDoc 言語仕様と一致した最小実装が得られる。

### インクリメンタル デリバリー

1. Phase 1（T001・T002）: 前提クリーンアップ
2. Phase 2（T003〜T009）: US1 完成（MVP）
3. Phase 3（T010・T011）: US2 完成
4. Phase 4（T012）: 最終確認

## タスク数サマリー

| フェーズ | タスク数 |
|---------|---------|
| Phase 1（前提クリーンアップ） | 2 |
| Phase 2（US1） | 7 |
| Phase 3（US2） | 2 |
| Phase 4（ポリッシュ） | 1 |
| **合計** | **12** |

並行実行可能タスク: T001・T002（独立）、T003〜T007（US1 内並行）
