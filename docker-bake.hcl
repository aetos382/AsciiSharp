target "tck" {
  dockerfile = "Source/AsciiSharp.TckAdapter/Dockerfile"
  target = "tck"
  context = "."
  contexts = {
    "asciidoc-tck" = "submodules/asciidoc-tck"
  }
  tags = [ "asciisharp-tck" ]
}

target "adapter-build-log" {
  dockerfile = "Source/AsciiSharp.TckAdapter/Dockerfile"
  target = "build-log"
  context = "."
  output = [ "type=local,dest=build-logs" ]
}

group "tck-with-log" {
  targets = [ "tck", "adapter-build-log" ]
}
