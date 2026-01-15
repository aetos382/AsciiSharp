# syntax=docker/dockerfile:latest
FROM mcr.microsoft.com/devcontainers/dotnet:2-10.0-noble

RUN dotnet workload update

USER vscode

RUN mkdir -p /home/vscode/.claude
