# Research: SectionTitleSyntax の構成改定と TextSyntax のリネーム

**Feature Branch**: `001-section-title-inline`
**Date**: 2026-02-05

## 1. 現在の実装分析

### 1.1 SectionTitleSyntax の現在の構造

**ファイル**: `Source/AsciiSharp/Syntax/SectionTitleSyntax.cs`

現在の実装:
- `BlockSyntax` を継承
- 内部で `List<SyntaxToken>` を保持し、すべてのトークンを管理
- `TitleContent` プロパティは `StringBuilder` で全テキストを結合して生成
- 空白はマーカー後の最初の空白のみスキップし、以降は `TitleContent` に含まれる

**現在のプロパティ**:
```csharp
public int Level { get; }           // = の数
public SyntaxToken? TitleText { get; }  // 最初のテキストトークン
public SyntaxToken? Marker { get; }     // 最初の = トークン
public string TitleContent { get; }     // 結合されたタイトル文字列
```

**問題点**:
- `TitleContent` は文字列であり、インライン要素として走査できない
- 将来のインラインマークアップ（リンク、書式設定など）に対応できない
- タイトル内の構造を保持していない

### 1.2 TextSyntax の現在の構造

**ファイル**: `Source/AsciiSharp/Syntax/TextSyntax.cs`

現在の実装:
- `InlineSyntax` を継承
- `Text` プロパティで `Internal.ToFullString()` を返却
- リーフノード（子要素なし）

**名前の問題**:
- `TextSyntax` という名前は汎用的すぎる
- 将来的にインラインマークアップ（リンク、強調など）が増えた際に、プレーンテキストとの区別が曖昧になる

### 1.3 ParagraphSyntax のパターン（参考）

**ファイル**: `Source/AsciiSharp/Syntax/ParagraphSyntax.cs`

段落は既にインライン要素コレクションを持っている:
```csharp
private readonly List<SyntaxNode> _inlineElements = [];
public IReadOnlyList<SyntaxNode> InlineElements => this._inlineElements;
```

この実装パターンを参考に、`SectionTitleSyntax` では Roslyn パターンに倣い `ImmutableArray<T>` を使用する。

## 2. 技術的決定事項

### Decision 1: SectionTitleSyntax の新構造

**決定**: Roslyn パターンに倣い、`ImmutableArray<InlineSyntax>` でインライン要素コレクションを持つ

**新しい構造**:
```csharp
public sealed class SectionTitleSyntax : BlockSyntax
{
    public int Level { get; }
    public SyntaxToken? Marker { get; }
    public ImmutableArray<InlineSyntax> InlineElements { get; }
    // TitleContent は削除
    // TitleText は削除
}
```

**根拠**:
- Roslyn の設計パターンに沿う（構文木はイミュータブル）
- `ImmutableArray<T>` は値型で、メモリ効率が良い
- 型安全性の向上（`SyntaxNode` ではなく `InlineSyntax`）
- 将来のインラインマークアップ対応が容易

**代替案（却下）**:
- `IReadOnlyList<InlineSyntax>` を使用 → Roslyn パターンに沿わない
- `TitleContent` を残す → 冗長であり、インライン要素から取得可能

### Decision 2: InlineElements の順序要件

**決定**: `InlineElements` は構文上の出現順に並べる

**要件**:
- `InlineElements` の各要素は、ソース テキスト内での出現順に格納される
- 各要素の `Position` は、前の要素の `Position` 以上でなければならない

**検証テスト**:
```csharp
// InlineElements が順序通りであることを検証
for (int i = 1; i < sectionTitle.InlineElements.Length; i++)
{
    Assert.IsTrue(
        sectionTitle.InlineElements[i].Position >= sectionTitle.InlineElements[i - 1].Position,
        "InlineElements は構文上の出現順に並んでいなければならない");
}
```

**根拠**:
- 構文木の走査が予測可能になる
- ソース テキストとの対応関係が明確になる

### Decision 3: TextSyntax のリネーム

