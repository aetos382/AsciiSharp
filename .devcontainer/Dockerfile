# syntax=docker/dockerfile:latest
FROM mcr.microsoft.com/devcontainers/base:2-noble

RUN apt-get update --yes && \
    apt-get install --yes --no-install-recommends --no-install-suggests \
      asciidoctor && \
    rm -rf /var/lib/apt/lists/*

USER vscode

RUN mkdir -p \
  /home/vscode/.claude \
  /home/vscode/.dotnet/tools \
  /home/vscode/.local/bin \
  /home/vscode/.local/node/bin \
  /home/vscode/.nuget/packages

ARG NODE_VERSION=v24.13.0

RUN curl -qfsSL https://nodejs.org/dist/${NODE_VERSION}/node-${NODE_VERSION}-linux-x64.tar.xz | tar -xJ --strip-components=1 -C /home/vscode/.local/node
