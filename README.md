# Catch-Up Platform (catch-up-platform)

## Overview

Catch-Up Platform is a small ASP.NET Core service that provides an API to manage users' favorite news sources. The project demonstrates a clean package structure separated into bounded contexts (news and shared) and follows simple command/query service patterns with Entity Framework Core persistence.

## Domain-Driven Design approach

The codebase applies a lightweight Domain-Driven Design style to keep the favorite-sources feature easy to evolve:

- **Bounded contexts**
    - `News`: owns the favorite-source use cases, aggregate, commands, queries, and API contracts.
    - `Shared`: owns cross-cutting abstractions used by multiple contexts, such as repository contracts, unit of work, auditing, and reusable application patterns.
- **Domain layer**
    - Contains the core business concepts: `FavoriteSource`, `NewsApiKey`, `SourceId`, and command/query records.
    - Keeps business rules close to the model, including uniqueness expectations and value-object validation.
- **Application layer**
    - Coordinates use cases through command/query services.
    - Returns explicit outcomes with a shared `Result<TValue, TError>` pattern for command handling.
    - Defines application-specific error types such as `CreateFavoriteSourceError`.
- **Interfaces layer**
    - Exposes the REST API through controllers and ASP.NET Core OpenAPI metadata.
    - Uses assemblers to translate request resources into commands and application results into HTTP `ActionResult` responses.
- **Infrastructure layer**
    - Persists aggregates with Entity Framework Core and MySQL.
    - Maps value objects to database columns, applies migrations on startup, and manages audit timestamps through a save-changes interceptor.

This structure keeps HTTP concerns, persistence concerns, and business rules separated while still allowing the API to stay simple to consume.

## Features

- API base route: `/api/v1/favorite-sources` (kebab-case route naming convention)
- List favorite sources by `newsApiKey`
- Retrieve a favorite source by `id`
- Retrieve a favorite source by `newsApiKey` + `sourceId`
- Create (persist) a new favorite source
- Duplicate detection enforced at application and database level (returns `409 Conflict` for known duplicate-pair violations)
- Localized validation responses (`400 Bad Request`)
- Unexpected create errors returned as `ProblemDetails` (`500 Internal Server Error`)
- Entity Framework naming strategy for `snake_case` identifiers and plural table names
- Structured logging for diagnostics and observability

## Technologies

- C# 14 and .NET 10 (`net10.0`)
- ASP.NET Core Web API (REST controllers + built-in OpenAPI generation)
- Scalar.AspNetCore for interactive API documentation
- Entity Framework Core 10 + MySQL provider (`MySql.EntityFrameworkCore`)
- Localization with `.resx` resources (`en`, `en-US`, `es`, `es-PE`)
- Structured logging with `ILogger<T>`
- PlantUML (architecture diagrams in `docs/`)

## Technical stories

The API-focused technical stories for frontend integration are in [`docs/user-stories.md`](docs/user-stories.md).

## Class diagram

A PlantUML class diagram that reflects the code structure and bounded contexts is available at [`docs/class-diagram.puml`](docs/class-diagram.puml).

## Getting started (quick)

### Prerequisites

- .NET SDK 10
- A reachable MySQL instance

### 1) Restore dependencies

```bash
dotnet restore catch-up-platform.sln
```

### 2) Configure connection string

By default, development uses `CatchUpPlatform.API/appsettings.Development.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "server=localhost;user=root;password=password;database=catch-up-wa"
}
```

### 3) Run the API

```bash
dotnet run --project CatchUpPlatform.API
```

Or from the API folder:

```bash
cd CatchUpPlatform.API
dotnet run
```

### Database initialization

The application automatically applies pending EF Core migrations on startup via `Database.Migrate()`.

- **Development**: Applies the current schema to the configured MySQL database.
- **First-time setup**: Ensure MySQL is running and the connection string points to an existing (empty or non-empty) database.
- **Constraints**: A unique composite index is enforced on `(NewsApiKey, SourceId)` to prevent duplicate favorites.
- **Schema changes**: Add and commit EF Core migrations when the data model changes.

### 4) Explore the API with Scalar

With the included launch profiles, Scalar is commonly available at:

- `http://localhost:5128/scalar`
- `https://localhost:7234/scalar`

The generated OpenAPI document is available at `/openapi/v1.json`.

### Production-style configuration

`CatchUpPlatform.API/appsettings.Production.json` supports environment variable expansion:

- `%DATABASE_URL%`
- `%DATABASE_USER%`
- `%DATABASE_PASSWORD%`
- `%DATABASE_SCHEMA%`

Set `ASPNETCORE_ENVIRONMENT=Production` before running with production settings.

## Error responses

The API returns standardized error responses:

- **400 Bad Request**: Request validation failed and returns a localized error message.
- **404 Not Found**: Resource not found.
- **409 Conflict**: Duplicate `newsApiKey` + `sourceId` pair for known duplicate-pair violations (detected before or during persistence).
- **500 Internal Server Error**: Unexpected server error (RFC7807 `ProblemDetails` format).

## Build and verify

```bash
dotnet build catch-up-platform.sln
```
