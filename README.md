# JV API

JV API is a pragmatic .NET 10 Web API boilerplate for starting new API projects with the pieces that are usually needed from day one: Minimal APIs, vertical slices, CQRS-style handlers, validation, authentication, persistence, logging, Docker, seed data, and tests.

This repository is a starting point, not a finished production product. Security policies, permissions, domain rules, deployment hardening, and project-specific modules are expected to be adjusted by the developer using the template.

## Stack

- .NET 10
- ASP.NET Core Minimal APIs
- Entity Framework Core 10
- SQL Server
- MediatR
- FluentValidation
- Mapster
- Sieve
- JWT Bearer authentication
- Serilog
- Serilog SQL Server sink
- MailKit
- Docker Compose
- xUnit
- EF Core InMemory for tests

## Solution Layout

```text
WebApi.slnx
Dockerfile
docker-compose.yml
.env.example
src/
  Domain/
  Application/
  Infrastructure/
  WebApi/
  Tests/
```

## Architecture

The solution uses a lightweight Clean Architecture / Vertical Slice style.

```text
Domain <- Application <- WebApi
          ^              |
          |              |
          +-- Infrastructure
```

In practice:

- `Domain` contains entities, shared domain abstractions, and option types used by infrastructure services.
- `Application` contains use cases: commands, queries, handlers, validators, DTOs, application exceptions, and persistence abstractions.
- `Infrastructure` contains framework and external-service implementations: EF Core, SQL Server, seed data, JWT, SMTP, file storage, logging cleanup, and dependency injection.
- `WebApi` contains the HTTP delivery layer: Minimal API endpoints, middleware, Swagger, CORS, rate limiting, and application startup.
- `Tests` contains handler, behavior, mapping, persistence, and infrastructure tests.

`WebApi` references `Infrastructure` because it is the composition root. Application handlers depend on abstractions instead of concrete infrastructure classes.

## Layer Responsibilities

### Domain

Path: `src/Domain`

Contains:

- entities: `User`, `Role`, `UserRole`, `RefreshToken`
- shared base class: `EntityBase`
- service ports: `IJwtService`, `IEmailService`, `IFileStorageService`, `ICurrentUserService`, `ILogCleanupService`
- option models: `JwtOptions`, `SmtpOptions`, `FileStorageOptions`, `LogCleanupOptions`

### Application

Path: `src/Application`

Contains:

- feature folders grouped by business area
- MediatR commands and queries
- handlers
- FluentValidation validators
- request and response DTOs owned by each slice
- shared DTOs such as `PagedResponseDto<T>`
- application exceptions
- `ValidationBehavior<TRequest, TResponse>`
- `IAppDbContext`

Current feature layout:

```text
Application/Features/
  Auth/
    Login/
    Register/
    Refresh/
    Revoke/
    Me/
```

### Infrastructure

Path: `src/Infrastructure`

Contains:

- EF Core `AppDbContext`
- entity configurations
- seed data
- audit and soft-delete interceptors
- JWT generation and token validation helpers
- SMTP email service
- local file storage service
- current-user service backed by `IHttpContextAccessor`
- log cleanup service and background worker
- health check registration
- infrastructure dependency injection
- database initialization extension

### WebApi

Path: `src/WebApi`

Contains:

- `Program.cs`
- Minimal API endpoints
- endpoint discovery and grouping
- route, tag, name, and description constants
- exception handling middleware
- Swagger configuration
- CORS configuration
- rate limiting configuration
- request pipeline configuration
- health endpoint

## Request Flow

For a typical request such as login:

1. `POST /api/v1/auth/login` reaches `LoginEndpoint`.
2. The endpoint receives `LoginRequest`.
3. The endpoint creates `LoginCommand`.
4. The endpoint sends the command with MediatR `ISender`.
5. `ValidationBehavior` runs `LoginValidator`.
6. `LoginHandler` executes the use case through `IAppDbContext` and domain services.
7. EF Core reads persisted data through `AppDbContext`.
8. The handler returns `LoginResponse`.
9. The endpoint returns `200 OK`.

The endpoint stays thin. Application code owns use-case orchestration. Infrastructure owns external details.

## Endpoint Pattern

Endpoints implement `IEndpoint`:

