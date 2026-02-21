# Implementation Plan: インライン要素とブロック要素のセマンティクス定義

**Branch**: `006-element-type-semantics` | **Date**: 2026-02-21 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/006-element-type-semantics/spec.md`

## Summary

`BlockSyntax` の適用範囲を AsciiDoc 言語仕様でブロックとされる要素のみに限定する。
`SectionTitleSyntax`・`AuthorLineSyntax`・`AttributeEntrySyntax`・`DocumentHeaderSyntax`・`DocumentBodySyntax` の基底クラスを `BlockSyntax` から `SyntaxNode` 直接継承に変更し、`StructuredTriviaSyntax` 抽象クラスを新規作成する。`SyntaxKind.TextSpan` enum 値は削除する。

## Technical Context

**Language/Version**: C# 14 / .NET 10.0（+ .NET Standard 2.0 multiターゲット）
**Primary Dependencies**: MSTest.Sdk、LightBDD.MsTest4（テストのみ）
**Storage**: N/A（ライブラリ）
**Testing**: MSTest + LightBDD（コアライブラリは BDD 必須）
**Target Platform**: .NET 10.0 + .NET Standard 2.0
**Project Type**: クラスライブラリ（型階層のリファクタリング）
**Performance Goals**: N/A（型階層の変更のみ、パフォーマンス影響なし）
**Constraints**: .NET Standard 2.0 対応必須（`Source/AsciiSharp`）
**Scale/Scope**: 内部型階層のリファクタリング。外部 API（Visitor インターフェース等）は変更なし

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原則 | 状態 | 備考 |
|------|------|------|
| III. BDD 必須 | ✅ PASS | `ElementTypeSemantics006Feature` を plan フェーズで作成。対象は `Source/AsciiSharp` |
| VI. フェーズ順序 | ✅ PASS | specify → clarify → plan（現在）の順序を遵守 |
| V. 警告ゼロ | ✅ PASS | 現在警告ゼロ。実装後も維持する |
| IV. 継続的品質保証 | ✅ PASS | 実装後に全テストを実行して確認する |
| II. モジュール設計 | ✅ PASS | 型階層の変更のみ。モジュール境界に影響なし |

## Project Structure

### Documentation (this feature)

```text
specs/006-element-type-semantics/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
Source/AsciiSharp/
├── SyntaxKind.cs                        ← TextSpan 削除、InlineText = 401 明示
└── Syntax/
    ├── BlockSyntax.cs                   ← XML ドキュメント コメント更新
    ├── StructuredTriviaSyntax.cs        ← 新規作成
    ├── SectionTitleSyntax.cs            ← BlockSyntax → SyntaxNode、XML ドキュメント コメント更新
    ├── AuthorLineSyntax.cs              ← BlockSyntax → SyntaxNode、XML ドキュメント コメント更新
    ├── AttributeEntrySyntax.cs          ← BlockSyntax → SyntaxNode、XML ドキュメント コメント更新
    ├── DocumentHeaderSyntax.cs          ← BlockSyntax → SyntaxNode、XML ドキュメント コメント更新
    └── DocumentBodySyntax.cs            ← BlockSyntax → SyntaxNode、XML ドキュメント コメント更新

Test/AsciiSharp.Specs/Features/
├── ElementTypeSemantics006Feature.cs       ← 新規（BDD Red）
├── ElementTypeSemantics006Feature.Steps.cs ← 新規（BDD Red スタブ）
├── BlockInlineSyntaxFeature.cs             ← SectionTitle 関連シナリオ更新（実装フェーズ）
└── BlockInlineSyntaxFeature.Steps.cs       ← SectionTitle 関連ステップ更新（実装フェーズ）
```

**Structure Decision**: 既存の単一プロジェクト構造を維持。型階層の変更のみなので新たなモジュール分割は不要。

## Complexity Tracking

> 憲章違反なし。追加の複雑性なし。

## Implementation Design

### Phase 0 で解決した調査事項

すべて research.md で解決済み（NEEDS CLARIFICATION なし）。

### 変更の依存関係と順序

```
1. SyntaxKind.TextSpan 削除（InlineText = 401 明示）
   ↓（独立）
2. BlockSyntax XML ドキュメント コメント更新
   ↓（独立）
3. StructuredTriviaSyntax 新規作成
   ↓（独立）
4. SectionTitleSyntax 基底クラス変更 + XML ドキュメント コメント更新
4. AuthorLineSyntax 基底クラス変更 + XML ドキュメント コメント更新
4. AttributeEntrySyntax 基底クラス変更 + XML ドキュメント コメント更新
4. DocumentHeaderSyntax 基底クラス変更 + XML ドキュメント コメント更新
4. DocumentBodySyntax 基底クラス変更 + XML ドキュメント コメント更新
   ↓（4 の後）
5. BlockInlineSyntaxFeature シナリオ更新
```

各ステップは独立して実施可能（2〜4 は並行可能）。

### BDD Red ステップ（plan フェーズで作成）

`ElementTypeSemantics006Feature` を以下の 2 シナリオで作成:

**シナリオ 1**: `AsciiDoc仕様のブロックとされないノードはBlockSyntaxではない()`
- Given: セクションタイトル・著者行・属性エントリを含む AsciiDoc 文書
- When: 文書を解析する
- Then: `SectionTitleSyntax` は `BlockSyntax` ではない
- Then: `DocumentHeaderSyntax` は `BlockSyntax` ではない
- Then: `AuthorLineSyntax` は `BlockSyntax` ではない
- Then: `AttributeEntrySyntax` は `BlockSyntax` ではない

**シナリオ 2**: `StructuredTriviaSyntaxはSyntaxNodeを継承している()`
- Then: `StructuredTriviaSyntax` は `SyntaxNode` のサブクラスである（リフレクションで確認）
- Then: `StructuredTriviaSyntax` は `BlockSyntax` のサブクラスではない
- Then: `StructuredTriviaSyntax` は `InlineSyntax` のサブクラスではない

### 既存テストの更新（実装フェーズ）

`BlockInlineSyntaxFeature.cs` の `セクション関連ノードはBlockSyntaxとして識別できる()` シナリオを更新:

- `SectionTitleノードはBlockSyntax()` → `SectionTitleノードはBlockSyntaxではない()` に変更
- シナリオ名を `セクションノードはBlockSyntaxだがSectionTitleはBlockSyntaxではない()` に変更

### コンストラクター変更の詳細

変更対象クラスは `BlockSyntax` から `SyntaxNode` に変更するだけ。コンストラクター本体は不変:

```csharp
// 変更前
internal SectionTitleSyntax(...) : base(internalNode, parent, position, syntaxTree)

// 変更後（: BlockSyntax → : SyntaxNode）
internal SectionTitleSyntax(...) : base(internalNode, parent, position, syntaxTree)
```

`SyntaxNode` コンストラクターのアクセス修飾子は `private protected` であり、同一アセンブリ内からの呼び出しに制限されているが、`Source/AsciiSharp` 内のクラスはすべて同一アセンブリなので問題ない。
