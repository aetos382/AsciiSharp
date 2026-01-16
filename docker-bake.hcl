target "tck" {
  dockerfile = "Source/TckAdapter/AsciiSharp.TckAdapter.Cli/Dockerfile"
  context = "."
  contexts = {
    "asciidoc-tck" = "submodules/asciidoc-tck"
  }
  tags = [ "asciisharp-tck" ]
}