```csharp
public interface IEndpoint
{
    string Group { get; }
    string Tag { get; }
    void Map(RouteGroupBuilder group);
}
```

At startup:

- `AddEndpoints()` scans the WebApi assembly and registers endpoint classes.
- `MapEndpoints()` scans the same assembly, creates route groups, and calls each endpoint's `Map` method.

Current endpoint base classes:

- `AuthEndpoint`

Logging is implemented through Serilog and SQL cleanup.

## API Endpoints

### Auth

Base route: `/api/v1/auth`

| Method | Route | Auth | Handler |
|---|---|---|---|
| `POST` | `/login` | No | `LoginHandler` |
| `POST` | `/register` | No | `RegisterHandler` |
| `POST` | `/refresh` | No | `RefreshTokenHandler` |
| `POST` | `/revoke` | Yes | `RevokeTokenHandler` |
| `GET` | `/me` | Yes | `GetCurrentUserHandler` |

### Health

| Method | Route | Description |
|---|---|---|
| `GET` | `/health` | Runs configured health checks, including the database check. |

## Authentication

The boilerplate includes JWT Bearer authentication.

Included behavior:

- users are stored in SQL Server
- passwords are hashed with `PasswordHasher<User>`
- login returns access and refresh tokens
- refresh tokens are persisted and rotated
- refresh tokens can be revoked
- JWT issuer, audience, lifetime, and signing key are configured through `Jwt`
- JWT options are validated on startup

Default configuration in `src/WebApi/appsettings.json`:

```json
"Jwt": {
  "SecretKey": "CHANGE-ME!!!",
  "Issuer": "WebApi",
  "Audience": "WebApi",
  "ExpirationMinutes": 60,
  "RefreshTokenExpirationDays": 1
}
```

Development and Docker override the secret through `appsettings.Development.json` or environment variables.

This is intentionally basic for a boilerplate. Project-specific authorization policies, permissions, claims, roles, MFA, account confirmation, password reset, and token storage hardening should be added by the consuming project when needed.

## Authorization

Current authorization is intentionally simple:

- public endpoints use `.AllowAnonymous()`
- protected endpoints use `.RequireAuthorization()`
- role/policy-based authorization is not implemented yet

This gives the template a clear default while keeping the permission model open for each project.

## Validation

FluentValidation validators live next to the use case they validate.

Examples:

- `LoginValidator`
- `RegisterValidator`

`ValidationBehavior<TRequest, TResponse>` runs all validators registered for the current MediatR request. Validation failures are converted to the custom `Application.Exceptions.ValidationException` and then mapped to HTTP `400` by `ExceptionHandlingMiddleware`.

## Exception Handling

`ExceptionHandlingMiddleware` translates application exceptions into JSON error responses.

| Exception | HTTP Status |
|---|---|
| `ValidationException` | `400 Bad Request` |
| `UnauthorizedException` | `401 Unauthorized` |
| `ForbiddenException` | `403 Forbidden` |
| `NotFoundException` | `404 Not Found` |
| `ConflictException` | `409 Conflict` |
| Any other exception | `500 Internal Server Error` |

The shared error shape is `ApiErrorResponseDto`.

## Persistence

`Application` handlers use `IAppDbContext` instead of depending directly on `Infrastructure.Persistence.AppDbContext`.

Current abstraction:

```csharp
public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
```

`Infrastructure` implements this with `AppDbContext` and registers it in dependency injection.

Current database schemas:

| Schema | Table | Purpose |
|---|---|---|
| `sec` | `Users` | Application users |
| `sec` | `Roles` | Authorization roles |
| `sec` | `UserRoles` | User-role join table |
| `sec` | `RefreshTokens` | Refresh token storage |
| `audit` | `Logs` | Serilog SQL sink table |

## Auditing And Soft Delete

All entities inherit from `EntityBase`:

```csharp
public abstract class EntityBase
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool Deleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
```

`AuditInterceptor`:

- sets `CreatedAt` and `CreatedBy` for added entities
- sets `UpdatedAt` and `UpdatedBy` for modified entities
- uses `ICurrentUserService`
- falls back to `SYSTEM` when there is no authenticated user

`SoftDeleteInterceptor`:

