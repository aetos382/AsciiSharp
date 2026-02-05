# syntax=docker/dockerfile:latest@sha256:b6afd42430b15f2d2a4c5a02b919e98a525b785b1aaff16747d2f623364e39b6
FROM mcr.microsoft.com/devcontainers/base:2-noble@sha256:3dcb059253b2ebb44de3936620e1cff3dadcd2c1c982d579081ca8128c1eb319

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
