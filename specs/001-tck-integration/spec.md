# Feature Specification: TCK 統合テスト基盤

**Feature Branch**: `001-tck-integration`
**Created**: 2026-01-29
**Status**: Draft
**Input**: User description: "TCK を使ったテストの仕組みを整えます。TCK の仕様と仕組みについては submodules/asciidoc-tck/README.adoc を参照してください。AsciiSharp.TckAdapter.Cli が標準入力から AsciiDoc 文書を受け取り、AsciiDoc.TckAdapter を使って ASG 形式に変換し、標準出力に出力します。docker-bake.hcl を使って docker buildx bake tck でビルドします。テストにパスしなかった構文要素を次の実装候補にします。GitHub CI で TCK を実行する仕組みを作ります。"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - TCK アダプターによる ASG 変換 (Priority: P1)

開発者として、AsciiDoc 文書を TCK の ASG (Abstract Semantic Graph) 形式に変換し、AsciiDoc Language Specification への準拠を検証したい。

**Why this priority**: これは TCK 統合の中核機能であり、他のすべての機能の基盤となります。アダプターが正しく動作しないと、TCK テストを実行できません。

**Independent Test**: 標準入力に TCK 形式の JSON を渡し、標準出力から ASG JSON が返されることを確認することで、独立してテスト可能です。

**Acceptance Scenarios**:

1. **Given** TCK 入力形式の JSON が標準入力に渡される、**When** CLI が実行される、**Then** ASG 形式の JSON が標準出力に出力される
2. **Given** block タイプの AsciiDoc 文書が入力される、**When** パースが実行される、**Then** 正しい ASG が生成される
3. **Given** inline タイプの AsciiDoc 文書が入力される、**When** パースが実行される、**Then** 正しい ASG が生成される
4. **Given** 正常にパースが完了する、**When** CLI が終了する、**Then** 終了コード 0 が返される

---

### User Story 2 - Docker によるビルドと配布 (Priority: P2)

開発者として、TCK アダプターを Docker コンテナとしてビルドし、一貫した実行環境で TCK テストを実行したい。

**Why this priority**: Docker 化により、ローカル環境と CI 環境で同じテスト環境を保証でき、"works on my machine" 問題を回避できます。P1 のアダプターが動作してから Docker 化するため P2 です。

**Independent Test**: `docker buildx bake tck` コマンドでイメージをビルドし、コンテナ内で TCK アダプターを実行してテスト可能です。

**Acceptance Scenarios**:

1. **Given** docker-bake.hcl が設定されている、**When** `docker buildx bake tck` が実行される、**Then** TCK アダプターの Docker イメージがビルドされる
2. **Given** Docker イメージが作成されている、**When** コンテナ内で TCK アダプターが実行される、**Then** 標準入力/出力を通じて正しく動作する
3. **Given** .NET 10.0 と PublishAot が設定されている、**When** ビルドが実行される、**Then** ネイティブ実行可能ファイルが生成される

---

### User Story 3 - GitHub Actions での自動 TCK 実行 (Priority: P3)

開発者として、プル リクエストやコミット時に自動的に TCK テストが実行され、AsciiDoc Language Specification への準拠状況を確認したい。

**Why this priority**: 継続的な準拠検証により、リグレッションを早期に検出できます。P1 と P2 の基盤があって初めて CI で実行できるため P3 です。

**Independent Test**: GitHub Actions ワークフローを手動でトリガーし、TCK テストが実行され、結果がレポートされることを確認できます。

**Acceptance Scenarios**:

1. **Given** GitHub Actions ワークフローが設定されている、**When** プル リクエストが作成される、**Then** TCK テストが自動実行される
2. **Given** TCK テストが実行される、**When** テストが完了する、**Then** 成功/失敗の結果が GitHub UI に表示される
3. **Given** Docker イメージがビルドされている、**When** CI 環境で TCK が実行される、**Then** asciidoc-tck コマンドがアダプターを呼び出す

---

### User Story 4 - 失敗したテストのレポート (Priority: P4)

開発者として、TCK テストで失敗した構文要素を明確に把握し、次に実装すべき機能の優先順位を決定したい。

**Why this priority**: テスト結果の可視化により、開発の方向性を決定できます。ただし、まずは TCK テストが実行できることが優先されるため P4 です。

**Independent Test**: TCK テストを実行し、失敗したテストのレポートが生成されることを確認できます。

**Acceptance Scenarios**:

