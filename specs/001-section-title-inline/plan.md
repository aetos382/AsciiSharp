# Implementation Plan: SectionTitleSyntax の構成改定と TextSyntax のリネーム

**Branch**: `001-section-title-inline` | **Date**: 2026-02-05 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-section-title-inline/spec.md`

## Summary

SectionTitleSyntax の内部構造を改定し、現在の「空白区切りによるタイトル分割」から「= トークン + 空白トリビア + InlineSyntax コレクション」の構成に変更する。これにより将来のインラインマークアップ対応の基盤を整備する。また、TextSyntax を InlineTextSyntax にリネームし、「一行の文字列」という意味を明確化する。

## Technical Context

**Language/Version**: C# 14
**Primary Dependencies**: なし（コア ライブラリはサードパーティ依存なし）
**Storage**: N/A
**Testing**: MSTest.Sdk, Reqnroll (BDD)
**Target Platform**: .NET 10.0, .NET Standard 2.0（マルチターゲット）
**Project Type**: ライブラリ（NuGet パッケージ）
**Performance Goals**: 初期段階では重視しない（AGENTS.md に記載）
**Constraints**: イミュータブル構文木、完全なテキスト復元可能性
**Scale/Scope**: コア ライブラリと関連テストプロジェクトの変更

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原則 | 状態 | 備考 |
|------|------|------|
| I. コード品質ファースト | ✅ PASS | 可読性・メンテナンス性を重視したリネーム |
| II. モジュール設計 | ✅ PASS | 既存のモジュール構造を維持 |
| III. BDD必須 | ✅ PASS | コア ライブラリが対象、.feature ファイルを作成 |
| IV. 継続的品質保証 | ✅ PASS | 各ステップでビルド・テスト実行 |
| V. 警告ゼロポリシー | ✅ PASS | リファクタリング時に警告解消 |
| VI. フェーズ順序の厳守 | ✅ PASS | specify → clarify → plan の順序を遵守 |

## Project Structure

### Documentation (this feature)

```text
specs/001-section-title-inline/
├── spec.md              # 機能仕様書（作成済み）
├── plan.md              # 本ファイル
├── research.md          # Phase 0 出力
├── data-model.md        # Phase 1 出力
├── quickstart.md        # Phase 1 出力
└── tasks.md             # Phase 2 出力（/speckit.tasks で作成）
```

### Source Code (repository root)

```text
Source/
├── AsciiSharp/                           # コア ライブラリ（変更対象）
│   ├── Syntax/
│   │   ├── SectionTitleSyntax.cs        # 構造変更
│   │   ├── TextSyntax.cs                # InlineTextSyntax.cs にリネーム
│   │   ├── InlineSyntax.cs              # 基底クラス（変更なし）
│   │   ├── ISyntaxVisitor.cs            # VisitText → VisitInlineText
│   │   └── ISyntaxVisitorOfT.cs         # VisitText → VisitInlineText
│   └── SyntaxKind.cs                    # Text → InlineText
│
├── AsciiSharp.Asg/                       # 参照更新
│   └── AsgConverter.cs                   # TitleContent 参照の更新
│
└── AsciiSharp.TckAdapter/                # 影響確認

Test/
├── AsciiSharp.Specs/                     # BDD テスト
│   ├── Features/                         # .feature ファイル追加
│   └── StepDefinitions/                  # TitleContent 参照の更新
│       ├── VisitorSteps.cs
│       ├── BasicParsingSteps.cs
│       ├── CommentParsingSteps.cs
│       └── IncrementalParsingSteps.cs
│
└── AsciiSharp.Tests/                     # ユニット テスト
```

**Structure Decision**: 既存のプロジェクト構造を維持。新規プロジェクトの追加は不要。

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| なし | - | - |

---

## Phase 0: Research

### 調査課題

1. **SectionTitleSyntax の現在の実装詳細**
   - 既に解決：`Source/AsciiSharp/Syntax/SectionTitleSyntax.cs` で確認済み
   - 現在は `TitleContent` プロパティで全テキストを結合して返却

2. **TextSyntax のリネーム影響範囲**
   - 既に解決：以下のファイルで参照あり
   - `ISyntaxVisitor.cs`: `VisitText` メソッド
   - `ISyntaxVisitorOfT.cs`: `VisitText` メソッド
   - `SyntaxKind.cs`: `Text` 列挙値

3. **TitleContent の参照箇所**
   - 既に解決：以下のファイルで参照あり
   - `AsgConverter.cs`: 行169
   - `VisitorSteps.cs`: 行138-142, 541
   - `BasicParsingSteps.cs`: 行156, 192
   - `CommentParsingSteps.cs`: 行76
   - `IncrementalParsingSteps.cs`: 行67, 127

4. **InlineSyntax コレクションの設計パターン**
   - 既存のコレクション実装（`ChildNodesAndTokens` など）を参考にする
   - イミュータブルなコレクションとして実装

### 技術的決定事項

research.md に詳細を記載。

---

## Phase 1: Design & Contracts

### 生成された成果物

| 成果物 | 説明 |
|--------|------|
| [data-model.md](./data-model.md) | エンティティ定義、クラス階層、バリデーション ルール |
| [quickstart.md](./quickstart.md) | 新 API の使用方法と移行ガイド |

**注**: 本フィーチャーはライブラリの内部変更であり、REST API などの外部コントラクトは存在しないため、`contracts/` ディレクトリは作成しない。

### 主要な設計決定

1. **ImmutableArray の採用**: Roslyn パターンに倣い、`ImmutableArray<InlineSyntax>` を使用
2. **InlineElements の順序保証**: 構文上の出現順に並ぶことを保証
3. **マーカー後の空白**: マーカーの TrailingTrivia として保持
4. **パッケージ依存**: `System.Collections.Immutable`（.NET Standard 2.0 用）

---

## Constitution Check（Phase 1 後の再評価）

| 原則 | 状態 | 備考 |
|------|------|------|
| I. コード品質ファースト | ✅ PASS | 可読性・メンテナンス性を重視した設計 |
| II. モジュール設計 | ✅ PASS | 既存のモジュール構造を維持 |
| III. BDD必須 | ✅ PASS | コア ライブラリが対象、.feature ファイルを作成予定 |
| IV. 継続的品質保証 | ✅ PASS | 各ステップでビルド・テスト実行予定 |
| V. 警告ゼロポリシー | ✅ PASS | リファクタリング時に警告解消予定 |
| VI. フェーズ順序の厳守 | ✅ PASS | specify → clarify → plan の順序を遵守中 |

**追加確認**:
- ✅ 破壊的変更は許容される（プロジェクト未公開のため）
- ✅ 新規パッケージ依存は最小限（System.Collections.Immutable のみ）
- ✅ Roslyn パターンに準拠した設計
