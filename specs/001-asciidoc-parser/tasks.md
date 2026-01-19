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

- [ ] T001 プロジェクト構造を作成（Source/AsciiSharp/, Test/AsciiSharp.Tests/, Test/AsciiSharp.Specs/, Benchmark/AsciiSharp.Benchmarks/）
- [ ] T002 AsciiSharp.csproj を作成（TargetFrameworks: netstandard2.0;net10.0）
- [ ] T003 [P] AsciiSharp.Tests.csproj を作成（TargetFrameworks: net10.0;net481, MSTest.Sdk 参照）
- [ ] T004 [P] AsciiSharp.Specs.csproj を作成（TargetFramework: net10.0, Reqnroll パッケージ参照）
- [ ] T005 [P] AsciiSharp.Benchmarks.csproj を作成（TargetFramework: net10.0, BenchmarkDotNet 参照）
- [ ] T006 Directory.Build.props を作成（WarningLevel=9999, AnalysisLevel=latest-all, .NET Standard 2.0 用 Polyfill 設定）
- [ ] T007 [P] .editorconfig を作成（コードスタイル統一）

---

## Phase 2: Foundational（ブロッキング前提条件）

**目的**: すべてのユーザーストーリーの実装前に完了すべきコアインフラストラクチャ

**⚠️ 重要**: このフェーズが完了するまで、ユーザーストーリーの作業を開始できません

### PEG 文法定義（参照仕様）

- [ ] T008 [P] PEG 文法定義ファイルを specs/001-asciidoc-parser/grammar/ に作成（FR-002 に基づく参照仕様、パーサー実装のガイドとして使用）

### コアインフラストラクチャ

- [ ] T009 SyntaxKind 列挙型を Source/AsciiSharp/SyntaxKind.cs に実装（Tokens, Trivia, Nodes 定義）
- [ ] T010 [P] TextSpan 構造体を Source/AsciiSharp/Text/TextSpan.cs に実装
- [ ] T011 [P] DiagnosticSeverity 列挙型と Diagnostic クラスを Source/AsciiSharp/Diagnostics/Diagnostic.cs に実装
- [ ] T012 [P] SourceText 抽象クラスを Source/AsciiSharp/Text/SourceText.cs に実装
- [ ] T013 [P] StringText クラス（SourceText の実装）を Source/AsciiSharp/Text/StringText.cs に実装
- [ ] T014 InternalNode 抽象クラスを Source/AsciiSharp/InternalSyntax/InternalNode.cs に実装
- [ ] T015 [P] InternalToken クラスを Source/AsciiSharp/InternalSyntax/InternalToken.cs に実装
- [ ] T016 [P] InternalTrivia 構造体を Source/AsciiSharp/InternalSyntax/InternalTrivia.cs に実装
- [ ] T017 [P] InternalNodeCache クラスを Source/AsciiSharp/InternalSyntax/InternalNodeCache.cs に実装（WeakReference ベース）
- [ ] T018 SyntaxNode 抽象クラスを Source/AsciiSharp/Syntax/SyntaxNode.cs に実装（外部構文木の基底）
- [ ] T019 [P] SyntaxToken 構造体を Source/AsciiSharp/Syntax/SyntaxToken.cs に実装
- [ ] T020 [P] SyntaxTrivia 構造体を Source/AsciiSharp/Syntax/SyntaxTrivia.cs に実装
- [ ] T021 [P] SyntaxList<T> 構造体を Source/AsciiSharp/Syntax/SyntaxList.cs に実装
- [ ] T022 [P] SyntaxNodeOrToken 構造体を Source/AsciiSharp/Syntax/SyntaxNodeOrToken.cs に実装
- [ ] T023 SyntaxTree クラスを Source/AsciiSharp/Syntax/SyntaxTree.cs に実装（内部構文木から外部構文木への変換を含む）
- [ ] T024 ITreeSink インターフェースを Source/AsciiSharp/Parser/ITreeSink.cs に実装（イベントベースパーサー用）
- [ ] T025 InternalTreeBuilder クラスを Source/AsciiSharp/InternalSyntax/InternalTreeBuilder.cs に実装（ITreeSink 実装）
- [ ] T026 .NET Standard 2.0 用 Polyfill を Source/AsciiSharp/Polyfills/ に追加（IsExternalInit, RequiredMemberAttribute など）

**Checkpoint**: 基盤完了 - ユーザーストーリーの実装を並列開始可能

