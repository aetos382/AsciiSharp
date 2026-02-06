# Implementation Plan: SectionTitleSyntax の構成改定と TextSyntax のリネーム

**Branch**: `001-section-title-inline` | **Date**: 2026-02-05 (updated: 2026-02-06) | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-section-title-inline/spec.md`

## Summary

SectionTitleSyntax の内部構造を改定し、現在の「空白区切りによるタイトル分割」から「= トークン + 空白トリビア + InlineSyntax コレクション」の構成に変更する。これにより将来のインラインマークアップ対応の基盤を整備する。また、TextSyntax を InlineTextSyntax にリネームし、「一行の文字列」という意味を明確化する。

**追加要件（2026-02-06）**:
- FR-011: `=` が 7 個以上の行はセクション見出しとして認識せず、段落として扱う
- FR-012: `=` の後に空白がない行はセクション見出しとして認識せず、段落として扱う

## Technical Context

**Language/Version**: C# 14
**Primary Dependencies**: なし（コア ライブラリはサードパーティ依存なし）
**Storage**: N/A
**Testing**: MSTest.Sdk, Reqnroll (BDD)
**Target Platform**: .NET 10.0, .NET Standard 2.0（マルチターゲット）
**Project Type**: ライブラリ（NuGet パッケージ）
**Performance Goals**: 初期段階では重視しない（CLAUDE.md に記載）
**Constraints**: イミュータブル構文木、完全なテキスト復元可能性
**Scale/Scope**: コア ライブラリと関連テストプロジェクトの変更

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原則 | 状態 | 備考 |
|------|------|------|
| I. コード品質ファースト | PASS | 可読性・メンテナンス性を重視したリネーム |
| II. モジュール設計 | PASS | 既存のモジュール構造を維持 |
| III. BDD必須 | PASS | コア ライブラリが対象、.feature ファイル作成済み |
| IV. 継続的品質保証 | PASS | 各ステップでビルド・テスト実行 |
| V. 警告ゼロポリシー | PASS | リファクタリング時に警告解消 |
| VI. フェーズ順序の厳守 | PASS | specify → clarify → plan の順序を遵守 |

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
│   ├── Parser/
│   │   ├── Lexer.cs                     # 連続する = をまとめてトークン化（実装済み）
│   │   └── Parser.cs                    # セクション見出し認識条件の変更（FR-011, FR-012）
│   ├── Syntax/
│   │   ├── SectionTitleSyntax.cs        # 構造変更（実装済み）
│   │   ├── InlineTextSyntax.cs          # リネーム済み（旧 TextSyntax.cs）
│   │   ├── InlineSyntax.cs              # 基底クラス（変更なし）
│   │   ├── ISyntaxVisitor.cs            # VisitText → VisitInlineText（実装済み）
│   │   └── ISyntaxVisitorOfT.cs         # VisitText → VisitInlineText（実装済み）
│   └── SyntaxKind.cs                    # Text → InlineText（実装済み）
│
├── AsciiSharp.Asg/                       # 参照更新（実装済み）
│   └── AsgConverter.cs
│
└── AsciiSharp.TckAdapter/                # 影響なし

Test/
├── AsciiSharp.Specs/                     # BDD テスト
│   ├── Features/
│   │   ├── SectionTitleInlineElements.feature    # インライン要素テスト（実装済み）
│   │   └── SectionTitleRecognition.feature       # 認識条件テスト（Red 確認済み）
│   └── StepDefinitions/
│       └── SectionTitleInlineElementsSteps.cs    # ステップ定義（実装済み）
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

すべての調査課題は解決済み。詳細は [research.md](./research.md) を参照。

### 追加調査（FR-011, FR-012）

1. **AsciiDoc 仕様でのセクション見出しレベル上限**
   - 決定: `=` が 7 個以上の場合はセクション見出しとして認識しない
   - 根拠: VSCode AsciiDoc Extension のプレビュー表示では Level 7 は見出しとして扱われない
   - AsciiDoc 言語仕様でも Level 0〜5（`=` 1〜6 個）が定義されている

2. **マーカー後の空白必須性**
   - 決定: `=` の後に空白がない場合はセクション見出しとして認識しない
   - 根拠: AsciiDoc 仕様ではマーカーとタイトル本文の間に少なくとも 1 つの空白が必要

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

### FR-011/FR-012 の設計

**変更対象メソッド**:

1. `IsAtSectionTitle()` — セクション見出し判定に 2 つの条件を追加:
   - `=` の数が 6 以下であること（FR-011）
   - `=` の後に空白トークンが続くこと（FR-012）

2. `IsAtDocumentTitle()` — ドキュメントタイトル判定にも同じ空白条件を追加:
   - `=` が 1 つであること（既存）
   - `=` の後に空白トークンが続くこと（FR-012）

**実装方針**:
```csharp
// IsAtSectionTitle: 行頭の EqualsToken かつ 6 文字以下 かつ 次に空白がある
private bool IsAtSectionTitle()
{
    return this.Current.Kind == SyntaxKind.EqualsToken
        && this.Current.Text.Length <= 6
        && this.Peek().Kind == SyntaxKind.WhitespaceToken;
}

