add_safe_directory() {
  local dir="$1"
  git config --global --get-all safe.directory | grep -qxF "$dir" || git config set --global --append safe.directory "$dir"
}

add_safe_directory '/workspaces/AsciiSharp/submodules/asciidoc-lang'
add_safe_directory '/workspaces/AsciiSharp/submodules/asciidoc-tck'