1. **Given** TCK テストが実行される、**When** 一部のテストが失敗する、**Then** 失敗したテストの一覧が出力される
2. **Given** 失敗したテストがある、**When** レポートが生成される、**Then** 失敗した構文要素が特定される
3. **Given** 失敗した構文要素が特定される、**When** 開発者がレポートを確認する、**Then** 次に実装すべき機能が明確になる

---

### Edge Cases

- **不正な入力形式**: 標準入力に不正な JSON が渡された場合、CLI は適切なエラー メッセージを出力し、非ゼロの終了コードを返す
- **パース失敗**: AsciiDoc 文書のパースが失敗した場合、エラー情報を含む ASG またはエラー レスポンスを返す
- **Docker ビルド失敗**: Docker ビルドが失敗した場合、明確なエラー メッセージが表示され、ビルドが中断される
- **CI 環境の問題**: GitHub Actions で Node.js 20 以上が利用できない場合、適切なセットアップが行われる
- **TCK サブモジュールの更新失敗**: asciidoc-tck サブモジュールの最新版への更新に失敗した場合（ネットワークエラー等）、CI ワークフローは即座に失敗する

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: CLI は標準入力から TCK 形式の JSON (`{"contents": "...", "path": "...", "type": "block|inline"}`) を読み取らなければならない
- **FR-002**: CLI は AsciiDoc 文書をパースし、ASG (Abstract Semantic Graph) 形式の JSON に変換しなければならない
- **FR-003**: CLI は生成した ASG JSON を標準出力に書き出さなければならない
- **FR-004**: CLI は正常終了時に終了コード 0 を返し、エラー時には非ゼロの終了コードを返さなければならない
- **FR-005**: docker-bake.hcl を使用して `docker buildx bake tck` でビルド可能でなければならない
- **FR-006**: Docker イメージには .NET 10.0 ランタイムと PublishAot でビルドされたネイティブ実行可能ファイルが含まれなければならない
- **FR-007**: GitHub Actions ワークフローで TCK テストを自動実行しなければならない
- **FR-008**: CI ワークフローは毎回実行時に asciidoc-tck サブモジュールを最新版に更新しなければならない。更新に失敗した場合、ワークフローは即座に失敗しなければならない
- **FR-008a**: CI ワークフローは Node.js 20 以上をセットアップしなければならない
- **FR-009**: TCK テストの実行には `--adapter-command` オプションでアダプターのパスを指定しなければならない
- **FR-010**: TCK テストの結果（成功/失敗）を明確に出力しなければならない
- **FR-011**: 失敗した TCK テストについて、失敗した構文要素を特定できる情報を提供しなければならない

### Key Entities

- **TCK Input**: TCK が CLI に渡す入力データ
  - `contents`: パース対象の AsciiDoc 文書の内容（文字列）
  - `path`: 入力ファイルのパス（文字列）
  - `type`: パース タイプ（"block" または "inline"）

- **ASG Output**: CLI が TCK に返す抽象構文グラフ
  - JSON 形式でエンコードされた階層的なドキュメント モデル
  - AsciiDoc 文書の構造要素とその意味を表現
  - AsciiDoc Language Specification への準拠を検証するための情報を含む

- **Test Result**: TCK テストの実行結果
  - テスト名とステータス（passed/failed）
  - 失敗したテストの詳細情報
  - 失敗した構文要素の識別情報

- **Docker Image**: TCK アダプターを含むコンテナ イメージ
  - .NET 10.0 ランタイム
  - PublishAot でビルドされたネイティブ実行可能ファイル
  - 標準入力/出力を通じた TCK との通信機能

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: TCK アダプターが標準入力から JSON を受け取り、100% のケースで有効な ASG JSON を標準出力に出力できる
- **SC-002**: `docker buildx bake tck` コマンドが失敗なく完了し、実行可能な Docker イメージが生成される
- **SC-003**: GitHub Actions ワークフローが、プル リクエストまたはコミット時に 5 分以内に TCK テストを完了する
- **SC-004**: TCK テストの失敗率が 0% になるまでの進捗が追跡可能である（失敗したテストのレポートにより次の実装候補が明確になる）
- **SC-005**: CI 環境で実行される TCK テストが、ローカル環境と同じ結果を 100% 再現できる

## Assumptions

- asciidoc-tck サブモジュールは既に Git リポジトリに追加されている
- AsciiSharp.TckAdapter ライブラリは既に ASG 変換機能を実装している、または本フィーチャーの一部として実装される
- Docker 環境がローカルおよび CI で利用可能である
- GitHub Actions の実行時間制限内（通常 6 時間）で TCK テストが完了する
- TCK の入力形式と ASG 出力形式は AsciiDoc TCK 仕様に準拠している
