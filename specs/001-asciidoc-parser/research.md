# Research: AsciiDoc パーサー

**Feature**: 001-asciidoc-parser
**Date**: 2026-01-18

## 1. Roslyn Red-Green Tree アーキテクチャ

### Decision: Roslyn スタイルの二層構文木を採用

### Rationale

Roslyn の Red-Green Tree は、IDE 向けパーサーで実績のある二層アーキテクチャである。不変性、構造共有、増分解析を効率的に実現できる。本プロジェクトでは、この構造を「内部構文木（InternalSyntax）」と「外部構文木（Syntax）」として実装する。

### アーキテクチャ概要

#### Green Tree（内部構文木 → 本プロジェクトでは InternalSyntax）

- **不変で永続的**な低レベルデータ構造
- **親参照を持たない**: 複数バージョン間でノードを安全に再利用可能
- **幅ベースのポジショニング**: 絶対位置ではなく自身の幅（Width）のみを記録
- **構造共有**: 同一内容のノードは複数バージョン間で共有

```csharp
// Roslyn の概念的な構造（本プロジェクトでは InternalNode として実装）
internal abstract class GreenNode
{
    public abstract int Width { get; }
    public abstract int SlotCount { get; }
    public abstract GreenNode? GetSlot(int index);
}
```

#### Red Tree（外部構文木 → 本プロジェクトでは Syntax）

- Green Tree を包むイミュータブルなファサード
- **オンデマンド構築**: アクセス時に動的生成、編集後は破棄
- **動的親参照**: 親参照はツリー走査時に計算
- **計算によるポジション**: 絶対位置は幅を累積して算出

```csharp
// Roslyn の概念的な構造（本プロジェクトでは SyntaxNode として実装）
public abstract class SyntaxNode
{
    internal GreenNode Green { get; }
    public SyntaxNode? Parent { get; }
    public int Position { get; }  // 計算により算出
    public int Width => Green.Width;
}
```

### Trivia 付属モデル

- **先行トリビア（Leading Trivia）**: トークンの直前の空白・コメント
- **後続トリビア（Trailing Trivia）**: トークンの直後、次のトークンまで
- 改行は後続トリビア、次行インデントは次トークンの先行トリビア
- ファイル先頭のトリビアは最初のトークンに、末尾は EOF トークンに付与

### Alternatives Considered

| 選択肢 | 却下理由 |
|--------|---------|
| 単一層 AST | 親参照を持つと構造共有ができず、増分編集が非効率 |
| 可変 AST | スレッドセーフでなく、undo/redo が複雑化 |
| パーサージェネレーター生成 | エラー回復の細かい制御が困難 |

---

## 2. 手書きパーサーのベストプラクティス

### Decision: イベントベースの再帰下降パーサーを実装

### Rationale

rust-analyzer の Rowan ライブラリが採用するパターンに従い、パーサー本体とツリー構築を分離することで、テスト容易性と柔軟性を確保する。

### 設計パターン

#### イベントベース構築

```csharp
// パーサーはイベントを発行
interface ITreeSink
{
    void StartNode(SyntaxKind kind);
    void FinishNode();
    void Token(SyntaxKind kind, int width);
    void Error(string message);
}

// パーサー本体
class Parser
{
    private readonly ITreeSink _sink;
    private readonly Lexer _lexer;

    public void ParseDocument()
    {
        _sink.StartNode(SyntaxKind.Document);
        // ... 解析ロジック
        _sink.FinishNode();
    }
}
```

#### ボトムアップ内部構文木構築

```csharp
// 本プロジェクトでは InternalTreeBuilder として実装
class GreenTreeBuilder : ITreeSink
{
    private readonly Stack<List<GreenNode>> _children = new();

    public void StartNode(SyntaxKind kind)
    {
        _children.Push(new List<GreenNode>());
    }

    public void FinishNode()
    {
        var children = _children.Pop();
        var node = new GreenNode(children);
        _children.Peek().Add(node);
    }
}
```

### エラー回復戦略

1. **同期ポイント**: ブロック境界（空行）、セクションヘッダーで回復
2. **エラーノード挿入**: 解析不能な部分をエラーノードとしてラップ
3. **スキップ**: 次の有効な構文要素まで入力をスキップ
4. **挿入**: 期待されるトークンを仮想的に挿入して継続

---

## 3. .NET Standard 2.0 と C# 14 の互換性

