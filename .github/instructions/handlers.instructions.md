---
name: "MediatR Handler Conventions"
description: "MediatR IRequestHandler implementation rules: primary constructor injection, FluentValidation-first pattern, [LoggerMessage] logging, BaseResponse error handling. Applies to all CQRS feature handlers."
applyTo: "**/Features/**/*Handler.cs"
---

# MediatR Handler Conventions

## Handler Skeleton

Every handler must follow this exact structure:

```csharp
public partial class CreateEmployeeHandler(
    IEmployeeRepository repository,
    IValidator<CreateEmployeeCommand> validator,
    ILogger<CreateEmployeeHandler> logger)
    : IRequestHandler<CreateEmployeeCommand, BaseResponse>
{
    public async Task<BaseResponse> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate first
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return new BaseResponse { Success = false, Message = validation.Errors[0].ErrorMessage };

        // 2. Business logic
        var employee = request.ToEmployee();
        await repository.CreateAsync(employee, cancellationToken);

        // 3. Log success
        LogEmployeeCreated(employee.Id);

        // 4. Return response
        return new BaseResponse { Success = true, Message = "Employee created successfully" };
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Employee {EmployeeId} created")]
    partial void LogEmployeeCreated(Guid employeeId);
}
```

## Rules

### 1. Primary Constructor — Always
Inject all dependencies via primary constructor. No `private readonly` field declarations + traditional constructor.

```csharp
// ✅ Correct
public class GetEmployeeByIdHandler(IEmployeeRepository repository)
    : IRequestHandler<GetEmployeeByIdQuery, EmployeeResponse?>

// ❌ Incorrect
public class GetEmployeeByIdHandler : IRequestHandler<GetEmployeeByIdQuery, EmployeeResponse?>
{
    private readonly IEmployeeRepository _repository;
    public GetEmployeeByIdHandler(IEmployeeRepository repository) { _repository = repository; }
}
```

### 2. Validate Before Business Logic — Always

```csharp
// ✅ Correct — validation gate first
var validation = await validator.ValidateAsync(request, cancellationToken);
if (!validation.IsValid)
    return new BaseResponse { Success = false, Message = validation.Errors[0].ErrorMessage };

// ❌ Incorrect — validation thrown, surfaces as HTTP 500
throw new ValidationException(validation.Errors);
```

### 3. Return BaseResponse, Never Throw

Wrap business logic in try/catch and return `Success = false` — never let exceptions propagate:

```csharp
// ✅ Correct
try
{
    // business logic
    return new BaseResponse { Success = true };
}
catch (Exception ex)
{
    logger.LogError(ex, "...");
    return new BaseResponse { Success = false, Message = "An error occurred" };
}

// ❌ Incorrect — unhandled exception → 500
throw;
```

### 4. [LoggerMessage] — Always, Never Interpolation

```csharp
// ✅ Correct — declare as partial void annotated with [LoggerMessage]
[LoggerMessage(Level = LogLevel.Information, Message = "Leave {LeaveId} approved for {EmployeeId}")]
partial void LogLeaveApproved(Guid leaveId, Guid employeeId);

// ❌ Incorrect — allocates on every call, risks leaking PII
_logger.LogInformation($"Leave {leave.Id} approved for {leave.EmployeeId}");
```

Class must be `partial` to support `[LoggerMessage]` source generation.

### 5. Mapper Extension Methods — Never Inline Mapping

```csharp
// ✅ Correct — extension method in Mappers/EmployeeMapper.cs
var employee = request.ToEmployee();
var dto = employee.ToEmployeeResponse();

// ❌ Incorrect — inline struct construction in handler
var employee = new Employee { Name = request.Name, Email = request.Email };
```

### 6. CancellationToken — Always Pass Through

Every async call must receive the `cancellationToken` parameter:

```csharp
// ✅ Correct
await repository.GetByIdAsync(request.Id, cancellationToken);

// ❌ Incorrect
await repository.GetByIdAsync(request.Id);
await repository.GetByIdAsync(request.Id, CancellationToken.None);
```