---

## Phase 3: User Story 1 - 基本的な AsciiDoc 文書の解析 (Priority: P1) 🎯 MVP

**Goal**: セクション、段落、リストなどの基本構造を含む AsciiDoc 文書を解析し、ロスレスな構文木を生成する

**Independent Test**: 単一の AsciiDoc 文書を解析し、構文木から元のテキストを再構築してラウンドトリップ検証を実行

### BDD テスト for User Story 1（Red フェーズ）

> **注意: 実装前にこれらのテストを書き、FAIL することを確認する**

- [ ] T027 [P] [US1] BasicParsing.feature を Test/AsciiSharp.Specs/Features/BasicParsing.feature に作成（Acceptance Scenarios 1-3 を Given-When-Then 形式で記述）
- [ ] T028 [US1] BasicParsingSteps.cs を Test/AsciiSharp.Specs/StepDefinitions/BasicParsingSteps.cs に作成（ステップ定義の実装）
- [ ] T029 [US1] BDD テストを実行し、Red（失敗）を確認

### Implementation for User Story 1（Green フェーズ）

**ドメインノードの実装**:

- [ ] T030 [P] [US1] DocumentSyntax クラスを Source/AsciiSharp/Syntax/DocumentSyntax.cs に実装
- [ ] T031 [P] [US1] DocumentHeaderSyntax クラスを Source/AsciiSharp/Syntax/DocumentHeaderSyntax.cs に実装
- [ ] T032 [P] [US1] DocumentBodySyntax クラスを Source/AsciiSharp/Syntax/DocumentBodySyntax.cs に実装
- [ ] T033 [P] [US1] SectionSyntax クラスを Source/AsciiSharp/Syntax/SectionSyntax.cs に実装
- [ ] T034 [P] [US1] SectionTitleSyntax クラスを Source/AsciiSharp/Syntax/SectionTitleSyntax.cs に実装
- [ ] T035 [P] [US1] ParagraphSyntax クラスを Source/AsciiSharp/Syntax/ParagraphSyntax.cs に実装
- [ ] T036 [P] [US1] UnorderedListSyntax クラスを Source/AsciiSharp/Syntax/UnorderedListSyntax.cs に実装
- [ ] T037 [P] [US1] OrderedListSyntax クラスを Source/AsciiSharp/Syntax/OrderedListSyntax.cs に実装
- [ ] T038 [P] [US1] ListItemSyntax クラスを Source/AsciiSharp/Syntax/ListItemSyntax.cs に実装
- [ ] T039 [P] [US1] TextSyntax クラス（インライン要素）を Source/AsciiSharp/Syntax/TextSyntax.cs に実装（基本テキストのみ）

**Lexer 実装**:

- [ ] T040 [US1] Lexer クラスを Source/AsciiSharp/Parser/Lexer.cs に実装（基本トークン: Text, NewLine, Whitespace, Equals, Asterisk, Dash をサポート）
- [ ] T041 [US1] Lexer に Trivia サポート（先行/後続トリビアの割り当て）を実装

**Parser 実装**:

- [ ] T042 [US1] Parser クラスを Source/AsciiSharp/Parser/Parser.cs に実装（イベントベース再帰下降パーサー、Document/Section/Paragraph/List をサポート）
- [ ] T043 [US1] Parser に ロスレス解析（空白・改行の完全保持）を実装
- [ ] T044 [US1] SyntaxTree.ParseText() メソッドを実装（エントリーポイント）

**検証**:

- [ ] T045 [US1] BDD テストを再実行し、Green（成功）を確認
- [ ] T046 [US1] ラウンドトリップ検証（構文木 → テキスト再構築）をテスト
- [ ] T047 [US1] ビルドを実行し、警告ゼロを確認

### Refactor for User Story 1

- [ ] T048 [US1] コードレビューと警告解消
- [ ] T049 [US1] Lexer/Parser の共通パターンをリファクタリング

**Checkpoint**: この時点で User Story 1 が完全に機能し、独立してテスト可能

---

## Phase 4: User Story 2 - エラー耐性解析 (Priority: P1)

**Goal**: 構文エラーを含む AsciiDoc 文書を解析しても、正常な部分を最大限に解析し、エディタで有用な情報を提供する

**Independent Test**: 意図的に構文エラーを含む文書を解析し、エラー部分が特定され、正常部分が正しく解析されることを検証