// IsAtDocumentTitle: 行頭の EqualsToken かつ 1 文字 かつ 次に空白がある
private bool IsAtDocumentTitle()
{
    return this.Current.Kind == SyntaxKind.EqualsToken
        && this.Current.Text.Length == 1
        && this.Peek().Kind == SyntaxKind.WhitespaceToken;
}

// IsAtSectionTitleOfLevelOrHigher: 既存の Level 比較 + 空白条件
private bool IsAtSectionTitleOfLevelOrHigher(int level)
{
    return this.Current.Kind == SyntaxKind.EqualsToken
        && this.Current.Text.Length <= level
        && this.Peek().Kind == SyntaxKind.WhitespaceToken;
}
```

**影響範囲**:
- Parser.cs の 3 メソッドのみ変更
- Lexer.cs は変更不要（連続する `=` のまとめ読みは既に対応済み）
- 構文木構造（SectionTitleSyntax）は変更不要

**検証**:
- `=` が 7 個以上 → `IsAtSectionTitle()` が false → `ParseParagraph()` にフォールバック
- `=` の後に空白なし → `IsAtSectionTitle()` が false → `ParseParagraph()` にフォールバック
- `== ` マーカーのみ（空タイトル）→ `IsAtSectionTitle()` が true → 既存のエラー回復で処理

---

## Constitution Check（Phase 1 後の再評価）

| 原則 | 状態 | 備考 |
|------|------|------|
| I. コード品質ファースト | PASS | 判定ロジックが明確で読みやすい |
| II. モジュール設計 | PASS | Parser 内部の変更のみ |
| III. BDD必須 | PASS | .feature ファイル作成済み、Red 確認済み |
| IV. 継続的品質保証 | PASS | 各ステップでビルド・テスト実行予定 |
| V. 警告ゼロポリシー | PASS | リファクタリング時に警告解消予定 |
| VI. フェーズ順序の厳守 | PASS | specify → clarify → plan の順序を遵守中 |

**追加確認**:
- 破壊的変更は許容される（プロジェクト未公開のため）
- 変更は Parser.cs の 3 メソッドのみで、影響範囲が限定的
- 既存のテスト（89 件）への影響なし（`==` のテストケースは空白を含む形式のみ）。新規 Red テスト 3 件を含め合計 92 件

## 現在の実装状況

### 完了済み
- Lexer: 連続する `=` をまとめてトークン化
- SectionTitleSyntax: `_children` パターンへの移行、`TitleContent` 削除
- TextSyntax → InlineTextSyntax リネーム
- SyntaxKind.Text → SyntaxKind.InlineText リネーム
- ISyntaxVisitor: VisitText → VisitInlineText
- InlineElements コレクションの追加
- SectionTitleRecognition.feature: FR-011/FR-012 の Red テスト作成

### 未実装（Green フェーズ）
- Parser.cs: `IsAtSectionTitle()` に `Text.Length <= 6` と空白チェックを追加
- Parser.cs: `IsAtDocumentTitle()` に空白チェックを追加
- Parser.cs: `IsAtSectionTitleOfLevelOrHigher()` に空白チェックを追加
