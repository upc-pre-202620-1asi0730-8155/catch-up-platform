# Architecture Decision Records (ADRs)

## About this document

This document records the significant architectural decisions of the CatchUp Platform REST API. Each record follows the lightweight format proposed by Michael Nygard: **Context**, **Decision**, **Consequences**, plus a **Status** and **Date**.

Guidelines for maintaining this log:

- One decision per record; records are short and focused on the *why*.
- Records are immutable once **Accepted**. To change a decision, add a new ADR that **Supersedes** the old one and update the old one's status.
- Statuses: `Proposed`, `Accepted`, `Deprecated`, `Superseded by ADR-XXX`.
- Number records sequentially and never reuse numbers.

## Index

| ADR | Title | Status |
|-----|-------|--------|
| [ADR-001](#adr-001--organize-the-code-by-bounded-context-with-layered-ddd-structure) | Organize the code by bounded context with a layered DDD structure | Accepted |
| [ADR-002](#adr-002--separate-command-and-query-services-cqs) | Separate command and query services (CQS) | Accepted |
| [ADR-003](#adr-003--model-expected-failures-with-a-result-type) | Model expected failures with a `Result` type | Accepted |
| [ADR-004](#adr-004--use-value-objects-for-newsapikey-and-sourceid) | Use value objects for `NewsApiKey` and `SourceId` | Accepted |
| [ADR-005](#adr-005--map-rest-resources-with-static-assemblers) | Map REST resources with static assemblers | Accepted |
| [ADR-006](#adr-006--persist-with-ef-core-on-mysql-using-repository-and-unit-of-work) | Persist with EF Core on MySQL using Repository and Unit of Work | Accepted |
| [ADR-007](#adr-007--use-snake_case-database-naming-and-automatic-audit-timestamps) | Use `snake_case` database naming and automatic audit timestamps | Accepted |
| [ADR-008](#adr-008--apply-database-migrations-on-startup) | Apply database migrations on startup | Accepted |
| [ADR-009](#adr-009--use-lowercase-kebab-case-routes) | Use lowercase kebab-case routes | Accepted |
| [ADR-010](#adr-010--report-errors-as-rfc-7807-problemdetails) | Report errors as RFC 7807 `ProblemDetails` | Accepted |
| [ADR-011](#adr-011--localize-user-facing-text-with-resx-resources) | Localize user-facing text with `.resx` resources | Accepted |
| [ADR-012](#adr-012--mask-news-api-keys-in-logs) | Mask News API keys in logs | Accepted |
| [ADR-013](#adr-013--use-built-in-openapi-and-scalar-instead-of-swashbuckle) | Use built-in OpenAPI and Scalar instead of Swashbuckle | Accepted |

---

## ADR-001 — Organize the code by bounded context with a layered DDD structure

- **Status:** Accepted
- **Date:** 2026-10-16

### Context
The API will grow beyond favorite sources. We need a structure that keeps domain rules independent from frameworks and prevents unrelated features from becoming entangled.

### Decision
Use a single deployable project organized first by bounded context (`News`, `Shared`) and then by layer:

- `Domain` — aggregates, value objects, commands, queries, repository contracts.
- `Application` — service contracts (`Services`), implementations (`Internal`), and application errors.
- `Infrastructure` — persistence and other technical adapters.
- `Interfaces` — REST controllers, resources, and assemblers.

Dependencies point inward: `Interfaces` and `Infrastructure` depend on `Application` and `Domain`, never the reverse. Cross-context technical concerns live in `Shared`.

### Consequences
- (+) Domain logic is testable without ASP.NET Core or EF Core.
- (+) New bounded contexts can be added as sibling folders.
- (−) More folders and types than a simple CRUD layout.
- (−) The dependency rule is enforced by convention only; a future split into separate projects or architecture tests could enforce it.

---

## ADR-002 — Separate command and query services (CQS)

- **Status:** Accepted
- **Date:** 2026-10-16

### Context
Writes (creating a favorite) have validation, uniqueness rules, and persistence side effects, while reads are simple lookups. Mixing both in one service hides these differences.

### Decision
Expose two application services per aggregate: `IFavoriteSourceCommandService` (handles commands such as `CreateFavoriteSourceCommand`) and `IFavoriteSourceQueryService` (handles queries such as `GetFavoriteSourceByIdQuery`). Commands and queries are immutable records; every handler accepts a `CancellationToken`.

### Consequences
- (+) Clear intent and smaller, focused classes.
- (+) Read and write paths can evolve independently (e.g., read models later).
- (−) Slightly more boilerplate per use case.
- This is CQS at the service level, not full CQRS: both sides share one database and model.

---

## ADR-003 — Model expected failures with a `Result` type

- **Status:** Accepted
- **Date:** 2026-10-16

### Context
Invalid keys, duplicate favorites, and similar outcomes are expected business results, not exceptional conditions. Using exceptions for them obscures control flow and is costly.

### Decision
Command handlers return `Result<TValue, TError>` (`Shared.Application.Patterns`) with an error enum such as `CreateFavoriteSourceError`. The REST layer translates the result into an HTTP response through `ActionResultFromCreateFavoriteSourceResultAssembler`. Exceptions are reserved for unexpected failures and are handled globally (see ADR-010).

### Consequences
- (+) Failure modes are explicit in the method signature and exhaustively mappable to HTTP status codes.
- (+) No exception-driven control flow for business rules.
- (−) Callers must handle both branches; a custom type is maintained instead of using a library.

---

## ADR-004 — Use value objects for `NewsApiKey` and `SourceId`

- **Status:** Accepted
- **Date:** 2026-10-16

### Context
Both identifiers are strings with validation rules. Passing raw strings around allows invalid values and parameter mix-ups (`newsApiKey` vs `sourceId`).

### Decision
Model them as immutable value objects (`NewsApiKey`, `SourceId`) that validate on construction. EF Core maps them to string columns with value converters. Raw strings are only accepted at the API boundary.

### Consequences
- (+) Invalid values cannot enter the domain; the compiler prevents argument mix-ups.
- (+) Behavior such as log masking (ADR-012) lives with the data it protects.
- (−) Requires value converters and mapping code.

---

## ADR-005 — Map REST resources with static assemblers

- **Status:** Accepted
- **Date:** 2026-10-16

### Context
Domain objects must not be exposed directly through the API, and controllers should stay thin.

### Decision
Use dedicated resources (`CreateFavoriteSourceResource`, `FavoriteSourceResource`) and static assembler classes in `Interfaces/REST/Transform`, named `<Target>From<Source>Assembler` (e.g. `FavoriteSourceResourceFromEntityAssembler`). The localized OpenAPI metadata assembler (`LocalizedOpenApiAssembler`) follows the same placement.

### Consequences
- (+) The public contract is decoupled from the domain model; controllers contain no mapping logic.
- (+) Pure static functions are trivial to unit test.
- (−) No automatic mapping; each new resource needs an assembler. Assemblers needing services (e.g. localizers) receive them as parameters.

---

## ADR-006 — Persist with EF Core on MySQL using Repository and Unit of Work

- **Status:** Accepted
- **Date:** 2026-10-16

### Context
The service needs relational persistence with a uniqueness constraint on `(news_api_key, source_id)` and a way to commit changes atomically per use case.

### Decision
Use Entity Framework Core with the MySQL provider (`MySql.EntityFrameworkCore`). Define repository interfaces in the domain (`IBaseRepository<T>`, `IFavoriteSourceRepository`), implement them in infrastructure, and commit through `IUnitOfWork.CompleteAsync`. Repositories do not save changes themselves. The connection string comes from configuration and supports environment variable expansion so secrets are not committed.

### Consequences
- (+) Domain and application code depend on abstractions, not EF Core.
- (+) A single commit point per use case.
- (+) The unique composite index backs the duplicate-favorite rule at the database level.
- (−) Switching databases requires changing the provider and re-validating migrations.

---

## ADR-007 — Use `snake_case` database naming and automatic audit timestamps

- **Status:** Accepted
- **Date:** 2026-10-16

### Context
MySQL conventions favor `snake_case`; C# favors `PascalCase`. Audit timestamps should not depend on each handler remembering to set them.

### Decision
Apply a custom `UseSnakeCaseNamingConvention` in `AppDbContext` for tables, columns, and indexes. Entities implementing `IAuditableEntity` receive `CreatedAt`/`UpdatedAt` automatically via `AuditableEntityInterceptor` on `SavingChanges`/`SavingChangesAsync`.

### Consequences
- (+) Idiomatic naming on both sides without per-property attributes.
- (+) Audit data is consistent and cannot be forgotten.
- (−) Custom convention code must be maintained; raw SQL must use the snake_case names.

---

## ADR-008 — Apply database migrations on startup

- **Status:** Accepted
- **Date:** 2026-10-16

### Context
The project is a learning/sandbox service that should run with minimal manual setup.

### Decision
Call `Database.Migrate()` when the application starts. It is idempotent when the schema is up to date.

### Consequences
- (+) Zero-step local and demo deployments.
- (−) Not recommended for multi-instance production: concurrent startups can race, and the application needs schema-altering privileges.
- Revisit (new ADR) before production use: run migrations as a separate deployment step or a migration bundle.

---

## ADR-009 — Use lowercase kebab-case routes

- **Status:** Accepted
- **Date:** 2026-10-16

### Context
Consistent, predictable URLs are part of the API contract (`/api/v1/favorite-sources`).

### Decision
Enable `LowercaseUrls` and register `KebabCaseRouteNamingConvention` so controller names such as `FavoriteSources` become `favorite-sources`. Versioning is expressed in the path (`v1`).

### Consequences
- (+) Uniform URLs without per-controller route boilerplate.
- (−) Renaming a controller changes its public URL; treat controller names as contract.

---

## ADR-010 — Report errors as RFC 7807 `ProblemDetails`

- **Status:** Accepted
- **Date:** 2026-10-16

### Context
Clients need a consistent, machine-readable error shape for validation errors and server failures, without leaking internals.

### Decision
Use `AddProblemDetails` with `UseExceptionHandler` for unexpected errors (HTTP 500), and a custom `InvalidModelStateResponseFactory` returning a validation `ProblemDetails` (`application/problem+json`, HTTP 400). Unexpected `Result` failures are mapped to `ProblemDetails` (HTTP 500) by the assembler, while duplicates return `409 Conflict` with a localized message. Titles and details are localized (ADR-011).

### Consequences
- (+) One error contract across the API, documented in the OpenAPI document via `[ProducesResponseType]`.
- (+) Exception details are not exposed to clients.
- (−) Handlers must not throw for expected conditions (see ADR-003).
- (−) The `409 Conflict` response is a plain localized message rather than `ProblemDetails`; aligning it is a possible follow-up.

---

## ADR-011 — Localize user-facing text with `.resx` resources

- **Status:** Accepted
- **Date:** 2026-10-16

### Context
Clients in different languages consume the API, and error messages and API documentation should follow the caller's language.

### Decision
Use ASP.NET Core localization with a shared marker class (`SharedResource`) and `.resx` files for the neutral, `en`, `en-US`, `es`, and `es-PE` cultures; `en` is the default. `UseRequestLocalization` selects the culture from the request (e.g., `Accept-Language`) and runs before OpenAPI mapping so generated documentation follows it. Data annotation messages use keys (`[Required(ErrorMessage = "RequiredNewsApiKey")]`) resolved from `SharedResource`. All resource files must keep the same set of keys.

Internal log messages and exception messages are intentionally **not** localized, to keep operational logs searchable and consistent.

### Consequences
- (+) Localized errors, validation messages, and API descriptions.
- (+) Adding a language is a new `.resx` file plus a supported-culture entry.
- (−) Every new user-facing string requires entries in all resource files.
- (−) The generated OpenAPI document varies by request culture, so caches must vary on `Accept-Language`.

---

## ADR-012 — Mask News API keys in logs

- **Status:** Accepted
- **Date:** 2026-10-16

### Context
`NewsApiKey` is a credential for a third-party service. Logging it verbatim would leak it to log sinks. Some log sites run before validation, so the value may be a raw string.

### Decision
Never log a News API key in clear text. Use `NewsApiKey.MaskedForLogging()` on validated instances and the static `NewsApiKey.MaskedForLogging(string?)` for raw input. This applies to all log statements in the command service and controller, and to any future ones. `EnableSensitiveDataLogging` for EF Core is limited to the Development environment.

### Consequences
- (+) Credentials do not reach logs while correlation by partial value remains possible.
- (−) Relies on developer discipline at each call site; a code review checklist item or analyzer could enforce it.

---

## ADR-013 — Use built-in OpenAPI and Scalar instead of Swashbuckle

- **Status:** Accepted
- **Date:** 2026-10-16

### Context
Swashbuckle was used to generate the API description and serve Swagger UI. ASP.NET Core now ships built-in OpenAPI document generation (`Microsoft.AspNetCore.OpenApi`), and Microsoft documents it as the supported path, while the UI is an independent choice (Swagger UI, ReDoc, Scalar, and others are all listed as options).

### Decision
Generate the document with `AddOpenApi`/`MapOpenApi` (served at `/openapi/v1.json`) and serve the API explorer with `Scalar.AspNetCore` (`/scalar`). Localized summaries, descriptions, and schema texts are applied through operation and schema transformers in `LocalizedOpenApiAssembler`. Response metadata uses `[ProducesResponseType]`. Swashbuckle packages and `Swagger*` attributes are removed.

The document and UI remain mapped in all environments to preserve previous behavior.

### Consequences
- (+) Fewer third-party dependencies for document generation; first-party support and updates.
- (+) Modern UI with request samples.
- (−) Transformer APIs differ from Swashbuckle filters; custom behavior was rewritten.
- (−) Exposing the explorer in production is a security and information-disclosure choice; restrict it to Development, or protect it, before a production release (new ADR).

---

## Template for new ADRs

```markdown
## ADR-XXX — Short, imperative title

- **Status:** Proposed | Accepted | Deprecated | Superseded by ADR-YYY
- **Date:** YYYY-MM-DD

### Context
Forces at play: requirements, constraints, and the problem being solved.

### Decision
What we decided, stated in active voice ("We will ...").

### Consequences
Positive and negative results, follow-ups, and when to revisit.
```
