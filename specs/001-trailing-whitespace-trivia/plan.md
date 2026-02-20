# Implementation Plan: 要素境界における行末トリビアの統一

**Branch**: `001-trailing-whitespace-trivia` | **Date**: 2026-02-20 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-trailing-whitespace-trivia/spec.md`

## Summary

セクションタイトル・著者行の Parser メソッドが末尾の `NewLineToken` をトークンとして emit している問題を修正する。`ParseAttributeEntry()` が実装済みのパターン（末尾 `WhitespaceToken` → `WhitespaceTrivia`、`NewLineToken` → `EndOfLineTrivia` として最終コンテンツトークンの後続トリビアに付与）を `ParseSectionTitle()` と `ParseAuthorLine()` にも適用し、要素境界での行末トリビア処理を統一する。新たな SyntaxKind の追加はなし。

## Technical Context

**Language/Version**: C# 14 / .NET 10.0 + .NET Standard 2.0
**Primary Dependencies**: なし（コア パーサー ライブラリの内部変更のみ）
**Storage**: N/A
**Testing**: MSTest.Sdk / LightBDD.MsTest4（BDD）
**Target Platform**: .NET 10.0 + .NET Standard 2.0
**Project Type**: Single library
**Performance Goals**: 既存と同等（変更はロジック整理のみ）
**Constraints**: ラウンドトリップ完全性を損なわないこと
**Scale/Scope**: Parser.cs の 2 メソッド変更のみ（`ParseSectionTitle()`、`ParseAuthorLine()`）

## Constitution Check

| Gate | Status | Notes |
|------|--------|-------|
| BDD 必須（III） | ✅ | LightBDD フィーチャー クラスを plan フェーズで作成・Red 確認 |
| フェーズ順序の厳守（VI） | ✅ | specify → plan → tasks → analyze → implement の順 |
| 警告ゼロ（V） | ✅ | Refactor ステップで確認 |
| 継続的品質保証（IV） | ✅ | 各フェーズ末にビルド・テスト実行 |

**複雑性の追加**: なし（新 SyntaxKind 不要、Lexer 変更不要）

## Project Structure

### Documentation (this feature)

```text
specs/001-trailing-whitespace-trivia/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
Source/
└── AsciiSharp/
    └── Parser/
        └── Parser.cs    # ParseSectionTitle(), ParseAuthorLine() を変更

Test/
└── AsciiSharp.Specs/
    └── Features/
        ├── TrailingWhitespaceFeature.cs        # シナリオ定義（新設計に更新）
        └── TrailingWhitespaceFeature.Steps.cs  # ステップ実装（新設計に更新）
```

**Structure Decision**: Single project 構成。変更は Parser.cs の 2 メソッドのみ。テストは既存の BDD フィーチャー クラスを新設計に合わせて修正する。

## Phase 0: Research Summary

→ `research.md` 参照

**主要な決定事項**:
1. 新 SyntaxKind 追加なし（`WhitespaceTrivia`=200、`EndOfLineTrivia`=201 を使用）
2. Lexer 変更なし（`NewLineToken` 出力は継続）
3. `InternalTrivia.cs` 変更なし（既存の `Whitespace()`、`EndOfLine()` を使用）
4. `ParseAttributeEntry()` が参照実装パターン（変更なし）
5. 変更対象: `ParseSectionTitle()`、`ParseAuthorLine()` の 2 メソッドのみ

## Phase 1: Design Summary

→ `data-model.md` 参照

**変更内容**:
- `ParseSectionTitle()`: ループ終了後に末尾の `WhitespaceToken` → `WhitespaceTrivia`、`NewLineToken` → `EndOfLineTrivia` として最終コンテンツトークンの trailing に付与
- `ParseAuthorLine()`: 同上のパターンを適用

**スコープ外**:
- `ParseParagraph()` の NewLineToken（段落内改行は別フィーチャー）
- `IsBlankLine()` / `SkipBlankLines()` の NewLineToken
- Lexer の変更

## Phase 1: BDD Feature Class

### TrailingWhitespaceFeature — 新設計のシナリオ

spec.md の User Story に対応するシナリオ：

**User Story 2（P2）: SyntaxTree における行末トリビアの識別**

- `セクションタイトルの末尾空白が_WhitespaceTrivia_と_EndOfLineTrivia_として識別される`
- `セクションタイトルの末尾に空白なしの_EndOfLineTrivia_のみが識別される`
- `セクションタイトルの末尾CRLF_が単一の_EndOfLineTrivia_として識別される`

**User Story 3（P3）: 元テキストの完全復元**

- `行末空白を含むセクションタイトルのラウンドトリップが保証される`
- `行末空白を含む著者行のラウンドトリップが保証される`

**Edge Cases**

- `文書末尾に改行なしの行末空白のみがある場合のラウンドトリップが保証される`
