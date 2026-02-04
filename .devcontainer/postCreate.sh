uv tool install specify-cli --from git+https://github.com/github/spec-kit.git

dotnet new install Reqnroll.Templates.DotNet
dotnet tool install -g csharp-ls

docker mcp feature enable profiles
docker mcp profile import .docker/mcp/profiles/default.yml

docker mcp client connect --global --profile default --quiet vscode
docker mcp client connect --global --profile default --quiet codex
docker mcp client connect --global --profile default --quiet claude-code
docker mcp client connect --global --profile default --quiet gemini
