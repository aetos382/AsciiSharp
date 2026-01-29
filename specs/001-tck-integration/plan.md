# Implementation Plan: TCK 統合テスト基盤

**Branch**: `001-tck-integration` | **Date**: 2026-01-29 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-tck-integration/spec.md`

## Summary

AsciiDoc Technology Compatibility Kit (TCK) を使用して AsciiSharp パーサーの AsciiDoc Language Specification への準拠を検証するためのテスト基盤を構築する。CLI アダプターが標準入力から TCK 形式の JSON を受け取り、AsgConverter を使って ASG (Abstract Semantic Graph) 形式に変換し、標準出力に出力する。Docker を使用したコンテナ化と GitHub Actions での自動実行も実装する。

## Technical Context

**Language/Version**: C# 14 / .NET 10.0
**Primary Dependencies**: System.Text.Json (AOT 対応 JSON シリアライゼーション)、AsciiSharp コア ライブラリ
**Storage**: N/A（ファイルストレージ不要、標準入出力のみ）
**Testing**: MSTest.Sdk、Reqnroll (BDD)
**Target Platform**: Linux x64 (Docker コンテナ内)、Windows x64 (開発環境)
**Project Type**: Single（既存のプロジェクト構造に統合）
**Performance Goals**: TCK テスト全体が CI 環境で 5 分以内に完了
**Constraints**: PublishAot 対応必須（Docker イメージサイズ最小化）、Node.js 20 以上（TCK 実行に必要）
**Scale/Scope**: TCK テストスイート全体（仕様に基づく全テストケース）

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原則 | ステータス | 説明 |
|------|-----------|------|
| I. コード品質ファースト | ✅ 準拠 | 可読性、メンテナンス性、テスト可能性を優先 |
| II. モジュール設計 | ✅ 準拠 | TckAdapter は独立したプロジェクトとして分離済み |
| III. BDD必須 | ✅ 準拠 | TckAdapter は BDD 対象外（constitution v1.4.0）。TCK テストによる外部検証で品質確保 |
| IV. 継続的品質保証 | ✅ 準拠 | CI で TCK テストを自動実行 |
| V. 警告ゼロポリシー | ✅ 準拠 | Refactor ステップで全警告を解消 |
| VI. フェーズ順序の厳守 | ✅ 準拠 | specify → clarify → plan → tasks → analyze → implement |

## Project Structure

### Documentation (this feature)

```text
specs/001-tck-integration/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── tck-input.schema.json
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
Source/
├── AsciiSharp/                           # コア ライブラリ
│   └── (既存のパーサー実装)
├── AsciiSharp.Asg/                       # ASG 変換ライブラリ
│   ├── Models/                           # ASG モデル (既存)
│   ├── Serialization/                    # JSON シリアライゼーション (既存)
│   └── AsgConverter.cs                   # 変換ロジック (既存)
└── AsciiSharp.TckAdapter/                # CLI アプリケーション
    ├── Program.cs                        # エントリポイント
    ├── TckInput.cs                       # TCK 入力モデル
    ├── TckJsonContext.cs                 # JSON シリアライゼーション
    └── Dockerfile                        # Docker ビルド設定

Test/
├── AsciiSharp.Asg.Tests/                 # ASG ユニット テスト
├── AsciiSharp.Specs/                     # BDD テスト（コア ライブラリ用）
└── AsciiSharp.Tests/                     # ユニット テスト

.github/workflows/
├── build.yml                             # ビルド・テスト (既存)
└── tck.yml                               # TCK テスト実行

docker-bake.hcl                           # Docker ビルド設定
```

**Structure Decision**: AsciiSharp.Asg ライブラリと AsciiSharp.TckAdapter CLI に分離。GitHub Actions に TCK ワークフローを追加。

## Complexity Tracking

> Constitution Check に違反はないため、この表は空です。

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| - | - | - |
