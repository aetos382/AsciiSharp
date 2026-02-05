# language: ja

@US2 @InlineTextSyntax @Rename
フィーチャ: TextSyntax を InlineTextSyntax として参照する
    ライブラリユーザーとして、
    プレーンテキストのインライン要素を InlineTextSyntax として参照し、
    一貫した命名規則で構文木を操作したい

    背景:
        前提 AsciiDoc パーサーが初期化されている

    @Acceptance @US2-1
    シナリオ: InlineTextSyntax の SyntaxKind は InlineText である
        前提 以下の AsciiDoc 文書がある:
            """
            = タイトル

            段落テキスト。
            """
        もし 文書を解析する
        ならば 段落の最初のインライン要素の SyntaxKind は InlineText である

    @Acceptance @US2-2
    シナリオ: Visitor で InlineTextSyntax を訪問する
        前提 以下の AsciiDoc 文書がある:
            """
            = タイトル

            段落テキスト。
            """
        もし 文書を解析する
        かつ Visitor でドキュメントを走査する
        ならば VisitInlineText メソッドが呼び出される

    @US2-3
    シナリオ: InlineTextSyntax から Text プロパティを取得する
        前提 以下の AsciiDoc 文書がある:
            """
            = タイトル

            Hello, World!
            """
        もし 文書を解析する
        ならば 段落の最初のインライン要素のテキストは "Hello, World!" である
