# Tasks: SectionTitleSyntax の構成改定と TextSyntax のリネーム

**Input**: Design documents from `/specs/001-section-title-inline/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, quickstart.md

**BDD**: このフィーチャーはコア ライブラリを対象とするため、BDD（Red-Green-Refactor）が必須。
.feature ファイルは plan フェーズで作成済み、Red 確認済み。

**Organization**: タスクはユーザーストーリーごとにグループ化。大部分は実装済みのため、残タスクは US3 の Green フェーズと Polish のみ。

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 並行実行可能（異なるファイル、依存関係なし）
- **[Story]**: 対応するユーザーストーリー（US1, US2, US3）
- ファイルパスは正確に記載

---

## Phase 1: Setup (共有インフラ)

**Purpose**: Lexer の変更と基盤整備

- [x] T001 Lexer で連続する `=` を単一トークンにまとめる変更を実装 in `Source/AsciiSharp/Parser/Lexer.cs`
- [x] T002 `UnreachableException` の Polyfill を追加 in `Polyfills/UnreachableException.cs`
- [x] T003 `Peek(offset)` の到達不能コードを `UnreachableException` に置換 in `Source/AsciiSharp/Parser/Parser.cs`

---

## Phase 2: User Story 2 - TextSyntax が InlineTextSyntax として参照される (Priority: P2)

**Goal**: `TextSyntax` を `InlineTextSyntax` にリネームし、`SyntaxKind.Text` を `SyntaxKind.InlineText` に統一する

**Independent Test**: コード全体で `TextSyntax` および `SyntaxKind.Text` への参照が 0 件であることを確認

**Note**: US1 が InlineTextSyntax を使用するため、先に実装

### 実装 (完了済み)

- [x] T004 [US2] `TextSyntax` を `InlineTextSyntax` にリネーム in `Source/AsciiSharp/Syntax/InlineTextSyntax.cs`（旧 `TextSyntax.cs`）
- [x] T005 [US2] `SyntaxKind.Text` を `SyntaxKind.InlineText` にリネーム in `Source/AsciiSharp/SyntaxKind.cs`
- [x] T006 [P] [US2] `ISyntaxVisitor.VisitText` を `VisitInlineText` にリネーム in `Source/AsciiSharp/Syntax/ISyntaxVisitor.cs`
- [x] T007 [P] [US2] `ISyntaxVisitor<T>.VisitText` を `VisitInlineText` にリネーム in `Source/AsciiSharp/Syntax/ISyntaxVisitorOfT.cs`
- [x] T008 [US2] すべての参照箇所を更新しビルド＆テスト成功を確認

**Checkpoint**: US2 完了。`TextSyntax` と `SyntaxKind.Text` への参照が 0 件。

---

## Phase 3: User Story 1 - セクションタイトルの構文木からインライン要素を取得する (Priority: P1)

**Goal**: SectionTitleSyntax を「= マーカー + 空白トリビア + InlineSyntax コレクション」構成に改定し、インライン要素として走査可能にする

**Independent Test**: `== タイトルテキスト` をパースし、SectionTitleSyntax の InlineElements が単一の InlineTextSyntax を含むことを検証

### BDD テスト (完了済み)

- [x] T009 [US1] .feature ファイルを作成 in `Test/AsciiSharp.Specs/Features/SectionTitleInlineElements.feature`
- [x] T010 [US1] ステップ定義を作成 in `Test/AsciiSharp.Specs/StepDefinitions/SectionTitleInlineElementsSteps.cs`

### 実装 (完了済み)

- [x] T011 [US1] SectionTitleSyntax を `_children` パターンに移行し、`Marker`/`Level`/`InlineElements` プロパティを追加 in `Source/AsciiSharp/Syntax/SectionTitleSyntax.cs`
- [x] T012 [US1] `TitleContent` プロパティを削除 in `Source/AsciiSharp/Syntax/SectionTitleSyntax.cs`
- [x] T013 [US1] Parser の `ParseSectionTitle()` で単一マーカートークン + 空白 TrailingTrivia を処理 in `Source/AsciiSharp/Parser/Parser.cs`
- [x] T014 [US1] `TitleContent` 参照箇所をインライン要素ベースに更新 in `Source/AsciiSharp.Asg/AsgConverter.cs`, `Test/AsciiSharp.Specs/StepDefinitions/`

**Checkpoint**: US1 完了。`== Hello` パース時に InlineElements が単一の InlineTextSyntax を含む。

---

## Phase 4: User Story 3 - セクション見出し認識条件の厳格化と空白トリビア保持 (Priority: P3)

**Goal**: `=` が 7 個以上の行、および `=` 後に空白がない行をセクション見出しではなく段落として扱う。空白トリビアの完全な復元を保証する。

**Independent Test**: `======= Title` がパース後に ParagraphSyntax となること。`==タイトル` がパース後に ParagraphSyntax となること。`==  タイトル`（空白2つ）の `ToFullString()` が入力と完全一致すること。

### BDD テスト (Red 確認済み)

- [x] T015 [US3] 認識条件の .feature ファイルを作成（5 シナリオ） in `Test/AsciiSharp.Specs/Features/SectionTitleRecognition.feature`
- [x] T016 [US3] セクションレベル検証のステップ定義を追加 in `Test/AsciiSharp.Specs/StepDefinitions/SectionTitleInlineElementsSteps.cs`

### 実装 (Green フェーズ — 未完了)

- [ ] T017 [US3] `IsAtSectionTitle()` に `Text.Length <= 6` と `Peek().Kind == WhitespaceToken` の条件を追加 in `Source/AsciiSharp/Parser/Parser.cs`
- [ ] T018 [US3] `IsAtDocumentTitle()` に `Peek().Kind == WhitespaceToken` の条件を追加 in `Source/AsciiSharp/Parser/Parser.cs`
- [ ] T019 [US3] `IsAtSectionTitleOfLevelOrHigher()` に `Peek().Kind == WhitespaceToken` の条件を追加 in `Source/AsciiSharp/Parser/Parser.cs`
- [ ] T020 [US3] ビルドとすべてのテスト（92 件）が成功することを確認

**Checkpoint**: US3 完了。Level 7+ と空白なしが段落として解析される。ToFullString() の完全復元が維持される。

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: リファクタリングと品質確認

- [ ] T021 ビルド警告がゼロであることを確認し、必要に応じて警告を解消
- [ ] T022 `IsAtSectionTitle()` / `IsAtDocumentTitle()` / `IsAtSectionTitleOfLevelOrHigher()` の XML ドキュメントコメントを更新 in `Source/AsciiSharp/Parser/Parser.cs`
- [ ] T023 quickstart.md の検証シナリオを手動で確認

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: 完了済み
- **Phase 2 (US2)**: Phase 1 に依存 → 完了済み
- **Phase 3 (US1)**: Phase 2 に依存（InlineTextSyntax を使用） → 完了済み
- **Phase 4 (US3)**: Phase 1 に依存、US1/US2 とは独立 → **Green フェーズ未完了**
- **Phase 5 (Polish)**: Phase 4 完了後に実行

### User Story Dependencies

- **US2 (P2)**: 他のストーリーに依存しない → 完了済み
- **US1 (P1)**: US2 に依存（InlineTextSyntax を使用） → 完了済み
- **US3 (P3)**: 他のストーリーに依存しない → Red 完了、Green 未実施

### 残タスクの実行順序

```
T017 → T018 → T019 → T020 → T021 → T022 → T023
```

T017〜T019 はすべて同一ファイル（Parser.cs）への変更のため並行実行不可。ただし変更量が小さいため、一括で実施して T020 で検証するのが効率的。

---

## Parallel Example: User Story 3

```text
# T017〜T019 は同一ファイルのため順次実行:
T017: IsAtSectionTitle() に条件追加
T018: IsAtDocumentTitle() に条件追加
T019: IsAtSectionTitleOfLevelOrHigher() に条件追加

