# JV API — Web API Boilerplate .NET 10

API empresarial base construida en .NET 10 con **Vertical Slice Architecture**, **CQRS** y **Minimal APIs**. Lista para clonar y empezar a implementar Features.

## Tecnologías

- .NET 10
- SQL Server + Entity Framework Core 10
- MediatR + FluentValidation
- Mapster
- Serilog (consola + sink SQL Server con auto-creación de tabla)
- Sieve (filtros, ordenamiento, paginación)
- JWT Bearer (autenticación propia, sin ASP.NET Identity)
- MailKit (SMTP)
- Swagger / OpenAPI
- xUnit + EF Core InMemory (tests)

## Arquitectura

```
WebApi.slnx
├── src/
│   ├── Domain/          Entidades y clase base (EntityBase)
│   ├── Infrastructure/  EF Core, JWT, SMTP, File Storage, Logging, Seed
│   ├── Shared/          DTOs, Excepciones, ValidationBehavior
│   ├── WebApi/          Endpoints, Middleware, Program.cs
│   └── Tests/           Tests unitarios (xUnit)
```

### Vertical Slice

Cada Feature vive en su propia carpeta con todos sus archivos:

```
Endpoints/Products/Create/
├── CreateProductCommand.cs
├── CreateProductHandler.cs
├── CreateProductValidator.cs
└── CreateProductEndpoint.cs
```

Sin Repository Pattern, sin Unit of Work, sin AutoMapper. Los Handlers usan `AppDbContext` directamente.

## Entidades y esquemas

| Schema | Tabla | Propósito |
|--------|-------|-----------|
| `sec` | `Users` | Usuarios |
| `sec` | `Roles` | Roles |
| `sec` | `UserRoles` | Usuario <-> Rol |
| `sec` | `RefreshTokens` | Refresh tokens JWT |
| `dbo` | `Products` | Productos (ejemplo) |
| `cat` | `Categories` | Categorías (ejemplo) |
| `audit` | `Logs` | Logs de Serilog |

Todas las entidades heredan de `EntityBase` con auditoría automática (`CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) y **Soft Delete** (`Deleted`, `DeletedAt`, `DeletedBy`).

## Endpoints

### Auth `/api/v1/auth`

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | `/login` | ❌ | Iniciar sesión |
| POST | `/register` | ❌ | Registrar usuario |
| POST | `/refresh` | ❌ | Renovar token |
| POST | `/revoke` | ✅ | Revocar refresh token |
| GET | `/me` | ✅ | Usuario actual |

### Products `/api/v1/products`

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | `/` | ✅ | Crear producto |
| PUT | `/{id}` | ✅ | Actualizar producto |
| DELETE | `/{id}` | ✅ | Eliminar producto (soft delete) |
| GET | `/{id}` | ❌ | Obtener producto |
| GET | `/` | ❌ | Listar productos (paginado + filtros) |

### Logs `/api/v1/logs`

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | `/test` | ❌ | Insertar un log de prueba (para verificar logging en BD) |

### Health

| Método | Ruta |
|--------|------|
| GET | `/health` |

## Cómo ejecutar

```bash
dotnet restore
dotnet run --project src/WebApi
```

En **Development**:

1. La base de datos se crea y migra automáticamente.
2. Se ejecuta el seed con datos de ejemplo.
3. Swagger disponible en `/swagger`.

### Usuario semilla

| Usuario | Contraseña | Rol |
|---------|-----------|-----|
| `admin` | `Admin123!` | Admin |

## Migraciones

```bash
dotnet ef migrations add <Nombre> --project src/Infrastructure --startup-project src/WebApi
dotnet ef database update --project src/Infrastructure --startup-project src/WebApi
```

## Agregar una Feature

1. Crear carpeta en `Endpoints/<Feature>/<Accion>/`.
2. Crear Command/Query, Handler, Validator y Endpoint.
3. Registrar el endpoint con `MapGroup` en `Program.cs`.

Ejemplo mínimo:

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

## Agregar una entidad

1. Crear clase en `Domain/Entities/` heredando de `EntityBase`.
2. Crear configuración en `Infrastructure/Persistence/Configurations/`.
3. Agregar `DbSet<T>` en `AppDbContext`.
4. Crear migración.

## JWT

Autenticación propia (sin ASP.NET Identity). Configuración en `appsettings.json`:

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

- Password hashing con `PasswordHasher<User>`.
- Refresh tokens con rotación y revocación.
- Esquema `sec.RefreshTokens`.

## Soft Delete y Auditoría

Implementado mediante **EF Core Interceptors**:

- `AuditInterceptor`: asigna `CreatedAt`/`CreatedBy` al insertar, `UpdatedAt`/`UpdatedBy` al modificar.
- `SoftDeleteInterceptor`: convierte `Delete` en soft delete (`Deleted = true`).

Ambos usan "system" como `CreatedBy`/`UpdatedBy`/`DeletedBy`.

## Servicios incluidos

### Email (SMTP)

`IEmailService` con implementación MailKit. Configurable desde `appsettings.json`.

### File Storage

`IFileStorageService` con implementación local en `App_Data/Storage/`.

### Logging

Serilog escribe a **consola** y a **SQL Server** (`[audit].[Logs]`) mediante `Serilog.Sinks.MSSqlServer`. La tabla se crea automáticamente al primer log.

**Cómo funciona:** Cualquier `ILogger<T>` inyectado en handlers, middlewares o services fluye a través de Serilog. Cada request HTTP se loggea automáticamente con `UseSerilogRequestLogging()`. Las excepciones capturadas por `ExceptionHandlingMiddleware` también se persisten.

**Uso en tu código:**
```csharp
public class MyHandler(ILogger<MyHandler> logger) : IRequestHandler<...>
{
    public async Task<...> Handle(..., CancellationToken ct)
    {
        logger.LogInformation("Procesando {Entity} con id {Id}", name, id);
        logger.LogError(ex, "Error al procesar {Id}", id);
    }
}
```

**Limpieza de logs:** Automática mediante `LogCleanupBackgroundService` (retención configurable, 7 días por defecto). Elimina filas antiguas de `[audit].[Logs]` basándose en `TimeStamp`.

## Configuración

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

En Development se usa `Server=localhost;Database=WebApi_Dev` con Integrated Security.

## Tests

```bash
dotnet test src/Tests
```

- xUnit + EF Core InMemory
- Tests de Auth (login), Products (CRUD, soft delete) y LogCleanupService
