# Implementation Plan: AsciiDoc パーサー

**Branch**: `001-asciidoc-parser` | **Date**: 2026-01-18 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-asciidoc-parser/spec.md`

## Summary

AsciiDoc 文書を解析し、Roslyn スタイルの二層構文木構造を持つロスレス構文木（Concrete Syntax Tree）を生成するパーサーを実装する。文法は PEG で定義するが、パーサー自体は手書きで実装する（パーサージェネレーターは使用しない）。エラー耐性解析、不変構文木、増分解析をサポートする。

**MVP スコープ**: ドキュメントヘッダー（タイトル、著者行）、セクション、段落、リンク、コメントのみ。リスト、テーブル、書式マークアップ、マクロは後続イテレーションで実装。

## Technical Context

**Language/Version**: C# 14 (latest)
**Target Framework**:
- コアライブラリ: .NET Standard 2.0 + .NET 10（マルチターゲット）
- テスト: .NET 10 / .NET Framework 4.8.1
**Primary Dependencies**:
- コアライブラリ: なし（依存関係最小化）
- Polyfill: 必要に応じて型ベースの機能のみ（ランタイム依存機能は不可）
**Parser Implementation**: 手書きパーサー（パーサージェネレーター不使用）
**Storage**: N/A（インメモリ処理のみ）
**Testing**:
- ユニットテスト: MSTest.Sdk
- BDD: Reqnroll
- ベンチマーク: BenchmarkDotNet
**Target Platform**: .NET Standard 2.0 互換環境（.NET Core 2.0+, .NET Framework 4.6.1+, Mono 5.4+ など）
**Project Type**: ライブラリ
**AOT Compatibility**:
- .NET 10 ターゲットで `IsAotCompatible=true` を有効化
- CI で AOT 互換性警告を検出
**Performance Goals**:
- 100KB 以下の文書: 初回解析 500ms 以内
- 増分解析: 全体解析の 10% 以下（変更が 5% 以下の場合）
**Constraints**:
- .NET Standard 2.0 互換（ランタイム依存の C# 機能は使用不可）
- AOT 互換（リフレクション最小化、.NET 10 でチェック）
- パーサージェネレーター不使用（手書き実装）
**Scale/Scope**:
- 一般的な文書サイズ: 100KB 以下
- MVP スコープ: ドキュメントヘッダー（タイトル、著者行）、セクション、段落、リンク、コメント

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### I. コード品質ファースト ✅

- 可読性: 二層構文木モデルは Roslyn の実績あるパターンに従う
- テスト可能性: 不変構文木により状態管理が単純化
- パフォーマンス: 増分解析、構造共有によりメモリ効率を確保
- 可観測性: Diagnostic エンティティで構文エラーを追跡

### II. モジュール設計 ✅

- **AsciiSharp**: コアパーサーライブラリ（.NET Standard 2.0）- Source/ フォルダ
- **AsciiSharp.Tests**: ユニットテスト（.NET 10 / .NET Framework 4.8.1）- Test/ フォルダ
- **AsciiSharp.Specs**: BDD 受け入れテスト（.NET 10）- Test/ フォルダ
- **AsciiSharp.Benchmarks**: パフォーマンステスト（.NET 10）- Benchmark/ フォルダ

### III. BDD 必須 ✅

- Reqnroll で Given-When-Then 形式のシナリオを記述
- User Story の Acceptance Scenarios をそのまま BDD テストに変換
- Red-Green-Refactor サイクルを厳守

### IV. 継続的品質保証 ✅

- 各 BDD サイクル後にビルドとテスト実行
- CI パイプラインで .NET 10 / .NET Framework 4.8.1 両方でテスト

### V. 警告ゼロポリシー ✅

- Directory.Build.props で WarningLevel=9999, AnalysisLevel=latest-all 設定済み
- Refactoring ステップで警告解消

## Project Structure

### Documentation (this feature)

```text
specs/001-asciidoc-parser/
├── spec.md              # 機能仕様書
├── plan.md              # この実装計画
├── research.md          # Phase 0: 技術調査
├── data-model.md        # Phase 1: データモデル
├── quickstart.md        # Phase 1: クイックスタートガイド
├── tasks.md             # Phase 2: タスク分解（/speckit.tasks で生成）
└── grammar/             # PEG 文法定義（参照仕様、パーサー実装ガイド）
    └── asciidoc.peg     # AsciiDoc の PEG 文法
```

### Source Code (repository root)

```text
Source/
└── AsciiSharp/                          # コアライブラリ (.NET Standard 2.0)
    ├── AsciiSharp.csproj
    ├── InternalSyntax/                  # 内部構文木
    │   ├── InternalNode.cs
    │   ├── InternalToken.cs
    │   └── InternalTrivia.cs
    ├── Syntax/                          # 外部構文木（公開 API）
    │   ├── SyntaxNode.cs
    │   ├── SyntaxToken.cs
    │   ├── SyntaxTrivia.cs
    │   └── [AsciiDoc 要素ごとのノード]
    ├── Parser/                          # 手書きパーサー
    │   ├── Lexer.cs                     # 字句解析
    │   ├── Parser.cs                    # 構文解析
    │   └── ParserRecovery.cs            # エラー回復
    ├── Text/                            # テキスト処理
    │   ├── SourceText.cs
    │   └── TextSpan.cs
    ├── Diagnostics/                     # 診断情報
    │   └── Diagnostic.cs
    └── Polyfills/                       # .NET Standard 2.0 用 Polyfill
        └── [必要に応じて追加]

Test/
├── AsciiSharp.Tests/                    # ユニットテスト
│   ├── AsciiSharp.Tests.csproj          # TargetFrameworks: net10.0;net481
│   ├── InternalSyntax/
│   ├── Syntax/
│   ├── Parser/
│   └── Text/
│
└── AsciiSharp.Specs/                    # BDD テスト (Reqnroll)
    ├── AsciiSharp.Specs.csproj          # TargetFramework: net10.0
    └── Features/
        ├── BasicParsing.feature
        ├── ErrorRecovery.feature
        └── [User Story ごとの .feature]

Benchmark/
└── AsciiSharp.Benchmarks/               # ベンチマーク
    ├── AsciiSharp.Benchmarks.csproj     # TargetFramework: net10.0
    └── ParserBenchmarks.cs
```

**Structure Decision**: 既存の Source/Test 構造を維持し、二層構文木アーキテクチャに基づいて InternalSyntax（内部構文木）と Syntax（外部構文木）を分離する。

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| 二層構文木構造 | 増分解析と構造共有に必須 | 単一構造では編集時のパフォーマンスが悪化 |
| .NET Standard 2.0 制約 | 広範な互換性が必要 | 最新 .NET のみでは .NET Framework ユーザーを排除 |
| 手書きパーサー | エラー回復と増分解析の細かい制御が必要 | パーサージェネレーターでは柔軟性が不足 |
| PEG 文法定義（参照用） | 文法の正式な仕様として実装ガイドに使用 | 口頭説明のみでは仕様の曖昧さが残る。ただしパーサージェネレーターは使用しない |
