# Implementation Plan: 行末空白 Trivia の識別と保持

**Branch**: `001-trailing-whitespace-trivia` | **Date**: 2026-02-19 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `specs/001-trailing-whitespace-trivia/spec.md`

## Summary

各行の行末にある空白文字群（改行文字を含む）を、他の Trivia 種別とは区別される単一の `TrailingWhitespaceTrivia` として SyntaxTree に保持する。これにより、元のテキストの完全なラウンドトリップを維持しつつ、ASG における行末空白の無視（意味的な幅への不算入）を実現する。

実装の核心は Lexer の `ScanTrailingTrivia()` と `ScanLeadingTrivia()` の変更、および既存の `EndOfLineTrivia` の廃止と `TrailingWhitespaceTrivia` への統合である。

## Technical Context

**Language/Version**: C# 14
**Primary Dependencies**: LightBDD.MsTest4（BDD テスト）、MSTest.Sdk
**Storage**: N/A
**Testing**: MSTest / LightBDD
**Target Platform**: .NET 10.0 / .NET Standard 2.0（コアライブラリ）
**Project Type**: ライブラリ（パーサー）
**Performance Goals**: 初期フェーズではパフォーマンスを重視しない
**Constraints**: .NET Standard 2.0 互換（`allows ref struct` 等の一部機能は不使用）
**Scale/Scope**: コアライブラリ（Source/AsciiSharp）のみが BDD 対象

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原則 | 状態 | 備考 |
|------|------|------|
| I. コード品質ファースト | ✅ | 可読性・メンテナンス性を最優先 |
| II. モジュール設計 | ✅ | Lexer / Parser / SyntaxKind / InternalTrivia が分離 |
| III. BDD 必須 | ✅ | `Source/AsciiSharp` が BDD 対象。LightBDD フィーチャークラスを plan フェーズで作成 |
| IV. 継続的品質保証 | ✅ | Green/Refactor ステップでビルド+全テスト実行 |
| V. 警告ゼロポリシー | ✅ | Refactor ステップで警告を解消 |
| VI. フェーズ順序の厳守 | ✅ | specify → clarify → plan（現在）→ tasks → analyze → implement |

**違反事項**: なし

## Project Structure

### Documentation (this feature)

```text
specs/001-trailing-whitespace-trivia/
├── spec.md              # 機能仕様
├── plan.md              # このファイル
├── research.md          # Phase 0 調査結果
├── data-model.md        # Phase 1 データモデル
├── quickstart.md        # Phase 1 クイックスタート
├── checklists/
│   └── requirements.md  # 仕様品質チェックリスト
└── tasks.md             # Phase 2 出力（/speckit.tasks コマンドで生成）
```

### Source Code (repository root)

```text
Source/AsciiSharp/
├── SyntaxKind.cs                      # TrailingWhitespaceTrivia 追加、EndOfLineTrivia 削除
├── InternalSyntax/
│   └── InternalTrivia.cs              # TrailingWhitespace() ファクトリ追加、EndOfLine() 削除
└── Parser/
    ├── Lexer.cs                       # ScanTrailingTrivia()・ScanLeadingTrivia() 変更
    └── Parser.cs                      # IsBlankLine() 等の変更

Test/AsciiSharp.Specs/Features/
├── TrailingWhitespaceFeature.cs       # 新シナリオ追加（BDD Red）
└── TrailingWhitespaceFeature.Steps.cs # 新ステップスタブ追加

Test/AsciiSharp.Tests/
└── （既存テストの SyntaxKind 参照更新）
```

**Structure Decision**: 既存のプロジェクト構造に従う。新規ファイルは作成せず、既存ファイルへの変更のみ。

## Phase 0: Research Findings

調査の詳細は [research.md](research.md) を参照。以下はサマリー。

### 重要な発見

1. **`ScanTrailingTrivia()` が空実装**: 即座に置き換え可能
2. **`EndOfLineTrivia`（201）を廃止**: `TrailingWhitespaceTrivia`（203）に統合。番号は詰める
3. **Unicode 対応不足**: `ScanWhitespace()` と `ScanNewLineAsTrivia()` が `' '`/`'\t'`/`'\r'`/`'\n'` のみ対応
4. **Parser の `NewLineToken` 依存**: `IsBlankLine()`、`ParseParagraph()`、`ParseSectionTitle()`、`SkipBlankLines()` を変更が必要
5. **Option A（Trivia ベース）を採用**: ゼロ幅 `EndOfLineToken` はインラインマクロが改行を跨ぐ場合に矛盾するため不採用（暫定）

### NEEDS CLARIFICATION の解消状況

| 項目 | 状態 | 決定内容 |
|------|------|---------|
| 空白のみの行の扱い | ✅ 解消 | 1 行 = 1 `TrailingWhitespaceTrivia`（FR-004 準拠） |
| EndOfLineTrivia の扱い | ✅ 解消 | 廃止して `TrailingWhitespaceTrivia` に統合 |
| 連続空白行のモデル | ✅ 解消（暫定） | 各行が個別の `TrailingWhitespaceTrivia` として先行トリビアに格納 |
| インラインマクロ跨ぎ | ✅ 解消（暫定） | Trivia ベース設計では問題なし |

