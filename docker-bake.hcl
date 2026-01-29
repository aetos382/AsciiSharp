target "tck" {
  dockerfile = "Source/AsciiSharp.TckAdapter/Dockerfile"
  context = "."
  contexts = {
    # これダメかね？
    "asciidoc-tck" = "https://gitlab.eclipse.org/eclipse/asciidoc-lang/asciidoc-tck.git"
  }
  tags = [ "asciisharp-tck" ]
}
