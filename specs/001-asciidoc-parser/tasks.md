# Tasks: AsciiDoc パーサー

**Input**: Design documents from `/specs/001-asciidoc-parser/`
**Prerequisites**: plan.md, spec.md, data-model.md, research.md

**Tests**: BDD テストを Reqnroll で実装します（Constitution に基づく必須要件）

**Organization**: タスクはユーザーストーリーごとにグループ化され、各ストーリーの独立した実装とテストを可能にします。

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 並列実行可能（異なるファイル、依存関係なし）
- **[Story]**: このタスクが属するユーザーストーリー（US1, US2, US3 など）
- 説明には正確なファイルパスを含める

## パス規則

プロジェクト構造（plan.md より）:
- コアライブラリ: `Source/AsciiSharp/`
- ユニットテスト: `Test/AsciiSharp.Tests/`
- BDD テスト: `Test/AsciiSharp.Specs/`
- ベンチマーク: `Benchmark/AsciiSharp.Benchmarks/`

---

## Phase 1: Setup（共有インフラストラクチャ）

**目的**: プロジェクトの初期化と基本構造の構築

- [x] T001 プロジェクト構造を作成（Source/AsciiSharp/, Test/AsciiSharp.Tests/, Test/AsciiSharp.Specs/, Benchmark/AsciiSharp.Benchmarks/）
- [x] T002 AsciiSharp.csproj を作成（TargetFrameworks: netstandard2.0;net10.0）
- [x] T003 [P] AsciiSharp.Tests.csproj を作成（TargetFrameworks: net10.0;net481, MSTest.Sdk 参照）
- [x] T004 [P] AsciiSharp.Specs.csproj を作成（TargetFramework: net10.0, Reqnroll パッケージ参照）
- [x] T005 [P] AsciiSharp.Benchmarks.csproj を作成（TargetFramework: net10.0, BenchmarkDotNet 参照）
- [x] T006 Directory.Build.props を作成（WarningLevel=9999, AnalysisLevel=latest-all, .NET Standard 2.0 用 Polyfill 設定）
- [x] T007 [P] .editorconfig を作成（コードスタイル統一）

---

## Phase 2: Foundational（ブロッキング前提条件）

**目的**: すべてのユーザーストーリーの実装前に完了すべきコアインフラストラクチャ

**⚠️ 重要**: このフェーズが完了するまで、ユーザーストーリーの作業を開始できません

### PEG 文法定義（参照仕様）

- [x] T008 [P] PEG 文法定義ファイルを specs/001-asciidoc-parser/grammar/ に作成（FR-002 に基づく参照仕様、パーサー実装のガイドとして使用）

### コアインフラストラクチャ

- [x] T009 SyntaxKind 列挙型を Source/AsciiSharp/SyntaxKind.cs に実装（Tokens, Trivia, Nodes 定義）
- [x] T010 [P] TextSpan 構造体を Source/AsciiSharp/Text/TextSpan.cs に実装
- [x] T011 [P] DiagnosticSeverity 列挙型と Diagnostic クラスを Source/AsciiSharp/Diagnostics/Diagnostic.cs に実装
- [x] T012 [P] SourceText 抽象クラスを Source/AsciiSharp/Text/SourceText.cs に実装
- [x] T013 [P] StringText クラス（SourceText の実装）を Source/AsciiSharp/Text/StringText.cs に実装
- [x] T014 InternalNode 抽象クラスを Source/AsciiSharp/InternalSyntax/InternalNode.cs に実装
- [x] T015 [P] InternalToken クラスを Source/AsciiSharp/InternalSyntax/InternalToken.cs に実装
- [x] T016 [P] InternalTrivia 構造体を Source/AsciiSharp/InternalSyntax/InternalTrivia.cs に実装
- [x] T017 [P] InternalNodeCache クラスを Source/AsciiSharp/InternalSyntax/InternalNodeCache.cs に実装（WeakReference ベース）
- [x] T018 SyntaxNode 抽象クラスを Source/AsciiSharp/Syntax/SyntaxNode.cs に実装（外部構文木の基底）
- [x] T019 [P] SyntaxToken 構造体を Source/AsciiSharp/Syntax/SyntaxToken.cs に実装
- [x] T020 [P] SyntaxTrivia 構造体を Source/AsciiSharp/Syntax/SyntaxTrivia.cs に実装
- [x] T021 [P] SyntaxList<T> 構造体を Source/AsciiSharp/Syntax/SyntaxList.cs に実装
- [x] T022 [P] SyntaxNodeOrToken 構造体を Source/AsciiSharp/Syntax/SyntaxNodeOrToken.cs に実装
- [x] T023 SyntaxTree クラスを Source/AsciiSharp/Syntax/SyntaxTree.cs に実装（内部構文木から外部構文木への変換を含む）
- [x] T024 ITreeSink インターフェースを Source/AsciiSharp/Parser/ITreeSink.cs に実装（イベントベースパーサー用）
- [x] T025 InternalTreeBuilder クラスを Source/AsciiSharp/InternalSyntax/InternalTreeBuilder.cs に実装（ITreeSink 実装）
- [x] T026 .NET Standard 2.0 用 Polyfill を Polyfills/ に追加（IsExternalInit, RequiredMemberAttribute など）

