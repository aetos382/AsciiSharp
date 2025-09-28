target "tck" {
  dockerfile = "Source/TckAdapter/AsciiSharp.TckAdapter.Cli/Dockerfile"
  context = "."
  tags = [ "asciisharp-tck" ]
  platforms = [ "linux/amd64" ]
}
