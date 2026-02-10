# Feature Specification: BDD フレームワークの LightBDD 移行

**Feature Branch**: `005-lightbdd-migration`
**Created**: 2026-02-10
**Status**: Draft
**Input**: User description: "Reqnroll の使用をやめ、LightBDD に移行する。Cucumber より C# でテストを書きたい。"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - BDD テストを C# で記述・管理する (Priority: P1)

開発者として、BDD テストシナリオを Gherkin (.feature ファイル) ではなく C# コードで記述したい。
これにより、IDE のリファクタリング支援、IntelliSense による補完、型安全性、コンパイル時チェックの恩恵を受けられる。

**Why this priority**: プロジェクトの BDD テスト基盤を Reqnroll から LightBDD に移行する中核的な要件であり、他のすべてのストーリーの前提条件となる。

**Independent Test**: LightBDD を使って最も基本的なシナリオ（空のドキュメントのパース）を C# で記述し、テストが実行・成功することで検証できる。

**Acceptance Scenarios**:

1. **Given** LightBDD.MsTest4 がテストプロジェクトに導入されている, **When** C# で BDD シナリオを記述して実行する, **Then** テストが正常に実行され結果が報告される
2. **Given** 既存の Reqnroll ステップ定義が存在する, **When** LightBDD のステップメソッド形式に変換する, **Then** 同等の振る舞いが検証可能である
3. **Given** パラメータ付きのシナリオがある, **When** LightBDD のパラメータ化ステップとして記述する, **Then** 複数の入力パターンでテストが実行される

---

### User Story 2 - 既存テストカバレッジの維持 (Priority: P1)

開発者として、移行後も既存のすべてのテストシナリオが同等のカバレッジで検証されることを確認したい。
Reqnroll で定義されていた全シナリオが LightBDD で再現される必要がある。

**Why this priority**: テストカバレッジの後退は品質リスクに直結する。移行と同時に達成すべき必須要件である。

**Independent Test**: 移行前後でテスト数とシナリオカバレッジを比較し、全シナリオが網羅されていることを確認する。

**Acceptance Scenarios**:

1. **Given** 14 個の .feature ファイルに定義されたシナリオ群がある, **When** LightBDD 形式に変換する, **Then** すべてのシナリオに対応する LightBDD テストが存在する
2. **Given** 各シナリオに定義されたアクセプタンス条件がある, **When** LightBDD テストを実行する, **Then** 全テストが成功する
3. **Given** エッジケース（BOM 付き文書、混在改行コード等）のシナリオがある, **When** LightBDD テストとして実行する, **Then** 同じ検証が行われる

---

### User Story 3 - Reqnroll 依存の完全除去 (Priority: P2)

開発者として、Reqnroll 関連のパッケージ、設定ファイル、生成コードをプロジェクトから完全に除去したい。
これにより、ビルドパイプラインが簡素化され、依存関係が削減される。

**Why this priority**: 移行の完了を示す仕上げの要件。P1 のシナリオ移行が完了してから実施する。

**Independent Test**: Reqnroll パッケージへの参照が一切なく、.feature ファイルが削除され、ビルドが成功することで検証できる。

**Acceptance Scenarios**:

1. **Given** Reqnroll 関連パッケージがプロジェクトに存在する, **When** パッケージ参照を削除する, **Then** ビルドが成功し Reqnroll への依存がない
2. **Given** .feature ファイルと reqnroll.json が存在する, **When** これらを削除する, **Then** プロジェクト構造から Gherkin ファイルが除去されている
3. **Given** CLAUDE.md に Reqnroll への言及がある, **When** プロジェクト文書を更新する, **Then** BDD フレームワークの記述が LightBDD に統一されている

---

### User Story 4 - テスト実行レポートの生成 (Priority: P3)

開発者として、LightBDD のレポート生成機能を活用し、テスト実行結果を見やすい形式で確認したい。

**Why this priority**: LightBDD の付加価値機能。移行の主要目的ではないが、導入メリットを最大化するために含める。

**Independent Test**: テスト実行後に HTML レポートが生成され、シナリオの成否が一覧表示されることで検証できる。

**Acceptance Scenarios**:

1. **Given** LightBDD が設定済みである, **When** テストスイートを実行する, **Then** HTML 形式のテスト実行レポートが生成される

---

### Edge Cases

- 既存ステップ定義で正規表現によるパラメータキャプチャを使用している箇所を、LightBDD の型安全なパラメータに正しく変換できるか？
- Reqnroll の Context Injection（コンストラクタ注入）パターンを LightBDD でどう再現するか？
- 日本語のステップメソッド名が LightBDD のレポートで正しく表示されるか？
- MSTest.Sdk プロジェクト形式と LightBDD.MsTest4 の互換性
- 並列テスト実行（`[assembly: Parallelize]`）が LightBDD で正しく動作するか？

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: テストプロジェクト（AsciiSharp.Specs）で LightBDD.MsTest4 パッケージを使用できなければならない
- **FR-002**: 既存の 14 個の .feature ファイルに定義されたすべてのシナリオが、C# の LightBDD テストとして再実装されなければならない
- **FR-003**: 各テストクラスは `FeatureFixture` を継承し、`[Scenario]` 属性でシナリオを定義しなければならない
- **FR-004**: ステップメソッドは Given/When/Then の命名規約に従い、`Runner.RunScenario()` で実行されなければならない
- **FR-005**: 各フィーチャークラスは自己完結型とし、必要なステップメソッドをすべて自身で保持しなければならない
- **FR-006**: パラメータ化されたシナリオ（セクションレベルの検証、リンクインデックスの検証等）が型安全に記述されなければならない
- **FR-007**: テスト実行後に HTML レポートが生成されなければならない
- **FR-008**: Reqnroll 関連のパッケージ参照、設定ファイル（reqnroll.json）、.feature ファイルが完全に除去されなければならない
- **FR-009**: プロジェクトのドキュメント（CLAUDE.md 等）で BDD フレームワークの記述が LightBDD に更新されなければならない
- **FR-010**: 移行後も `dotnet test` コマンドで全テストが実行・成功しなければならない

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 既存の全 BDD シナリオ（68 シナリオ）が LightBDD テストとして実装され、すべて成功する
- **SC-002**: プロジェクト内に Reqnroll への参照が一切存在しない
- **SC-003**: プロジェクト内に .feature ファイルが存在しない
- **SC-004**: `dotnet test` 実行後に LightBDD の HTML テストレポートが生成される
- **SC-005**: ビルド警告がゼロの状態が維持される

## Assumptions

- LightBDD.MsTest4 は MSTest.Sdk プロジェクト形式と互換性がある（NuGet パッケージとして追加可能）
- LightBDD のステップメソッド名に日本語を使用しても、テスト実行とレポート生成に問題ない
- 各フィーチャークラスは自己完結型とし、ステップ定義間の再利用は行わない（コード重複は許容する）
- 並列テスト実行は LightBDD でもサポートされる
- Central Package Management（Directory.Packages.props）で LightBDD パッケージを管理する
