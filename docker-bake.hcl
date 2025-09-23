target "tck" {
  dockerfile = "Source/TckAdapter/AsciiSharp.TckAdapter.Cli/Dockerfile"
  context = "."
  tags = [ "asciisharp-tck" ]
  platforms = [ "linux/amd64" ]
  labels = {
    "org.opencontainers.image.authors" = "aetos [https://github.com/aetos382]"
    "org.opencontainers.image.licenses" = "Apache-2.0"
  }
}
