# Tasks: SectionTitleSyntax の構成改定と TextSyntax のリネーム

**Input**: Design documents from `/specs/001-section-title-inline/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, quickstart.md

**BDD**: このフィーチャーはコア ライブラリを対象とするため、BDD（Red-Green-Refactor）が必須です。

**Organization**: タスクはユーザー ストーリーごとにグループ化されています。

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 並列実行可能（異なるファイル、依存関係なし）
- **[Story]**: タスクが属するユーザー ストーリー（例: US1, US2, US3）
- 説明には正確なファイル パスを含む

## Path Conventions

```text
Source/
├── AsciiSharp/                           # コア ライブラリ
│   ├── Syntax/
│   │   ├── SectionTitleSyntax.cs
│   │   ├── TextSyntax.cs → InlineTextSyntax.cs
│   │   ├── ISyntaxVisitor.cs
│   │   └── ISyntaxVisitorOfT.cs
│   └── SyntaxKind.cs
├── AsciiSharp.Asg/
│   └── AsgConverter.cs
Test/
├── AsciiSharp.Specs/
│   ├── Features/
│   └── StepDefinitions/
```

---

## Phase 1: Setup

**Purpose**: パッケージ依存関係の追加

- [x] T001 `dotnet package add` で System.Collections.Immutable パッケージを Source/AsciiSharp/AsciiSharp.csproj に追加
- [x] T002 Source/AsciiSharp/AsciiSharp.csproj を編集し、パッケージ参照を .NET Standard 2.0 のみに条件付きで限定

**Checkpoint**: ImmutableArray<T> が使用可能になる

---

## Phase 2: Foundational (BDD Red - .feature ファイル作成)

**Purpose**: BDD テストの基盤となる .feature ファイルの作成（Red ステップ）

**⚠️ CRITICAL**: .feature ファイル作成後、テストを実行して失敗することを確認してから実装に進む

- [x] T003 [P] Test/AsciiSharp.Specs/Features/SectionTitleInlineElements.feature を作成（US1 用）
- [x] T004 [P] Test/AsciiSharp.Specs/Features/InlineTextSyntaxRename.feature を作成（US2 用）
- [x] T005 [P] Test/AsciiSharp.Specs/Features/SectionTitleTrivia.feature を作成（US3 用）
- [x] T006 テストを実行し、すべての新規シナリオが失敗することを確認（Red 確認）

**Checkpoint**: BDD Red ステップ完了 - 失敗するテストが存在する

---

## Phase 3: User Story 2 - TextSyntax が InlineTextSyntax として参照される (Priority: P2)

**Goal**: TextSyntax を InlineTextSyntax にリネームし、すべての参照箇所を更新する

**Independent Test**: `grep -r "TextSyntax" Source/` が 0 件になること

**Note**: US1 が依存するため、優先度 P2 だが先に実装する

### Implementation for User Story 2

- [x] T007 [US2] Source/AsciiSharp/Syntax/TextSyntax.cs を InlineTextSyntax.cs にリネームし、クラス名を InlineTextSyntax に変更
- [x] T008 [US2] Source/AsciiSharp/SyntaxKind.cs の `Text` を `InlineText` にリネーム
- [x] T009 [P] [US2] Source/AsciiSharp/Syntax/ISyntaxVisitor.cs の `VisitText` を `VisitInlineText` にリネーム
- [x] T010 [P] [US2] Source/AsciiSharp/Syntax/ISyntaxVisitorOfT.cs の `VisitText` を `VisitInlineText` にリネーム
- [x] T011 [US2] Source/AsciiSharp/Syntax/InlineTextSyntax.cs の Accept メソッドを更新（visitor.VisitInlineText）
- [x] T012 [US2] Source/AsciiSharp/Syntax/ParagraphSyntax.cs の SyntaxKind.Text 参照を SyntaxKind.InlineText に更新
- [x] T013 [P] [US2] Test/AsciiSharp.Specs/StepDefinitions/VisitorSteps.cs の VisitText 参照を更新
- [ ] T014 [US2] ビルドとテストを実行し、US2 関連のシナリオが成功することを確認（Green）
- [ ] T015 [US2] コード整形とリファクタリング（Refactor）

**Checkpoint**: TextSyntax への参照が 0 件、InlineTextSyntax に統一される

---

## Phase 4: User Story 1 - セクションタイトルの構文木からインライン要素を取得する (Priority: P1)

**Goal**: SectionTitleSyntax に ImmutableArray<InlineSyntax> InlineElements プロパティを追加し、TitleContent を削除する

**Independent Test**: `== Hello World` をパースして InlineElements に単一の InlineTextSyntax が含まれること

### Implementation for User Story 1

- [ ] T016 [US1] Source/AsciiSharp/Syntax/SectionTitleSyntax.cs に ImmutableArray<InlineSyntax> InlineElements プロパティを追加
- [ ] T017 [US1] Source/AsciiSharp/Syntax/SectionTitleSyntax.cs のコンストラクタを更新し、InlineElements を構築
- [ ] T018 [US1] Source/AsciiSharp/Syntax/SectionTitleSyntax.cs から TitleContent プロパティを削除
- [ ] T019 [US1] Source/AsciiSharp/Syntax/SectionTitleSyntax.cs から TitleText プロパティを削除
- [ ] T020 [US1] Source/AsciiSharp/Syntax/SectionTitleSyntax.cs の ChildNodesAndTokens() を更新し、InlineElements を含める
- [ ] T021 [US1] Source/AsciiSharp.Asg/AsgConverter.cs の TitleContent 参照を InlineElements 経由に更新
- [ ] T022 [P] [US1] Test/AsciiSharp.Specs/StepDefinitions/BasicParsingSteps.cs の TitleContent 参照を更新
- [ ] T023 [P] [US1] Test/AsciiSharp.Specs/StepDefinitions/CommentParsingSteps.cs の TitleContent 参照を更新
- [ ] T024 [P] [US1] Test/AsciiSharp.Specs/StepDefinitions/IncrementalParsingSteps.cs の TitleContent 参照を更新
- [ ] T025 [P] [US1] Test/AsciiSharp.Specs/StepDefinitions/VisitorSteps.cs の TitleContent 参照を更新
- [ ] T026 [US1] InlineElements の順序検証テストを追加（各要素の Position は前の要素以上）
- [ ] T027 [US1] ビルドとテストを実行し、US1 関連のシナリオが成功することを確認（Green）
- [ ] T028 [US1] コード整形とリファクタリング（Refactor）

**Checkpoint**: SectionTitleSyntax.InlineElements が機能し、TitleContent への参照が 0 件

---

## Phase 5: User Story 3 - SectionTitleSyntax の空白トリビアが適切に保持される (Priority: P3)

**Goal**: マーカー（`=`）とタイトル本文の間の空白がマーカーの TrailingTrivia として保持され、ToFullString() で完全に復元される

**Independent Test**: `==  タイトル`（2つのスペース）をパースして ToFullString() で完全復元すること

### Implementation for User Story 3

- [ ] T029 [US3] Source/AsciiSharp/Syntax/SectionTitleSyntax.cs でマーカー後の空白を TrailingTrivia として処理
- [ ] T030 [US3] 空白なし（`==タイトル`）のケースが正しく処理されることを確認
- [ ] T031 [US3] 複数空白（`==  タイトル`）のケースが正しく処理されることを確認
- [ ] T032 [US3] ビルドとテストを実行し、US3 関連のシナリオが成功することを確認（Green）
- [ ] T033 [US3] コード整形とリファクタリング（Refactor）

**Checkpoint**: ToFullString() による完全な復元が保証される

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: 全体の整合性確認と最終調整

- [ ] T034 すべてのテストを実行し、100% 成功することを確認
- [ ] T035 ビルド警告がゼロであることを確認
- [ ] T036 [P] `grep -r "TextSyntax" Source/ Test/` で TextSyntax への参照が 0 件であることを確認
- [ ] T037 [P] `grep -r "TitleContent" Source/ Test/` で TitleContent への参照が 0 件であることを確認
- [ ] T038 [P] `grep -r "SyntaxKind\.Text[^T]" Source/ Test/` で SyntaxKind.Text への参照が 0 件であることを確認
- [ ] T039 quickstart.md の API 使用例が動作することを確認

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: 依存なし - 即座に開始可能
- **Foundational (Phase 2)**: Setup 完了後 - .feature ファイル作成、Red 確認
- **User Story 2 (Phase 3)**: Foundational 完了後 - リネーム作業
- **User Story 1 (Phase 4)**: User Story 2 完了後 - InlineTextSyntax を使用するため
- **User Story 3 (Phase 5)**: User Story 1 完了後 - SectionTitleSyntax の構造変更後
- **Polish (Phase 6)**: すべてのユーザー ストーリー完了後

### User Story Dependencies

```
US2 (TextSyntax リネーム)
  ↓
