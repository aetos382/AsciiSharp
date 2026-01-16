# syntax=docker/dockerfile:latest@sha256:b6afd42430b15f2d2a4c5a02b919e98a525b785b1aaff16747d2f623364e39b6
FROM mcr.microsoft.com/dotnet/sdk:10.0.102-noble-aot-amd64@sha256:dd57b0d2ace6310fc99b141d82925960b489fade10b9045bb668b6ea7b22e439 AS adapter-build

ENV NUGET_PACKAGES=/var/cache/nuget

WORKDIR /workspaces

COPY ["global.json", "Directory.*", "NuGet.config", ".globalconfig", "./"]

COPY ["Source/Directory.*", "Source/"]

COPY [ \
  "Source/AsciiSharp/AsciiSharp.csproj",  \
  "Source/AsciiSharp/packages.lock.json", \
  "Source/AsciiSharp/" \
]

COPY [ \
  "Source/TckAdapter/AsciiSharp.TckAdapter/AsciiSharp.TckAdapter.csproj", \
  "Source/TckAdapter/AsciiSharp.TckAdapter/packages.lock.json", \
  "Source/TckAdapter/AsciiSharp.TckAdapter/" \
]

COPY [ \
  "Source/TckAdapter/AsciiSharp.TckAdapter.Cli/AsciiSharp.TckAdapter.Cli.csproj", \
  "Source/TckAdapter/AsciiSharp.TckAdapter.Cli/packages.lock.json", \
  "Source/TckAdapter/AsciiSharp.TckAdapter.Cli/" \
]

COPY ["Source", "Source/"]

RUN --mount=type=cache,id=nuget,target=/var/cache/nuget \
[ \
  "dotnet", "publish", \
  "Source/TckAdapter/AsciiSharp.TckAdapter.Cli/AsciiSharp.TckAdapter.Cli.csproj", \
  "--configuration", "Release", \
  "--runtime", "linux-x64", \
  "--output", "/workspaces/publish", \
  "--nologo" \
]

FROM node:lts-trixie@sha256:bea2f8264c42f56e253295ebd89db8e7c4ff96a03339adeb4a7628bd548d9476 AS tck-build

WORKDIR /workspaces

COPY --from=asciidoc-tck . /workspaces/asciidoc-tck/

WORKDIR /workspaces/asciidoc-tck

RUN \
  --mount=type=cache,target=/root/.npm \
  --mount=type=cache,target=/workspaces/asciidoc-tck/node_modules \
  npm ci && npm install -D postject && npm run dist

FROM debian:trixie-slim@sha256:77ba0164de17b88dd0bf6cdc8f65569e6e5fa6cd256562998b62553134a00ef0

WORKDIR /workspaces

COPY --from=adapter-build --chmod=555 ["/workspaces/publish", "tck-adapter/"]
COPY --from=tck-build --chmod=555 ["/workspaces/asciidoc-tck/dist", "tck/"]
COPY --from=tck-build --chmod=555 ["/workspaces/asciidoc-tck/tests", "tests/"]

ENTRYPOINT [ "tck/asciidoc-tck" ]
CMD [ "cli", "--adapter-command", "tck-adapter/AsciiSharp.TckAdapter.Cli" ]
