# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/devcontainers/dotnet:2-10.0-noble

RUN <<EOF
sed -i.bak -E \
  '/^URIs:/ {
    N; /Suites:.*-security/! {
      s@http://archive\.ubuntu\.com/ubuntu/@http://ftp.udx.icscoe.jp/Linux/ubuntu/@;
    }
  }' \
  /etc/apt/sources.list.d/ubuntu.sources
EOF

RUN dotnet workload update

USER vscode

RUN mkdir -p /home/vscode/.claude
