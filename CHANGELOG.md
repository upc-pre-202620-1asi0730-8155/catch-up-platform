# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-10-26

### Added

- Domain-driven design implementation for News bounded context:
  - Aggregate: `FavoriteSource` with audit tracking support (`FavoriteSourceAudit`).
  - Value objects: `NewsApiKey` and `SourceId`.
  - Commands and Queries: `CreateFavoriteSourceCommand`, `GetFavoriteSourceByIdQuery`, `GetFavoriteSourceByNewsApiKeyAndSourceIdQuery`, and `GetAllFavoriteSourcesByNewsApiKeyQuery`.
  - Service contracts & implementations: `IFavoriteSourceCommandService`, `IFavoriteSourceQueryService`, `FavoriteSourceCommandService`, and `FavoriteSourceQueryService`.
  - Domain repository contracts & EF Core implementation: `IFavoriteSourceRepository` and `FavoriteSourceRepository`.
  - Error modeling: `ECreateFavoriteSourceError` enum for structured failure handling.
- REST API layer for Favorite Sources:
  - Controller `FavoriteSourcesController` exposing endpoints at `/api/v1/favorite-sources`.
  - Resources: `CreateFavoriteSourceResource` and `FavoriteSourceResource`.
  - Assemblers: `CreateFavoriteSourceCommandFromResourceAssembler`, `FavoriteSourceResourceFromEntityAssembler`, and `ActionResultFromCreateFavoriteSourceResultAssembler`.
  - Localized OpenAPI metadata generation via `LocalizedOpenApiAssembler` with Scalar interactive API documentation.
- Shared domain and infrastructure kernel:
  - `Result<TValue, TError>` pattern abstraction for railway-oriented result handling.
  - Auditing support via `IAuditableEntity` interface and EF Core `AuditableEntityInterceptor`.
  - Generic repository and unit of work abstractions (`IBaseRepository<TEntity>`, `IUnitOfWork`) and EF Core implementations (`BaseRepository<TEntity>`, `UnitOfWork`).
  - ASP.NET routing conventions with `KebabCaseRouteNamingConvention`.
  - EF Core database naming conventions supporting snake_case column names and pluralized table names.
  - Multilingual localization support (.NET `.resx`) with `SharedResource` strings for English (`en`, `en-US`) and Spanish (`es`, `es-PE`).
- Project configuration & documentation:
  - Application startup configuration with Entity Framework Core MySQL provider, automatic schema migrations, and RFC 7807 Problem Details error formatting.
  - Development and production configuration profiles supporting environment variables.
  - Architecture Decision Records (ADRs) in `docs/adrs.md`.
  - PlantUML class diagram in `docs/class-diagram.puml`.
  - Technical user stories in `docs/user-stories.md`.
