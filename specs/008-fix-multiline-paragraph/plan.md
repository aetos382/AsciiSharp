# Implementation Plan: 複数行パラグラフの SyntaxTree 修正

**Branch**: `008-fix-multiline-paragraph` | **Date**: 2026-02-24 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/008-fix-multiline-paragraph/spec.md`

## Summary

複数行パラグラフが別々の `InlineTextSyntax` ノードに分割される問題と、パラグラフの `location.end` が1列ずれる問題を修正する。パーサー（`ParseInlineText()`）を修正して連続するプレーンテキスト行を1つの `InlineTextSyntax` にまとめ、ASG 変換時に改行を `\n` に正規化する。

## Technical Context

**Language/Version**: C# 14 / .NET 10.0（コアライブラリは .NET Standard 2.0 も対象）
**Primary Dependencies**: LightBDD.MsTest4（BDD テスト）, MSTest.Sdk
**Storage**: N/A（パーサーライブラリ）
**Testing**: MSTest.Sdk + LightBDD.MsTest4
**Target Platform**: .NET 10.0 / .NET Standard 2.0
**Project Type**: ライブラリ（BDD 対象: `Source/AsciiSharp`）
**Performance Goals**: 初期フェーズでは非重視
**Constraints**: .NET Standard 2.0 互換性が必要（`Source/AsciiSharp`）

## Constitution Check

| 原則 | 状態 | 備考 |
|------|------|------|
| I. コード品質ファースト | ✅ | 可読性・メンテナンス性・テスト可能性を優先 |
| II. モジュール設計 | ✅ | パーサー・構文ノード・ASG の責務は分離済み |
| III. BDD必須 | ✅ | `Source/AsciiSharp` が対象。フィーチャー クラスを plan フェーズで作成する |
| IV. 継続的品質保証 | ✅ | 各修正後にビルドとテストを実行 |
| V. 警告ゼロポリシー | ✅ | Refactor ステップで解消する |
| VI. フェーズ順序の厳守 | ✅ | specify → clarify → plan の順序を遵守済み |

**Complexity Tracking**: 追加の複雑性は特になし。既存パターン（`ParseSectionTitle()` の改行トリビア付与）を踏襲する。

## Project Structure

### Documentation (this feature)

```text
specs/008-fix-multiline-paragraph/
├── spec.md              # 仕様書
├── plan.md              # このファイル
├── research.md          # Phase 0 調査結果
└── tasks.md             # Phase 2 出力（/speckit.tasks で生成）
```

### Source Code (変更対象)

```text
Source/AsciiSharp/
├── Parser/
│   └── Parser.cs                  # ParseInlineText() を修正
└── Syntax/
    └── InlineTextSyntax.cs        # Text プロパティの確認（修正不要の可能性あり）

Source/AsciiSharp.Asg/
└── AsgConverter.cs                # VisitInlineText() で \n 正規化

Test/AsciiSharp.Specs/Features/
├── MultipleLinesParagraphFeature.cs        # 新規作成（シナリオ定義）
└── MultipleLinesParagraphFeature.Steps.cs  # 新規作成（ステップ実装 - 未実装）
```

**Structure Decision**: 既存のプロジェクト構造を維持。新規ファイルは既存の `Features/` ディレクトリに追加。

---

## Phase 0: Research

### 調査結果

#### 1. `InternalNode.ToTrimmedString()` の動作確認

**Decision**: `InlineTextSyntax.Text` (`this.Internal.ToTrimmedString()`) の修正は不要。

**根拠**:
- `ToTrimmedString()` は `ToFullString()` から `LeadingTriviaWidth` + `TrailingTriviaWidth` 分を除いた部分文字列を返す
- `InternalSyntaxNode.TrailingTriviaWidth` は「最後の非 null 子ノードの `TrailingTriviaWidth`」を使用する
- つまり、最終行のコンテンツトークンに改行トリビアを付与すれば、`ToTrimmedString()` は自動的に改行を除いたテキストを返す
- 中間行の改行（コンテンツトークンとして保持）は `ToFullString()` に含まれ、正しく結合テキストに現れる

**具体例**:
```
InlineTextSyntax (内部構造)
  TextToken("This paragraph has")
  ...
  TextToken("72")  with trailing trivia "\n"  ← 中間行の最後のトークン（ただし改行はコンテンツ）
  NewLineToken("\n")                           ← 中間行の改行（コンテンツトークン）
  TextToken("character limit")
  ...
  TextToken("editor.")  with trailing trivia "\n"  ← 最終行の改行はトリビア

