# language: ja

@US5
フィーチャ: 構文ノードの階層構造
    開発者として、
    構文木を走査する際に、
    型システムを利用してブロック要素とインライン要素を区別したい

    背景:
        前提 AsciiDoc パーサーが初期化されている

    @Acceptance @Scenario1 @P1
    シナリオ: ブロック要素は BlockSyntax として識別できる
        前提 以下の AsciiDoc 文書がある:
            """
            = タイトル

            段落テキスト
            """
        もし 文書を解析する
        ならば Document ノードは BlockSyntax である
        かつ Paragraph ノードは BlockSyntax である
        かつ Document ノードは InlineSyntax ではない
        かつ Paragraph ノードは InlineSyntax ではない

    @Acceptance @Scenario2 @P1
    シナリオ: インライン要素は InlineSyntax として識別できる
        前提 以下の AsciiDoc 文書がある:
            """
            https://example.com[リンク]
            """
        もし 文書を解析する
        ならば Link ノードは InlineSyntax である
        かつ Link ノードは BlockSyntax ではない

    @Acceptance @Scenario3 @P1
    シナリオ: セクション関連ノードは BlockSyntax として識別できる
        前提 以下の AsciiDoc 文書がある:
            """
            = ドキュメントタイトル

            == セクション1

            セクション内容
            """
        もし 文書を解析する
        ならば Section ノードは BlockSyntax である
        かつ SectionTitle ノードは BlockSyntax である

    @Acceptance @Scenario4 @P2
    シナリオ: すべてのブロックノードを一括で取得できる
        前提 以下の AsciiDoc 文書がある:
            """
            = タイトル

            段落1 https://example.com[リンク]

            == セクション

            段落2
            """
        もし 文書を解析する
        かつ すべての BlockSyntax ノードをクエリする
        ならば 取得したノードに Document が含まれる
        かつ 取得したノードに Paragraph が含まれる
        かつ 取得したノードに Section が含まれる
        かつ 取得したノードに Link は含まれない

    @Acceptance @Scenario5 @P2
    シナリオ: すべてのインラインノードを一括で取得できる
        前提 以下の AsciiDoc 文書がある:
            """
            https://example.com[リンク]
            """
        もし 文書を解析する
        かつ すべての InlineSyntax ノードをクエリする
        ならば 取得したノードに Link が含まれる
        かつ 取得したノードに Paragraph は含まれない