**Checkpoint**: 基盤完了 - ユーザーストーリーの実装を並列開始可能

---

## Phase 3: User Story 1 - 基本的な AsciiDoc 文書の解析 (Priority: P1) 🎯 MVP

**Goal**: セクションと段落を含む AsciiDoc 文書を解析し、ロスレスな構文木を生成する

**MVP スコープ**: リストは後続イテレーションで実装

**Independent Test**: 単一の AsciiDoc 文書を解析し、構文木から元のテキストを再構築してラウンドトリップ検証を実行

### BDD テスト for User Story 1（Red フェーズ）

> **注意: 実装前にこれらのテストを書き、FAIL することを確認する**

- [x] T027 [P] [US1] BasicParsing.feature を Test/AsciiSharp.Specs/Features/BasicParsing.feature に作成（Acceptance Scenarios 1-3 を Given-When-Then 形式で記述）
- [x] T028 [US1] BasicParsingSteps.cs を Test/AsciiSharp.Specs/StepDefinitions/BasicParsingSteps.cs に作成（ステップ定義の実装）
- [x] T029 [US1] BDD テストを実行し、Red（失敗）を確認

### Implementation for User Story 1（Green フェーズ）

**ドメインノードの実装**:

- [x] T030 [P] [US1] DocumentSyntax クラスを Source/AsciiSharp/Syntax/DocumentSyntax.cs に実装
- [x] T031 [P] [US1] DocumentHeaderSyntax クラスを Source/AsciiSharp/Syntax/DocumentHeaderSyntax.cs に実装
- [x] T032 [P] [US1] DocumentBodySyntax クラスを Source/AsciiSharp/Syntax/DocumentBodySyntax.cs に実装
- [x] T033 [P] [US1] SectionSyntax クラスを Source/AsciiSharp/Syntax/SectionSyntax.cs に実装
- [x] T034 [P] [US1] SectionTitleSyntax クラスを Source/AsciiSharp/Syntax/SectionTitleSyntax.cs に実装
- [x] T035 [P] [US1] ParagraphSyntax クラスを Source/AsciiSharp/Syntax/ParagraphSyntax.cs に実装
- ~~[ ] T036 [P] [US1] UnorderedListSyntax クラスを実装~~ *(延期: 後続イテレーション)*
- ~~[ ] T037 [P] [US1] OrderedListSyntax クラスを実装~~ *(延期: 後続イテレーション)*
- ~~[ ] T038 [P] [US1] ListItemSyntax クラスを実装~~ *(延期: 後続イテレーション)*
- [x] T039 [P] [US1] TextSyntax クラス（インライン要素）を Source/AsciiSharp/Syntax/TextSyntax.cs に実装（基本テキストのみ）

**Lexer 実装**:

- [x] T040 [US1] Lexer クラスを Source/AsciiSharp/Parser/Lexer.cs に実装（基本トークン: Text, NewLine, Whitespace, Equals, Asterisk, Dash をサポート）
- [x] T041 [US1] Lexer に Trivia サポート（先行/後続トリビアの割り当て）を実装

**Parser 実装**:

- [x] T042 [US1] Parser クラスを Source/AsciiSharp/Parser/Parser.cs に実装（イベントベース再帰下降パーサー、Document/Section/Paragraph をサポート）
- [x] T043 [US1] Parser に ロスレス解析（空白・改行の完全保持）を実装
- [x] T044 [US1] SyntaxTree.ParseText() メソッドを実装（エントリーポイント）

**検証**:

- [x] T045 [US1] BDD テストを再実行し、Green（成功）を確認
- [x] T046 [US1] ラウンドトリップ検証（構文木 → テキスト再構築）をテスト
- [x] T047 [US1] ビルドを実行し、警告ゼロを確認

### Refactor for User Story 1

- [x] T048 [US1] コードレビューと警告解消
- [x] T049 [US1] Lexer/Parser の共通パターンをリファクタリング

**Checkpoint**: この時点で User Story 1 が完全に機能し、独立してテスト可能

---

## Phase 4: User Story 2 - エラー耐性解析 (Priority: P1)

**Goal**: 構文エラーを含む AsciiDoc 文書を解析しても、正常な部分を最大限に解析し、エディタで有用な情報を提供する

**Independent Test**: 意図的に構文エラーを含む文書を解析し、エラー部分が特定され、正常部分が正しく解析されることを検証

### BDD テスト for User Story 2（Red フェーズ）

- [x] T050 [P] [US2] ErrorRecovery.feature を Test/AsciiSharp.Specs/Features/ErrorRecovery.feature に作成（Acceptance Scenarios 1-3）
- [x] T051 [US2] ErrorRecoverySteps.cs を Test/AsciiSharp.Specs/StepDefinitions/ErrorRecoverySteps.cs に作成
- [x] T052 [US2] BDD テストを実行し、Red を確認

### Implementation for User Story 2（Green フェーズ）

**エラー回復機能の実装**:

- [x] T053 [P] [US2] MissingToken 機能を InternalToken.Missing() として実装（専用クラスは不要）
- [x] T054 [P] [US2] エラーノード機能を既存の構文木機能で実装（専用クラスは不要）
- [x] T055 [US2] Parser にエラー回復ロジックを実装（同期ポイント戦略: 空行、セクションヘッダー）
- [x] T056 [US2] Parser にエラー回復ロジックを統合（スキップ・挿入・MissingToken 生成）
- [x] T057 [US2] Diagnostic の生成と SyntaxTree への集約を実装
- [x] T058 [US2] エラーノードの IsMissing プロパティと ContainsDiagnostics フラグを実装

**検証**:

- [x] T059 [US2] BDD テストを再実行し、Green を確認
- [x] T060 [US2] 複数エラーを含む文書で正常部分が解析されることをテスト
- [x] T061 [US2] ビルドを実行し、警告ゼロを確認

### Refactor for User Story 2

- [x] T062 [US2] エラーメッセージの多言語化対応（リソースファイルに英語メッセージを追加）
- [x] T063 [US2] エラー回復ロジックのリファクタリング（AddError ヘルパーメソッド抽出）

**Checkpoint**: User Stories 1 と 2 が両方とも独立して機能

---

## Phase 5: User Story 3 - リンクの解析 (Priority: P2)

**Goal**: URL リンクを解析し、リンクの位置とターゲットを正確に把握する

**MVP スコープ**: リンクのみ。書式マークアップ（太字、斜体等）、マクロ、属性参照は後続イテレーションで実装

**Independent Test**: URL リンクを含む段落を解析し、リンクの位置とターゲットが正確に識別されることを検証

### BDD テスト for User Story 3（Red フェーズ）

- [x] T064 [P] [US3] LinkParsing.feature を Test/AsciiSharp.Specs/Features/LinkParsing.feature に作成（Acceptance Scenarios 1-4）
- [x] T065 [US3] LinkParsingSteps.cs を Test/AsciiSharp.Specs/StepDefinitions/LinkParsingSteps.cs に作成
- [x] T066 [US3] BDD テストを実行し、Red を確認（3 テスト失敗を確認）

### Implementation for User Story 3（Green フェーズ）

**インライン要素ノードの実装**:

- [x] T067 [P] [US3] ~~InlineSyntax 抽象クラスを Source/AsciiSharp/Syntax/InlineSyntax.cs に実装~~ *(不要: LinkSyntax は直接 SyntaxNode を継承で十分)*
- ~~[ ] T068 [P] [US3] FormattedTextSyntax クラス（太字・斜体・等幅）を実装~~ *(延期: 後続イテレーション)*
- [x] T069 [P] [US3] LinkSyntax クラスを Source/AsciiSharp/Syntax/LinkSyntax.cs に実装
- ~~[ ] T070 [P] [US3] MacroSyntax クラスを実装~~ *(延期: 後続イテレーション)*
- ~~[ ] T071 [P] [US3] AttributeReferenceSyntax クラスを実装~~ *(延期: 後続イテレーション)*

**Lexer 拡張**:

- [x] T072 [US3] Lexer にリンク関連トークンを追加（[, ], :, / など）

**Parser 拡張**:

- [x] T073 [US3] Parser にリンク解析ロジックを追加（ParseLink メソッド）
- ~~[ ] T074 [US3] ネストしたインライン書式の解析を実装~~ *(延期: 後続イテレーション)*
- ~~[ ] T075 [US3] URL マクロ、相互参照、脚注マクロの解析を実装~~ *(延期: 後続イテレーション)*

**検証**:

- [x] T076 [US3] BDD テストを再実行し、Green を確認
- [x] T077 [US3] 複数リンクと表示テキスト付きリンクのテスト
- [x] T078 [US3] ビルドを実行し、警告ゼロを確認

### Refactor for User Story 3

- [x] T079 [US3] リンク解析ロジックの最適化 *(現状で十分、追加の最適化は不要)*
- [x] T080 [US3] インライン解析の共通パターンをリファクタリング *(現状で十分、追加のリファクタリングは不要)*

**Checkpoint**: User Stories 1, 2, 3 が独立して機能

---

## Phase 6: User Story 4 - 構文木の不変性とクエリ (Priority: P2)

**Goal**: 構文木を変更せずにクエリを行い、必要に応じて元の構文木を保持したまま新しい構文木を作成する

**Independent Test**: 構文木に対して変更操作を行い、元の構文木が変更されず、新しい構文木が正しい変更を反映していることを検証

### BDD テスト for User Story 4（Red フェーズ）

- [x] T081 [P] [US4] Immutability.feature を Test/AsciiSharp.Specs/Features/Immutability.feature に作成（Acceptance Scenarios 1-3）
- [x] T082 [US4] ImmutabilitySteps.cs を Test/AsciiSharp.Specs/StepDefinitions/ImmutabilitySteps.cs に作成
- [x] T083 [US4] BDD テストを実行し、Red を確認（2 テスト失敗: ReplaceNodeCore が NotImplementedException）

### Implementation for User Story 4（Green フェーズ）

**クエリ API の実装**:

- [x] T084 [P] [US4] SyntaxNode にクエリメソッドを追加（DescendantNodes, DescendantTokens, Ancestors など）*(既存実装で対応済み)*
- [x] T085 [P] [US4] LINQ 拡張メソッドを Source/AsciiSharp/Syntax/SyntaxExtensions.cs に実装（OfKind, FirstAncestor など）

**変更 API の実装**:

- [x] T086 [US4] SyntaxNode.ReplaceNode メソッドを実装（新しい構文木を返す）
- ~~[ ] T087 [US4] SyntaxNode.InsertNodeBefore/After メソッドを実装~~ *(延期: BDD 受け入れ基準に含まれず)*
- ~~[ ] T088 [US4] SyntaxNode.RemoveNode メソッドを実装~~ *(延期: BDD 受け入れ基準に含まれず)*
- [x] T089 [US4] SyntaxTree.WithRootAndOptions メソッドを実装（構文木全体の置き換え）*(既存実装で対応済み)*

**不変性保証のテスト**:

- [x] T090 [US4] 構文木変更後に元のインスタンスが変更されないことを BDD テストで検証（Test/AsciiSharp.Specs/Features/Immutability.feature）
- [x] T091 [US4] BDD テストを再実行し、Green を確認（19 成功、1 スキップ）
- [x] T092 [US4] ビルドを実行し、警告ゼロを確認

### Refactor for User Story 4

- [x] T093 [US4] クエリ API のパフォーマンス最適化 *(現状で十分、追加の最適化は不要)*
- [x] T094 [US4] 変更 API の共通パターンをリファクタリング（ReplaceInDescendants ヘルパーメソッドとして実装）

