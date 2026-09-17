# AGENTS.md

Instructions for any AI coding agent (or human) working in this repository. This file is
self-contained — do not assume any other spec/notes file exists in a fresh clone.

## What this is

JV API is a .NET 10 Web API boilerplate: Minimal APIs, vertical-slice CQRS handlers,
FluentValidation, EF Core, JWT auth, Serilog, Docker. `main` is the only branch — clone it
directly, there is no separate "starter" branch to switch to.

## Architecture

```
Domain <- Application <- WebApi
          ^              |
          +-- Infrastructure
```

- **`src/Domain`** — entities (`User`, `Role`, `UserRole`, `RefreshToken`, `Category`, `Product`),
  the `EntityBase` audit/soft-delete base class, service ports (`IJwtService`, `IEmailService`,
  `IFileStorageService`, `ICurrentUserService`, `ILogCleanupService`), option types (`JwtOptions`,
  `SmtpOptions`, `FileStorageOptions`, `LogCleanupOptions`). No external dependencies.
- **`src/Application`** — CQRS use cases under `Features/<Area>/<UseCase>/`, the `IAppDbContext`
  abstraction, `ValidationBehavior<TRequest,TResponse>`, shared DTOs, application exceptions.
  Depends only on `Domain`.
- **`src/Infrastructure`** — `AppDbContext`, EF entity configurations, migrations, seed data,
  audit/soft-delete interceptors, concrete service implementations, DI wiring
  (`AddInfrastructure`), `InitializeDatabaseAsync`.
