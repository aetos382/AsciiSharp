mkdir -p /home/vscode/.local/bin
mkdir -p /home/vscode/.dotnet/tools

uv tool install specify-cli --from git+https://github.com/github/spec-kit.git
