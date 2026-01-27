# Research: SyntaxTree Visitor パターン

**Date**: 2026-01-26
**Feature**: 002-syntaxtree-visitor

## 調査項目

### 1. Visitor パターンの設計アプローチ

**Decision**: インターフェイスのみを提供する（デフォルト実装クラスなし）

**Rationale**:
- シンプルで明確な API
- 利用者が完全に実装を制御できる
- 不要な基底クラスの継承を強制しない

**Alternatives considered**:
- Roslyn 完全準拠（Visit/DefaultVisit + 抽象クラス）: 過剰な複雑性
- デフォルト実装クラスの提供: 利用者の選択肢を狭める

### 2. インターフェイス設計

**Decision**: 以下の 2 つのインターフェイスを定義する

```csharp
// 戻り値なし Visitor
public interface ISyntaxVisitor
{
    void VisitDocument(DocumentSyntax node);
    void VisitDocumentHeader(DocumentHeaderSyntax node);
    void VisitDocumentBody(DocumentBodySyntax node);
    void VisitSection(SectionSyntax node);
    void VisitSectionTitle(SectionTitleSyntax node);
    void VisitParagraph(ParagraphSyntax node);
    void VisitText(TextSyntax node);
    void VisitLink(LinkSyntax node);
}

// 戻り値あり Visitor
public interface ISyntaxVisitor<TResult>
{
    TResult VisitDocument(DocumentSyntax node);
    TResult VisitDocumentHeader(DocumentHeaderSyntax node);
    TResult VisitDocumentBody(DocumentBodySyntax node);
    TResult VisitSection(SectionSyntax node);
    TResult VisitSectionTitle(SectionTitleSyntax node);
    TResult VisitParagraph(ParagraphSyntax node);
    TResult VisitText(TextSyntax node);
    TResult VisitLink(LinkSyntax node);
}
```

**Rationale**:
- SyntaxNode 派生クラスのみを対象とする
- SyntaxToken / SyntaxTrivia は構造体であり、Accept パターンに適さない
- トークンやトリビアへのアクセスは、ノードの ChildNodesAndTokens() や LeadingTrivia/TrailingTrivia プロパティ経由で可能

### 3. SyntaxNode への Accept メソッド追加

**Decision**: 各 SyntaxNode 派生クラスに Accept メソッドを追加する

```csharp
// SyntaxNode に抽象メソッドを追加
public abstract void Accept(ISyntaxVisitor visitor);
public abstract TResult Accept<TResult>(ISyntaxVisitor<TResult> visitor);

// 各派生クラスで実装（DocumentSyntax の例）
public override void Accept(ISyntaxVisitor visitor) => visitor.VisitDocument(this);
public override TResult Accept<TResult>(ISyntaxVisitor<TResult> visitor) => visitor.VisitDocument(this);
```

**Rationale**:
- ダブルディスパッチにより型安全な訪問を実現
- 各ノードが自身の型を知っているため、適切な VisitXxx メソッドを呼び出せる

### 4. .NET Standard 2.0 互換性

**Decision**: null 許容参照型の条件付きコンパイルを使用

**Rationale**:
- .NET Standard 2.0 では null 許容参照型がサポートされない
- Polyfill プロジェクトで既に対応済みのパターンを活用

## 提供するもの

- `ISyntaxVisitor` インターフェイス
- `ISyntaxVisitor<TResult>` インターフェイス
- 各 SyntaxNode 派生クラス（8 種）への Accept メソッド追加

## 提供しないもの

- デフォルト実装クラス（SyntaxVisitorBase 等）
- 自動走査クラス（SyntaxWalker 等）
- 構文木変換クラス（SyntaxRewriter 等）
- Visit / DefaultVisit メソッド
- VisitToken / VisitTrivia メソッド（トークン・トリビアは構造体のため対象外）

## 参考資料

- [Microsoft Learn - Syntax analysis](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/get-started/syntax-analysis)
- [dotnet/roslyn - CSharpSyntaxVisitor.cs](https://github.com/dotnet/roslyn/blob/main/src/Compilers/CSharp/Portable/Syntax/CSharpSyntaxVisitor.cs)