### BDD テスト for User Story 2（Red フェーズ）

- [ ] T050 [P] [US2] ErrorRecovery.feature を Test/AsciiSharp.Specs/Features/ErrorRecovery.feature に作成（Acceptance Scenarios 1-3）
- [ ] T051 [US2] ErrorRecoverySteps.cs を Test/AsciiSharp.Specs/StepDefinitions/ErrorRecoverySteps.cs に作成
- [ ] T052 [US2] BDD テストを実行し、Red を確認

### Implementation for User Story 2（Green フェーズ）

**エラー回復機能の実装**:

- [ ] T053 [P] [US2] MissingTokenSyntax クラス（欠落トークンを表現）を Source/AsciiSharp/Syntax/MissingTokenSyntax.cs に実装
- [ ] T054 [P] [US2] ErrorNodeSyntax クラス（エラー部分をラップ）を Source/AsciiSharp/Syntax/ErrorNodeSyntax.cs に実装
- [ ] T055 [US2] ParserRecovery クラスを Source/AsciiSharp/Parser/ParserRecovery.cs に実装（同期ポイント戦略: 空行、セクションヘッダー）
- [ ] T056 [US2] Parser にエラー回復ロジックを統合（スキップ・挿入・エラーノード生成）
- [ ] T057 [US2] Diagnostic の生成と SyntaxTree への集約を実装
- [ ] T058 [US2] エラーノードの IsMissing プロパティと ContainsDiagnostics フラグを実装

**検証**:

- [ ] T059 [US2] BDD テストを再実行し、Green を確認
- [ ] T060 [US2] 複数エラーを含む文書で正常部分が解析されることをテスト
- [ ] T061 [US2] ビルドを実行し、警告ゼロを確認

### Refactor for User Story 2

- [ ] T062 [US2] エラーメッセージの多言語化対応検討（将来拡張）
- [ ] T063 [US2] エラー回復ロジックのリファクタリング

**Checkpoint**: User Stories 1 と 2 が両方とも独立して機能

---

## Phase 5: User Story 3 - インラインマークアップの解析 (Priority: P2)

**Goal**: 太字、斜体、等幅、リンク、マクロなどのインライン要素を解析し、各書式の開始・終了位置を正確に把握

**Independent Test**: 様々なインラインマークアップを含む段落を解析し、各要素の種類と位置が正確に識別されることを検証

### BDD テスト for User Story 3（Red フェーズ）

- [ ] T064 [P] [US3] InlineMarkup.feature を Test/AsciiSharp.Specs/Features/InlineMarkup.feature に作成（Acceptance Scenarios 1-3）
- [ ] T065 [US3] InlineMarkupSteps.cs を Test/AsciiSharp.Specs/StepDefinitions/InlineMarkupSteps.cs に作成
- [ ] T066 [US3] BDD テストを実行し、Red を確認

### Implementation for User Story 3（Green フェーズ）

**インライン要素ノードの実装**:

- [ ] T067 [P] [US3] InlineSyntax 抽象クラスを Source/AsciiSharp/Syntax/InlineSyntax.cs に実装
- [ ] T068 [P] [US3] FormattedTextSyntax クラス（太字・斜体・等幅）を Source/AsciiSharp/Syntax/FormattedTextSyntax.cs に実装
- [ ] T069 [P] [US3] LinkSyntax クラスを Source/AsciiSharp/Syntax/LinkSyntax.cs に実装
- [ ] T070 [P] [US3] MacroSyntax クラスを Source/AsciiSharp/Syntax/MacroSyntax.cs に実装
- [ ] T071 [P] [US3] AttributeReferenceSyntax クラスを Source/AsciiSharp/Syntax/AttributeReferenceSyntax.cs に実装

**Lexer 拡張**:

