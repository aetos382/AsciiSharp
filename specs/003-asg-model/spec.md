# Feature Specification: AsciiDoc ASG モデルクラス

**Feature Branch**: `001-asg-model`
**Created**: 2026-01-28
**Status**: Draft
**Input**: AsciiDoc ASG を生成する機能を追加する。Source/AsciiSharp.Asg/AsciiSharp.Asg.csproj に、ASG に対応するクラス群を追加する。

## User Scenarios & Testing

### User Story 1 - SyntaxTree から ASG への変換 (Priority: P1)

TCK テスト実行時、AsciiSharp の構文木（SyntaxTree）を AsciiDoc TCK が期待する ASG（Abstract Semantic Graph）JSON 形式に変換できる。

**Why this priority**: TCK 準拠テストを実行するための基本機能であり、この機能がなければ TCK との連携が不可能。

**Independent Test**: DocumentSyntax を含む SyntaxTree を ASG 変換し、JSON として正しい構造が出力されることを検証できる。

**Acceptance Scenarios**:

1. **Given** DocumentSyntax を含む SyntaxTree が存在する, **When** ASG 変換を実行する, **Then** name="document", type="block" を含む JSON が生成される
2. **Given** SectionSyntax を含む DocumentSyntax がある, **When** ASG 変換を実行する, **Then** blocks 配列内に section ノードが含まれる
3. **Given** ParagraphSyntax 内に TextSyntax がある, **When** ASG 変換を実行する, **Then** paragraph の inlines 配列内に text ノードが含まれる

---

### User Story 2 - location 情報の出力 (Priority: P2)

ASG の各ノードには、ソース文書内での位置情報（行番号・列番号）が含まれる。

**Why this priority**: TCK は位置情報も検証対象としており、正確な location がなければテストに合格しない。

**Independent Test**: 各 ASG ノードの location プロパティが正しい開始位置と終了位置を持つことを検証できる。

**Acceptance Scenarios**:

1. **Given** 1行目3列目から始まるタイトルテキストがある, **When** ASG 変換を実行する, **Then** location の開始位置が {line: 1, col: 3} となる
2. **Given** 複数行にまたがる段落がある, **When** ASG 変換を実行する, **Then** location の終了位置が正しい行・列を示す

---

### User Story 3 - DocumentHeader の ASG 変換 (Priority: P2)

文書ヘッダー（タイトル）がある場合、ASG の header プロパティとして出力される。

**Why this priority**: TCK テストには header を含む文書のテストケースが含まれている。

**Independent Test**: DocumentHeaderSyntax を持つ文書を変換し、header.title 配列が正しく出力されることを検証できる。

**Acceptance Scenarios**:

1. **Given** DocumentHeaderSyntax にタイトルがある, **When** ASG 変換を実行する, **Then** header.title 配列に text ノードが含まれる
2. **Given** ヘッダーのない文書がある, **When** ASG 変換を実行する, **Then** header プロパティは出力されない

---

### Edge Cases

- DocumentSyntax が空（Header も Body もない）場合、blocks は空配列として出力される
- SectionSyntax がネストしている場合、blocks 内に再帰的に section が含まれる
- TextSyntax が空文字列の場合、value は空文字列として出力される
- 未対応の SyntaxNode タイプに遭遇した場合、そのノードはスキップし、処理を継続する

## Requirements

### Functional Requirements

- **FR-001**: TckAdapter プロジェクトは、DocumentSyntax を ASG の document ノードに変換できなければならない
- **FR-002**: TckAdapter プロジェクトは、SectionSyntax を ASG の section ノードに変換できなければならない
- **FR-003**: TckAdapter プロジェクトは、ParagraphSyntax を ASG の paragraph ノードに変換できなければならない
- **FR-004**: TckAdapter プロジェクトは、TextSyntax を ASG の text ノードに変換できなければならない
- **FR-005**: 各 ASG ノードは、対応する SyntaxNode の位置情報から location を計算して出力しなければならない
- **FR-006**: DocumentHeaderSyntax がある場合、document の header プロパティとして出力しなければならない
- **FR-007**: SectionSyntax は level プロパティを出力しなければならない
- **FR-008**: SectionTitleSyntax 内の各インライン要素は、title 配列内の個別ノードとして出力しなければならない

### Key Entities

- **AsgNode (基底クラス)**: すべての ASG ノードの共通プロパティ（name, type, location）を持つ
- **AsgDocument**: document ブロック。header?, blocks を持つ（attributes は省略）
- **AsgHeader**: 文書ヘッダー。title（インライン配列）, location を持つ
- **AsgSection**: section ブロック。title, level, blocks を持つ
- **AsgParagraph**: paragraph ブロック。inlines を持つ
- **AsgText**: text 文字列。value を持つ
- **AsgLocation**: 位置情報。開始位置と終了位置（行・列）のペア

### Assumptions

- 現時点で対応する SyntaxNode タイプは: DocumentSyntax, DocumentHeaderSyntax, DocumentBodySyntax, SectionSyntax, SectionTitleSyntax, ParagraphSyntax, TextSyntax
- LinkSyntax と AuthorLineSyntax は TCK の出力例に含まれていないため、今回のスコープからは除外する
- JSON シリアライズには System.Text.Json を使用する（.NET 10 標準）
- ASG クラスのプロパティ名は .NET 標準の PascalCase を使用し、JSON 出力時にカスタム変換ロジックで TCK 形式（lowercase）に変換する

## Clarifications

### Session 2026-01-28

- Q: JSON プロパティ命名規則をどうするか？ → A: .NET 標準（PascalCase）を採用し、JSON シリアライズ時にカスタム変換ロジックで TCK 形式に変換
- Q: 未対応 SyntaxNode の処理方法は？ → A: スキップ（無視）- 未対応ノードは出力に含めず処理を継続
- Q: document の attributes プロパティの扱いは？ → A: 省略（出力なし）- 属性パーサー未実装のため
- Q: section/header の title 配列の構成方法は？ → A: SectionTitleSyntax 内の各 TextSyntax を text ノードとして配列に格納
- Q: JSON 構造の検証方法は？ → A: 検証は TCK が行うため、自前での検証は不要

## Success Criteria

### Measurable Outcomes

- **SC-001**: SyntaxTree から ASG への変換処理が正常に完了する
- **SC-002**: 生成された JSON を TCK に渡した際、対応するテストケースが合格する
- **SC-003**: すべての対応 SyntaxNode タイプが例外なく変換できる
