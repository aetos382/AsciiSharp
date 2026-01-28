# Implementation Plan: AsciiDoc ASG モデルクラス

**Branch**: `001-asg-model` | **Date**: 2026-01-28 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-asg-model/spec.md`
**Status**: 実装完了

## Summary

AsciiSharp の SyntaxTree を AsciiDoc TCK が期待する ASG (Abstract Semantic Graph) JSON 形式に変換する機能を実装する。`ISyntaxVisitor<T>` パターンを使用して各 SyntaxNode を対応する ASG ノードに変換し、System.Text.Json の Source Generator を使用して AOT 互換の JSON シリアライズを実現する。

## Technical Context

**Language/Version**: C# 14 / .NET 10.0
**Primary Dependencies**: System.Text.Json、AsciiSharp.Syntax (ISyntaxVisitor<T>)
**Storage**: N/A
**Testing**: MSTest.Sdk、Reqnroll (BDD)
**Target Platform**: .NET 10.0 (TckAdapter プロジェクト)
**Project Type**: ライブラリ
**Performance Goals**: N/A（初期フェーズでは重視しない）
**Constraints**: AOT 互換性必須（IsAotCompatible=true）
**Scale/Scope**: TCK テストケースに対応する ASG 変換

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原則 | 状態 | 確認内容 |
|------|------|----------|
| I. コード品質ファースト | ✅ | 可読性・メンテナンス性を考慮した設計 |
| II. モジュール設計 | ✅ | Asg/ ディレクトリに Models, Serialization, Converter を分離 |
| III. BDD必須 | ⚠️ | 実装先行のため、.feature ファイルは後続タスクで作成 |
| IV. 継続的品質保証 | ✅ | ビルド・テスト成功後にコミット |
| V. 警告ゼロポリシー | ✅ | 警告ゼロを達成（CA1822 は csproj で抑制、理由明記） |

## Project Structure

### Documentation (this feature)

```text
specs/001-asg-model/
├── plan.md              # このファイル
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (後続)
```

### Source Code (実装済み)

```text
Source/TckAdapter/AsciiSharp.TckAdapter/
├── AsciiSharp.TckAdapter.csproj   # プロジェクトファイル（NoWarn 設定追加）
└── Asg/
    ├── AsgConverter.cs            # ISyntaxVisitor<AsgNode?> 実装
    ├── Models/
    │   ├── AsgNode.cs             # 基底クラス（Location プロパティ）
    │   ├── AsgBlockNode.cs        # ブロック要素の基底（Type="block"）
    │   ├── AsgInlineNode.cs       # インライン要素の基底
    │   ├── AsgDocument.cs         # document ブロック
    │   ├── AsgSection.cs          # section ブロック
    │   ├── AsgParagraph.cs        # paragraph ブロック
    │   ├── AsgHeader.cs           # 文書ヘッダー
    │   ├── AsgText.cs             # text インライン
    │   ├── AsgPosition.cs         # 位置情報（line, col）
    │   └── AsgLocation.cs         # 位置範囲（start, end）
    └── Serialization/
        ├── AsgJsonContext.cs              # AOT 互換 JsonSerializerContext
        └── AsgLocationJsonConverter.cs    # Location の配列形式変換

Test/AsciiSharp.Specs/Features/
└── (後続で .feature ファイルを作成)
```

**Structure Decision**: TckAdapter プロジェクト内に `Asg/` ディレクトリを作成し、Models、Serialization、変換ロジックを分離配置。

## Complexity Tracking

| 追加項目 | 理由 | シンプルな代替案を却下した理由 |
|----------|------|-------------------------------|
| AsgLocationJsonConverter | TCK が期待する `[{start}, {end}]` 配列形式に対応 | デフォルトの JSON シリアライズではオブジェクト形式になるため |
| CA1822 抑制 (csproj) | JSON シリアライズに必要なインスタンス プロパティ | 各ファイルに pragma を書くより csproj 一括が保守しやすい |

## Implementation Notes

### 設計決定

1. **ISyntaxVisitor<T> パターン**: AsciiSharp のビジターパターンを活用して型安全な変換を実現
2. **yield return**: ConvertBlocks、ConvertSectionContent でイテレーターを使用し、メモリ効率を向上
3. **AOT 互換**: `AsgJsonContext` で JsonSerializerContext を定義し、リフレクションフリーの JSON シリアライズを実現
4. **位置情報変換**: AsciiSharp の 0-based オフセットを TCK の 1-based {line, col} に変換

### 未対応ノード

以下の SyntaxNode は現時点でスキップ（null を返す）:
- `LinkSyntax`
- `AuthorLineSyntax`
- `DocumentHeaderSyntax`（直接変換せず、ConvertHeader 経由）
- `DocumentBodySyntax`（直接変換せず、ConvertBlocks 経由）
- `SectionTitleSyntax`（直接変換せず、ConvertTitleInlines 経由）
