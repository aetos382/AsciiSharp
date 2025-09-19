FROM mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim AS adapter-build

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
  "Source/AsciiSharp/packages.lock.json", \
  "Source/AsciiSharp/" \
]

COPY [ \
  "Source/AsciiSharp.TckAdapter/AsciiSharp.TckAdapter.csproj", \
  "Source/AsciiSharp.TckAdapter/packages.lock.json", \
  "Source/AsciiSharp.TckAdapter/" \
]

RUN \
  #--mount=type=cache,target=/var/cache/nuget \
  ["dotnet", "restore", "Source/AsciiSharp.TckAdapter", "--locked-mode"]

COPY ["README.md", "."]
COPY ["Source", "Source/"]

RUN [ \
  "dotnet", "publish", \
  "Source/AsciiSharp.TckAdapter", \
  "--configuration", "Release", \
  "--self-contained", \
  "--runtime", "linux-x64", \
  "--no-restore" \
]

FROM node:22-bookworm AS tck-build

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

FROM debian:bookworm

WORKDIR /workspace

COPY --from=adapter-build --chmod=555 ["/workspace/artifacts/bin/AsciiSharp.TckAdapter/release_linux-x64", "tck-adapter/"]
COPY --from=tck-build --chmod=555 ["/workspace/asciidoc-tck/dist", "tck/"]
COPY --from=tck-build --chmod=555 ["/workspace/asciidoc-tck/tests", "tests/"]

ENTRYPOINT [ "tck/asciidoc-tck" ]
CMD [ "cli", "--adapter-command", "tck-adapter/AsciiSharp.TckAdapter" ]
