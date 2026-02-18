# syntax=docker/dockerfile:latest
FROM mcr.microsoft.com/dotnet/sdk:10.0.103-noble-aot-amd64 AS adapter-build

ENV NUGET_PACKAGES=/var/cache/nuget

ARG CI=false
ENV CI=${CI:-false}

WORKDIR /workspaces

COPY ["global.json", "Directory.*", "NuGet.config", ".globalconfig", "./"]

COPY ["Source/Directory.Build.*", "Source/"]

COPY [ \
  "Source/AsciiSharp/AsciiSharp.csproj",  \
  "Source/AsciiSharp/packages.lock.json", \
  "Source/AsciiSharp/" \
]

COPY [ \
  "Source/AsciiSharp.Asg/AsciiSharp.Asg.csproj", \
  "Source/AsciiSharp.Asg/packages.lock.json", \
  "Source/AsciiSharp.Asg/" \
]

COPY [ \
  "Source/AsciiSharp.TckAdapter/AsciiSharp.TckAdapter.csproj", \
  "Source/AsciiSharp.TckAdapter/packages.lock.json", \
  "Source/AsciiSharp.TckAdapter/" \
]

# dotnet restore を分離するのはうまくいかない
# https://github.com/dotnet/sdk/issues/51766

COPY ["Source", "Source/"]

RUN --mount=type=cache,id=nuget-cache,target=/var/cache/nuget \
[ \
  "dotnet", "publish", \
  "Source/AsciiSharp.TckAdapter/AsciiSharp.TckAdapter.csproj", \
  "--configuration", "Release", \
  "--runtime", "linux-x64", \
  "--self-contained", \
  "--nologo", \
  "-bl:logs/AsciiSharp.TckAdapter.binlog" \
]

RUN [ "dotnet", "build-server", "shutdown" ]

FROM node:lts-trixie-slim AS tck

WORKDIR /workspaces

RUN ["chown", "node:node", "."]

USER node

COPY --from=asciidoc-tck --chown=node:node ["./", "/workspaces/asciidoc-tck/"]
COPY --from=adapter-build --chown=node:node --chmod=544 ["/workspaces/artifacts/publish/AsciiSharp.TckAdapter/release_linux-x64/AsciiSharp.TckAdapter", "tck-adapter/"]

ENV ASCIIDOC_TCK_ADAPTER=/workspaces/tck-adapter/AsciiSharp.TckAdapter

WORKDIR /workspaces/asciidoc-tck

RUN ["mkdir", "-p", "/home/node/.npm"]

RUN \
  --mount=type=cache,id=npm-cache,target=/home/node/.npm,uid=1000,gid=1000 \
  npm ci && yes | npm run dist

CMD [ \
  "node", \
  "harness/bin/asciidoc-tck.js", "cli", \
  "--adapter-command", "/workspaces/tck-adapter/AsciiSharp.TckAdapter" \
]

FROM scratch AS build-log

WORKDIR /

COPY --from=adapter-build /workspaces/logs/* .