**決定**: `TextSyntax` を `InlineTextSyntax` にリネーム

**根拠**:
- 「インライン要素としてのプレーンテキスト」という意味が明確
- 将来の `LinkSyntax`、`EmphasisSyntax` などと並べたときに一貫性がある

**影響範囲**:
- ファイル名: `TextSyntax.cs` → `InlineTextSyntax.cs`
- クラス名: `TextSyntax` → `InlineTextSyntax`
- Visitor メソッド: `VisitText` → `VisitInlineText`
- SyntaxKind: `Text` → `InlineText`

### Decision 4: SyntaxKind.Text のリネーム

**決定**: `SyntaxKind.Text` を `SyntaxKind.InlineText` にリネーム

**根拠**:
- クラス名との一貫性
- `TextToken`（トークン）と `InlineText`（ノード）の区別が明確

### Decision 5: TitleContent の代替

**決定**: タイトル文字列の取得はインライン要素コレクションから行う

**取得方法**:
```csharp
// 例: 単一の InlineTextSyntax の場合
var titleText = sectionTitle.InlineElements
    .OfType<InlineTextSyntax>()
    .Select(t => t.Text)
    .FirstOrDefault();

// または ToFullString() を使用
var titleFullString = string.Join("", sectionTitle.InlineElements.Select(e => e.ToFullString()));
```

**根拠**:
- インライン要素コレクションから取得可能であり、冗長なプロパティは不要
- 将来的にリンクなどが混在した場合も、個別に処理可能

### Decision 6: 空白トリビアの扱い

**決定**: マーカー（`=`）とタイトル本文の間の空白は、マーカーの TrailingTrivia として扱う

**実装**:
- マーカートークンの TrailingTrivia として保持
- `ToFullString()` での完全な復元が可能

**根拠**:
- マーカーに付随する空白として扱うのが自然
- 既存のトリビア管理パターンに沿う

### Decision 7: ImmutableArray の導入

**決定**: `System.Collections.Immutable` パッケージを使用して `ImmutableArray<T>` を導入

**根拠**:
- Roslyn の設計パターンに準拠
- 構文木のイミュータビリティを型レベルで表現
- .NET Standard 2.0 では `System.Collections.Immutable` NuGet パッケージが必要
- .NET 10.0 では標準ライブラリに含まれる

**実装上の注意**:
- `ImmutableArray<T>.Builder` を使用して効率的に構築
- 空のコレクションは `ImmutableArray<T>.Empty` を使用

## 3. 影響分析

### 3.1 変更が必要なファイル

**コア ライブラリ（Source/AsciiSharp）**:
| ファイル | 変更内容 |
|---------|---------|
| `Syntax/SectionTitleSyntax.cs` | 構造変更、TitleContent 削除、ImmutableArray 使用 |
| `Syntax/TextSyntax.cs` | リネーム → InlineTextSyntax.cs |
| `Syntax/ISyntaxVisitor.cs` | VisitText → VisitInlineText |
| `Syntax/ISyntaxVisitorOfT.cs` | VisitText → VisitInlineText |
| `SyntaxKind.cs` | Text → InlineText |
| `Syntax/ParagraphSyntax.cs` | SyntaxKind.Text → SyntaxKind.InlineText |

**Asg ライブラリ（Source/AsciiSharp.Asg）**:
| ファイル | 変更内容 |
|---------|---------|
| `AsgConverter.cs` | TitleContent 参照の更新 |

**テスト（Test/AsciiSharp.Specs）**:
| ファイル | 変更内容 |
|---------|---------|
| `StepDefinitions/VisitorSteps.cs` | TitleContent 参照の更新、VisitText → VisitInlineText |
| `StepDefinitions/BasicParsingSteps.cs` | TitleContent 参照の更新 |
| `StepDefinitions/CommentParsingSteps.cs` | TitleContent 参照の更新 |
| `StepDefinitions/IncrementalParsingSteps.cs` | TitleContent 参照の更新 |