ToFullString() = "...72\ncharacter...editor.\n"
TrailingTriviaWidth = 1 (最終改行の "\n")
ToTrimmedString() = "...72\ncharacter...editor."  ✓
```

#### 2. `ParseInlineText()` の修正方針

**Decision**: 行をまたいで読み続けるループに変更し、`ParseSectionTitle()` パターンで最終行の改行をトリビア化する。

**根拠**:
- 現在の `ParseSectionTitle()` の InlineText 処理（行末の空白・改行をトリビア付与）が先例となっている
- 複数行対応では、「次の行がプレーンテキストなら継続」という判定ロジックを追加する
- 中間行の改行はコンテンツトークンとして直接 sink に出力する

**継続条件の判定**: 改行を消費した後、`IsAtEnd()` でも `IsBlankLine()` でも `IsAtSectionTitle()` でもなければ次行を継続。

**実装パターン（概略）**:
```csharp
private bool ParseInlineText()
{
    this._sink.StartNode(SyntaxKind.InlineText);

    InternalToken? lastContentToken = null;
    InternalToken? pendingWhitespace = null;
    bool newLineConsumed = false;

    while (true)
    {
        // 行内のトークンを読む（改行・EOF まで）
        while (!IsAtEnd() && Current.Kind != NewLineToken && !IsAtLink())
        {
            // ParseSectionTitle パターンで空白保留・コンテンツ確定
            // ...
        }

        // 行末処理
        if (Current.Kind == NewLineToken)
        {
            // 次の行が継続かチェック（Peek で先読み）
            bool isContinuation = !IsBlankLine() after advance && !IsAtSectionTitle() after advance;

            if (isContinuation)
            {
                // 中間行: 最後のコンテンツトークンを出力し、改行をコンテンツとして出力
                FlushPendingTokens();
                EmitCurrentToken(); // 改行トークンをコンテンツとして
                continue; // 次の行を読み続ける
            }
            else
            {
                // 最終行: 改行をトリビアとして付与して終了
                AddTrailingTrivia(改行);
                FlushLastContentToken();
                break;
            }
        }
    }

    this._sink.FinishNode();
    return newLineConsumed;
}
```

**注意**: `ParseParagraph()` の改行消費ループとの調整が必要（`ParseInlineText()` が改行を消費した場合は `ParseParagraph()` でスキップ）。これは `ParseSectionTitle()` の `titleNewLineConsumed` フラグと同じパターン。

#### 3. `AsgConverter.VisitInlineText()` の改行正規化

**Decision**: `VisitInlineText()` で `node.Text` に対して改行文字を `\n` に正規化する。

**根拠**:
- FR-004: ASG 変換時に `\n` に正規化することは許容された変形
- `\r\n` → `\n`、`\r` → `\n` に置換
- `Span` ベースの位置計算（`GetLocation()`）は元のオフセットを使うため、正規化は位置情報に影響しない

**実装**:
```csharp
AsgNode? ISyntaxVisitor<AsgNode?>.VisitInlineText(InlineTextSyntax node)
{
    ArgumentNullException.ThrowIfNull(node);

    return new AsgText
    {
        Value = node.Text
            .Replace("\r\n", "\n")
            .Replace("\r", "\n"),
        Location = this.GetLocation(node)
    };
}
```

#### 4. 既存テストへの影響分析

**影響を受けるテスト**:
- `BasicParsingFeature` の「複数の段落の解析」シナリオ - 複数行段落の `InlineElements.Count` を確認している場合は修正が必要
- `AsciiSharp.Tests` の段落関連テスト - `InlineElements` の数や内容を検証しているテストは修正が必要

**対応方針**: 既存テストが `InlineElements` の各ノード数や個別テキストを検証している場合、SyntaxTree 構造の変更に合わせて修正する（これは Refactor ステップで対応）。

---

## Phase 1: Design

### LightBDD フィーチャー クラス設計

以下のシナリオを `MultipleLinesParagraphFeature.cs` に定義する。

**シナリオ 1 (FR-001)**: 複数行パラグラフが単一の InlineTextSyntax ノードとして解析される
- Given: 2行にわたるパラグラフを含む AsciiDoc 文書
- When: SyntaxTree として解析する
- Then: `ParagraphSyntax.InlineElements.Count == 1`
- Then: `InlineElements[0]` が `InlineTextSyntax` である

**シナリオ 2 (FR-001, FR-002)**: 複数行 InlineTextSyntax のテキストが改行で結合される
- Given: 2行にわたるパラグラフを含む AsciiDoc 文書
- When: SyntaxTree として解析する
- Then: `InlineTextSyntax.Text` が `"line1\nline2"` 形式（改行で結合）

**シナリオ 3 (FR-003, FR-004)**: 複数行 InlineTextSyntax の ASG 出力が正しい
- Given: 2行にわたるパラグラフを含む AsciiDoc 文書
- When: ASG に変換する
- Then: `inlines` に1つの `text` ノードが含まれる
- Then: `value` が `\n` で結合された文字列
- Then: `location.end.col` が末尾コンテンツ文字の列番号

**シナリオ 4 (SC-002)**: 単一行・複数パラグラフの location が正確に計算される
- Given: 単一行パラグラフが2つ（1行の空行で区切られた）文書
- When: ASG に変換する
- Then: 各パラグラフの `location.end.col` が最後のコンテンツ文字の列番号

### data-model.md 不要

このフィーチャーはパーサーの内部動作の修正であり、外部 API の変更はない。エンティティモデルに変更はないため、data-model.md は作成しない。

### quickstart.md 不要

既存ライブラリの内部修正であり、API 変更がないため quickstart.md は不要。

---

## BDD Red フェーズ

以下のファイルを作成し、テストが失敗することを確認する（Red 確認）。

### `Test/AsciiSharp.Specs/Features/MultipleLinesParagraphFeature.cs`

```csharp
using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest4;

