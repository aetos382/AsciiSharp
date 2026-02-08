# language: ja

@US2
フィーチャ: 属性エントリの解析
    ライブラリユーザーとして、
    ドキュメント ヘッダー内の属性エントリ（:name: value 形式）を解析し、
    属性名と属性値を取得したい

    背景:
        前提 AsciiDoc パーサーが初期化されている

    @Acceptance @Scenario1
    シナリオ: 値あり属性エントリの解析
        前提 以下の AsciiDoc 文書がある:
            """
            = Document Title
            :icons: font
            """
        もし 文書を解析する
        ならば 構文木のルートは Document ノードである
        かつ Document ノードは Header を持つ
        かつ Header は 1 個の属性エントリを持つ
        かつ 属性エントリ 1 の名前は "icons" である
        かつ 属性エントリ 1 の値は "font" である

    @Acceptance @Scenario2
    シナリオ: 値なし属性エントリの解析
        前提 以下の AsciiDoc 文書がある:
            """
            = Document Title
            :toc:
            """
        もし 文書を解析する
        ならば Document ノードは Header を持つ
        かつ Header は 1 個の属性エントリを持つ
        かつ 属性エントリ 1 の名前は "toc" である
        かつ 属性エントリ 1 の値は空である

    @Acceptance @Scenario3
    シナリオ: 複数の属性エントリの解析
        前提 以下の AsciiDoc 文書がある:
            """
            = Document Title
            :icons: font
            :toc:
            """
        もし 文書を解析する
        ならば Document ノードは Header を持つ
        かつ Header は 2 個の属性エントリを持つ
        かつ 属性エントリ 1 の名前は "icons" である
        かつ 属性エントリ 1 の値は "font" である
        かつ 属性エントリ 2 の名前は "toc" である
        かつ 属性エントリ 2 の値は空である

    @Acceptance @Scenario4
    シナリオ: 属性エントリを含む文書のラウンドトリップ
        前提 以下の AsciiDoc 文書がある:
            """
            = Document Title
            :icons: font
            :toc:

            body
            """
        もし 文書を解析する
        かつ 構文木から完全なテキストを取得する
        ならば 再構築されたテキストは元の文書と一致する

    @EdgeCase
    シナリオ: ハイフンを含む属性名の解析
        前提 以下の AsciiDoc 文書がある:
            """
            = Document Title
            :my-custom-attr: value
            """
        もし 文書を解析する
        ならば Header は 1 個の属性エントリを持つ
        かつ 属性エントリ 1 の名前は "my-custom-attr" である
        かつ 属性エントリ 1 の値は "value" である

    @EdgeCase
    シナリオ: 属性エントリのない文書
        前提 以下の AsciiDoc 文書がある:
            """
            = Document Title

            body
            """
        もし 文書を解析する
        ならば Document ノードは Header を持つ
        かつ Header は 0 個の属性エントリを持つ
