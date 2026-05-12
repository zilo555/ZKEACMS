# ZKEACMS Architecture

## Overview

ZKEACMS is a modular ASP.NET Core 9 CMS built around a reusable core framework, a web host, and a plugin-based feature system. It focuses on visual page building, widget composition, responsive layouts, and multi-database support.

The solution is organized so that the core framework provides shared infrastructure, the main CMS project contains business logic, the web host provides the runtime entry point, and plugins extend the system with isolated feature modules.

## Solution Structure

### `src/EasyFrameWork`

Shared framework and infrastructure layer. This project contains the common building blocks used across the CMS, including:

- Entity Framework Core integration and data access helpers
- Reflection, serialization, logging, caching, DI, and utility helpers
- Base types for application context, localization, validation, and error handling
- Supporting components for storage, image handling, and notification workflows

This project targets `net9.0` and references `Microsoft.AspNetCore.App` plus a small set of foundational packages such as EF Core, YamlDotNet, OpenXML, and IdGen.

### `src/ZKEACMS`

Main CMS domain project. This layer contains the business logic, CMS models, services, modules, and shared base classes that implement the product itself.

Typical responsibilities here include:

- Page, layout, widget, theme, and content composition logic
- CMS data models and repositories
- Application services and module registration
- Shared abstractions used by plugins and the web host

### `src/ZKEACMS.WebHost`

ASP.NET Core web application that boots the CMS.

Key characteristics:

- Targets `net9.0`
- References both `EasyFrameWork` and `ZKEACMS`
- Uses `Serilog.AspNetCore` for logging
- Enables Razor runtime compilation for development friendliness
- Publishes `Templates.zip` and locale files with the application

This project is the runtime entry point for local development and deployment.

### `src/Plugins`

Feature modules packaged as independent projects. Each plugin exposes a focused capability and is intended to be loaded into the CMS as an extension.

Common plugin areas include:

- Article and product content features
- Form generation
- Sitemap and redirection support
- Style editing and global scripts
- Audit trail, spam/spider logging, and distribution-related utilities
- Shop, message, search, animation, and updater features

Plugins should inherit from `PluginBase` and follow the repository's plugin conventions.

### `tools`

Developer utilities and packaging helpers.

- `PluginTemplate`: template files for generating new plugins
- `MsSql2Any`: schema conversion and migration support
- `PackWidgetTemplate`: widget packaging helper
- `LanguageScriptGen`: localization/script generation helper
- `TestStaticFiles`: local static file tooling

### `Database`

Database scripts and export assets for supported database engines.

- `script.sql` for SQL Server schema creation
- `MySql/` dump scripts for MySQL
- `SQLite/` database file and export helpers for SQLite
- `DM/` scripts for Dameng
- `Update/` incremental update scripts

## Runtime Architecture

### Request Flow

1. The ASP.NET Core host starts from `src/ZKEACMS.WebHost`.
2. The host loads application configuration from `appsettings.json` and environment-specific overrides if present.
3. Shared framework services are registered from `EasyFrameWork`.
4. CMS services from `ZKEACMS` are composed into the application.
5. Plugins contribute additional routes, widgets, content types, and behaviors.
6. The application serves CMS pages, management screens, and static assets.

### Layering

The solution follows a layered structure:

- Presentation and hosting: `ZKEACMS.WebHost`
- Application and domain logic: `ZKEACMS`
- Shared infrastructure: `EasyFrameWork`
- Extensibility: `src/Plugins/*`

This separation keeps infrastructure concerns isolated from product logic and makes feature delivery possible through plugins rather than direct core modification.

## Configuration

Primary application settings live in `src/ZKEACMS.WebHost/appsettings.json`.

Important settings include:

- `Database.DbType`: selects the database provider
- `Database.ConnectionString`: connection string for the active database
- `Serilog`: file-based logging configuration
- `CDN`: optional CDN integration
- `Culture.Code`: default UI culture

Supported database types are:

- `MsSql`
- `Sqlite`
- `MySql`
- `DM`

For SQLite deployments, the repository expects `App_Data/Database.sqlite` to exist, and `Database/SQLite/ZKEACMS.sqlite.sql` can be used to initialize a fresh database.

## Plugin Model

Plugins are first-class feature units in the system. The goal is to isolate functionality into independently maintainable projects that can be packaged, copied, or excluded without changing the core host.

Typical plugin responsibilities:

- Registering services and module metadata
- Exposing CMS widgets, pages, or blocks
- Adding admin UI or configuration screens
- Providing background tasks or integration logic

Development guidance:

- Keep a plugin focused on one feature area
- Prefer reusing `EasyFrameWork` and `ZKEACMS` services over duplicating infrastructure
- Use the `PluginTemplate` tool under `tools/` for new extensions

## Data Architecture

The system uses Entity Framework Core and supports multiple database engines through a shared abstraction layer.

Database assets are maintained separately from application code so deployments can target different environments:

- SQL Server is the default configuration in `appsettings.json`
- SQLite is supported for lightweight local or embedded scenarios
- MySQL and Dameng are supported through dedicated scripts and configuration options

The repository also includes update scripts under `Database/Update` for versioned schema migration.

## Frontend And Assets

The CMS uses a traditional asset pipeline with Gulp-backed style and script processing.

Notable frontend concepts:

- Responsive design across desktop and mobile sizes
- Layouts built on a Bootstrap 3 grid system
- Widgets as composable HTML components
- Theme customization through LESS variables

Build the frontend assets with `npx gulp` when validating CSS, LESS, or bundled JavaScript output.

## Build And Run

Recommended repository commands:

- `./Build.cmd` on Windows or `./Build.sh` on Linux/macOS for a full solution build
- `./Run.cmd` or `./Run.sh` to launch the web host in development mode
- `Publish.cmd` or `Publish.sh` for deployment packaging

For database initialization and schema generation, prefer the scripts under `Database/` rather than ad hoc schema creation.

## Testing

Test coverage is split by concern:

- Unit tests under `test/` using MSTest
- End-to-end tests under `test/End-To-End/` using Playwright
- Repository-specific test projects for framework, core CMS, and plugin areas

Prefer targeted tests for the area you change before running the full suite.

## Operational Notes

- The web host writes logs to `./Logs/log-.log` through Serilog.
- Runtime templates are packaged with the host as `Templates.zip`.
- Localization files are copied into the output so translations stay available at runtime.
- Keep package version changes centralized in `Directory.Packages.props`.

## Extending The System

When adding a new feature:

1. Put shared infrastructure changes in `EasyFrameWork` only if they are broadly reusable.
2. Put CMS domain changes in `ZKEACMS`.
3. Put runtime-facing integration in `ZKEACMS.WebHost` only when the host must change.
4. Put feature-specific behavior into a plugin project under `src/Plugins`.

This keeps the system modular and reduces coupling between the host and feature units.
