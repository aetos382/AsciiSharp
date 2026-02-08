# Research: TCK header-body-output テスト対応

**Branch**: `004-tck-header-body` | **Date**: 2026-02-08

## R-001: TCK テスト比較方式

**Decision**: TCK は `assert.deepEqual` によるセマンティック比較を使用する

**Rationale**: `harness/lib/suites/suite.js` を調査した結果、TCK ハーネスは Node.js の `assert.deepEqual` を使用してテスト出力を検証している。これはオブジェクトの構造と値を再帰的に比較する方式であり、JSON プロパティの出力順序は結果に影響しない。

**Alternatives considered**:
- 文字列完全一致比較 → 不採用（TCK の実装がセマンティック比較であることを確認）

## R-002: 属性エントリの SyntaxTree 表現

**Decision**: `SyntaxKind.AttributeEntry` ノードを追加し、内部に開きコロン・属性名・閉じコロン・値テキストのトークンを保持する。閉じコロン後の空白と行末の改行はトリビアとして扱う。

**Rationale**: 既存の Green/Red Tree パターンに従い、`InternalTreeBuilder` の `StartNode(SyntaxKind.AttributeEntry)` / `FinishNode()` でノードを構築する。トリビアの扱いは、セクション タイトルの `=` マーカー後の空白がトリビアとして扱われるパターンと一致する（`InternalToken.WithTrivia()` を使用）。

**構造（値あり: `:icons: font\n`）**:
```
AttributeEntry
├── ColonToken (:)                              ← 開きコロン
├── TextToken (icons)                           ← 属性名
├── ColonToken (:) [trailingTrivia: " "]        ← 閉じコロン（空白はトリビア）
└── TextToken (font) [trailingTrivia: "\n"]     ← 属性値（改行はトリビア）
```

**構造（値なし: `:toc:\n`）**:
```
AttributeEntry
├── ColonToken (:)                              ← 開きコロン
├── TextToken (toc)                             ← 属性名
└── ColonToken (:) [trailingTrivia: "\n"]       ← 閉じコロン（改行はトリビア）
```

**Alternatives considered**:
- 空白と改行を独立トークンとして保持 → 不採用（既存のトリビア パターンとの一貫性を優先）
- 属性エントリ行全体を単一トークンとして保持 → 不採用（LSP 連携時にトークン レベルの精度が必要）
- 専用の Lexer トークン（`AttributeEntryToken`）を追加 → 不採用（既存の `ColonToken` と `TextToken` で十分表現可能）

## R-003: パーサーでの属性エントリ認識条件

**Decision**: ドキュメント ヘッダー内で、行頭が `:` で始まり、後続に属性名と閉じ `:` が続く行を属性エントリとして認識する

**Rationale**: AsciiDoc 仕様では属性エントリは `:name: value` 形式で、行頭の `:` で開始する。パーサーの `ParseDocumentHeader()` メソッド内で、タイトル行と著者行のパース後、空行に達するまでの間、行頭 `:` をチェックして属性エントリを認識する。

**認識ルール**:
1. 行頭が `ColonToken` である
2. 直後に `TextToken`（属性名）が続く
3. その後に `ColonToken`（閉じコロン）が続く
4. 閉じコロン後は任意: 空白 + 値テキスト + 改行、または直接改行

**Alternatives considered**:
- Lexer レベルで属性エントリを認識 → 不採用（Lexer はコンテキスト非依存であるべき）

## R-004: ASG での attributes プロパティの型

**Decision**: `IReadOnlyDictionary<string, string>` を使用し、値は常に文字列とする

**Rationale**: TCK の期待出力を分析すると、属性値はすべて文字列型である（`"icons": "font"`, `"toc": ""`）。`object` 型は不要であり、`string` 型で十分。`IReadOnlyDictionary` はイミュータブル性を表現する。AsgDocument の既存の `IReadOnlyList` パターンとも整合する。

**Alternatives considered**:
- `Dictionary<string, object?>` → 不採用（値の型が文字列のみであるため過剰）
- `Dictionary<string, string>` → 不採用（公開 API にはイミュータブル インターフェースが適切）

## R-005: JSON プロパティ順序の制御

**Decision**: `[JsonPropertyOrder]` 属性はプロジェクト全体で使用しない

**Rationale**: TCK が `assert.deepEqual` によるセマンティック比較を使用するため、JSON プロパティの出力順序は結果に影響しない（R-001 参照）。`[JsonPropertyOrder]` は不要な複雑性であり、新規追加も既存コードへの導入も行わない。現在のコードベースには `[JsonPropertyOrder]` は使用されていない。

## R-006: AsgJsonContext への型登録

**Decision**: `AsgJsonContext` に `IReadOnlyDictionary<string, string>` を登録する

**Rationale**: System.Text.Json のソース生成では、シリアライズ対象の型を明示的に登録する必要がある。`AsgDocument` に `IReadOnlyDictionary<string, string>` プロパティを追加するため、この型を `[JsonSerializable]` として登録する。

## R-007: DocumentHeaderSyntax への AttributeEntry 統合

**Decision**: `DocumentHeaderSyntax` に `ImmutableArray<AttributeEntrySyntax>` プロパティを追加する

**Rationale**: 既存の `SectionTitleSyntax? Title` や `AuthorLineSyntax? AuthorLine` と同様のパターンで、ヘッダー内の属性エントリを構造的にアクセス可能にする。Red Tree コンストラクタの switch 文に `SyntaxKind.AttributeEntry` ケースを追加する。

## R-008: ISyntaxVisitor への VisitAttributeEntry 追加

**Decision**: `ISyntaxVisitor` と `ISyntaxVisitor<TResult>` の両インターフェースに `VisitAttributeEntry(AttributeEntrySyntax node)` メソッドを追加する

**Rationale**: 既存のビジター パターンに従い、新しいノード型に対応する訪問メソッドが必要。これは破壊的変更（インターフェースへのメソッド追加）だが、現時点で外部消費者はおらず、プロジェクト内の実装（AsgConverter の Visitor）を更新すれば十分。