- [ ] T072 [US3] Lexer にインラインマークアップトークンを追加（*, _, `, [, ], {, } など）

**Parser 拡張**:

- [ ] T073 [US3] Parser にインライン解析ロジックを追加（ParseInlines メソッド）
- [ ] T074 [US3] ネストしたインライン書式の解析を実装
- [ ] T075 [US3] URL マクロ、相互参照、脚注マクロの解析を実装

**検証**:

- [ ] T076 [US3] BDD テストを再実行し、Green を確認
- [ ] T077 [US3] ネストした書式のエッジケースをテスト
- [ ] T078 [US3] ビルドを実行し、警告ゼロを確認

### Refactor for User Story 3

- [ ] T079 [US3] インライン解析ロジックの最適化
- [ ] T080 [US3] 共通インラインパターンのリファクタリング

**Checkpoint**: User Stories 1, 2, 3 が独立して機能

---

## Phase 6: User Story 4 - 構文木の不変性とクエリ (Priority: P2)

**Goal**: 構文木を変更せずにクエリを行い、必要に応じて元の構文木を保持したまま新しい構文木を作成する

**Independent Test**: 構文木に対して変更操作を行い、元の構文木が変更されず、新しい構文木が正しい変更を反映していることを検証

### BDD テスト for User Story 4（Red フェーズ）

- [ ] T081 [P] [US4] Immutability.feature を Test/AsciiSharp.Specs/Features/Immutability.feature に作成（Acceptance Scenarios 1-3）
- [ ] T082 [US4] ImmutabilitySteps.cs を Test/AsciiSharp.Specs/StepDefinitions/ImmutabilitySteps.cs に作成
- [ ] T083 [US4] BDD テストを実行し、Red を確認

### Implementation for User Story 4（Green フェーズ）

**クエリ API の実装**:

- [ ] T084 [P] [US4] SyntaxNode にクエリメソッドを追加（DescendantNodes, DescendantTokens, Ancestors など）
- [ ] T085 [P] [US4] LINQ 拡張メソッドを Source/AsciiSharp/Syntax/SyntaxExtensions.cs に実装（OfKind, FirstAncestor など）

**変更 API の実装**:

- [ ] T086 [US4] SyntaxNode.ReplaceNode メソッドを実装（新しい構文木を返す）
- [ ] T087 [US4] SyntaxNode.InsertNodeBefore/After メソッドを実装
- [ ] T088 [US4] SyntaxNode.RemoveNode メソッドを実装
- [ ] T089 [US4] SyntaxTree.WithRootAndOptions メソッドを実装（構文木全体の置き換え）

**不変性保証のテスト**:

- [ ] T090 [US4] 構文木変更後に元のインスタンスが変更されないことをユニットテストで検証（Test/AsciiSharp.Tests/Syntax/ImmutabilityTests.cs）
- [ ] T091 [US4] BDD テストを再実行し、Green を確認
- [ ] T092 [US4] ビルドを実行し、警告ゼロを確認

### Refactor for User Story 4

- [ ] T093 [US4] クエリ API のパフォーマンス最適化
- [ ] T094 [US4] 変更 API の共通パターンをリファクタリング

**Checkpoint**: User Stories 1-4 が独立して機能

---

## Phase 7: User Story 5 - 増分解析 (Priority: P3)

**Goal**: 文書の一部を編集したとき、変更された部分のみを再解析し、変更されていない部分の解析結果を再利用する

**Independent Test**: 大きな文書の一部を変更し、再解析時間が文書全体の解析時間より大幅に短いことを検証

### BDD テスト for User Story 5（Red フェーズ）

- [ ] T095 [P] [US5] IncrementalParsing.feature を Test/AsciiSharp.Specs/Features/IncrementalParsing.feature に作成（Acceptance Scenarios 1-3）
- [ ] T096 [US5] IncrementalParsingSteps.cs を Test/AsciiSharp.Specs/StepDefinitions/IncrementalParsingSteps.cs に作成
- [ ] T097 [US5] BDD テストを実行し、Red を確認

### Implementation for User Story 5（Green フェーズ）

**増分解析インフラの実装**:

- [ ] T098 [P] [US5] TextChange 構造体を Source/AsciiSharp/Text/TextChange.cs に実装（変更範囲と新しいテキストを表現）
- [ ] T099 [P] [US5] SourceText.WithChanges メソッドを実装（変更を適用した新しい SourceText を返す）
- [ ] T100 [US5] IncrementalParser クラスを Source/AsciiSharp/Parser/IncrementalParser.cs に実装
- [ ] T101 [US5] 変更範囲に基づく再解析範囲の特定ロジックを実装（影響を受けるブロック境界の検出）
- [ ] T102 [US5] 内部構文木の構造共有（変更されていない部分のノードを再利用）を実装
- [ ] T103 [US5] SyntaxTree.WithChanges メソッドを実装（増分解析のエントリーポイント）

**パフォーマンス検証**:

- [ ] T104 [US5] Benchmark/AsciiSharp.Benchmarks/ParserBenchmarks.cs にベンチマークを追加（初回解析 vs 増分解析）
- [ ] T105 [US5] BDD テストを再実行し、Green を確認
- [ ] T106 [US5] ベンチマークを実行し、SC-004（増分解析が 10% 以下の時間）を検証
- [ ] T107 [US5] ビルドを実行し、警告ゼロを確認

### Refactor for User Story 5

- [ ] T108 [US5] 増分解析ロジックの最適化
- [ ] T109 [US5] 構造共有のメモリ効率改善

**Checkpoint**: User Stories 1-5 が独立して機能

---

## Phase 8: User Story 6 - テーブルの解析 (Priority: P3)

**Goal**: AsciiDoc のテーブル構文（区切り文字、列指定、ヘッダー行、セル書式など）を正しく解析する

**Independent Test**: 様々なテーブル形式（基本、CSV、DSV、複雑なセル）を解析し、行・列・セルの構造が正しく認識されることを検証

### BDD テスト for User Story 6（Red フェーズ）

- [ ] T110 [P] [US6] TableParsing.feature を Test/AsciiSharp.Specs/Features/TableParsing.feature に作成（Acceptance Scenarios 1-3）
- [ ] T111 [US6] TableParsingSteps.cs を Test/AsciiSharp.Specs/StepDefinitions/TableParsingSteps.cs に作成
- [ ] T112 [US6] BDD テストを実行し、Red を確認

### Implementation for User Story 6（Green フェーズ）

**テーブルノードの実装**:

- [ ] T113 [P] [US6] TableSyntax クラスを Source/AsciiSharp/Syntax/TableSyntax.cs に実装
- [ ] T114 [P] [US6] TableRowSyntax クラスを Source/AsciiSharp/Syntax/TableRowSyntax.cs に実装
- [ ] T115 [P] [US6] TableCellSyntax クラスを Source/AsciiSharp/Syntax/TableCellSyntax.cs に実装

**Lexer 拡張**:

- [ ] T116 [US6] Lexer にテーブル区切りトークンを追加（|===, |, , など）

**Parser 拡張**:

- [ ] T117 [US6] Parser にテーブル解析ロジックを追加（ParseTable メソッド）
- [ ] T118 [US6] CSV 形式のデータ解析を実装
- [ ] T119 [US6] セルのスパン情報（ColSpan, RowSpan）解析を実装
- [ ] T120 [US6] テーブルヘッダーの識別を実装

**検証**:

- [ ] T121 [US6] BDD テストを再実行し、Green を確認
- [ ] T122 [US6] 複雑なテーブル（結合セル、CSV）のエッジケースをテスト
- [ ] T123 [US6] ビルドを実行し、警告ゼロを確認

### Refactor for User Story 6

- [ ] T124 [US6] テーブル解析ロジックの最適化
- [ ] T125 [US6] CSV パーサーのリファクタリング

**Checkpoint**: すべてのユーザーストーリーが独立して機能

---

## Phase 9: Polish & Cross-Cutting Concerns

**目的**: 複数のユーザーストーリーにまたがる改善

- [ ] T126 [P] DocumentHeaderSyntax に AuthorLine, RevisionLine, AttributeEntry のサポートを追加（Source/AsciiSharp/Syntax/）
- [ ] T127 [P] コメント解析（単一行 //, ブロック ////）を Source/AsciiSharp/Parser/Lexer.cs に実装
- [ ] T128 [P] include ディレクティブ、条件付きディレクティブの構文認識を追加（実際のファイル読み込みは行わない）
- [ ] T129 [P] BOM（Byte Order Mark）処理を SourceText に実装
- [ ] T130 [P] 混在する改行コード（CR, LF, CRLF）のサポートを Lexer に実装
- [ ] T131 [P] ネストレベル制限を Parser に実装（無限ループ防止）
- [ ] T132 パフォーマンステストを Benchmark/AsciiSharp.Benchmarks/ParserBenchmarks.cs に追加（SC-003 検証: 100KB 文書 500ms 以内）
- [ ] T133 ベンチマークを実行し、パフォーマンス目標を検証
- [ ] T134 [P] XML ドキュメントコメントを公開 API に追加
- [ ] T135 [P] README.md を作成（クイックスタート、API 概要）
- [ ] T136 全体のコードレビューとリファクタリング
- [ ] T137 CI パイプライン設定（.NET 10 / .NET Framework 4.8.1 テスト、AOT 互換性チェック）
- [ ] T138 quickstart.md の検証シナリオを実行
- [ ] T139 最終ビルドと警告ゼロの確認

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

## 並列実行例: User Story 1

```bash
# User Story 1 のすべてのドメインノードを同時起動:
Task: "[US1] DocumentSyntax を Source/AsciiSharp/Syntax/DocumentSyntax.cs に実装"
Task: "[US1] DocumentHeaderSyntax を Source/AsciiSharp/Syntax/DocumentHeaderSyntax.cs に実装"
Task: "[US1] DocumentBodySyntax を Source/AsciiSharp/Syntax/DocumentBodySyntax.cs に実装"
Task: "[US1] SectionSyntax を Source/AsciiSharp/Syntax/SectionSyntax.cs に実装"
Task: "[US1] SectionTitleSyntax を Source/AsciiSharp/Syntax/SectionTitleSyntax.cs に実装"
Task: "[US1] ParagraphSyntax を Source/AsciiSharp/Syntax/ParagraphSyntax.cs に実装"
Task: "[US1] UnorderedListSyntax を Source/AsciiSharp/Syntax/UnorderedListSyntax.cs に実装"
Task: "[US1] OrderedListSyntax を Source/AsciiSharp/Syntax/OrderedListSyntax.cs に実装"
Task: "[US1] ListItemSyntax を Source/AsciiSharp/Syntax/ListItemSyntax.cs に実装"
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