namespace AsciiSharp.Specs.Features;

/// <summary>
/// 複数行パラグラフの SyntaxTree 修正に関するフィーチャー テスト。
/// </summary>
[FeatureDescription(@"複数行パラグラフの正しい解析
ライブラリユーザーとして、
複数行にわたるパラグラフを含む AsciiDoc 文書を解析し、
行をまたいだテキストが1つのインライン要素として取得したい")]
[Label("TCK:block/paragraph/multiple-lines")]
public sealed partial class MultipleLinesParagraphFeature : FeatureFixture
{
    [Scenario]
    public void 複数行パラグラフが単一のInlineTextSyntaxノードとして解析される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 複数行にわたる以下のパラグラフがある(
                "This paragraph has multiple lines that wrap after reaching the 72\n" +
                "character limit set by the text editor.\n"),
            when => 文書を解析する(),
            then => パラグラフのインライン要素が_N個である(1),
            then => 最初のインライン要素がInlineTextSyntaxである());
    }

    [Scenario]
    public void 複数行InlineTextSyntaxのテキストが改行で結合される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 複数行にわたる以下のパラグラフがある(
                "This paragraph has multiple lines that wrap after reaching the 72\n" +
                "character limit set by the text editor.\n"),
            when => 文書を解析する(),
            then => InlineTextSyntaxのTextが(
                "This paragraph has multiple lines that wrap after reaching the 72\n" +
                "character limit set by the text editor."));
    }

    [Scenario]
    public void 複数行パラグラフのASG出力でtextノードが1つ出力される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 複数行にわたる以下のパラグラフがある(
                "This paragraph has multiple lines that wrap after reaching the 72\n" +
                "character limit set by the text editor.\n"),
            when => 文書を解析する(),
            when => ASGに変換する(),
            then => パラグラフのinlinesが_N個のtextノードを持つ(1),
            then => textノードのvalueが(
                "This paragraph has multiple lines that wrap after reaching the 72\n" +
                "character limit set by the text editor."),
            then => textノードのlocationEndColが(39));
    }

    [Scenario]
    public void 単一行パラグラフのlocationEndColが正確に計算される()
    {
        Runner.RunScenario(
            given => パーサーが初期化されている(),
            given => 以下の複数パラグラフ文書がある(
                "This document has multiple paragraphs.\n" +
                "\n" +
                "Paragraphs are separated by one or more empty lines.\n"),
            when => 文書を解析する(),
            when => ASGに変換する(),
            then => 最初のパラグラフのlocationEndColが(38),
            then => 最後のパラグラフのlocationEndColが(52));
    }
}
```

### `Test/AsciiSharp.Specs/Features/MultipleLinesParagraphFeature.Steps.cs`

```csharp
using AsciiSharp.Asg;
using AsciiSharp.Asg.Models;
using AsciiSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs.Features;

