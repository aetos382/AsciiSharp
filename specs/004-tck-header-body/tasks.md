# Tasks: TCK header-body-output テスト対応

**Input**: Design documents from `/specs/004-tck-header-body/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, quickstart.md

**Organization**: タスクはユーザー ストーリー単位で構成。各ストーリーは独立して実装・テスト可能。

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 並行実行可能（異なるファイル、依存なし）
- **[Story]**: 対応するユーザー ストーリー（US1, US2, US3）

## Phase 1: US1 - TCK header-body-output の ASG 出力 (Priority: P1) 🎯 MVP

**Goal**: AsgDocument に `attributes` フィールドを追加し、ヘッダー + ボディ ドキュメントの ASG 出力が TCK 期待出力と一致するようにする

**Independent Test**: `echo '{"contents":"= Document Title\n\nbody\n","path":"test.adoc","type":"block"}' | dotnet run --project Source/AsciiSharp.TckAdapter/` の出力が `header-body-output.json` と一致する

### Implementation for User Story 1

- [x] T001 [P] [US1] AsgDocument に `Attributes` プロパティ（`IReadOnlyDictionary<string, string>`）を追加する。デフォルト値は空の辞書とする — `Source/AsciiSharp.Asg/Models/AsgDocument.cs`
- [x] T002 [P] [US1] AsgJsonContext に `IReadOnlyDictionary<string, string>` の `[JsonSerializable]` 登録を追加する — `Source/AsciiSharp.Asg/Serialization/AsgJsonContext.cs`
- [x] T003 [US1] AsgConverter の `Convert()` メソッドで `Attributes` に空辞書を設定する — `Source/AsciiSharp.Asg/AsgConverter.cs`
- [x] T004 [US1] AsgConverterTests に header-body の attributes 出力テスト（空オブジェクト）を追加する — `Test/AsciiSharp.Asg.Tests/AsgConverterTests.cs`
- [x] T005 [US1] TckAdapter で header-body-input.adoc を手動実行し、期待出力との差分を確認・修正する

**Checkpoint**: `header-body-output` テスト ケースが TCK と一致する。既存テストが全てパスする。

---

## Phase 2: US2 - 属性エントリの SyntaxTree レベルでのパース (Priority: P2)

**Goal**: パーサーが `:name: value` 形式の属性エントリをドキュメント ヘッダー内で認識し、構文木ノードとして保持する

**Independent Test**: `:icons: font` を含むドキュメントをパースし、`DocumentHeaderSyntax.AttributeEntries` から属性名と値を取得できる

**設計上の重要事項**: `DocumentHeaderSyntax.AttributeEntries` は常に非 null とする（`SyntaxList<AttributeEntrySyntax>`）。属性エントリがないドキュメント ヘッダーでも空の `SyntaxList` を返す。これは TCK がドキュメント属性がない文書でも `"attributes": {}` を要求していることと整合する。構文木の子ノード コレクションには Roslyn の設計哲学に準拠して `SyntaxList<T>` を使用する（D-006, R-009）。

### BDD Red ステップ

- [x] T006 [US2] BDD ステップ定義を作成する（コンパイル可能な最小限のスケルトン）— `Test/AsciiSharp.Specs/StepDefinitions/AttributeEntrySteps.cs`

### SyntaxList<T> 移行（D-006 前提作業）

- [x] T007a [US2] `SectionTitleSyntax.InlineElements` を `ImmutableArray<InlineSyntax>` から `SyntaxList<InlineSyntax>` に変更する。主な変更点は `.Length` → `.Count`。影響ファイル:
  - `Source/AsciiSharp/Syntax/SectionTitleSyntax.cs` — プロパティ型変更、ビルダー → `SyntaxList` コンストラクタ
  - `Source/AsciiSharp.Asg/AsgConverter.cs` — `InlineElements.Length` → `.Count`
  - `Test/AsciiSharp.Specs/StepDefinitions/SectionTitleInlineElementsSteps.cs` — `InlineElements.Length` → `.Count` (複数箇所)

### Implementation for User Story 2

- [x] T007 [US2] `SyntaxKind` に `AttributeEntry` 列挙値を追加する — `Source/AsciiSharp/SyntaxKind.cs`
- [x] T008 [P] [US2] `ISyntaxVisitor` に `VisitAttributeEntry(AttributeEntrySyntax node)` を追加する — `Source/AsciiSharp/Syntax/ISyntaxVisitor.cs`
- [x] T009 [P] [US2] `ISyntaxVisitor<TResult>` に `VisitAttributeEntry(AttributeEntrySyntax node)` を追加する — `Source/AsciiSharp/Syntax/ISyntaxVisitor.cs` もしくは対応するファイル
- [x] T010 [US2] `AttributeEntrySyntax` (Red Tree ノード) を作成する。`Name` プロパティ（属性名テキスト）と `Value` プロパティ（属性値テキスト、空可）、`Accept` メソッドを含む — `Source/AsciiSharp/Syntax/AttributeEntrySyntax.cs`
- [x] T011 [US2] `DocumentHeaderSyntax` に `AttributeEntries` プロパティ（`SyntaxList<AttributeEntrySyntax>`）を追加する。コンストラクタの switch 文に `SyntaxKind.AttributeEntry` ケースを追加する。**属性エントリがない場合でも空の `SyntaxList` を返すこと** — `Source/AsciiSharp/Syntax/DocumentHeaderSyntax.cs`
- [x] T012 [US2] `Parser.ParseAttributeEntry()` メソッドを実装する。D-001 の Green Tree 構造（開きコロン + 属性名 + 閉じコロン [trailingTrivia: 空白] + 属性値 [trailingTrivia: 改行]）に従う — `Source/AsciiSharp/Parser/Parser.cs`
- [x] T013 [US2] `ParseDocumentHeader()` を拡張し、タイトル・著者行の後に属性エントリ行（行頭 `ColonToken`）を認識するループを追加する — `Source/AsciiSharp/Parser/Parser.cs`
- [x] T014 [US2] 既存の Visitor 実装を `VisitAttributeEntry` に対応させる（AsgConverter 含む）— 各 Visitor 実装ファイル
- [x] T015 [US2] 属性エントリのないドキュメント ヘッダーで `AttributeEntries` が空の `SyntaxList`（null でない）であることを BDD テストで検証する — `Test/AsciiSharp.Specs/StepDefinitions/AttributeEntrySteps.cs`

**Checkpoint**: `AttributeEntryParsing.feature` の全シナリオがパスする。既存テストが全てパスする。ラウンドトリップが成功する。属性エントリなしの場合も `AttributeEntries` が空コレクションである。

---

## Phase 3: US3 - 属性エントリの ASG 変換 (Priority: P3)

**Goal**: ドキュメント ヘッダー内の属性エントリを ASG の `attributes` フィールドにキー・値ペアとして出力する

**Independent Test**: `:icons: font` と `:toc:` を含むドキュメントの ASG 出力に `"attributes": { "icons": "font", "toc": "" }` が含まれる

**Dependencies**: US1（Attributes プロパティ）+ US2（パーサーの属性エントリ認識）が完了していること

### Implementation for User Story 3

- [x] T016 [US3] AsgConverter の `ConvertHeader()` または `VisitDocument()` で `DocumentHeaderSyntax.AttributeEntries` から `Dictionary<string, string>` を構築し、`AsgDocument.Attributes` に設定する。値なし属性は空文字列とする — `Source/AsciiSharp.Asg/AsgConverter.cs`
- [x] T017 [US3] AsgConverterTests に属性エントリ付きドキュメントの ASG 変換テストを追加する（値あり・値なし・複数属性・属性エントリなし）— `Test/AsciiSharp.Asg.Tests/AsgConverterTests.cs`

**Checkpoint**: 属性エントリ付きドキュメントの ASG 出力が TCK 期待と一致する。全テストがパスする。

---

## Phase 4: Polish & Cross-Cutting Concerns

**Purpose**: リファクタリング、警告解消、最終検証

- [x] T018 ビルド警告を解消する（警告ゼロポリシー）
- [x] T019 quickstart.md の手動検証手順を実行し、出力を確認する
- [x] T020 `dotnet test` で全テスト（BDD + ユニット テスト）がパスすることを確認する

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (US1)**: 独立して開始可能。ASG モデルの変更のみ
- **Phase 2 (US2)**: 独立して開始可能。コア パーサーの変更。BDD で検証
- **Phase 3 (US3)**: Phase 1 と Phase 2 の両方が完了後に開始
- **Phase 4 (Polish)**: 全ユーザー ストーリーが完了後

### User Story Dependencies

```
US1 (ASG attributes 空出力) ──────┐
                                   ├──→ US3 (属性エントリ ASG 変換)
