mkdir -p /home/vscode/.local/bin
mkdir -p /home/vscode/.dotnet/tools

uv tool install specify-cli --from git+https://github.com/github/spec-kit.git

dotnet new install Reqnroll.Templates.DotNet
dotnet tool install -g csharp-ls

dotnet restore
dotnet tool restore
dotnet husky install