**Checkpoint**: User Stories 1-4 が独立して機能

---

## Phase 7: User Story 5 - 増分解析 (Priority: P3)

**Goal**: 文書の一部を編集したとき、変更された部分のみを再解析し、変更されていない部分の解析結果を再利用する

**Independent Test**: 大きな文書の一部を変更し、再解析時間が文書全体の解析時間より大幅に短いことを検証

### BDD テスト for User Story 5（Red フェーズ）

- [x] T095 [P] [US5] IncrementalParsing.feature を Test/AsciiSharp.Specs/Features/IncrementalParsing.feature に作成（Acceptance Scenarios 1-3）
- [x] T096 [US5] IncrementalParsingSteps.cs を Test/AsciiSharp.Specs/StepDefinitions/IncrementalParsingSteps.cs に作成
- [x] T097 [US5] BDD テストを実行し、Red を確認

### Implementation for User Story 5（Green フェーズ）

**増分解析インフラの実装**:

- [x] T098 [P] [US5] TextChange 構造体を Source/AsciiSharp/Text/TextChange.cs に実装（変更範囲と新しいテキストを表現）
- [x] T099 [P] [US5] SourceText.WithChanges メソッドを実装（変更を適用した新しい SourceText を返す）
- [x] T100 [US5] IncrementalParser クラスを Source/AsciiSharp/Parser/IncrementalParser.cs に実装
- [x] T101 [US5] 変更範囲に基づく再解析範囲の特定ロジックを実装（影響を受けるブロック境界の検出）
- [x] T102 [US5] 内部構文木の構造共有（変更されていない部分のノードを再利用）を実装
- [x] T103 [US5] SyntaxTree.WithChanges メソッドを実装（増分解析のエントリーポイント）

**検証**:

- [x] T104 [US5] BDD テストを再実行し、Green を確認
- [x] T105 [US5] ビルドを実行し、警告ゼロを確認

### Refactor for User Story 5

- [x] T106 [US5] 増分解析ロジックの最適化
- [x] T107 [US5] 構造共有のメモリ効率改善

**Checkpoint**: User Stories 1-5 が独立して機能

---

## ~~Phase 8: User Story 6 - テーブルの解析~~ (延期: 後続イテレーション)

> **注**: テーブル解析は MVP スコープ外。後続イテレーションで実装予定。

~~**Goal**: AsciiDoc のテーブル構文（区切り文字、列指定、ヘッダー行、セル書式など）を正しく解析する~~

以下のタスクはすべて延期:
- ~~T110-T125: テーブル関連タスク~~

**Checkpoint**: MVP では User Stories 1-5 が独立して機能

---

## Phase 9: Polish & Cross-Cutting Concerns

**目的**: 複数のユーザーストーリーにまたがる改善

### ~~DescriptionList と DelimitedBlock のサポート~~ (延期: 後続イテレーション)

> **注**: リスト系と区切りブロックは MVP スコープ外

- ~~[ ] T108-T112: DescriptionList, DelimitedBlock 関連タスク~~ *(延期)*

### その他の改善

- [x] T113 [P] DocumentHeaderSyntax に AuthorLine のサポートを追加（Source/AsciiSharp/Syntax/）
  - **注**: RevisionLine, AttributeEntry は後続イテレーションで実装
- [x] T114 [P] コメント解析（単一行 //, ブロック ////）を Source/AsciiSharp/Parser/Lexer.cs に実装
- ~~[ ] T115 [P] include ディレクティブ、条件付きディレクティブの構文認識を追加~~ *(延期: 後続イテレーション)*
- [x] T116 [P] BOM（Byte Order Mark）処理を SourceText に実装
- [x] T117 [P] 混在する改行コード（CR, LF, CRLF）のサポートを Lexer に実装
- [x] T118 [P] ネストレベル制限を Parser に実装（無限ループ防止）
- [x] T119 空の文書（空文字列）の解析テストを追加（Edge Case: 空のドキュメントノードを持つ有効な構文木が生成されることを検証）
- [x] T119a [US1] BasicParsing.feature の「本文のみを含む文章の解析」シナリオを補完（段落数検証、段落テキスト検証、ラウンドトリップ検証を追加）
- [x] T119b [US1] BasicParsingSteps.cs に「段落のテキストは "..." である」ステップ定義を追加（Test/AsciiSharp.Specs/StepDefinitions/BasicParsingSteps.cs）*(既存ステップで対応可能、追加不要)*
- [x] T119c [US1] ヘッダーなし文書のテストを実行し、Green を確認
- [x] T120 [P] XML ドキュメントコメントを公開 API に追加
- [x] T121 [P] README.md を作成（クイックスタート、API 概要）
- [x] T122 全体のコードレビューとリファクタリング
- [x] T123 CI パイプライン設定（.NET 10 / .NET Framework 4.8.1 テスト、AOT 互換性チェック）
- [x] T124 quickstart.md の検証シナリオを実行
- [x] T125 最終ビルドと警告ゼロの確認

