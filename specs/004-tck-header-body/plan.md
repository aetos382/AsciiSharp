# Implementation Plan: TCK header-body-output テスト対応

**Branch**: `004-tck-header-body` | **Date**: 2026-02-08 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/004-tck-header-body/spec.md`

## Summary

AsciiDoc TCK の `header-body-output` テスト ケースをパスさせるため、以下を実装する:
1. SyntaxTree レベルで属性エントリ（`:name: value`）のパースを追加
2. ASG モデルに `attributes` フィールドを追加し、属性エントリをキー・値ペアとして出力
3. 既存の ASG 変換で header-body テストの期待出力と一致する JSON を生成

## Technical Context

**Language/Version**: C# 14 / .NET 10.0 + .NET Standard 2.0
**Primary Dependencies**: AsciiSharp (コア パーサー), AsciiSharp.Asg (ASG 変換), AsciiSharp.TckAdapter (TCK アダプター)
**Storage**: N/A
**Testing**: MSTest.Sdk + Reqnroll (BDD)
**Target Platform**: .NET 10.0 (+ .NET Standard 2.0 互換)
**Project Type**: ライブラリ
**Performance Goals**: 初期段階では重視しない（CLAUDE.md 方針）
**Constraints**: .NET Standard 2.0 互換性を維持すること
**Scale/Scope**: 単一テスト ケース（header-body-output）の合格 + 属性エントリ パース基盤

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原則 | 適合状況 | 備考 |
|------|---------|------|
| I. コード品質ファースト | PASS | 可読性・メンテナンス性・テスト可能性を重視した設計 |
| II. モジュール設計 | PASS | 既存のプロジェクト分割（AsciiSharp / Asg / TckAdapter）に従う |
| III. BDD 必須 | PASS | コア ライブラリ（AsciiSharp）の変更は BDD で検証。Asg はユニット テスト |
| IV. 継続的品質保証 | PASS | Green/Refactor 後にビルド・テスト実行 |
| V. 警告ゼロポリシー | PASS | Refactor ステップで警告解消 |
| VI. フェーズ順序の厳守 | PASS | specify → clarify → plan → tasks → analyze → implement |

## Project Structure

### Documentation (this feature)

```text
specs/004-tck-header-body/
├── spec.md              # 仕様書
├── plan.md              # 本ファイル
├── research.md          # Phase 0 調査結果
├── data-model.md        # Phase 1 データモデル
├── quickstart.md        # Phase 1 クイックスタート
└── checklists/
    └── requirements.md  # 品質チェックリスト
```

### Source Code (repository root)

```text
Source/
├── AsciiSharp/                         # コア ライブラリ
│   ├── SyntaxKind.cs                   # [変更] AttributeEntry 追加
│   ├── Parser/
│   │   └── Parser.cs                   # [変更] ParseAttributeEntry() 追加
│   └── Syntax/
│       ├── AttributeEntrySyntax.cs     # [新規] Red Tree ノード
│       ├── DocumentHeaderSyntax.cs     # [変更] AttributeEntries プロパティ追加
│       ├── ISyntaxVisitor.cs           # [変更] VisitAttributeEntry 追加
│       └── ISyntaxVisitorOfT.cs        # [変更] VisitAttributeEntry 追加
├── AsciiSharp.Asg/
│   ├── Models/
│   │   └── AsgDocument.cs             # [変更] Attributes プロパティ追加
│   ├── AsgConverter.cs                 # [変更] 属性エントリ変換ロジック追加
│   └── Serialization/
│       └── AsgJsonContext.cs           # [変更] Dictionary 型登録
└── AsciiSharp.TckAdapter/
    └── (変更なし)

Test/
├── AsciiSharp.Specs/
│   ├── Features/
│   │   └── AttributeEntryParsing.feature  # [新規] BDD テスト
│   └── StepDefinitions/
│       └── AttributeEntrySteps.cs         # [新規] ステップ定義
└── AsciiSharp.Asg.Tests/
    └── AsgConverterTests.cs               # [変更] attributes テスト追加
```

**Structure Decision**: 既存のプロジェクト構造を維持し、必要なファイルのみを追加・変更する。新規プロジェクトの作成は不要。

## Design Decisions

### D-001: 属性エントリの Green Tree 構造

属性エントリは `StartNode(SyntaxKind.AttributeEntry)` / `FinishNode()` で構築される `InternalSyntaxNode` として表現する。閉じコロン後の空白と行末の改行はトリビアとして扱う。

**値あり（`:icons: font\n`）**:
```
AttributeEntry
├── ColonToken (:)                              ← 開きコロン
├── TextToken (icons)                           ← 属性名
├── ColonToken (:) [trailingTrivia: " "]        ← 閉じコロン（空白はトリビア）
└── TextToken (font) [trailingTrivia: "\n"]     ← 属性値（改行はトリビア）
```

**値なし（`:toc:\n`）**:
```
AttributeEntry
├── ColonToken (:)                              ← 開きコロン
├── TextToken (toc)                             ← 属性名
└── ColonToken (:) [trailingTrivia: "\n"]       ← 閉じコロン（改行はトリビア）
```

### D-002: パーサーでの認識フロー

`ParseDocumentHeader()` を拡張:

```
ParseDocumentHeader():
  StartNode(DocumentHeader)
  ParseSectionTitle()                    # 既存
  ParseAuthorLine() (条件付き)           # 既存
  while (行頭が ColonToken):             # 新規
    ParseAttributeEntry()
  SkipBlankLines()                       # 既存
  FinishNode()
```

### D-003: ASG の attributes フィールド

- 型: `IReadOnlyDictionary<string, string>`
- 属性なし → 空辞書 `{}` を出力（null ではない）
- `[JsonIgnore]` は使用しない（常に出力）
- AsgConverter の `VisitDocument` で DocumentHeaderSyntax の AttributeEntries から変換

### D-004: ISyntaxVisitor の拡張

- `VisitAttributeEntry(AttributeEntrySyntax node)` を両インターフェースに追加
- AsgConverter の Visitor 実装では `null` を返す（属性エントリは VisitDocument 内でヘッダーから直接取得するため）

### D-005: JSON プロパティ順序

- `[JsonPropertyOrder]` はプロジェクト全体で使用しない
- TCK がセマンティック比較（`assert.deepEqual`）を使用するため、出力順序は無関係

### D-006: 構文木の子ノード コレクション型

- 構文木の子ノード コレクションには `SyntaxList<T>` を使用する（Roslyn の設計哲学に準拠、R-009/R-010 参照）
- `ImmutableArray<T>` は構文木外の結果セットに使用する
- 既存の `SectionTitleSyntax.InlineElements` も `ImmutableArray<InlineSyntax>` → `SyntaxList<InlineSyntax>` に移行する
- AsciiSharp の `SyntaxList<T>` は現時点で配列ベースの簡易実装（Green Tree 統合なし）だが、公開 API 型として採用し、内部最適化は後続タスクとする