### 段階的デリバリー

1. Setup + Foundational を完了 → 基盤完成
2. User Story 1 を追加 → 独立してテスト → デプロイ/デモ（MVP!）
3. User Story 2 を追加 → 独立してテスト → デプロイ/デモ
4. User Story 3 を追加 → 独立してテスト → デプロイ/デモ
5. User Story 4 を追加 → 独立してテスト → デプロイ/デモ
6. User Story 5 を追加 → 独立してテスト → デプロイ/デモ
7. User Story 6 を追加 → 独立してテスト → デプロイ/デモ
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

## タスク要約

- **総タスク数**: 139
- **Phase 1 (Setup)**: 7 タスク
- **Phase 2 (Foundational)**: 19 タスク（PEG 文法定義含む）
- **User Story 1**: 23 タスク（BDD テスト含む）
- **User Story 2**: 14 タスク（BDD テスト含む）
- **User Story 3**: 17 タスク（BDD テスト含む）
- **User Story 4**: 14 タスク（BDD テスト含む）
- **User Story 5**: 15 タスク（BDD テスト含む）
- **User Story 6**: 16 タスク（BDD テスト含む）
- **Phase 9 (Polish)**: 14 タスク

### 並列実行機会

- Phase 1: 5 タスク並列可能
- Phase 2: 15 タスク並列可能（PEG 文法定義含む）
- User Story 1: 9 タスク並列可能（ドメインノード）
- User Story 2: 2 タスク並列可能
- User Story 3: 5 タスク並列可能（インラインノード）
- User Story 4: 2 タスク並列可能
- User Story 5: 2 タスク並列可能
- User Story 6: 3 タスク並列可能
- Phase 9: 8 タスク並列可能

### 各ストーリーの独立テスト基準

- **US1**: 単一の AsciiDoc 文書を解析し、ラウンドトリップ検証が成功
- **US2**: エラーを含む文書で正常部分が 95% 以上解析される
- **US3**: インライン要素の種類と位置が正確に識別される
- **US4**: 構文木変更後に元のインスタンスが変更されていない
- **US5**: 増分解析が全体解析の 10% 以下の時間で完了
- **US6**: 複雑なテーブルの行・列・セル構造が正しく認識される

### 推奨 MVP スコープ

**User Story 1 と 2**（基本解析 + エラー耐性）- これだけで実用的なパーサーとして機能し、インタラクティブエディタの基盤となる

---

## Notes

- [P] タスク = 異なるファイル、依存関係なし
- [Story] ラベルは特定のユーザーストーリーへのタスクの追跡可能性を示す
- 各ユーザーストーリーは独立して完了・テスト可能であるべき
- 実装前にテストが失敗することを確認（BDD Red-Green-Refactor）
- 各タスクまたは論理グループ後にコミット
- 任意のチェックポイントで停止してストーリーを独立して検証
- 避けるべきこと: 曖昧なタスク、同一ファイルの競合、ストーリーの独立性を損なうクロスストーリー依存関係