- **`src/WebApi`** — composition root: `Program.cs`, Minimal API endpoints, route/name/tag
  constants, exception-handling middleware, Swagger/CORS/rate-limiting wiring. Only project
  allowed to reference `Infrastructure` directly (it's the composition root).
- **`src/Tests`** — single xUnit project, folder-per-area, EF Core InMemory + hand-written fakes
  (`FakeJwtService`, `FakeCurrentUserService` in `TestBase.cs`). No mocking library is used —
  don't add one for a single test; follow the existing fake pattern instead.

`src/Shared` does not exist — do not recreate it without a concrete reason; an earlier empty
scaffold for it was removed because nothing used it.

## Adding a new feature — copy the Products "Create" slice

`Products` (with `Categories` as its parent lookup) is the canonical, permanent reference
feature. Do not treat it as disposable sample code — extend it or copy its shape for new
features. To add a new use case, touch files in this order (illustrated with `Create`):

1. `src/Domain/Entities/Product.cs` — entity extends `EntityBase`.
2. `src/Infrastructure/Persistence/Configurations/ProductConfiguration.cs` — `IEntityTypeConfiguration<T>`, FK, `HasQueryFilter(x => !x.Deleted)`.
3. Add the `DbSet<T>` to both `Infrastructure/Persistence/AppDbContext.cs` and
   `Application/Abstractions/Persistence/IAppDbContext.cs` (handlers only see the interface).
4. `src/Application/Features/Products/Common/ProductResponse.cs` — shared response used by
   Get/List/Update, annotated with `[Sieve(CanFilter=..., CanSort=...)]` for querying.
5. `src/Application/Features/Products/Create/CreateProductRequest.cs` — plain HTTP-facing DTO.
6. `.../Create/CreateProductCommand.cs` — `record ... : IRequest<CreateProductResponse>`.
7. `.../Create/CreateProductResponse.cs` — command-specific response (narrower than the shared
   `ProductResponse` — see "Response DTO rule" below).
8. `.../Create/CreateProductValidator.cs` — `AbstractValidator<CreateProductCommand>`.
9. `.../Create/CreateProductHandler.cs` — `IRequestHandler<TCommand, TResponse>`, uses
   `IAppDbContext` + Mapster `.Adapt<T>()`/`.ProjectToType<T>()`. Never inject the concrete
   `AppDbContext`.
10. Route constants in `src/WebApi/Constants/ProductRoutes.cs` (reuse `ApiGroups`,
    `EndpointNames`, `EndpointDescriptions`, `EndpointTags` for the area).
11. `src/WebApi/Endpoints/Abstractions/ProductsEndpoint.cs` — one abstract base per area, fixes
    `Group`/`Tag`.
12. `src/WebApi/Endpoints/Products/CreateProductEndpoint.cs` — concrete `IEndpoint`, maps the
    route, builds the command, sends via `ISender`, `.RequireAuthorization()` or
    `.AllowAnonymous()`.
13. Test under `src/Tests/Products/` — see "Test pattern" below.
14. Optional: an `.rest` file under `http/products/` for manual smoke testing.

**No manual registration step exists anywhere** — both MediatR handlers/validators and
`IEndpoint` implementations are discovered by assembly reflection:
- `WebApi/Extensions/ApiServicesExtensions.cs` → `AddMediatR(...RegisterServicesFromAssembly...)`
  + `AddValidatorsFromAssembly`.
- `WebApi/Endpoints/EndpointRegistration.cs` (`AddEndpoints`) and
  `WebApi/Extensions/EndpointExtensions.cs` (`MapEndpoints`) both scan for non-abstract
  `IEndpoint` implementers and wire them up. Dropping a new `IEndpoint` class into
  `WebApi/Endpoints/<Area>/` is enough — nothing in `Program.cs` needs to change.

## Conventions

- **Naming**: file name matches class name; suffixes `*Command`/`*Query`, `*Handler`,
  `*Request`, `*Response`, `*Validator`, `*Endpoint`.
- **Validators**: required for commands that carry a body (Create/Update). Parameterless
  queries and ID-only deletes (`GetProductQuery`, `GetProductsQuery`, `DeleteProductCommand`)
  intentionally have none — there's nothing to validate.
- **Response DTO rule**: `Create` returns a narrow response with only what the client needs
  immediately (e.g. `Id`, `Name`). `Get`/`List`/`Update` share the richer `Common/<Entity>Response.cs`,
  which carries Sieve filter/sort attributes for querying. This is intentional, not an
  inconsistency — don't "fix" it by unifying them.
- **No Repository/Unit-of-Work pattern, no AutoMapper, no ASP.NET Identity.** Handlers use
  `IAppDbContext` directly; mapping uses Mapster (`.Adapt<T>()`, `.ProjectToType<T>()`); auth
  uses custom `sec.Users/Roles/UserRoles/RefreshTokens` tables with `PasswordHasher<T>`, not
  Identity.
- **Auth seam**: `.RequireAuthorization()` vs `.AllowAnonymous()` directly on the endpoint's
  route builder. No role/policy-based authorization exists yet — it's an intentional, documented
  gap for consuming projects to fill in.
- **Persisted timestamps use `DateTimeOffset`, not `DateTime`.** `EntityBase.CreatedAt/UpdatedAt/DeletedAt`,
  `RefreshToken.ExpiresAt/RevokedAt`, and any DTO field that crosses the API boundary (e.g.
  `LoginResponse.ExpiresAt`) are `DateTimeOffset`, mapped to SQL Server's `datetimeoffset`. This
  makes the UTC offset explicit in both storage and JSON instead of relying on every caller
  knowing that a "naive" `DateTime` is always UTC. Use `DateTimeOffset.UtcNow` when setting these;
  only convert to `DateTime` at a framework boundary that requires it (e.g.
  `SecurityTokenDescriptor.Expires` via `.UtcDateTime` in `JwtService`). If you add a new entity,
  give any timestamp property this same type — don't reintroduce `DateTime`. The Serilog
  `audit.Logs` table is the one exception: it's owned by the Serilog MSSqlServer sink, not EF, so
  `LogCleanupService`/`ILogCleanupService` keep `DateTime` for that comparison.

## Test pattern

Standard: instantiate the real handler directly against `TestBase`'s EF Core InMemory
`AppDbContext`, using the fakes in `TestBase.cs` (`FakeJwtService`, `FakeCurrentUserService`)
for external dependencies. See `src/Tests/Products/CreateProductHandlerTests.cs` as the
template — call the handler, assert on both the returned response and the DB state. Don't
duplicate coverage by re-implementing the same assertions by hand against the DbContext outside
the handler.

## Commands

```bash
dotnet restore WebApi.slnx
dotnet build WebApi.slnx
dotnet test src/Tests/Tests.csproj
dotnet run --project src/WebApi/WebApi.csproj

# migrations
dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/WebApi
dotnet ef database update --project src/Infrastructure --startup-project src/WebApi
```

## Removing the Products/Categories sample

Products/Categories ships permanently in every clone as the reference feature, but if a
project genuinely doesn't need a catalog, remove it with this checklist:

1. Delete `src/Domain/Entities/Product.cs` and `Category.cs`.
2. Delete `src/Infrastructure/Persistence/Configurations/ProductConfiguration.cs` and
   `CategoryConfiguration.cs`.
3. Remove the `Products`/`Categories` `DbSet<T>` properties from `AppDbContext.cs` and
   `IAppDbContext.cs`.
4. Delete `src/Application/Features/Products/` entirely.
5. Delete `src/WebApi/Endpoints/Products/`, `src/WebApi/Endpoints/Abstractions/ProductsEndpoint.cs`,
   and `src/WebApi/Constants/ProductRoutes.cs`; remove the `Products` entry from `ApiGroups.cs`,
   `EndpointNames.cs`, `EndpointDescriptions.cs`, `EndpointTags.cs`.
6. Delete `src/Tests/Products/`.
7. Delete `http/products/`.
8. Remove the category/product seeding logic (`EnsureCategoriesAsync`/`EnsureProductsAsync`
   and their call sites) from `src/Infrastructure/Seed/SeedData.cs`.
9. Delete all tracked migrations under `src/Infrastructure/Migrations/` (both
   `InitialCreate` and `UseDateTimeOffsetForTimestamps` touch Products/Categories columns) and
   generate a fresh `InitialCreate` migration for your project's actual schema — don't try to
   hand-edit the existing migration files.

## Production hardening checklist

Before using this as a real public API: replace seed credentials, define authorization
policies/roles, decide whether public registration stays enabled, set real CORS origins and
JWT secret, move secrets to a secret manager, review refresh-token storage/revocation, add
persistent file storage if local disk isn't enough, review log retention, add integration
tests around HTTP endpoints.