US2 (SyntaxTree パース) ──────────┘
```

- **US1**: 独立（ASG モデル + コンバーターの最小変更）
- **US2**: 独立（コア パーサー + Red Tree + BDD テスト）
- **US3**: US1 + US2 に依存（両方の成果を組み合わせる）

### Within Each User Story

- US1: モデル変更 (T001, T002) → コンバーター変更 (T003) → テスト (T004) → 手動検証 (T005)
- US2: BDD スケルトン (T006) → SyntaxList 移行 (T007a) → SyntaxKind (T007) → Visitor (T008, T009) → Red Tree (T010) → Header 更新 (T011) → Parser (T012, T013) → Visitor 実装更新 (T014) → 空コレクション検証 (T015)
- US3: コンバーター変更 (T016) → テスト (T017)

### Parallel Opportunities

```bash
# US1 内での並行:
# T001 と T002 は異なるファイルのため並行可能

# US2 内での並行:
# T008 と T009 は Visitor インターフェースの異なるファイルのため並行可能

# ストーリー間の並行:
# US1 と US2 は独立しているため並行可能
```

---

## Implementation Strategy

### MVP First (US1 Only)

1. Phase 1 (US1) を完了 → TCK header-body-output テストがパス
2. **STOP and VALIDATE**: TckAdapter で手動検証
3. 既存テスト全パスを確認

### Incremental Delivery

1. US1 完了 → TCK header-body-output テストがパス（MVP!）
2. US2 完了 → 属性エントリのパースが可能に（BDD テストがパス）
3. US3 完了 → 属性エントリの ASG 変換が可能に（TCK attribute-entries テストへの準備完了）
4. Polish → 警告ゼロ、最終検証

---

## Notes

- BDD テスト（AttributeEntryParsing.feature）はコア ライブラリ（AsciiSharp）の変更を検証する（US2）
- ASG の変更（US1, US3）はユニット テスト（AsgConverterTests）で検証する
- `DocumentHeaderSyntax.AttributeEntries` は常に非 null。属性エントリがなくても空の `SyntaxList` を返す
- 構文木の子ノード コレクションには `SyntaxList<T>` を使用する（D-006, R-009）。`SectionTitleSyntax.InlineElements` も同時に移行する（T007a）
- T004 (US1: 空 attributes テスト) と T017 (US3: attributes テスト) は「属性なし → 空 `{}`」の検証が重複するが、異なるフェーズで独立検証するための意図的な重複である
- `[JsonPropertyOrder]` は使用しない（R-005）
- 閉じコロン後の空白と改行はトリビアとして扱う（D-001）
- Commit は各タスク完了時または BDD サイクル完了時に行う
