# Copilot Instructions — Leave Management API

## Project Overview

A Clean Architecture .NET 10 leave management system. HR admins manage employees and their leave requests through a REST API. Core patterns: CQRS with MediatR, repository pattern with soft deletes, FluentValidation at the handler boundary, and BDD integration tests backed by PostgreSQL via Testcontainers.

## Tech Stack

- **.NET 10 / C# 14** (`net10.0`, `LangVersion 14`, nullable and implicit usings enabled)
- **MediatR 12.5.0** — CQRS command/query dispatch
- **FluentValidation 12.1.0** — input validation, injected into handlers
- **EF Core 10.0.0 + Npgsql 10.0.0** — ORM on PostgreSQL 16
- **Scalar 2.13.21 + Swashbuckle 10.0.1** — interactive API docs at `/scalar/v1`
- **xUnit + FluentAssertions 8.8.0** — unit tests
- **Reqnroll 3.2.1 + Testcontainers.PostgreSql 4.1.0** — integration BDD tests
- **Respawn 6.2.1** — database state reset between test scenarios

## Architecture & Layering

Four projects, strict dependency direction:

```
API → Application → Domain        ✅ allowed
Infrastructure → Domain           ✅ allowed
Infrastructure → Application      ✅ allowed
Application → Infrastructure      ❌ FORBIDDEN — use IBaseRepository<T> / service interfaces
Domain → Application              ❌ FORBIDDEN
```

| Project | Role |
|---------|------|
| `LeaveManagement.API` | Controllers dispatch to MediatR; zero business logic |
| `LeaveManagement.Application` | CQRS features, validators, mapper extensions |
| `LeaveManagement.Domain` | Entities, enumerations, `IBaseRepository<T>`, service contracts |
| `LeaveManagement.Infrastructure` | `ApplicationContext`, `BaseRepository`, `LeaveRepository` |

## C# 14 & .NET 10 Conventions

| Rule | ✅ Correct | ❌ Incorrect |
|------|-----------|-------------|
| Constructor injection | `public class CreateEmployeeHandler(IEmployeeRepository repo, IValidator<CreateEmployeeCommand> v)` | Private readonly fields + traditional constructor |
| Collection expressions | `string[] tags = [..existing, "new"];` | `new List<string>(existing) { "new" }` |
| Required DTO properties | `public required string Name { get; init; }` | Mutable `{ get; set; }` on request objects |
| Null coalescing | `string name = input ?? string.Empty;` | `if (name == null) name = ""` |
| Async method naming | `GetEmployeeAsync(id, ct)` | `GetEmployee(id)` for I/O methods |

## CQRS & MediatR Conventions

Feature folder: `src/LeaveManagement.Application/Features/<Entity>/<Commands|Queries>/<FeatureName>/`

Each feature contains **exactly three files**:

```
CreateEmployeeCommand.cs     # IRequest<BaseResponse> — input data, no logic
CreateEmployeeHandler.cs     # IRequestHandler — validate → business logic → return response
CreateEmployeeValidator.cs   # AbstractValidator<CreateEmployeeCommand>
```

Naming: `Create|Update|Delete[Entity]Command` · `Get[Entity]ByIdQuery` · `Get[Entity]sListQuery`

**Handler pattern — validation before logic:**

```csharp
// ✅ Correct
var validation = await _validator.ValidateAsync(request, ct);
if (!validation.IsValid)
    return new BaseResponse { Success = false, Message = validation.Errors[0].ErrorMessage };
// ... business logic follows

// ❌ Incorrect — throwing from handler surfaces as 500
throw new ValidationException(validation.Errors);
```

**Mapper extension methods** live in `src/LeaveManagement.Application/Mappers/`:

```csharp
// ✅ Correct
var employee = command.ToEmployee();
var response = employee.ToEmployeeResponse();

// ❌ Incorrect — inline mapping in handler
var employee = new Employee { Name = command.Name, Email = command.Email };
```

Canonical example: `src/LeaveManagement.Application/Features/Employees/Commands/CreateEmployee/`

## EF Core Conventions

- `ApplicationContext.SaveChangesAsync()` auto-sets `CreatedAt` / `UpdatedAt` — never set these manually.
- **Soft deletes**: set `entity.DeletedAt = DateTime.UtcNow` in the repository — never call `dbContext.Remove()` or issue raw `DELETE` SQL.
- `BaseRepository.GetAllAsync()` already filters `DeletedAt == default` — never bypass or duplicate this filter.
- Schema is created via `Database.EnsureCreatedAsync()` (dev only) — add `dotnet ef migrations` before production.

## Logging Standards

```csharp
// ✅ Correct — [LoggerMessage] source-generated (see CreateEmployeeHandler.cs)
[LoggerMessage(Level = LogLevel.Information, Message = "Employee {EmployeeId} created")]
partial void LogEmployeeCreated(Guid employeeId);

// ❌ Incorrect — string interpolation allocates on every call and leaks PII
_logger.LogInformation($"Employee {employee.Name} id={employee.Id} created");
```

## Security Requirements

- Connection string key `"LeaveManagementConnectionString"` must come from environment variables or user secrets — never hardcode.
- All DB access goes through EF Core — no `ExecuteSqlRaw()` with unsanitised user input.
- CORS is currently `AllowAny*` — **restrict origins, headers, and methods before any non-development deployment**.
- Never log full request bodies that may contain PII.

## Testing Requirements

**Unit tests** (`tests/LeaveManagement.Application.UnitTests`):
- xUnit `[Fact]` / `[Theory]`; naming convention: `MethodName_StateUnderTest_ExpectedBehaviour`
- FluentAssertions: `.Should().Be()`, `.Should().BeEquivalentTo()`, `.Should().Throw<T>()`
- No external dependencies; mock repositories

**Integration tests** (`tests/LeaveManagement.API.IntegrationTests`):
- Reqnroll `.feature` files + step definitions under `StepDefinitions/`
- `CustomWebApplicationFactory` replaces production DB with Testcontainers PostgreSQL
- `DatabaseHook` resets state between scenarios (Respawn + manual identity-sequence SQL reset)
- **Docker must be running** — tests fail immediately without it

## Build & Test

```bash
dotnet build
dotnet test tests/LeaveManagement.Application.UnitTests          # unit, no Docker
dotnet test tests/LeaveManagement.API.IntegrationTests           # BDD, requires Docker
dotnet run --project src/LeaveManagement.API
# Scalar docs: https://localhost:<port>/scalar/v1   Health: GET /health
```

## What NOT to Do

❌ Hard-delete entities — always soft-delete via `DeletedAt`
❌ Bypass `BaseRepository.GetAllAsync()` soft-delete filter with raw queries
❌ Inject `ApplicationContext` into `Application` layer — use `IBaseRepository<T>`
❌ Log with string interpolation — use `[LoggerMessage]` partial methods
❌ Throw exceptions from handlers — return `BaseResponse { Success = false }`
❌ Map entities inline in handlers — add extension methods to `Mappers/`
❌ Put business logic in controllers — controllers only call `_mediator.Send()`

## Why These Rules Exist

1. **`[LoggerMessage]` over interpolation** — the source generator emits structured logs with zero string allocation at inactive log levels; interpolation always allocates and can leak employee PII into log aggregators.
2. **Soft deletes** — employees and leaves must be auditable; hard deletes break historical payroll reporting and orphan foreign-key references.
3. **`BaseResponse` instead of exceptions** — MediatR pipelines handle validation at the handler boundary; letting `ValidationException` propagate produces misleading 500 responses for what are 400-level client errors.
