# Implementation Plan: TCK block/document/body-only テスト修正

**Branch**: `007-tck-fix` | **Date**: 2026-02-22 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/007-tck-fix/spec.md`

## Summary

ヘッダーを持たない AsciiDoc ドキュメントを ASG に変換する際、`attributes` フィールドが余分に JSON 出力に含まれるバグを修正する。`AsgDocument.Attributes` プロパティを null 許容に変更し、`AsgConverter.VisitDocument` でヘッダーなし時は `null` を設定することで、TCK の `block/document/body-only` テストを通過させる。変更対象は 2 ファイルのみで、既存の API との互換性への影響は最小限。

## Technical Context

**Language/Version**: C# 14 / .NET 10.0
**Primary Dependencies**: `System.Text.Json`（.NET 10.0 に同梱）
**Storage**: N/A
**Testing**: MSTest.Sdk、外部検証（TCK テスト：Docker 経由）
**Target Platform**: Linux x64（TCK は Docker コンテナ内で実行）
**Project Type**: ライブラリ（`AsciiSharp.Asg`、.NET 10.0 のみ）
**Performance Goals**: N/A（null チェック追加のみで実行パスに影響なし）
**Constraints**: `AsciiSharp.Asg` は .NET 10.0 対象のため .NET Standard 2.0 互換性不要
**Scale/Scope**: 2 ファイル・数行の変更

## Constitution Check

| 原則 | 状態 | 備考 |
|---|---|---|
| III. BDD 必須 | ✅ 遵守 | 変更対象は `AsciiSharp.Asg`（BDD 対象外）。TCK テストによる外部検証で品質を確保する |
| IV. 継続的品質保証 | ✅ 計画済み | 実装後に `dotnet build` と `dotnet test` を実行し、TCK で最終確認 |
| V. 警告ゼロポリシー | ✅ 計画済み | null 許容化に伴う警告（CS8618 等）がないことを Refactor ステップで確認 |
| VI. フェーズ順序 | ✅ 遵守 | specify 完了 → plan（現在）→ tasks → implement |

## Project Structure

### Documentation (this feature)

```text
specs/007-tck-fix/
├── plan.md              ← このファイル（/speckit.plan の出力）
├── data-model.md        ← Phase 1 出力
├── tasks.md             ← /speckit.tasks で生成
└── checklists/
    └── requirements.md
```

### Source Code (repository root)

```text
Source/AsciiSharp.Asg/
├── Models/
│   └── AsgDocument.cs        ← 変更1: Attributes プロパティを null 許容に変更
└── AsgConverter.cs           ← 変更2: VisitDocument でヘッダーなし時に Attributes = null を設定
```

**Structure Decision**: 既存ファイル 2 つへの最小限の変更。新規ファイルは不要。テスト追加も不要（TCK による外部検証で充足）。

## Research（Phase 0）

**調査不要。** 変更内容は spec.md の「実装詳細」セクションで完全に定義済みであり、技術的な不確実性はない。

| 調査項目 | 結果 |
|---|---|
| `JsonIgnoreCondition.WhenWritingNull` の利用可否 | .NET 10.0 の `System.Text.Json` で利用可能。`AsciiSharp.Asg` は .NET 10.0 のみ対象のため問題なし |
| .NET Standard 2.0 互換性 | `AsciiSharp.Asg` は .NET 10.0 のみのため考慮不要 |
| 既存 API への影響 | `AsgDocument.Attributes` の型が `IReadOnlyDictionary<string, string>` → `IReadOnlyDictionary<string, string>?` に変わるが、現在の呼び出し元は `AsgConverter` 経由のみで影響は最小限 |
| 他の失敗 TCK テストへの波及 | ヘッダーなしの block テスト（paragraph、list、listing、section、sidebar）は同じ原因で失敗している可能性があり、この修正で同時に通過する見込みがある |

## Design（Phase 1）

### データモデル変更（data-model.md 参照）

**AsgDocument — 変更箇所**:

| プロパティ | 変更前 | 変更後 |
|---|---|---|
| `Attributes` の型 | `IReadOnlyDictionary<string, string>` | `IReadOnlyDictionary<string, string>?` |
| デフォルト値 | `= new Dictionary<string, string>()` | なし（null がデフォルト） |
| JSON シリアライズ | 常に出力（空でも `{}`） | null のとき省略、非 null のとき出力 |

**AsgConverter.Visitor.VisitDocument — 変更箇所**:

| 変更点 | 変更前 | 変更後 |
|---|---|---|
| `attributes` 変数の設定 | `ConvertAttributes(node.Header)` | `node.Header is not null ? ConvertAttributes(node.Header) : null` |

### API contracts

N/A。本変更は JSON シリアライズ出力の修正であり、REST/GraphQL エンドポイントは存在しない。

### テスト戦略（BDD 対象外）

`AsciiSharp.Asg` は BDD 対象外のため LightBDD フィーチャー クラスは作成しない。以下の順序で検証する。

1. `dotnet build` でビルドが成功することを確認
2. `dotnet test` で既存ユニットテストがすべて通ることを確認
3. TCK 実行（`docker buildx bake tck && docker run --rm asciisharp-tck`）で以下を確認:
   - `block/document/body-only` がパスに変わる
   - `block/header/attribute entries below title` が引き続きパス

## Complexity Tracking

違反なし。
