FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS dotnet-build

WORKDIR /workspace

COPY ["Directory.*", "."]
COPY ["NuGet.config", "."]
COPY ["README.md", "."]

COPY ["Source/Directory.*", "Source/"]

COPY ["Source/AsciiSharp/AsciiSharp.csproj", "Source/AsciiSharp/"]
COPY ["Source/AsciiSharp/packages.lock.json", "Source/AsciiSharp/"]

COPY ["Test/Directory.Build.props", "Test/"]

COPY ["Test/AsciiSharp.TckAdapter/AsciiSharp.TckAdapter.csproj", "Test/AsciiSharp.TckAdapter/"]
COPY ["Test/AsciiSharp.TckAdapter/packages.lock.json", "Test/AsciiSharp.TckAdapter/"]

RUN ["dotnet", "restore", "Test/AsciiSharp.TckAdapter"]

COPY ["Source", "Source/"]
COPY ["Test/AsciiSharp.TckAdapter", "Test/AsciiSharp.TckAdapter/"]

RUN ["dotnet", "publish", "Test/AsciiSharp.TckAdapter", "-r", "linux-x64", "--configuration", "Release", "--self-contained"]

VOLUME nuget-packages

FROM node:20-bookworm

WORKDIR /workspace

COPY --from=dotnet-build ["/workspace/artifacts/bin/AsciiSharp.TckAdapter/release_linux-x64", "tck-adapter/"]

RUN npm init --yes
