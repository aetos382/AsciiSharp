# language: ja

@US3
フィーチャ: リンクの解析
  URL リンクを含むテキストを解析し、
  リンクの位置とターゲットを正確に把握できる。

  背景:
    前提 AsciiDoc パーサーが利用可能である

  @AC1
  シナリオ: URL リンクを含む段落の解析
    前提 以下の AsciiDoc 文書がある:
      """
      これは https://example.com へのリンクです。
      """
    もし 文書を解析する
    ならば 構文木が生成される
    かつ 構文木に Link ノードが含まれる
    かつ Link ノードのターゲット URL は "https://example.com" である

  @AC2
  シナリオ: 表示テキスト付きリンクの解析
    前提 以下の AsciiDoc 文書がある:
      """
      詳細は https://example.com[公式サイト] を参照してください。
      """
    もし 文書を解析する
    ならば 構文木が生成される
    かつ 構文木に Link ノードが含まれる
    かつ Link ノードのターゲット URL は "https://example.com" である
    かつ Link ノードの表示テキストは "公式サイト" である

  @AC3
  シナリオ: 複数のリンクを含む段落の解析
    前提 以下の AsciiDoc 文書がある:
      """
      https://example1.com と https://example2.com の両方を参照してください。
      """
    もし 文書を解析する
    ならば 構文木が生成される
    かつ 構文木に 2 個の Link ノードが含まれる
    かつ 1 番目の Link ノードのターゲット URL は "https://example1.com" である
    かつ 2 番目の Link ノードのターゲット URL は "https://example2.com" である

  @AC4
  シナリオ: リンクを含む文書のラウンドトリップ
    前提 以下の AsciiDoc 文書がある:
      """
      = ドキュメント

      リンク: https://example.com[Example]
      """
    もし 文書を解析する
    かつ 構文木から完全なテキストを取得する
    ならば 再構築されたテキストは元の文書と一致する