public sealed partial class MultipleLinesParagraphFeature
{
    private SyntaxTree? _syntaxTree;
    private AsgDocument? _asgDocument;
    private string _input = string.Empty;

    private void パーサーが初期化されている()
    {
        Assert.Inconclusive("未実装");
    }

    private void 複数行にわたる以下のパラグラフがある(string input)
    {
        Assert.Inconclusive("未実装");
    }

    private void 以下の複数パラグラフ文書がある(string input)
    {
        Assert.Inconclusive("未実装");
    }

    private void 文書を解析する()
    {
        Assert.Inconclusive("未実装");
    }

    private void ASGに変換する()
    {
        Assert.Inconclusive("未実装");
    }

    private void パラグラフのインライン要素が_N個である(int count)
    {
        Assert.Inconclusive("未実装");
    }

    private void 最初のインライン要素がInlineTextSyntaxである()
    {
        Assert.Inconclusive("未実装");
    }

    private void InlineTextSyntaxのTextが(string expected)
    {
        Assert.Inconclusive("未実装");
    }

    private void パラグラフのinlinesが_N個のtextノードを持つ(int count)
    {
        Assert.Inconclusive("未実装");
    }

    private void textノードのvalueが(string expected)
    {
        Assert.Inconclusive("未実装");
    }

    private void textノードのlocationEndColが(int expectedCol)
    {
        Assert.Inconclusive("未実装");
    }

    private void 最初のパラグラフのlocationEndColが(int expectedCol)
    {
        Assert.Inconclusive("未実装");
    }

    private void 最後のパラグラフのlocationEndColが(int expectedCol)
    {
        Assert.Inconclusive("未実装");
    }
}
```

---

## 実装タスクの概要

1. **T001**: BDD Red — `MultipleLinesParagraphFeature.cs` と `.Steps.cs` を作成してテスト失敗を確認
2. **T002**: `ParseInlineText()` を複数行対応に修正（中間行の改行はコンテンツ、最終行の改行はトリビア）
3. **T003**: `AsgConverter.VisitInlineText()` で改行を `\n` に正規化
4. **T004**: 既存テストのリグレッション修正（`InlineElements` 構造変更への追従）
5. **T005**: BDD Green — `MultipleLinesParagraphFeature.Steps.cs` のステップ実装
6. **T006**: Refactor — 警告解消・コード整理
