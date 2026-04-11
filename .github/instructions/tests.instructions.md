---
name: "Test Conventions"
description: "xUnit + FluentAssertions + Reqnroll conventions for this project. Applies to all unit and integration test files."
applyTo: "**/*Tests.cs,**/*Test.cs,**/tests/**/*.cs"
---

# Test Conventions — Leave Management API

## Unit Tests (xUnit)

### Naming
Every test method MUST follow: `MethodName_StateUnderTest_ExpectedBehaviour`

```csharp
// ✅ Correct
[Fact]
public async Task HandleAsync_ValidCommand_ReturnsSuccessResponse()

// ❌ Incorrect
[Fact]
public async Task TestCreate()
```

### Structure — AAA Pattern

```csharp
[Fact]
public async Task HandleAsync_ValidCommand_ReturnsSuccessResponse()
{
    // Arrange
    var command = new CreateEmployeeCommand { Name = "Jane Doe", Email = "jane@test.com" };
    var mockRepo = Substitute.For<IEmployeeRepository>();
    var validator = new CreateEmployeeValidator();
    var handler = new CreateEmployeeHandler(mockRepo, validator);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeTrue();
}
```

### FluentAssertions

Always use FluentAssertions — never `Assert.True(x == y)`:

```csharp
// ✅ Correct
result.Should().NotBeNull();
result.Success.Should().BeTrue();
result.Message.Should().Contain("created");
employees.Should().HaveCount(3);
employees.Should().BeEquivalentTo(expected, opt => opt.ExcludingMissingMembers());
act.Should().ThrowAsync<Exception>();

// ❌ Incorrect
Assert.True(result.Success);
Assert.Equal(3, employees.Count);
```

### Mocking
Use `NSubstitute` (or the mock library already in the project) — never mock concrete classes.

```csharp
// ✅ Correct
var mockRepo = Substitute.For<IEmployeeRepository>();
mockRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
        .Returns(new Employee { Id = Guid.NewGuid(), Name = "Test" });

// ❌ Incorrect — mocking concretions is fragile
var repo = new EmployeeRepository(dbContext);
```

### What NOT to Do in Unit Tests

- ❌ No `Thread.Sleep()` or `Task.Delay()` — use async all the way
- ❌ No magic strings scattered across tests — define with `const` or test helpers
- ❌ No real database connections — unit tests are infrastructure-free
- ❌ No multiple asserts testing different behaviours — one `[Fact]` per behaviour

---

## Integration Tests (Reqnroll)

### Step Definition Structure

```csharp
[Binding]
public class EmployeeSteps(ScenarioContext context, CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Given("an employee exists with name {string}")]
    public async Task GivenAnEmployeeExists(string name) { ... }

    [When("I send a POST request to {string}")]
    public async Task WhenISendPost(string route) { ... }

    [Then("the response status should be {int}")]
    public async Task ThenStatusIs(int statusCode) { ... }
}
```

### Scenario Context
Use `ScenarioContext` to share state between steps — never static fields.

```csharp
// ✅ Correct
context["CreatedEmployeeId"] = response.Id;
var id = context.Get<Guid>("CreatedEmployeeId");

// ❌ Incorrect — static state bleeds between scenarios
private static Guid _createdId;
```

### Verification
After each integration test `[Then]` step, verify with:
1. HTTP status code assertion
2. Response body deserialization + FluentAssertions
3. (Optional) direct DB check via `ApplicationContext` from the factory scope

### Docker Requirement
Integration tests require Docker running for Testcontainers. If the container fails to start, check:
- Docker Desktop is running
- Port 5432 is not in use by another PostgreSQL instance
- `CustomWebApplicationFactory` connection string override is working
