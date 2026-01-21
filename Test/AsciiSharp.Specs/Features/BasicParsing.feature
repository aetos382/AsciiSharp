# language: en
@US1
Feature: 基本的な AsciiDoc 文書の解析
    ライブラリユーザーとして、
    セクションと段落を含む AsciiDoc 文書を解析し、
    ロスレスな構文木を取得したい

    Background:
        Given AsciiDoc パーサーが初期化されている

    @Acceptance @Scenario1
    Scenario: ドキュメントタイトルとセクションを含む文書の解析
        Given 以下の AsciiDoc 文書がある:
            """
            = ドキュメントタイトル

            == セクション 1

            これは段落です。

            == セクション 2

            これも段落です。
            """
        When 文書を解析する
        Then 構文木のルートは Document ノードである
        And Document ノードは Header を持つ
        And Header のタイトルは "ドキュメントタイトル" である
        And Document ノードは 2 個のセクションを持つ
        And セクション 1 のタイトルは "セクション 1" である
        And セクション 2 のタイトルは "セクション 2" である

    @Acceptance @Scenario2
    Scenario: 構文木からの元テキスト再構築（ラウンドトリップ）
        Given 以下の AsciiDoc 文書がある:
            """
            = タイトル

            == セクション

            段落テキスト。
            複数行の段落。

            別の段落。
            """
        When 文書を解析する
        And 構文木から完全なテキストを取得する
        Then 再構築されたテキストは元の文書と一致する

    @Acceptance @Scenario3
    Scenario: ネストされたセクションの解析
        Given 以下の AsciiDoc 文書がある:
            """
            = メインタイトル

            == レベル 2 セクション

            レベル 2 の内容。

            === レベル 3 セクション

            レベル 3 の内容。

            ==== レベル 4 セクション

            レベル 4 の内容。
            """
        When 文書を解析する
        Then 構文木のルートは Document ノードである
        And セクションのネスト構造が正しく解析されている

    @Paragraph
    Scenario: 複数の段落の解析
        Given 以下の AsciiDoc 文書がある:
            """
            = タイトル

            最初の段落。

            2番目の段落。
            これは同じ段落の続き。

            3番目の段落。
            """
        When 文書を解析する
        Then Document ノードは 3 個の段落を持つ

    @Header
    Scenario: ドキュメントヘッダーのみの文書
        Given 以下の AsciiDoc 文書がある:
            """
            = ドキュメントタイトル
            著者名
            """
        When 文書を解析する
        Then 構文木のルートは Document ノードである
        And Header のタイトルは "ドキュメントタイトル" である
        And Header は著者行を持つ

    @Whitespace
    Scenario: 空白と改行の保持
        Given 以下の AsciiDoc 文書がある:
            """
            = タイトル

            段落テキスト。
            """
        When 文書を解析する
        And 構文木から完全なテキストを取得する
        Then 再構築されたテキストは元の文書と一致する
        And すべての空白と改行が保持されている
