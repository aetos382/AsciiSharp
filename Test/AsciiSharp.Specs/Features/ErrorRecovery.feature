# language: en
@US2 @エラー回復
Feature: エラー耐性解析
    エディタ開発者として、
    構文エラーを含む AsciiDoc 文書を解析しても、
    正常な部分を最大限に解析し、有用な情報を提供したい

    Background:
        Given AsciiDoc パーサーが利用可能である

    @Red @Acceptance
    Scenario: 不完全なセクションタイトルを含む文書の解析
        Given 以下の AsciiDoc 文書がある:
            """
            = ドキュメントタイトル

            == セクション1

            これは正常な段落です。

            ==

            == セクション2

            これも正常な段落です。
            """
        When 文書を解析する
        Then 構文木が生成される
        And 構文木に診断情報が含まれる
        And 診断情報の数は 1 以上である
        And "セクション1" のセクションが正しく解析される
        And "セクション2" のセクションが正しく解析される

    @ignore @Acceptance
    Scenario: 閉じられていない区切りブロックを含む文書の解析
        # 注: 区切りブロック（コードブロック等）は MVP スコープ外のため延期
        Given 以下の AsciiDoc 文書がある:
            """
            = ドキュメントタイトル

            == セクション1

            ----
            コードブロックの内容

            == セクション2

            正常な段落です。
            """
        When 文書を解析する
        Then 構文木が生成される
        And 構文木に診断情報が含まれる
        And "セクション2" が何らかの形で認識される

    @Red @Acceptance
    Scenario: 正常部分の最大解析
        Given 以下の AsciiDoc 文書がある:
            """
            = タイトル

            == 正常セクション1

            正常な段落1。

            == [不正な属性

            不正なセクション。

            == 正常セクション2

            正常な段落2。
            """
        When 文書を解析する
        Then 構文木が生成される
        And 正常な段落の数は 2 以上である
        And 構文木からテキストを再構築できる

    @Red @Diagnostic
    Scenario: 診断情報の位置情報が正確である
        Given 以下の AsciiDoc 文書がある:
            """
            = タイトル

            ==
            """
        When 文書を解析する
        Then 構文木に診断情報が含まれる
        And 診断情報に位置情報が含まれる
        And 診断情報の重大度が "Error" または "Warning" である

    @Red @MissingToken
    Scenario: 欠落トークンの検出
        Given 以下の AsciiDoc 文書がある:
            """
            = タイトル

            ==

            段落テキスト。
            """
        When 文書を解析する
        Then 構文木が生成される
        And 構文木に欠落ノードが含まれる
        And 欠落ノードの IsMissing プロパティが true である

    @Red @SkippedTokens
    Scenario: スキップされたトークンの保持
        Given 以下の AsciiDoc 文書がある:
            """
            = タイトル

            @@@不正なトークン@@@

            正常な段落。
            """
        When 文書を解析する
        Then 構文木が生成される
        And スキップされたトークンがトリビアとして保持される
        And 構文木からテキストを再構築できる