### Decision: Polyfill を使用して C# 14 機能を活用

### Rationale

.NET Standard 2.0 は広範な互換性を提供するが、最新の C# 機能を使用することでコード品質を向上させる。型ベースの機能は Polyfill で対応可能。

### 使用可能な C# 機能（Polyfill あり）

| 機能 | 必要な Polyfill |
|------|----------------|
| `init` アクセサー | `IsExternalInit` |
| `required` メンバー | `RequiredMemberAttribute`, `SetsRequiredMembersAttribute` |
| `record` 型 | コンパイラ生成（追加不要） |
| Nullable 参照型 | `NullableAttribute`（コンパイラが自動生成） |
| Index/Range (`^`, `..`) | `Index`, `Range` 型 |
| パターンマッチング拡張 | なし（コンパイラ機能） |
| コレクション式 `[]` | なし（コンパイラ機能、IEnumerable 対応） |

### 使用不可な機能（ランタイム依存）

| 機能 | 理由 |
|------|------|
| `Span<T>` / `Memory<T>` | .NET Standard 2.0 に含まれない（System.Memory パッケージで追加可能） |
| Default Interface Methods | CLR サポートが必要 |
| Static Abstract Members | CLR サポートが必要 |

### 推奨 Polyfill パッケージ

```xml
<ItemGroup Condition="'$(TargetFramework)' == 'netstandard2.0'">
  <PackageReference Include="System.Memory" Version="4.5.5" />
  <PackageReference Include="Microsoft.Bcl.HashCode" Version="1.1.1" />
  <PackageReference Include="PolySharp" Version="1.14.1" PrivateAssets="all" />
</ItemGroup>
```

---

## 4. AsciiDoc 構文構造

### Decision: submodules/asciidoc-lang の仕様に準拠

### 主要な構文要素

参照: `submodules/asciidoc-lang/docs/`

#### ブロック要素

| 要素 | 説明 | 優先度 |
|------|------|--------|
| Document | 文書全体（Header + Body） | P1 |
| Section | セクション（= 〜 ======） | P1 |
| Paragraph | 段落（空行区切り） | P1 |
| List | 順序/非順序/説明リスト | P1 |
| Delimited Block | 区切りブロック（----, ====, など） | P2 |
| Table | テーブル（\|===） | P3 |

#### インライン要素

| 要素 | 構文 | 優先度 |
|------|------|--------|
| Bold | `*text*` または `**text**` | P2 |
| Italic | `_text_` または `__text__` | P2 |
| Monospace | `` `text` `` または ``` ``text`` ``` | P2 |
| Link | `link:url[text]` または `url[text]` | P2 |
| Macro | `name:target[attrs]` | P2 |
| Attribute Reference | `{name}` | P2 |

#### 特殊要素

| 要素 | 説明 | 優先度 |
|------|------|--------|
| Attribute Entry | `:name: value` | P1 |
| Comment | `//` または `////...////` | P1 |
| Include Directive | `include::path[]` | P2（構文のみ） |
| Conditional Directive | `ifdef`, `ifndef`, `ifeval` | P3（構文のみ） |

---

## 5. パフォーマンス考慮事項

### Decision: 構造共有と遅延計算を最大限活用

### 最適化戦略

1. **内部ノードのキャッシング**: 同一内容のノードは共有
2. **遅延外部ノード生成**: アクセスされるまで生成しない
3. **位置計算の最適化**: 走査中に累積計算
4. **文字列インターニング**: 頻出トークン文字列の共有

### メモリ効率

| 項目 | 戦略 |
|------|------|
| 内部ノード | 最小限のフィールド（Kind, Width, Children） |
| 外部ノード | 3フィールドのみ（Internal, Parent, Position） |
| Trivia | 配列で一括保持（個別オブジェクト化を避ける） |

---

## 出典

- [Persistence, façades and Roslyn's red-green trees - Eric Lippert](https://ericlippert.com/2012/06/08/red-green-trees/)
- [Red-Green Trees - yaakov.online](https://blog.yaakov.online/red-green-trees/)
- [Rowan - rust-analyzer GitHub](https://github.com/rust-analyzer/rowan)
- [AsciiDoc Language Documentation](https://docs.asciidoctor.org/asciidoc/latest/)
- [PolySharp - GitHub](https://github.com/Sergio0694/PolySharp)