## Phase 1: Design

設計の詳細は [data-model.md](data-model.md) を参照。以下はサマリー。

### SyntaxKind の変更

```csharp
// 変更後の Trivia 範囲
WhitespaceTrivia = 200,
SingleLineCommentTrivia,   // 201（旧 202 から繰り上げ）
MultiLineCommentTrivia,    // 202（旧 203 から繰り上げ）
TrailingWhitespaceTrivia,  // 203（新規）
// EndOfLineTrivia (旧 201) は削除
```

### InternalTrivia の変更

- `TrailingWhitespace(string text)` ファクトリメソッドを追加
- `EndOfLine()` ファクトリメソッドを削除

### Lexer の変更

1. **Unicode 対応ヘルパー追加**:
   - `IsNonNewlineWhitespace(char)`: Unicode White_Space のうち改行文字以外
   - `IsNewlineStart(char)`: CR、LF、NEL、FF、LS、PS を判定

2. **`ScanTrailingTrivia()` の実装**（static → instance メソッドに変更）:
   - 行末の `[非改行空白文字*][改行文字|EOF]` を `TrailingWhitespaceTrivia` として収集
   - CRLF は 1 つの改行として処理

3. **`ScanLeadingTrivia()` の変更**:
   - 空白のみの行（先読みで改行が続くことを確認）を `TrailingWhitespaceTrivia` として収集
   - `ScanNewLineAsTrivia()` の生成するトリビアを `TrailingWhitespaceTrivia` に変更

### Parser の変更

| メソッド | 変更内容 |
|----------|---------|
| `IsBlankLine()` | 次トークンの先行トリビアに `TrailingWhitespaceTrivia` が含まれるかを検査 |
| `SkipBlankLines()` | 先行トリビアの `TrailingWhitespaceTrivia` を消費するロジックに変更 |
| `ParseParagraph()` | 行末終端の検出を `TrailingWhitespaceTrivia` 後続トリビアで行う |
| `ParseSectionTitle()` | `NewLineToken` → `TrailingWhitespaceTrivia` で終端検出 |

### エッジケース設計

ユーザー指定のエッジケースについての設計決定:

| エッジケース | 設計 |
|-------------|------|
| 段落テキスト・セクションタイトル後の行末空白 | 後続トリビアとして `TrailingWhitespaceTrivia` を付与 |
| 空白のみの行が連続 | 各行が個別の `TrailingWhitespaceTrivia`（次コンテンツの先行トリビアに格納） |
| 文書末尾に改行なしの行末空白 | EOF を改行の代替として処理し `TrailingWhitespaceTrivia` に格納 |
| 文書冒頭に空白のみの行 | 最初のコンテンツトークンの先行トリビアに `TrailingWhitespaceTrivia` |

## BDD Red ステップ（Plan フェーズで作成）

constitution に従い、plan フェーズで LightBDD フィーチャークラスを作成し、Red（テスト失敗）を確認する。

### 追加するシナリオ（`TrailingWhitespaceFeature.cs`）

既存の 4 シナリオ（ラウンドトリップ確認）は維持し、以下の新シナリオを追加する:

**User Story 1（Trivia 種別の識別）対応**:
1. `行末の空白と改行がTrailingWhitespaceTriviaとして識別される` - `"タイトル   \n"` の末尾が正しい種別であること
2. `CRLFのみの行末がTrailingWhitespaceTriviaとして識別される` - `"タイトル\r\n"` のケース
3. `改行のみの行末がTrailingWhitespaceTriviaとして識別される` - `"タイトル\n"` のケース
4. `空白のみの行がTrailingWhitespaceTriviaとして識別される` - `"   \n"` の全体が単一の Trivia

**User Story 2（ASG での無視）対応**:
5. `行末空白あり・なしで要素幅が等しい` - ASG 変換後の幅比較（ASG 実装後に Green にする）

**エッジケース対応**:
6. `文書末尾に改行なしの行末空白が保持される` - `"テキスト   "` で EOF
7. `文書冒頭の空白のみ行が保持される` - `"   \n== タイトル\n"` の冒頭空白行
8. `連続する空白のみ行がそれぞれ保持される` - `"para1\n\n\npara2\n"` の 2 つの空行

## 実装方針の注記

- `.NET Standard 2.0` と `.NET 10.0` の両方で動作するよう実装する
- `char.GetUnicodeCategory()` は .NET Standard 2.0 で利用可能
- Unicode 対応の空白・改行判定は `switch` 式または明示的な比較で実装する（`CharUnicodeInfo` を活用）
- 既存テストが参照している `SyntaxKind.EndOfLineTrivia` は `SyntaxKind.TrailingWhitespaceTrivia` に更新する

## Complexity Tracking

Constitution 違反なし。複雑性の追加なし。
