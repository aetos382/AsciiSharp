# Implementation Plan: SyntaxTree Visitor パターン

**Branch**: `002-syntaxtree-visitor` | **Date**: 2026-01-26 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/002-syntaxtree-visitor/spec.md`

## Summary

インターフェイスベースの Visitor パターンを実装し、AsciiSharp の構文木を走査できるようにする。ISyntaxVisitor（戻り値なし）と ISyntaxVisitor&lt;TResult&gt;（戻り値あり）の 2 つのインターフェイスを提供し、各 SyntaxNode 派生クラスに Accept メソッドを追加する。

## Technical Context

**Language/Version**: C# 14 / .NET 10.0 + .NET Standard 2.0
**Primary Dependencies**: なし（AsciiSharp コアライブラリ内で完結）
**Storage**: N/A
**Testing**: MSTest.Sdk + Reqnroll（BDD）
**Target Platform**: .NET 10.0, .NET Standard 2.0 互換ランタイム
**Project Type**: ライブラリ（既存プロジェクト Source/AsciiSharp に追加）
**Performance Goals**: 1000 ノード以上の構文木を走査可能
**Constraints**: .NET Standard 2.0 互換性維持
**Scale/Scope**: 既存 SyntaxNode 派生クラス 8 種

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原則 | ステータス | 備考 |
|------|-----------|------|
| I. コード品質ファースト | ✅ PASS | シンプルなインターフェイス設計で可読性確保 |
| II. モジュール設計 | ✅ PASS | 既存 Syntax 名前空間に追加、責務明確 |
| III. BDD 必須 | ✅ PASS | Reqnroll で受け入れテストを記述 |
| IV. 継続的品質保証 | ✅ PASS | Green/Refactor 後にビルド・テスト実行 |
| V. 警告ゼロポリシー | ✅ PASS | Refactor 中に警告解消 |

**Gate Result**: PASS - Phase 0 に進行可能

## Project Structure

### Documentation (this feature)

```text
specs/002-syntaxtree-visitor/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
Source/AsciiSharp/
├── Syntax/
│   ├── SyntaxNode.cs           # 既存: 抽象基底クラス（Accept メソッド追加）
│   ├── DocumentSyntax.cs       # 既存: 文書ノード（Accept メソッド追加）
│   ├── DocumentHeaderSyntax.cs # 既存: ヘッダーノード（Accept メソッド追加）
│   ├── DocumentBodySyntax.cs   # 既存: ボディノード（Accept メソッド追加）
│   ├── SectionSyntax.cs        # 既存: セクションノード（Accept メソッド追加）
│   ├── SectionTitleSyntax.cs   # 既存: セクションタイトルノード（Accept メソッド追加）
│   ├── ParagraphSyntax.cs      # 既存: 段落ノード（Accept メソッド追加）
│   ├── TextSyntax.cs           # 既存: テキストノード（Accept メソッド追加）
│   ├── LinkSyntax.cs           # 既存: リンクノード（Accept メソッド追加）
│   ├── ISyntaxVisitor.cs       # 新規: 戻り値なし Visitor インターフェイス
│   └── ISyntaxVisitor`1.cs     # 新規: 戻り値あり Visitor インターフェイス
└── ...

Test/AsciiSharp.Specs/
├── Features/
│   └── Visitor/
│       └── SyntaxVisitor.feature    # BDD シナリオ
└── StepDefinitions/
    └── VisitorSteps.cs              # ステップ定義

Test/AsciiSharp.Tests/
└── Syntax/
    └── SyntaxVisitorTests.cs        # ユニットテスト
```

**Structure Decision**: 既存の Source/AsciiSharp/Syntax 名前空間に Visitor インターフェイスを追加する。既存の SyntaxNode 派生クラスには Accept メソッドを追加する。テストは AsciiSharp.Specs（BDD）と AsciiSharp.Tests（ユニット）の両方に追加する。

## Complexity Tracking

> 憲法違反なし - 複雑性の正当化は不要