- converts EF delete operations into updates
- sets `Deleted = true`
- sets `DeletedAt`
- sets `DeletedBy = "system"`

Entity configurations use query filters like `HasQueryFilter(x => !x.Deleted)` to hide soft-deleted rows from normal queries.

## Database Initialization

Database initialization runs from `Program.cs` through `InitializeDatabaseAsync()`.

Configuration:

```json
"Database": {
  "ApplyMigrations": true,
  "SeedOnStartup": true
}
```

When `ApplyMigrations` is `true`, EF Core applies pending migrations at startup.

When `SeedOnStartup` is `true`, seed data is inserted idempotently.

This behavior is intentional for the boilerplate because it makes local development and first runs simple.

This starter branch intentionally does not include tracked EF Core migrations. The expectation is that each new project creates its own first migration after renaming the database and adding the entities it needs.

## Seed Data

The seed creates:

- role `Admin`
- role `User`
- user `admin`

Default seed user:

| Username | Password | Role |
|---|---|---|
| `admin` | `Admin123!` | `Admin` |

The seed is meant for development and template demonstration. Change or remove it when building a real project from this boilerplate.

## Querying, Filtering, And Pagination

Sieve and `PagedRequestDto` remain available in the starter for future listing endpoints, but this branch does not ship a sample catalog or list endpoint anymore. Add those pieces only when your project needs them.

## Logging

Serilog is configured through `builder.Host.AddSerilog()`.

Configured sinks:

- console, through `appsettings.json`
- SQL Server, when `DefaultConnection` is configured

The SQL Server sink writes to:

```text
[audit].[Logs]
```

`LogCleanupBackgroundService` runs automatically and deletes old rows from `[audit].[Logs]` according to:

```json
"LogCleanup": {
  "RetentionDays": 7,
  "RunIntervalHours": 24
}
```

## Health Checks

Health checks are registered in `Infrastructure.DependencyInjection`.

Current checks:

- EF Core database check for `AppDbContext`

Endpoint:

```text
GET /health
```

The endpoint returns the overall health status and individual check entries.

## CORS

CORS is configured from:

```json
"Cors": {
  "AllowedOrigins": []
}
```

Development defaults:

```json
"Cors": {
  "AllowedOrigins": [ "http://localhost:4200", "http://localhost:3000" ]
}
```

When origins are configured, the default policy allows credentials, any method, and any header for those origins.

## Rate Limiting

A fixed-window rate limiter is configured globally.

Default values:

```json
"RateLimiting": {
  "PermitLimit": 100,
  "WindowSeconds": 60
}
```

Development overrides the limit to `1000` requests per window.

Rejected requests return HTTP `429`.

## Swagger

Swagger is registered with metadata from:

```json
"Swagger": {
  "Title": "JV API",
  "Description": "Enterprise .NET 10 API boilerplate with Vertical Slice Architecture, CQRS and Minimal APIs",
  "Version": "v1"
}
```

Swagger UI is enabled only in development.

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

## Included Services

### Email

`IEmailService` is implemented by `EmailService` using MailKit.

Configuration:

```json
"Smtp": {
  "Host": "",
  "Port": 587,
  "Username": "",
  "Password": "",
  "From": "noreply@example.com",
  "FromName": "Web API",
  "EnableSsl": true
}
```

### File Storage

`IFileStorageService` is implemented by `LocalFileStorageService`.

Default path:

```json
"FileStorage": {
  "LocalPath": "App_Data/Storage"
}
```

### Current User

`ICurrentUserService` reads from `HttpContext.User` and exposes:

- `UserId`
- `Username`
- `Roles`
- `IsAuthenticated`

## Configuration

Main configuration file:

```text
src/WebApi/appsettings.json
```

Development override:

```text
src/WebApi/appsettings.Development.json
```

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
- `AllowedHosts`

Docker Compose uses environment variables from `.env`.

## Local Development

### Clone This Starter Branch

If you want to clone the repository directly into this clean starter branch:

```bash
git clone --branch project-starter --single-branch <repo-url>
```

Example:

```bash
git clone --branch project-starter --single-branch https://github.com/<owner>/<repo>.git
```

If you already cloned the repository and want to switch to this branch:

```bash
git switch project-starter
```

Restore packages:

```bash
dotnet restore WebApi.slnx
```

