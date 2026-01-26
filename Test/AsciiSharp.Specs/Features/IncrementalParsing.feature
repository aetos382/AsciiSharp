# language: ja

@US5
フィーチャ: 増分解析
    開発者として、
    エディタで文書の一部を編集したとき、
    パーサーが変更された部分のみを再解析し、
    変更されていない部分の解析結果を再利用したい

    背景:
        前提 AsciiDoc パーサーが初期化されている

    @Acceptance @Scenario1
    シナリオ: 単一ブロック内の編集で他のブロックは再利用される
        前提 以下の AsciiDoc 文書がある:
            """
            = Document Title

            == Section 1

            First paragraph.

            == Section 2

            Second paragraph.

            == Section 3

            Third paragraph.
            """
        もし 文書を解析する
        かつ すべてのセクションの内部ノードへの参照を保持する
        かつ "First paragraph." を "Edited paragraph." に変更して増分解析する
        ならば インデックス 2 のセクションの内部ノードは再利用されている
        かつ インデックス 3 のセクションの内部ノードは再利用されている

    @Acceptance @Scenario2
    シナリオ: 同一インスタンスの維持
        前提 以下の AsciiDoc 文書がある:
            """
            = Document Title

            == Section A

            Content of section A.

            == Section B

            Content of section B.
            """
        もし 文書を解析する
        かつ 名前 "B" のセクションの内部ノードへの参照を保持する
        かつ "Content of section A." を "Modified content." に変更して増分解析する
        ならば 名前 "B" のセクションの内部ノードは再利用されている

    @Acceptance @Scenario3
    シナリオ: 増分解析後もラウンドトリップが成功する
        前提 以下の AsciiDoc 文書がある:
            """
            = Document Title

            Original content.
            """
        もし 文書を解析する
        かつ "Original content." を "Modified content." に変更して増分解析する
        かつ 構文木から完全なテキストを取得する
        ならば 再構築されたテキストは変更後の文書と一致する

    @StructuralSharing
    シナリオ: 構造共有により変更されていないノードは再利用される
        前提 以下の AsciiDoc 文書がある:
            """
            = Document Title

            == Section 1

            Paragraph 1.

            == Section 2

            Paragraph 2.
            """
        もし 文書を解析する
        かつ すべてのセクションの内部ノードへの参照を保持する
        かつ "Paragraph 2." を "Modified." に変更して増分解析する
        ならば インデックス 1 のセクションの内部ノードは再利用されている
