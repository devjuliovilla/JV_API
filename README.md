# JV API

Production-oriented .NET 10 Web API boilerplate built with Minimal APIs, CQRS, MediatR, FluentValidation, Entity Framework Core, and a vertical-slice application layer.

## Stack

- .NET 10
- ASP.NET Core Minimal APIs
- Entity Framework Core 10 + SQL Server
- MediatR
- FluentValidation
- Mapster
- Sieve
- JWT Bearer authentication
- Serilog
- MailKit
- xUnit + EF Core InMemory

## Solution layout

```text
WebApi.slnx
src/
  Application/
  Domain/
  Infrastructure/
  Tests/
  WebApi/
```

## Architecture

The solution is organized into four runtime layers plus tests.

### Domain

`src/Domain` contains the core model:

- entities such as `User`, `Product`, `Category`, `Role`, and `RefreshToken`
- the shared base entity `EntityBase`
- service ports such as `IJwtService`, `IEmailService`, `IFileStorageService`, `ICurrentUserService`, and `ILogCleanupService`
- configuration option types such as `JwtOptions`, `SmtpOptions`, `FileStorageOptions`, and `LogCleanupOptions`

This project contains business model types and abstractions only.

### Application

`src/Application` contains use-case orchestration.

It includes:

- commands and queries
- handlers
- validators
- application exceptions
- request/response DTOs co-located with each feature slice
- shared application models such as paged responses
- `IAppDbContext`, the persistence abstraction consumed by handlers

Feature folders are grouped by business area:

```text
Application/
  Features/
    Auth/
      Login/
      Register/
      Refresh/
      Revoke/
      Me/
    Products/
      Common/
      Create/
      Update/
      Delete/
      Get/
      List/
```

Each slice owns the types that change with that use case. For example:

```text
Application/Features/Products/Create/
  CreateProductCommand.cs
  CreateProductHandler.cs
  CreateProductRequest.cs
  CreateProductResponse.cs
  CreateProductValidator.cs
```

### How Application works

The request flow is:

1. An HTTP endpoint in `WebApi` receives the request.
2. The endpoint builds a command or query and sends it through `ISender`.
3. MediatR resolves the handler from `Application`.
4. `ValidationBehavior` runs all FluentValidation validators for that request type.
5. The handler executes the use case using `IAppDbContext` and domain service ports.
6. The handler returns an application response DTO.
7. `WebApi` writes the HTTP response.

This keeps HTTP concerns in `WebApi`, use-case orchestration in `Application`, and infrastructure details behind abstractions.

### Persistence abstraction

Handlers do not depend on the EF Core `AppDbContext` directly. They depend on:

```csharp
public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
```

`Infrastructure` implements that contract through `AppDbContext` and registers it in DI.

### Infrastructure

`src/Infrastructure` contains adapters and framework integration:

- EF Core `AppDbContext`
- entity configurations
- migrations
- database seed
- interceptors for auditing and soft delete
- JWT generation
- SMTP email sending
- file storage
- log cleanup service and background worker
- dependency injection and database initialization extensions

Infrastructure is the place where external libraries and runtime concerns are wired together.

### WebApi

`src/WebApi` is the delivery layer.

It contains:

- Minimal API endpoints
- endpoint grouping
- middleware
- Swagger configuration
- service registration for API-specific concerns
- `Program.cs`

The endpoints are thin. Their job is to translate HTTP input into application requests and return HTTP responses.

### Tests

`src/Tests` contains the current unit and lightweight integration-style tests using EF Core InMemory.

## Dependency direction

```text
Domain <- Application <- WebApi
          ^              |
          |              |
          +-- Infrastructure
```

In practice:

- `Application` depends on `Domain`
- `Infrastructure` depends on `Application` and `Domain`
- `WebApi` depends on `Application` and `Infrastructure`

`WebApi` uses `Infrastructure` only as the composition root and runtime host.

## Request pipeline

