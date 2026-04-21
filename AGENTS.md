# AGENTS.md - ZKEACMS

This file defines the default behavior for Copilot in this repository.

## Priorities

1. Follow the user's request first.
2. Prefer repository scripts and existing conventions over ad hoc commands.
3. Make the smallest change that solves the problem.
4. Do not modify unrelated files or code paths.
5. Code comments are not necessary, but if there are special, explan WHY not HOW.

## Build and Run

Build the full solution with `dotnet build` or build a specific project with `dotnet build xxxx.csproj`.

## Dependency Management

This repository uses Central Package Management (CPM).

- Update package versions only in `Directory.Packages.props` at the root.
- Do not add package versions inside individual `.csproj` files.
- If a package version change is required, keep the scope limited to the smallest set of packages needed.

## Database Configuration

Configure file:`src/ZKEACMS.WebHost/appsettings.json`.

- Database: Supported `DbType` values: `MsSql`, `Sqlite`, `MySql`, `DM` (Dameng).

## Project Structure

- `src/EasyFrameWork`: Core framework and infrastructure.
- `src/ZKEACMS`: Main CMS business logic and base classes.
- `src/ZKEACMS.WebHost`: ASP.NET Core web host entry point.
- `src/Plugins/`: Pluggable features. New plugins must inherit from `PluginBase`.
- `tools/`:
  - `PluginTemplate`: Template for generating new plugins.
  - `MsSql2Any`: Tools for database migration and schema conversion.

## Testing

- Unit tests live under `test/` and use MSTest. Run them with `dotnet test`.
- E2E tests live under `test/End-To-End/` and use Playwright.
- Install E2E dependencies with `npm install` inside `test/End-To-End/`.
- Run E2E tests with `npx playwright test`.
- Prefer targeted tests for the area you changed before running broader suites.

## Working Rules

- Preserve existing style and public APIs unless the task requires a breaking change.
- Avoid broad refactors when a focused fix is enough.
- Do not revert or overwrite user changes unless explicitly asked.
- When a change affects build, assets, tests, or publishing, verify the relevant path if possible.
