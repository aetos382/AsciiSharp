# バックログ（後続イテレーションで対応予定）

MVP では対応せず、後のバージョンで実装予定の機能一覧。

## ブロック要素

- リスト（順序付き、順序なし、説明リスト）
- テーブル
- 区切りブロック（サイドバー、引用、リテラル、リスト、ソースコード等）
- 属性エントリ

## インライン要素

- 書式マークアップ（太字、斜体、等幅、上付き、下付き等）
- マクロ（image, kbd, btn, menu 等）
- 属性参照（{attribute-name}）
- 相互参照（xref, アンカー）
- 脚注

## ドキュメントヘッダー

- リビジョン行
- 属性定義

## プリプロセッサ

- include ディレクティブの実際のファイル読み込み
- 条件ディレクティブ（ifdef, ifndef, ifeval）の条件評価

## パフォーマンス最適化

- 初回解析の数値目標設定
- 増分再解析の数値目標設定

## フラグメント

- 完全なドキュメントでない文書フラグメントをパースできる必要がある？
- include をサポートする際に合わせて検討する。

## 改行と空白

- [SearchValuesStorage](https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/String.Manipulation.cs) にある `NewLineChars` および `WhiteSpacesChars` に準拠することを検討する。
- 行末の改行と空白は AsciiDoc 正規化仕様に倣って Trivia として保持する。