US1 (InlineElements 追加) ← US2 に依存（InlineTextSyntax を使用）
  ↓
US3 (トリビア処理) ← US1 に依存（SectionTitleSyntax 構造変更後）
```

### Within Each User Story

1. .feature ファイル作成（Red）
2. 実装（Green）
3. リファクタリング（Refactor）
4. コミット

### Parallel Opportunities

- T003, T004, T005: .feature ファイル作成は並列実行可能
- T009, T010: ISyntaxVisitor の更新は並列実行可能
- T013: VisitorSteps の更新は他の StepDefinitions と並列実行可能
- T022, T023, T024, T025: StepDefinitions の TitleContent 参照更新は並列実行可能
- T036, T037, T038: 最終確認の grep は並列実行可能

---

## Parallel Example: Phase 2 (Foundational)

```bash
# Launch all .feature file creation together:
Task: "Test/AsciiSharp.Specs/Features/SectionTitleInlineElements.feature を作成"
Task: "Test/AsciiSharp.Specs/Features/InlineTextSyntaxRename.feature を作成"
Task: "Test/AsciiSharp.Specs/Features/SectionTitleTrivia.feature を作成"
```

## Parallel Example: User Story 1 (StepDefinitions 更新)

```bash
# Launch all StepDefinitions updates together:
Task: "Test/AsciiSharp.Specs/StepDefinitions/BasicParsingSteps.cs の TitleContent 参照を更新"
Task: "Test/AsciiSharp.Specs/StepDefinitions/CommentParsingSteps.cs の TitleContent 参照を更新"
Task: "Test/AsciiSharp.Specs/StepDefinitions/IncrementalParsingSteps.cs の TitleContent 参照を更新"
Task: "Test/AsciiSharp.Specs/StepDefinitions/VisitorSteps.cs の TitleContent 参照を更新"
```

---

## Implementation Strategy

### MVP First (User Story 2 + 1)

1. Complete Phase 1: Setup（パッケージ追加）
2. Complete Phase 2: Foundational（.feature ファイル作成、Red 確認）
3. Complete Phase 3: User Story 2（リネーム）
4. Complete Phase 4: User Story 1（InlineElements）
5. **STOP and VALIDATE**: テスト実行、独立検証
6. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → 基盤準備完了
2. User Story 2 完了 → リネーム完了
3. User Story 1 完了 → コア機能完了（MVP!）
4. User Story 3 完了 → トリビア処理完了
5. Polish → 最終調整

---

## Notes

- [P] タスク = 異なるファイル、依存関係なし
- [Story] ラベル = タスクを特定のユーザー ストーリーにマッピング
- 各ユーザー ストーリーは独立して完了・テスト可能（ただし依存関係あり）
- テストが失敗することを確認してから実装
- 各タスクまたは論理グループ後にコミット
- 任意のチェックポイントで停止してストーリーを独立検証可能
