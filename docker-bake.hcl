target "tck" {
  dockerfile = "Source/AsciiSharp.TckAdapter/Dockerfile"
  context = "."
  contexts = {
    "asciidoc-tck" = "submodules/asciidoc-tck"
  }
  tags = [ "asciisharp-tck" ]
}
