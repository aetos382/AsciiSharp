mkdir -p /home/vscode/.local/bin
mkdir -p /home/vscode/.dotnet/tools

brew install gemini-cli

uv tool install specify-cli --from git+https://github.com/github/spec-kit.git

dotnet new install Reqnroll.Templates.DotNet
dotnet tool install -g csharp-ls