# 実装完了後に検証:
T020: dotnet build && dotnet test
```

---

## Implementation Strategy

### 残タスクの実行方針

1. **T017〜T019**: Parser.cs の 3 メソッドを一括変更（変更量が極めて小さいため）
2. **T020**: ビルド＆テスト実行で Green 確認
3. **T021〜T022**: Refactor フェーズとして警告解消と XML ドキュメント更新
4. **T023**: quickstart.md の検証

### サマリ

| カテゴリ | タスク数 | 状態 |
|---------|---------|------|
| Phase 1: Setup | 3 | 完了 |
| Phase 2: US2 | 5 | 完了 |
| Phase 3: US1 | 6 | 完了 |
| Phase 4: US3 (Red) | 2 | 完了 |
| Phase 4: US3 (Green) | 4 | 未実施 |
| Phase 5: Polish | 3 | 未実施 |
| **合計** | **23** | **完了 16 / 残 7** |

---

## Notes

- [P] タスク = 異なるファイル、依存関係なし
- [Story] ラベル = タスクを特定のユーザーストーリーにマッピング
- 各ユーザーストーリーは独立して完了・テスト可能
- テストが失敗することを確認してから実装（Red-Green-Refactor）
- 各タスクまたは論理グループ後にコミット
- 任意のチェックポイントで停止してストーリーを独立検証可能
