# Implementation Plan: BlockSyntax と InlineSyntax 階層の導入

**Branch**: `001-block-inline-syntax` | **Date**: 2026-01-30 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `specs/001-block-inline-syntax/spec.md`

## Summary

SyntaxNode の継承階層に `BlockSyntax` と `InlineSyntax` という中間抽象クラスを導入し、AsciiDoc のブロック要素とインライン要素を型レベルで区別できるようにする。これは Roslyn スタイルの構文木設計に準拠したマーカークラスの追加であり、既存の API に破壊的変更を加えない。

## Technical Context

**Language/Version**: C# 14 / .NET 10.0 (コアライブラリは .NET Standard 2.0 との互換性も維持)
**Primary Dependencies**: なし（内部リファクタリングのため新規依存なし）
**Storage**: N/A
**Testing**: MSTest.Sdk + Reqnroll (BDD)
**Target Platform**: .NET 10.0 / .NET Standard 2.0 / .NET Framework 4.8.1
**Project Type**: ライブラリ（単一プロジェクト重点）
**Performance Goals**: 初期段階では重視しない（CLAUDE.md の方針に従う）
**Constraints**: .NET Standard 2.0 互換性を維持する必要がある
**Scale/Scope**: 小規模 - 2 つの抽象クラス追加と 10 クラスの継承元変更

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原則 | ステータス | 備考 |
|------|-----------|------|
| I. コード品質ファースト | ✅ PASS | 既存パターンに従った設計 |
| II. モジュール設計 | ✅ PASS | 明確な責務（ブロック/インラインの区別） |
| III. BDD 必須 | ✅ PASS | .feature ファイルを本フェーズで作成 |
| IV. 継続的品質保証 | ✅ PASS | 実装後にビルド・テスト実行 |
| V. 警告ゼロポリシー | ✅ PASS | リファクタリングステップで解消予定 |
| VI. フェーズ順序の厳守 | ✅ PASS | specify → clarify → plan の順序で進行中 |

**ゲート結果**: すべてパス - Phase 0 に進行可能

## Project Structure

### Documentation (this feature)

```text
specs/001-block-inline-syntax/
├── spec.md              # 仕様書 (/speckit.specify で作成済み)
├── plan.md              # 本ファイル (/speckit.plan で作成)
├── research.md          # Phase 0 出力
├── data-model.md        # Phase 1 出力
└── tasks.md             # Phase 2 出力 (/speckit.tasks で作成)
```

### Source Code (repository root)

```text
Source/AsciiSharp/
├── Syntax/
│   ├── SyntaxNode.cs           # 既存: 基底抽象クラス
│   ├── BlockSyntax.cs          # 新規: ブロック要素の中間抽象クラス
│   ├── InlineSyntax.cs         # 新規: インライン要素の中間抽象クラス
│   ├── DocumentSyntax.cs       # 変更: SyntaxNode → BlockSyntax
│   ├── DocumentHeaderSyntax.cs # 変更: SyntaxNode → BlockSyntax
│   ├── DocumentBodySyntax.cs   # 変更: SyntaxNode → BlockSyntax
│   ├── SectionSyntax.cs        # 変更: SyntaxNode → BlockSyntax
│   ├── SectionTitleSyntax.cs   # 変更: SyntaxNode → BlockSyntax
│   ├── ParagraphSyntax.cs      # 変更: SyntaxNode → BlockSyntax
│   ├── AuthorLineSyntax.cs     # 変更: SyntaxNode → BlockSyntax
│   ├── TextSyntax.cs           # 変更: SyntaxNode → InlineSyntax
│   └── LinkSyntax.cs           # 変更: SyntaxNode → InlineSyntax
└── SyntaxKind.cs               # 参照のみ（変更なし）

Test/AsciiSharp.Specs/
└── Features/
    └── SyntaxHierarchy/
        └── BlockInlineSyntax.feature  # 新規: BDD テスト
```

**Structure Decision**: 既存の `Source/AsciiSharp/Syntax/` ディレクトリに新しいクラスファイルを追加。テストは既存の `Test/AsciiSharp.Specs/Features/` 配下に新しいサブディレクトリを作成して配置。

## Complexity Tracking

> **GATE: 違反なし - 正当化不要**

本機能は単純な継承階層の追加であり、Constitution の原則に違反する複雑性は導入されない。

## Design Decisions

### 抽象クラスの設計

1. **マーカークラスとしての役割**
   - `BlockSyntax` と `InlineSyntax` は現時点では追加メンバーを持たない
   - 型システムによる分類のみを目的とする
   - 将来的に共通の振る舞いを追加する拡張ポイントとなる

2. **コンストラクタの可視性**
   - `private protected` コンストラクタを使用（`SyntaxNode` と同じパターン）
   - アセンブリ外からの直接継承を防ぐ

3. **既存 API への影響**
   - `SyntaxNode` の public API は変更なし
   - 既存のコードは引き続き動作する（バイナリ互換）
   - 新しい型チェック（`is BlockSyntax`）のみが追加される