Build:

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

In development:

- `appsettings.Development.json` provides a local SQL Server connection string
- Swagger UI is enabled
- CORS allows `localhost:4200` and `localhost:3000`
- startup can apply migrations and seed data

## Docker

Copy the sample environment file:

```bash
cp .env.example .env
```

On Windows PowerShell:

```powershell
Copy-Item .env.example .env
```

Then run:

```bash
docker compose up --build
```

Docker Compose starts:

- SQL Server 2022
- the API container on port `8080`

Important variables from `.env.example`:

- `ASPNETCORE_ENVIRONMENT`
- `SQLSERVER_SA_PASSWORD`
- `DB_NAME`
- `JWT_SECRET_KEY`
- `JWT_ISSUER`
- `JWT_AUDIENCE`
- `DATABASE_APPLY_MIGRATIONS`
- `DATABASE_SEED_ON_STARTUP`

API URL in Docker:

```text
http://localhost:8080
```

## Migrations

This branch is intended to be the base for new projects, so the tracked migration files were removed on purpose.

Recommended first-run flow:

1. Change the database name in `src/WebApi/appsettings.Development.json` or in your environment variables.
2. Add your own entities and EF Core configurations.
3. Create the first migration for your project.
4. Apply that migration to your new database.

Example local connection string change:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=MyNewApi_Dev;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
}
```

Create the first migration:

```bash
dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/WebApi
```

Apply it:

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/WebApi
```

If `Database:ApplyMigrations` stays enabled, the API can also apply pending migrations on startup after your first migration exists.

Create a migration:

```bash
dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/WebApi
```

Apply migrations manually:

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/WebApi
```

The boilerplate can also apply migrations at startup when `Database:ApplyMigrations` is enabled.

## Tests

Tests live in:

```text
src/Tests
```

Current coverage includes:

- auth handlers: register, login, refresh, revoke, current user
- log cleanup service behavior

Run tests:

```bash
dotnet test src/Tests/Tests.csproj
```

## Adding A New Feature

For a new use case:

1. Create a folder under `src/Application/Features/<Area>/<UseCase>/`.
2. Add a command or query.
3. Add a handler.
4. Add request/response DTOs if the slice owns them.
5. Add a validator when input validation is needed.
6. Add an endpoint under `src/WebApi/Endpoints/<Area>/`.
7. Use `ISender` in the endpoint to send the command/query.
8. Add constants for routes, names, tags, or descriptions when needed.
9. Add tests for the handler and any important behavior.

Example shape:

```text
Application/Features/Orders/Create/
  CreateOrderCommand.cs
  CreateOrderHandler.cs
  CreateOrderRequest.cs
  CreateOrderResponse.cs
  CreateOrderValidator.cs

WebApi/Endpoints/Orders/
  CreateOrderEndpoint.cs
```

The endpoint should translate HTTP input into an application request. The handler should own the use-case flow.

## Adding A New Entity

1. Add the entity under `src/Domain/Entities/`.
2. Add `DbSet<T>` to `AppDbContext`.
3. Add EF configuration under `src/Infrastructure/Persistence/Configurations/`.
4. Expose the `DbSet<T>` through `IAppDbContext` if application handlers need it.
5. Add or update application handlers.
6. Add a migration.
7. Add tests.

## Boilerplate Decisions

These choices are intentional defaults for a starter project:

- migrations can run at startup
- seed data can run at startup
- the admin seed user exists for quick testing
- authorization is simple and policy-free by default
- request DTOs currently live with their application slice
- endpoints are discovered by reflection
- handlers use EF Core through `IAppDbContext`
- Mapster is used directly in handlers/projections
- Sieve remains available for future list endpoints

These are easy to change when a real project needs stricter boundaries or a different style.

## Production Hardening Checklist

Before using this template for a real public API, review:

- replace seed credentials
- define authorization policies and roles
- decide whether public registration should remain enabled
- configure real CORS origins
- configure real JWT issuer/audience/secret
- move secrets to user secrets, environment variables, or a secret manager
- review refresh token storage and revocation strategy
- configure persistent file storage if local disk is not enough
- review database migration strategy
- review logging retention and sensitive log data
- add integration tests around HTTP endpoints
- add CI build and test workflow
