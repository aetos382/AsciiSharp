target "tck" {
  dockerfile = "Source/AsciiSharp.TckAdapter/Dockerfile"
  context = "."
  contexts = {
    "asciidoc-tck" = "https://git@gitlab.eclipse.org/eclipse/asciidoc-lang/asciidoc-tck.git"
  }
  tags = [ "asciisharp-tck" ]
}
