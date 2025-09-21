FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS adapter-build

RUN --mount=type=cache,target=/var/cache/apt \
  apt-get update && \
  apt-get upgrade --yes --no-install-recommends --auto-remove --purge && \
  apt-get install --yes --no-install-recommends \
    clang \
    zlib1g-dev

ENV NUGET_PACKAGES=/var/cache/nuget

WORKDIR /workspace

COPY ["global.json", "Directory.*", "NuGet.config", "./"]

COPY ["Source/Directory.*", "Source/"]

COPY [ \
  "Source/AsciiSharp/AsciiSharp.csproj",  \
  "Source/AsciiSharp/" \
]

COPY [ \
  "Source/TckAdapter/AsciiSharp.TckAdapter/AsciiSharp.TckAdapter.csproj", \
  "Source/TckAdapter/AsciiSharp.TckAdapter/" \
]

COPY [ \
  "Source/TckAdapter/AsciiSharp.TckAdapter.Cli/AsciiSharp.TckAdapter.Cli.csproj", \
  "Source/TckAdapter/AsciiSharp.TckAdapter.Cli/" \
]

RUN \
  #--mount=type=cache,target=/var/cache/nuget \
  [ \
    "dotnet", "restore", \
    "Source/TckAdapter/AsciiSharp.TckAdapter.Cli/AsciiSharp.TckAdapter.Cli.csproj", \
    "--runtime", "linux-x64" \
]

COPY ["README.md", "."]
COPY ["Source", "Source/"]

RUN [ \ 
  "dotnet", "publish", \
  "Source/TckAdapter/AsciiSharp.TckAdapter.Cli/AsciiSharp.TckAdapter.Cli.csproj", \
  "--configuration", "Release", \
  "--self-contained", \
  "--runtime", "linux-x64", \
  "--no-restore" \
]

FROM node:22-trixie AS tck-build

WORKDIR /workspace

RUN git clone \
  -b main \
  --single-branch \
  --depth 1 \
  https://gitlab.eclipse.org/eclipse/asciidoc-lang/asciidoc-tck.git

WORKDIR /workspace/asciidoc-tck

RUN \
  --mount=type=cache,target=/root/.npm \
  --mount=type=cache,target=/workspace/asciidoc-tck/node_modules \
  npm ci && npm install -D postject && npm run dist

FROM debian:trixie-slim

WORKDIR /workspace

COPY --from=adapter-build --chmod=555 ["/workspace/artifacts/publish/AsciiSharp.TckAdapter.Cli/release_linux-x64", "tck-adapter/"]
COPY --from=tck-build --chmod=555 ["/workspace/asciidoc-tck/dist", "tck/"]
COPY --from=tck-build --chmod=555 ["/workspace/asciidoc-tck/tests", "tests/"]

ENTRYPOINT [ "tck/asciidoc-tck" ]
CMD [ "cli", "--adapter-command", "tck-adapter/AsciiSharp.TckAdapter" ]
