# Research: インライン要素とブロック要素のセマンティクス定義

## R-001: 影響を受けるクラスの継承チェーン

**Summary**: 変更対象クラスのコンストラクター シグネチャ調査

**Decision**: すべての変更対象クラスは `BlockSyntax(InternalNode, SyntaxNode?, int, SyntaxTree?)` を介して `SyntaxNode` を呼び出している。`BlockSyntax` はパススルーのみでロジックを持たないため、基底クラスを `BlockSyntax` から `SyntaxNode` に変更するだけでよい。

**Findings**:

```
BlockSyntax
└── private protected BlockSyntax(InternalNode, SyntaxNode?, parent, int position, SyntaxTree?)
    └── : base(internalNode, parent, position, syntaxTree)  // SyntaxNode へ委譲

SyntaxNode
└── private protected SyntaxNode(InternalNode, SyntaxNode?, int, SyntaxTree?)
    // 同一シグネチャ
```

各変更対象クラスのコンストラクター呼び出し:
- `SectionTitleSyntax` → `: base(internalNode, parent, position, syntaxTree)`
- `AuthorLineSyntax` → `: base(internalNode, parent, position, syntaxTree)`
- `AttributeEntrySyntax` → `: base(internalNode, parent, position, syntaxTree)`
- `DocumentHeaderSyntax` → `: base(internalNode, parent, position, syntaxTree)`
- `DocumentBodySyntax` → `: base(internalNode, parent, position, syntaxTree)`

**Conclusion**: `: BlockSyntax` を `: SyntaxNode` に変更し、`base()` 呼び出しはそのままでよい。

---

## R-002: SyntaxKind.TextSpan の使用状況

**Summary**: `SyntaxKind.TextSpan` の参照有無と削除後の影響

**Decision**: `SyntaxKind.TextSpan` を削除し、`InlineText = 401` を明示的に付与する。

**Findings**:

- `SyntaxKind.TextSpan = 400` はプロダクションコード内で参照されていない
- パーサー、レキサー、AsgConverter いずれも `SyntaxKind.TextSpan` を参照しない
- 削除後、`InlineText` は自動インクリメントにより前の値（`AttributeEntry` の次）になってしまう。インライン種別を 400 番台に維持するため、`InlineText = 401` を明示する
- `Link` は `InlineText + 1 = 402` として自動インクリメント維持

**Impact**:

| 種別 | 変更前 | 変更後 |
|------|--------|--------|
| `TextSpan` | 400 | 削除 |
| `InlineText` | 401（暗黙） | 401（明示） |
| `Link` | 402（暗黙） | 402（暗黙、変わらず） |

---

## R-003: 既存テストへの影響

**Summary**: 変更後に壊れる既存テストの特定

**Decision**: `BlockInlineSyntaxFeature` の `セクション関連ノードはBlockSyntaxとして識別できる()` シナリオを実装フェーズで更新する。

**Findings**:

`BlockInlineSyntaxFeature.cs` に以下のシナリオが存在する:

```csharp
[Scenario]
public void セクション関連ノードはBlockSyntaxとして識別できる()
{
    Runner.RunScenario(
        given => 以下のAsciiDoc文書がある("== セクションタイトル\n"),
        when => 文書を解析する(),
        then => SectionノードはBlockSyntax(),
        then => SectionTitleノードはBlockSyntax()  ← 実装後に失敗
    );
}
```

実装後、`SectionTitleSyntax` は `BlockSyntax` でなくなるため、このステップが失敗する。
実装フェーズで以下のように更新する:
- シナリオ名を変更（`SectionTitleSyntax` が `BlockSyntax` ではないことを記述）
- ステップを `SectionTitleノードはBlockSyntaxではない()` に変更

**Other tests**: `SyntaxVisitorFeature`、`SectionTitleTriviaFeature`、`SectionTitleRecognitionFeature` 等は特定の具体型を直接使用しており、`BlockSyntax` キャストは行っていないため影響なし。

---

## R-004: StructuredTriviaSyntax の設計

**Summary**: 新規抽象クラスの設計

**Decision**: `BlockSyntax` と同一のシグネチャ・構造で作成する。具象実装はこの仕様のスコープ外。

**Rationale**: `BlockSyntax` はマーカークラス（ロジックなし）として機能しており、`StructuredTriviaSyntax` も同様のマーカークラスとして設計する。将来の具象クラス（`MultilineCommentTriviaSyntax` 等）が継承しやすいようにコンストラクターを `protected` で定義する。

**Design**:

```csharp
/// <summary>
/// トリビアであるが内部に構文構造を持つノードの抽象基底クラス。
/// </summary>
public abstract class StructuredTriviaSyntax : SyntaxNode
{
    private protected StructuredTriviaSyntax(
        InternalNode internalNode,
        SyntaxNode? parent,
        int position,
        SyntaxTree? syntaxTree)
        : base(internalNode, parent, position, syntaxTree) { }
}
```

---

## R-005: ASG コンバーターへの影響

**Summary**: `AsciiSharp.Asg` プロジェクトへの影響調査

**Decision**: `AsgConverter.cs` は変更不要。

**Findings**: `AsgConverter.cs` は `SectionTitleSyntax`、`DocumentHeaderSyntax`、`DocumentBodySyntax`、`AuthorLineSyntax`、`AttributeEntrySyntax` を Visitor パターンの具体型として直接参照しているが、`BlockSyntax` としてキャストする箇所はない。基底クラスの変更は Visitor インターフェース経由の呼び出しに影響しない。

---

## R-006: LightBDD テスト設計

**Summary**: 新規 BDD フィーチャー クラスの設計

**Decision**: `ElementTypeSemantics006Feature` を 2 シナリオで作成する。

**Scenarios**:

1. `AsciiDoc仕様のブロックとされないノードはBlockSyntaxではない()`:
   - `SectionTitleSyntax`, `DocumentHeaderSyntax`, `AuthorLineSyntax`, `AttributeEntrySyntax` が `is BlockSyntax` で `false` になることを確認
   - 実際のパース済み文書からノードを取得してチェック

2. `StructuredTriviaSyntaxはSyntaxNodeを継承している()`:
   - `typeof(StructuredTriviaSyntax).IsSubclassOf(typeof(SyntaxNode))` → `true`
   - `typeof(StructuredTriviaSyntax).IsSubclassOf(typeof(BlockSyntax))` → `false`
   - `typeof(StructuredTriviaSyntax).IsSubclassOf(typeof(InlineSyntax))` → `false`
   - 抽象クラス自体への型テストのため、リフレクションを使用

**Note**: `DocumentBodySyntax` は内部でしか参照されず、テスト用文書からパース結果として直接取得しにくいため、スコープ外とする（FR-002 の実装で変更されれば十分）。