---

## 依存関係と実行順序

### フェーズ依存関係

- **Setup (Phase 1)**: 依存関係なし - すぐに開始可能
- **Foundational (Phase 2)**: Setup 完了に依存 - すべてのユーザーストーリーをブロック
- **User Stories (Phase 3-8)**: すべて Foundational フェーズ完了に依存
  - ユーザーストーリーは並列実行可能（リソースがあれば）
  - または優先順位順に順次実行（P1 → P2 → P3）
- **Polish (Phase 9)**: すべての必要なユーザーストーリー完了に依存

### ユーザーストーリー依存関係

- **User Story 1 (P1)**: Foundational 完了後に開始可能 - 他のストーリーに依存しない
- **User Story 2 (P1)**: Foundational 完了後に開始可能 - US1 に依存（基本解析が必要）
- **User Story 3 (P2)**: Foundational 完了後に開始可能 - US1 に依存（段落解析が必要）
- **User Story 4 (P2)**: Foundational 完了後に開始可能 - US1 に依存（構文木が必要）
- **User Story 5 (P3)**: US1, US2 完了後に開始推奨 - 基本解析とエラー回復が安定している必要がある
- **User Story 6 (P3)**: Foundational 完了後に開始可能 - US1 に依存（ブロック解析が必要）

### 各ユーザーストーリー内

- BDD テスト（Red）→ 実装（Green）→ リファクタリング（Refactor）の順序を厳守
- ドメインノード → Lexer → Parser の順
- コア実装 → 統合 → 検証
- ストーリー完了後、次の優先度に進む

### 並列実行機会

- Setup フェーズの [P] タスクは並列実行可能
- Foundational フェーズの [P] タスクは並列実行可能（Phase 2 内で）
- Foundational フェーズ完了後、各ユーザーストーリーは並列開始可能（チーム容量がある場合）
- 各ストーリー内の [P] タスク（ドメインノードなど）は並列実行可能
- 異なるユーザーストーリーは異なるチームメンバーが並列作業可能

---

## 並列実行例: User Story 1 (MVP)

```bash
# User Story 1 のすべてのドメインノードを同時起動（MVP スコープ）:
Task: "[US1] DocumentSyntax を Source/AsciiSharp/Syntax/DocumentSyntax.cs に実装"
Task: "[US1] DocumentHeaderSyntax を Source/AsciiSharp/Syntax/DocumentHeaderSyntax.cs に実装"
Task: "[US1] DocumentBodySyntax を Source/AsciiSharp/Syntax/DocumentBodySyntax.cs に実装"
Task: "[US1] SectionSyntax を Source/AsciiSharp/Syntax/SectionSyntax.cs に実装"
Task: "[US1] SectionTitleSyntax を Source/AsciiSharp/Syntax/SectionTitleSyntax.cs に実装"
Task: "[US1] ParagraphSyntax を Source/AsciiSharp/Syntax/ParagraphSyntax.cs に実装"
# 注: リスト関連（UnorderedListSyntax, OrderedListSyntax, ListItemSyntax）は後続イテレーションで実装
```

---

## 実装戦略

### MVP First（User Story 1 と 2 のみ）

1. Phase 1: Setup を完了
2. Phase 2: Foundational を完了（重要 - すべてのストーリーをブロック）
3. Phase 3: User Story 1 を完了
4. Phase 4: User Story 2 を完了
5. **停止して検証**: User Story 1 と 2 を独立してテスト
6. 準備ができたらデプロイ/デモ