For a typical feature such as product creation:

1. `POST /api/v1/products`
2. `CreateProductEndpoint` receives `CreateProductRequest`
3. The endpoint sends `CreateProductCommand`
4. `CreateProductValidator` validates the MediatR command
5. `CreateProductHandler` uses `IAppDbContext`
6. The handler persists the entity and returns `CreateProductResponse`
7. The endpoint returns `201 Created`

## Features

### Auth

Base route: `/api/v1/auth`

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/login` | No | Authenticate and receive access/refresh tokens |
| POST | `/register` | No | Register a user |
| POST | `/refresh` | No | Rotate refresh token and issue a new access token |
| POST | `/revoke` | Yes | Revoke a refresh token |
| GET | `/me` | Yes | Return the current authenticated user |

### Products

Base route: `/api/v1/products`

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/` | Yes | Create a product |
| PUT | `/{id}` | Yes | Update a product |
| DELETE | `/{id}` | Yes | Soft-delete a product |
| GET | `/{id}` | No | Get a single product |
| GET | `/` | No | List products with pagination, filtering, and sorting |

### Logs

Base route: `/api/v1/logs`

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/test` | No | Write a test log entry through Serilog |

### Health

| Method | Route | Description |
|---|---|---|
| GET | `/health` | Run configured health checks |

## Entities and database schemas

| Schema | Table | Purpose |
|---|---|---|
| `sec` | `Users` | Application users |
| `sec` | `Roles` | Authorization roles |
| `sec` | `UserRoles` | User-role join table |
| `sec` | `RefreshTokens` | Refresh token storage |
| `dbo` | `Products` | Product catalog sample |
| `cat` | `Categories` | Product categories |
| `audit` | `Logs` | Serilog SQL sink table |

All entities derive from `EntityBase`:

- `Id`
- `CreatedAt`
- `CreatedBy`
- `UpdatedAt`
- `UpdatedBy`
- `Deleted`
- `DeletedAt`
- `DeletedBy`

## Auditing and soft delete

These behaviors are implemented with EF Core interceptors.

### `AuditInterceptor`

- stamps `CreatedAt` and `CreatedBy` on inserts
- stamps `UpdatedAt` and `UpdatedBy` on updates
- uses `ICurrentUserService` to resolve the current user
- falls back to `SYSTEM` when no authenticated user is available

### `SoftDeleteInterceptor`

- converts deletes into updates
- sets `Deleted = true`
- sets `DeletedAt`
- sets `DeletedBy = "system"`

## Exceptions and HTTP mapping

Application exceptions are translated in `ExceptionHandlingMiddleware`.

| Exception | HTTP status |
|---|---|
| `ValidationException` | 400 |
| `UnauthorizedException` | 401 |
| `ForbiddenException` | 403 |
| `NotFoundException` | 404 |
| `ConflictException` | 409 |

## Authentication

The API uses JWT Bearer authentication.

- access tokens are generated by `IJwtService`
- passwords are hashed with `PasswordHasher<User>`
- refresh tokens are stored in the database
- refresh flow rotates tokens and supports revocation

Configuration lives in `src/WebApi/appsettings.json`:

```json
"Jwt": {
  "SecretKey": "CHANGE-ME!!!",
  "Issuer": "WebApi",
  "Audience": "WebApi",
  "ExpirationMinutes": 60,
  "RefreshTokenExpirationDays": 1
}
```

JWT options are validated on startup. The application fails fast when the secret key is missing, still set to `CHANGE-ME!!!`, shorter than 32 bytes, or when issuer/audience are missing.

## Database initialization

Database initialization is controlled by the `Database` configuration section:

```json
"Database": {
  "ApplyMigrations": true,
  "SeedOnStartup": true
}
```

When `ApplyMigrations` is enabled, EF Core migrations are applied on startup. This is intended for local/dev and simple deployments where the application owns schema updates.

When `SeedOnStartup` is enabled, seed data is inserted idempotently. Existing roles, users, categories, and sample products are not duplicated.

## Logging

Serilog is configured in `Program.cs`.

It writes to:

- console
- SQL Server through `Serilog.Sinks.MSSqlServer`

The SQL sink writes to `[audit].[Logs]` and can auto-create the table.

The API also includes `LogCleanupBackgroundService`, which periodically deletes old log records according to `LogCleanup.RetentionDays`.

## Included services

### Email

`IEmailService` is implemented with MailKit.

### File storage

`IFileStorageService` is implemented with local filesystem storage under `App_Data/Storage`.

### Current user

`ICurrentUserService` reads the authenticated user from `HttpContext` and is used by application and infrastructure components that need user context.

## Configuration

Main runtime configuration is in `src/WebApi/appsettings.json`.

Available sections:

- `ConnectionStrings`
- `Database`
- `Jwt`
- `Serilog`
- `Smtp`
- `FileStorage`
- `LogCleanup`
- `RateLimiting`
- `Cors`
- `Sieve`
- `Swagger`

Development-specific values can be overridden in `src/WebApi/appsettings.Development.json`.

Visual Studio and `dotnet run` use `appsettings.json`, `appsettings.Development.json`, user secrets, environment variables, and `launchSettings.json` as usual. Docker-specific `.env` files are not required for local Visual Studio runs.

## Local development

Restore packages:

```bash
dotnet restore WebApi.slnx
```

Build the solution:

```bash
dotnet build WebApi.slnx
```

Run tests:

```bash
dotnet test src/Tests/Tests.csproj
```

Run the API:

```bash
dotnet run --project src/WebApi/WebApi.csproj
```

When enabled through configuration, startup performs:

1. database migration
2. idempotent seed data initialization

In development, Swagger UI is also exposed.

## Docker

Docker Compose reads environment values from a local `.env` file. Start by copying the example file:

```bash
cp .env.example .env
```

On Windows PowerShell:

```powershell
Copy-Item .env.example .env
```

Then adjust values as needed and run:

```bash
docker compose up --build
```

The `.env` file is intentionally ignored by git. Use `.env.example` as the template for new projects.

Important Docker variables:

- `ASPNETCORE_ENVIRONMENT`
- `SQLSERVER_SA_PASSWORD`
- `DB_NAME`
- `JWT_SECRET_KEY`
- `JWT_ISSUER`
- `JWT_AUDIENCE`
- `DATABASE_APPLY_MIGRATIONS`
- `DATABASE_SEED_ON_STARTUP`

### Seed user

| Username | Password | Role |
|---|---|---|
| `admin` | `Admin123!` | `Admin` |

## Migrations

Create a migration:

```bash
dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/WebApi
```

Apply migrations:

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/WebApi
```

## Adding a new feature

For a new use case:

1. Create a folder under `src/Application/Features/<Area>/<UseCase>/`
2. Add request/response DTOs if the slice owns them
3. Add the command or query
4. Add the handler
5. Add the validator if needed
6. Add the HTTP endpoint under `src/WebApi/Endpoints/...`
7. Register that endpoint in the corresponding endpoint group

The endpoint should stay thin and delegate business flow to `Application`.

## Adding a new entity

1. Add the entity to `src/Domain/Entities/`
2. Add EF configuration in `src/Infrastructure/Persistence/Configurations/`
3. Expose a `DbSet<T>` in `AppDbContext`
4. If needed by handlers, expose it through `IAppDbContext`
5. Add a migration

## Tests

Current test coverage includes:

- auth handlers for register, login, refresh, revoke, and current user
- product handlers for create, get, update, and delete
- MediatR validation behavior
- product persistence and mapping tests
- soft-delete query filter checks
- log cleanup service tests

Tests follow Arrange/Act/Assert comments for readability.

Run them with:

```bash
dotnet test src/Tests/Tests.csproj
```
