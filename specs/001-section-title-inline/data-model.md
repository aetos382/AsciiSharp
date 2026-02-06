# Data Model: SectionTitleSyntax の構成改定と TextSyntax のリネーム

**Feature Branch**: `001-section-title-inline`
**Date**: 2026-02-05

## 1. エンティティ定義

### 1.1 SectionTitleSyntax（変更）

セクションタイトルを表すブロック要素。

```csharp
public sealed class SectionTitleSyntax : BlockSyntax
{
    /// <summary>
    /// セクションレベル（= の数）。
    /// </summary>
    public int Level { get; }

    /// <summary>
    /// セクションマーカー（= の並び）。
    /// マーカー後の空白は TrailingTrivia として保持される。
    /// </summary>
    public SyntaxToken? Marker { get; }

    /// <summary>
    /// タイトルを構成するインライン要素のコレクション。
    /// 構文上の出現順に並ぶ（各要素の Position は前の要素以上）。
    /// </summary>
    public ImmutableArray<InlineSyntax> InlineElements { get; }
}
```

**削除されるプロパティ**:
- `TitleContent` - タイトル文字列（インライン要素から取得可能）
- `TitleText` - 最初のテキストトークン（不要）

**不変条件（Invariants）**:
- `Level` は 1〜6 の範囲（`=` 1〜6 個）
- `=` が 7 個以上の場合はセクション見出しとして認識されない（FR-011）
- マーカー後に空白が必須（FR-012）。空白がない場合はセクション見出しとして認識されない
- `InlineElements` は構文上の出現順に並ぶ
- `InlineElements[i].Position >= InlineElements[i-1].Position`（i > 0 の場合）

### 1.2 InlineTextSyntax（旧 TextSyntax）

一行のプレーンテキストを表すインライン要素。

```csharp
public sealed class InlineTextSyntax : InlineSyntax
{
    /// <summary>
    /// テキストの内容。
    /// </summary>
    public string Text { get; }
}
```

**変更点**:
- クラス名: `TextSyntax` → `InlineTextSyntax`
- ファイル名: `TextSyntax.cs` → `InlineTextSyntax.cs`

### 1.3 InlineSyntax（変更なし）

インライン要素の抽象基底クラス。

```csharp
public abstract class InlineSyntax : SyntaxNode
{
    // 既存の実装を維持
}
```

### 1.4 SyntaxKind（変更）

```csharp
public enum SyntaxKind
{
    // ... 既存の値 ...

    // Nodes (Inlines) - 変更
    TextSpan = 400,
    InlineText,  // 旧: Text
    Link,
}
```

### 1.5 ISyntaxVisitor（変更）

```csharp
public interface ISyntaxVisitor
{
    // ... 既存のメソッド ...
    void VisitInlineText(InlineTextSyntax node);  // 旧: VisitText(TextSyntax node)
    // ...
}

public interface ISyntaxVisitor<TResult>
{
    // ... 既存のメソッド ...
    TResult VisitInlineText(InlineTextSyntax node);  // 旧: VisitText(TextSyntax node)
    // ...
}
```

## 2. クラス階層

```
SyntaxNode (抽象基底)
├─ BlockSyntax (抽象)
│  ├─ SectionTitleSyntax (sealed) ← 変更対象
│  ├─ ParagraphSyntax (sealed) ← SyntaxKind.Text 参照の更新
│  └─ ...
│
└─ InlineSyntax (抽象)
   ├─ InlineTextSyntax (sealed) ← リネーム (旧 TextSyntax)
   ├─ LinkSyntax (sealed)
   └─ ...
```

## 3. 状態遷移

該当なし（構文木はイミュータブルであり、状態遷移は存在しない）

## 4. バリデーション ルール

### 4.1 SectionTitleSyntax

| ルール | 説明 | エラー時の動作 |
|--------|------|---------------|
| Level は 1〜6 | セクションレベルは 1〜6（`=` 1〜6 個） | 7 個以上は段落として解析 |
| マーカー後の空白 | マーカーの後に空白が必須 | 空白なしは段落として解析 |
| InlineElements の順序 | 構文上の出現順に並ぶ | コンストラクタで保証 |

### 4.2 InlineTextSyntax

| ルール | 説明 | エラー時の動作 |
|--------|------|---------------|
| Text は空でない | 空のテキストノードは作成されない | パーサーがスキップ |

## 5. 関連性マトリクス

```
┌─────────────────────┬──────────────────┬──────────────┐
│                     │ SectionTitleSyntax│ InlineTextSyntax│
├─────────────────────┼──────────────────┼──────────────┤
│ SectionTitleSyntax  │        -         │  contains 0..n   │
│ InlineTextSyntax    │   contained in   │        -         │
│ ParagraphSyntax     │        -         │  contains 0..n   │
└─────────────────────┴──────────────────┴──────────────┘
```

## 6. パッケージ依存関係

### 6.1 追加パッケージ

| パッケージ | バージョン | 用途 | 対象 |
|-----------|----------|------|------|
| System.Collections.Immutable | 最新 | ImmutableArray<T> | .NET Standard 2.0 |

**注**: .NET 10.0 では標準ライブラリに含まれるため、条件付き参照とする。

```xml
<ItemGroup Condition="'$(TargetFramework)' == 'netstandard2.0'">
  <PackageReference Include="System.Collections.Immutable" />
</ItemGroup>
```