### 段階的デリバリー（MVP スコープ）

1. Setup + Foundational を完了 → 基盤完成
2. User Story 1 を追加 → 独立してテスト → デプロイ/デモ（MVP!）
3. User Story 2 を追加 → 独立してテスト → デプロイ/デモ
4. User Story 3 を追加 → 独立してテスト → デプロイ/デモ
5. User Story 4 を追加 → 独立してテスト → デプロイ/デモ
6. User Story 5 を追加 → 独立してテスト → デプロイ/デモ
7. ~~User Story 6 を追加~~ *(延期: 後続イテレーション)*
8. 各ストーリーは前のストーリーを壊さずに価値を追加

### 並列チーム戦略

複数の開発者がいる場合:

1. チーム全体で Setup + Foundational を完了
2. Foundational 完了後:
   - Developer A: User Story 1
   - Developer B: User Story 2（US1 の基本解析完了後）
   - Developer C: User Story 3（US1 完了後）
3. ストーリーは独立して完了し、統合

---

## タスク要約（MVP スコープ）

- **MVP 総タスク数**: 約 95（延期タスク除く）
- **Phase 1 (Setup)**: 7 タスク
- **Phase 2 (Foundational)**: 19 タスク（PEG 文法定義含む）
- **User Story 1**: 20 タスク（リスト関連 3 タスク延期）
- **User Story 2**: 14 タスク（BDD テスト含む）
- **User Story 3**: 11 タスク（書式/マクロ関連 6 タスク延期）
- **User Story 4**: 14 タスク（BDD テスト含む）
- **User Story 5**: 13 タスク（BDD テスト含む、パフォーマンス検証タスク削除）
- ~~**User Story 6**: 16 タスク~~ *(延期: 後続イテレーション)*
- **Phase 9 (Polish)**: 約 10 タスク（DescriptionList, DelimitedBlock 等延期）

### 延期されたタスク（後続イテレーション）

- リスト関連: T036-T038
- 書式マークアップ: T068, T074
- マクロ/属性参照: T070, T071, T075
- テーブル: 延期
- DescriptionList/DelimitedBlock: T108-T112
- ディレクティブ: T115

### 並列実行機会

- Phase 1: 5 タスク並列可能
- Phase 2: 15 タスク並列可能（PEG 文法定義含む）
- User Story 1: 6 タスク並列可能（ドメインノード、リスト除く）
- User Story 2: 2 タスク並列可能
- User Story 3: 5 タスク並列可能（インラインノード）
- User Story 4: 2 タスク並列可能
- User Story 5: 2 タスク並列可能
- ~~User Story 6: 3 タスク並列可能~~ *(延期)*
- Phase 9: 約 5 タスク並列可能（延期タスク除く）

### 各ストーリーの独立テスト基準（MVP スコープ）

- **US1**: 単一の AsciiDoc 文書（セクション、段落）を解析し、ラウンドトリップ検証が成功
- **US2**: エラーを含む文書で正常部分が 95% 以上解析される
- **US3**: リンクの位置とターゲットが正確に識別される
- **US4**: 構文木変更後に元のインスタンスが変更されていない
- **US5**: 増分解析により変更部分のみが再解析される
- ~~**US6**: テーブル解析~~ *(延期: 後続イテレーション)*

### MVP スコープ

**User Story 1-5** を MVP スコープとする：
- **US1**: 基本解析（セクション、段落、ヘッダー）
- **US2**: エラー耐性解析
- **US3**: リンク解析
- **US4**: 不変性とクエリ
- **US5**: 増分解析

これにより、インタラクティブエディタの基盤として機能するパーサーが提供される

---

## Notes

- [P] タスク = 異なるファイル、依存関係なし
- [Story] ラベルは特定のユーザーストーリーへのタスクの追跡可能性を示す
- 各ユーザーストーリーは独立して完了・テスト可能であるべき
- 実装前にテストが失敗することを確認（BDD Red-Green-Refactor）
- 各タスクまたは論理グループ後にコミット
- 任意のチェックポイントで停止してストーリーを独立して検証
- 避けるべきこと: 曖昧なタスク、同一ファイルの競合、ストーリーの独立性を損なうクロスストーリー依存関係
