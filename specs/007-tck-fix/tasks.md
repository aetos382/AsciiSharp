# Tasks: TCK block/document/body-only テスト修正

**Input**: Design documents from `/specs/007-tck-fix/`
**Prerequisites**: plan.md ✅, spec.md ✅, data-model.md ✅

**Organization**: User Story は 1 件のみ（US1）。変更ファイルは 2 つ（Asg: `AsgDocument.cs`、`AsgConverter.cs`）。ASG の動作検証は TCK のみで行う。

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 並列実行可能（異なるファイル、依存なし）
- **[Story]**: 所属するユーザーストーリー（US1）

---

## Phase 1: ベースライン確認

**Purpose**: 変更前の状態（ビルド・テスト）を確認し、修正後の比較基準を確立する

- [ ] T001 `dotnet build` でソリューション全体がビルドできることを確認する
- [ ] T002 `dotnet test` で既存のすべてのテストが通ることを確認する

**Checkpoint**: ビルドとテストが通っている状態を確認してから変更を開始する

---

## Phase 2: User Story 1 - ボディのみのドキュメントをASGに変換する (Priority: P1) 🎯 MVP

**Goal**: ヘッダーなしの AsciiDoc ドキュメントを ASG に変換した際、`attributes` フィールドが JSON 出力から省略される（TCK `block/document/body-only` テストがパスする）

**Independent Test**: TCK を実行し `block/document/body-only` がパス（✔）に変わることで検証する

### Implementation for User Story 1

- [ ] T003 [US1] `Attributes` プロパティに `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` を付与し、型を `IReadOnlyDictionary<string, string>?`（null 許容）に変更し、デフォルト値を削除する（`Source/AsciiSharp.Asg/Models/AsgDocument.cs`）
- [ ] T004 [US1] `VisitDocument` で `attributes` 変数の設定を `node.Header is not null ? ConvertAttributes(node.Header) : null` に変更する（`Source/AsciiSharp.Asg/AsgConverter.cs`）
- [ ] T005 [US1] `dotnet build` でビルドが成功し警告がゼロであることを確認する
- [ ] T006 [US1] `dotnet test` ですべての既存テストが通ることを確認する（Green 確認）
- [ ] T007 [US1] `docker buildx bake tck` で TCK Docker イメージをビルドする
- [ ] T008 [US1] `docker run --rm asciisharp-tck` で TCK を実行し、`block/document/body-only` がパス（✔）に変わることを確認する

**Checkpoint**: T008 完了時点で US1 の受け入れ基準（SC-001）を満たす

---

## Phase 3: Polish & 最終確認

**Purpose**: 変更の影響範囲を確認し、既存のパス済みテストが維持されていることを確認する

- [ ] T009 TCK の出力を確認し、以下を検証する:
  - `block/document/body-only` がパス（✔）になっていること（SC-001）
  - `block/header/attribute entries below title` が引き続きパス（✔）であること（SC-003）
  - 通過テスト数が 1 件以上増加していること（SC-002）
  - `block/document/header-body` テストが今回の変更で新たなエラーを発生させていないこと（FR-005）
- [ ] T010 `VisitDocument` のコメントが変更の意図（FR-001、FR-002）を説明していることを確認する（`Source/AsciiSharp.Asg/AsgConverter.cs`）

---

## Dependencies & Execution Order

```
T001, T002（並列可）
      ↓
T003 → T004 → T005 → T006 → T007 → T008
                                       ↓
                                   T009, T010
```

---

## Notes

- Asg 側の変更は 2 ファイルのみ（`AsgDocument.cs`、`AsgConverter.cs`）
- ASG の動作は TCK による外部検証のみで確認する
- TCK イメージのビルドには数分かかる場合がある
- `docker buildx bake tck` は Docker BuildKit が有効な環境で実行する
