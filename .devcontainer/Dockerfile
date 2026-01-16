# syntax=docker/dockerfile:latest
FROM mcr.microsoft.com/devcontainers/dotnet:2-10.0-noble@sha256:97961d9ef283e2404a79fb9bdca5837b6af57f5f8688556e21cbc5686af72770

RUN dotnet workload update

USER vscode

RUN mkdir -p /home/vscode/.claude
RUN mkdir -p /home/vscode/.nuget/packages
