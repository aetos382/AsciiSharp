# syntax=docker/dockerfile:latest@sha256:b6afd42430b15f2d2a4c5a02b919e98a525b785b1aaff16747d2f623364e39b6
FROM golang:1-trixie@sha256:fb4b74a39c7318d53539ebda43ccd3ecba6e447a78591889c0efc0a7235ea8b3 AS go-build

RUN CGO_ENABLED=0 go install github.com/jrandolf/mcpr@latest

FROM mcr.microsoft.com/devcontainers/base:2-noble@sha256:3dcb059253b2ebb44de3936620e1cff3dadcd2c1c982d579081ca8128c1eb319

USER vscode

RUN mkdir -p \
  /home/vscode/.copilot \
  /home/vscode/.codex \
  /home/vscode/.claude \
  /home/vscode/.gemini \
  /home/vscode/.dotnet/tools \
  /home/vscode/.local/bin \
  /home/vscode/.nuget/packages

COPY --from=go-build /go/bin/mcpr /home/vscode/.local/bin/
