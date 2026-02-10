---
paths:
  - "**/*.slnx"
  - "**/*.props"
  - "**/*.targets"
  - "**/*.csproj"
---

# MSBuild プロジェクトの更新に関わるルール

- 原則として `dotnet` CLI を用いて行うこと。
- `dotnet` CLI がサポートしていない操作のみ、直接編集によって行ってよい。

# `dotnet` CLI を用いて行うべき操作の例

- プロジェクトへの NuGet パッケージの追加: `dotnet package add <package-id> --project <project-path>`
- プロジェクトからの NuGet パッケージの削除: `dotnet package remove <package-id> --project <project-path>`
- プロジェクトの新規作成: `dotnet new <template-id> --name <name>`
- ソリューションへのプロジェクトの追加: `dotnet solution <solution-path> add <project-path>`
