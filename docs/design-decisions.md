# 設計に関する未決定事項

このドキュメントでは、AsciiSharp の設計に関して未決定の考慮事項をまとめています。

## プリプロセッサについて

- AsciiDoc には [Includes](https://docs.asciidoctor.org/asciidoc/latest/directives/include/) および [Conditionals](https://docs.asciidoctor.org/asciidoc/latest/directives/conditionals/) の 2 種類のプリプロセッサがあります。
- フロントマターの処理を行うなど、独自の[プリプロセッサ拡張](https://docs.asciidoctor.org/asciidoctor/latest/extensions/preprocessor/)を提供することもできます。
- [AsciiDoctor](https://docs.asciidoctor.org/) の仕様では、[プリプロセッサはドキュメントが解析される前に処理される](https://docs.asciidoctor.org/asciidoctorj/latest/extensions/preprocessor/)とあります。
- しかし、`include` は外部ファイルの読み込みを伴うことから、これをパースの時点で処理すると、パースの純粋性が失われ、また、パースに非同期 I/O 処理が絡むことになるため、好ましくありません。
- そのため、まず、プリプロセッサ指令を処理せず、そのまま保持した構文木を得ることができ、その後、必要に応じてプリプロセスを行うことができるようにします。
- `include` に関しては、追加で `IDictionary<string, Document>` のようなパース済みのコレクションを与えることで、同期的に適用できる設計にします。
- ただし、利便性のため、最初からプリプロセスした構文木を得られる非同期ユーティリティ メソッドも提供します。その場合、外部ドキュメントを取得する方法は `IExternalDocumentResolver` 的なインターフェイスで抽象化します。
- 文書に関するエラー報告は、プリプロセッサの処理が行われる前の座標で行われます。

## 文書ヘッダーについて

- AsciiDoc では文書の冒頭に[文書ヘッダー](https://docs.asciidoctor.org/asciidoc/latest/document/header/)が配置される場合があります。文書ヘッダーとコンテンツは空行で区切られます。
- `include` によって取り込まれる外部文書内に最初の空行がある場合や、`ifdef` の条件によって最初の空行の位置が変動する場合があります。その場合、文書ヘッダーとコンテンツの区切りは、プリプロセッサを処理してみるまで決定されません。
- この不確定性は、パースとプリプロセスを分離することに対する抵抗になり得ます。

## 正規化と内部エンコーディングについて

- AsciiDoc ではドキュメントは [UTF-8 に正規化される](https://docs.asciidoctor.org/asciidoc/latest/normalization/)と規定されています。
- 構文木の内部表現として .NET の `string` で保持するのか、UTF-8 エンコードされた `byte[]` で持つのかを検討します。
- いずれの場合でも、コード上の位置情報は Unicode のコードポイント単位で数えるものとします。
- 正規化の仕様では、レンダリングされた文書の生成においては、改行コードは `\n` とすると規定されています。
- 入力ソースファイルはその他の改行コード（`\r\n`等）も受理します。
- 正規化仕様は元のテキスト文字列の完全再現が可能であるという仕様と競合するため、方針を決定する必要があります。
  - オプション 1: 構文木からの再文字列化は「HTML、DocBook 等のレンダリング」ではないから正規化仕様は適用されない。
  - オプション 2: 出力文書の改行コードはオプションで指定することができる。