### 3.2 パッケージ依存関係

**追加が必要なパッケージ**:
- `System.Collections.Immutable`（.NET Standard 2.0 ターゲット用）

**注**: .NET 10.0 ターゲットでは追加パッケージ不要（標準ライブラリに含まれる）

### 3.3 後方互換性

**破壊的変更**:
- `TitleContent` プロパティの削除
- `TextSyntax` クラス名の変更
- `SyntaxKind.Text` の名前変更
- `VisitText` メソッドの名前変更

**緩和策**:
- なし（このプロジェクトはまだ公開されておらず、後方互換性は不要）

## 4. リスクと対策

| リスク | 影響 | 対策 |
|--------|------|------|
| 既存テストの失敗 | 高 | テストコードの同時更新 |
| パーサー側の変更漏れ | 中 | コンパイルエラーで検出可能 |
| ToFullString() の復元失敗 | 高 | 既存のテストケースで検証 |
| ImmutableArray の .NET Standard 2.0 対応 | 低 | NuGet パッケージで対応可能 |
| InlineElements の順序不正 | 中 | 順序検証テストを追加 |

## 5. 追加調査（2026-02-06）: セクション見出し認識条件

### Decision 8: `=` が 7 個以上の行の扱い（FR-011）

**決定**: `=` が 7 個以上の行はセクション見出しとして認識せず、段落（本文テキスト）として扱う

**根拠**:
- AsciiDoc 言語仕様では Level 0〜5（`=` 1〜6 個）が定義されている
- VSCode AsciiDoc Extension のプレビュー表示でも Level 7 は見出しとして扱われない
- ユーザーとの仕様確認（Session 2026-02-06）で合意

**代替案（却下）**:
- `=` の数に上限を設けず、すべてセクション見出しとして扱う → AsciiDoc 仕様に反する
- パーサーでエラーを報告してセクション見出しとして扱う → 段落として扱うのが自然

**実装影響**:
- `IsAtSectionTitle()` に `this.Current.Text.Length <= 6` の条件を追加
- `IsAtSectionTitleOfLevelOrHigher()` は既に `Text.Length <= level` で比較しており、level は最大 6 のため影響なし

### Decision 9: マーカー後の空白必須（FR-012）

**決定**: `=` の後に空白がない行はセクション見出しとして認識せず、段落として扱う

**根拠**:
- AsciiDoc 仕様ではマーカーとタイトル本文の間に少なくとも 1 つの空白が必要
- `==タイトル` のような形式は AsciiDoc の標準的な構文ではない
- ユーザーとの仕様確認（Session 2026-02-06）で合意

**代替案（却下）**:
- 空白なしでもセクション見出しとして許容する → AsciiDoc 仕様に反する
- 空白なしの場合にパーサーがエラーを報告する → 段落として扱うのが自然

**実装影響**:
- `IsAtSectionTitle()` に `this.Peek().Kind == SyntaxKind.WhitespaceToken` の条件を追加
- `IsAtDocumentTitle()` に同じ条件を追加
- `IsAtSectionTitleOfLevelOrHigher()` に同じ条件を追加
- `Peek()` の呼び出しが増えるが、Queue ベースの先読みバッファにより追加コストは軽微

## 6. 結論

すべての調査課題が解決され、実装に進む準備が整った。

**主要な決定事項**:
- `SectionTitleSyntax` は `ImmutableArray<InlineSyntax>` でインライン要素を保持
- `InlineElements` は構文上の出現順に並ぶ（各要素の Position は前の要素以上）
- マーカー後の空白はマーカーの TrailingTrivia として扱う
- `TextSyntax` は `InlineTextSyntax` にリネーム
- `SyntaxKind.Text` は `SyntaxKind.InlineText` にリネーム
- `TitleContent` プロパティは削除
- `=` が 7 個以上の行はセクション見出しとして認識しない（FR-011）
- `=` の後に空白がない行はセクション見出しとして認識しない（FR-012）
