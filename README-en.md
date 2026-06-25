# JV API — Web API Boilerplate .NET 10

Enterprise-grade base API built with .NET 10 using **Vertical Slice Architecture**, **CQRS** and **Minimal APIs**. Clone and start implementing Features immediately.

## Technologies

- .NET 10
- SQL Server + Entity Framework Core 10
- MediatR + FluentValidation
- Mapster
- Serilog (console + SQL Server)
- Sieve (filtering, sorting, pagination)
- JWT Bearer (custom auth, no ASP.NET Identity)
- MailKit (SMTP)
- Swagger / OpenAPI
- xUnit + EF Core InMemory (tests)

## Architecture

```
WebApi.slnx
├── src/
│   ├── Domain/          Entities and base class (EntityBase)
│   ├── Infrastructure/  EF Core, JWT, SMTP, File Storage, Logging, Seed
│   ├── Shared/          DTOs, Exceptions, ValidationBehavior
│   ├── WebApi/          Endpoints, Middleware, Program.cs
│   └── Tests/           Unit tests (xUnit)
```

### Vertical Slice

Each Feature lives in its own folder with all its files:

```
Endpoints/Products/Create/
├── CreateProductCommand.cs
├── CreateProductHandler.cs
├── CreateProductValidator.cs
└── CreateProductEndpoint.cs
```

No Repository Pattern, no Unit of Work, no AutoMapper. Handlers use `AppDbContext` directly.

## Entities and schemas

| Schema | Table | Purpose |
|--------|-------|---------|
| `sec` | `Users` | Application users |
| `sec` | `Roles` | Authorization roles |
| `sec` | `UserRoles` | User <-> Role mapping |
| `sec` | `RefreshTokens` | JWT refresh tokens |
| `dbo` | `Products` | Products (example) |
| `cat` | `Categories` | Categories (example) |
| `log` | `Logs` | Serilog log entries |

All entities inherit from `EntityBase` with automatic auditing (`CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) and **Soft Delete** (`Deleted`, `DeletedAt`, `DeletedBy`).

## Endpoints

### Auth `/api/v1/auth`

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/login` | ❌ | Sign in |
| POST | `/register` | ❌ | Register user |
| POST | `/refresh` | ❌ | Refresh token |
| POST | `/revoke` | ✅ | Revoke refresh token |
| GET | `/me` | ✅ | Current user |

### Products `/api/v1/products`

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/` | ✅ | Create product |
| PUT | `/{id}` | ✅ | Update product |
| DELETE | `/{id}` | ✅ | Delete product (soft delete) |
| GET | `/{id}` | ❌ | Get product |
| GET | `/` | ❌ | List products (paginated + filters) |

### Health

| Method | Route |
|--------|-------|
| GET | `/health` |

## How to run

```bash
dotnet restore
dotnet run --project src/WebApi
```

In **Development**:

1. The database is created and migrated automatically.
2. Seed data is applied with sample records.
3. Swagger available at `/swagger`.

### Seed user

| Username | Password | Role |
|----------|----------|------|
| `admin` | `Admin123!` | Admin |

## Migrations

```bash
dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/WebApi
dotnet ef database update --project src/Infrastructure --startup-project src/WebApi
```

## Adding a Feature

1. Create a folder under `Endpoints/<Feature>/<Action>/`.
2. Create Command/Query, Handler, Validator and Endpoint.
3. Register the endpoint with `MapGroup` in `Program.cs`.

Minimal example:

```csharp
public sealed record CreateFooCommand(string Name) : IRequest<FooResponse>;

public sealed class CreateFooHandler(AppDbContext db) : IRequestHandler<CreateFooCommand, FooResponse>
{
    public async Task<FooResponse> Handle(CreateFooCommand request, CancellationToken ct)
    {
        var foo = request.Adapt<Foo>();
        db.Foos.Add(foo);
        await db.SaveChangesAsync(ct);
        return foo.Adapt<FooResponse>();
    }
}
```

## Adding an entity

1. Create a class in `Domain/Entities/` inheriting from `EntityBase`.
2. Create a configuration in `Infrastructure/Persistence/Configurations/`.
3. Add `DbSet<T>` in `AppDbContext`.
4. Create a migration.

## JWT

Custom authentication (no ASP.NET Identity). Configuration in `appsettings.json`:

```json
{
  "Jwt": {
    "SecretKey": "...",
    "Issuer": "WebApi",
    "Audience": "WebApi",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 1
  }
}
```

- Password hashing with `PasswordHasher<User>`.
- Refresh tokens with rotation and revocation.
- Schema `sec.RefreshTokens`.

## Soft Delete and Auditing

Implemented via **EF Core Interceptors**:

- `AuditInterceptor`: sets `CreatedAt`/`CreatedBy` on insert, `UpdatedAt`/`UpdatedBy` on modify.
- `SoftDeleteInterceptor`: converts `Delete` to soft delete (`Deleted = true`).

Both use "system" as `CreatedBy`/`UpdatedBy`/`DeletedBy`.

## Included services

### Email (SMTP)

`IEmailService` with MailKit implementation. Configurable from `appsettings.json`.

### File Storage

`IFileStorageService` with local implementation at `App_Data/Storage/`.

### Logging

Serilog writes to console and SQL Server (`log.Logs`). Automatic log cleanup via `LogCleanupBackgroundService` (configurable retention, default 7 days).

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=WebApi;..."
  },
  "Jwt": { ... },
  "Smtp": { ... },
  "FileStorage": { "LocalPath": "App_Data/Storage" },
  "LogCleanup": { "RetentionDays": 7, "RunIntervalHours": 24 },
  "RateLimiting": { "PermitLimit": 100, "WindowSeconds": 60 },
  "Cors": { "AllowedOrigins": [] },
  "Sieve": { "DefaultPageSize": 25, "MaxPageSize": 100 }
}
```

Development uses `Server=localhost;Database=WebApi_Dev` with Integrated Security.

## Tests

```bash
dotnet test src/Tests
```

- xUnit + EF Core InMemory
- Tests for Auth (login), Products (CRUD, soft delete) and LogCleanupService
