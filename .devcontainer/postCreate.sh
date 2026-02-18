uv tool install specify-cli --from git+https://github.com/github/spec-kit.git

dotnet tool install -g csharp-ls

npm install -g npm
npm install -g @anthropic-ai/claude-code

# https://github.com/anthropics/claude-code/issues/19275
cat .claude/settings.json |
jq -r '.enabledPlugins | to_entries[] | select(.value == true) | .key' |
while read -r plugin; do
  claude plugin install "$plugin"
done
