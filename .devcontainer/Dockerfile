# syntax=docker/dockerfile:latest@sha256:b6afd42430b15f2d2a4c5a02b919e98a525b785b1aaff16747d2f623364e39b6
FROM mcr.microsoft.com/devcontainers/base:2-noble@sha256:3dcb059253b2ebb44de3936620e1cff3dadcd2c1c982d579081ca8128c1eb319

RUN apt-get update --yes && \
    apt-get install --yes --no-install-recommends --no-install-suggests \
      asciidoctor && \
    rm -rf /var/lib/apt/lists/*

USER vscode

RUN mkdir -p \
  /home/vscode/.copilot \
  /home/vscode/.codex \
  /home/vscode/.claude \
  /home/vscode/.gemini \
  /home/vscode/.dotnet/tools \
  /home/vscode/.local/bin \
  /home/vscode/.nuget/packages \
  /home/vscode/.docker/cli-plugins

ARG DOCKER_MCP_GATEWAY_VERSION=v0.39.0

RUN curl -qfsSL https://github.com/docker/mcp-gateway/releases/download/${DOCKER_MCP_GATEWAY_VERSION}/docker-mcp-linux-amd64.tar.gz | tar -xzC /home/vscode/.docker/cli-plugins
