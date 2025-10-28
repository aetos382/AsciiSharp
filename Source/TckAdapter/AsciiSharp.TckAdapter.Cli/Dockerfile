FROM mcr.microsoft.com/dotnet/sdk:10.0.100-rc.2-noble-aot AS adapter-build

ENV NUGET_PACKAGES=/var/cache/nuget

WORKDIR /workspace

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

RUN --mount=type=cache,id=nuget,target=/var/cache/nuget [ \
  "dotnet", "restore", \
  "Source/TckAdapter/AsciiSharp.TckAdapter.Cli/AsciiSharp.TckAdapter.Cli.csproj", \
  "--locked-mode", \
  "--nologo" \
]

COPY ["Source", "Source/"]

RUN --mount=type=cache,id=nuget,target=/var/cache/nuget [ \ 
  "dotnet", "publish", \
  "Source/TckAdapter/AsciiSharp.TckAdapter.Cli/AsciiSharp.TckAdapter.Cli.csproj", \
  "--configuration", "Release", \
  "--runtime", "linux-x64", \
  "-p:PublishAot=true", \
  "--output", "/workspace/publish", \
  "--no-restore", \
  "--nologo" \
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

COPY --from=adapter-build --chmod=555 ["/workspace/publish", "tck-adapter/"]
COPY --from=tck-build --chmod=555 ["/workspace/asciidoc-tck/dist", "tck/"]
COPY --from=tck-build --chmod=555 ["/workspace/asciidoc-tck/tests", "tests/"]

ENTRYPOINT [ "tck/asciidoc-tck" ]
CMD [ "cli", "--adapter-command", "tck-adapter/AsciiSharp.TckAdapter.Cli" ]
