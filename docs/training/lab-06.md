# Lab 06 — End-to-End Workflow (Capstone)

**Duration:** 70 minutes
**Module:** Day 2 — Module 4
**Level:** Advanced

---

## Learning Objectives

Apply all 6 primitives together in a realistic feature delivery cycle.
By the end of this lab you will have used:
- Always-On Instructions (passive — always active)
- File-Based Instructions (passive — activates on test files)
- `/generate-feature` Prompt (explicit invocation)
- `run-and-fix-tests` Skill (automatic intent match)
- `@clean-architecture-refactor-expert` Custom Agent (persona-driven)
- `/review-code` Prompt (quality gate)

---

## Feature to Implement

> **"As an HR manager, I want to search employees by department so I can quickly find all members of a specific team."**

This is a realistic greenfield feature that touches every layer: controller → CQRS query → handler → repository → response DTO → tests.

---

## Phase 1 — Understand the Codebase (10 min)

### Step 1 — Architecture overview

In Copilot Chat (default agent), type `/explain-architecture`.

When prompted for `scope`, enter: `the CQRS query layer`

Study the output. Understand:
- Where to add the new `GetEmployeesByDepartmentQuery` in Application
- Where to add the handler and validator
- What the `IEmployeeRepository` contract should expose
- What `EmployeeResponse` already contains

### Step 2 — Explore existing patterns

Ask in default Copilot Chat:
```
Show me how GetEmployeeByIdQuery is implemented — from controller to repository.
Explain the pattern I should follow for the new GetEmployeesByDepartment feature.
```

---

## Phase 2 — Implement the Feature (20 min)

In **Agent mode** (default), ask Copilot to implement the feature:

```
Implement a GetEmployeesByDepartment query feature on the Leave Management API.

Requirements:
- New GET endpoint: GET /api/employees/search?department=Engineering
- Returns a list of EmployeeResponse
- Follows the same CQRS pattern as GetEmployeeById
- Validates that department is not blank (return BaseResponse { Success = false } if invalid)
- Returns an empty list (not an error) if no employees match
- Logs the query at Information level using [LoggerMessage]
```

**Observe that Copilot automatically:**
- Creates `GetEmployeesByDepartmentQuery.cs`, `GetEmployeesByDepartmentHandler.cs`, `GetEmployeesByDepartmentValidator.cs`
- Adds a method signature to `IEmployeeRepository`
- Returns `EmployeeResponse` — not the `Employee` entity
- Uses primary constructor injection (not `private readonly` fields)
- Follows naming conventions from `copilot-instructions.md`

If Copilot breaks any rule (e.g. injects `ApplicationContext`, throws instead of returning `BaseResponse`, uses `_logger.LogInformation($"...")`), **point it out** and ask it to fix the violation.

---

## Phase 3 — Generate Tests (10 min)

In Copilot Chat (**Agent mode**), type:
```
/generate-feature
```

When prompted for `entityName`, enter: `Employee`
When prompted for `featureType`, enter: `Query`
When prompted for `operation`, enter: `GetByDepartment`

**Verify the generated tests include:**
- `GetEmployeesByDepartment_ValidDepartment_ReturnsMatchingEmployees`
- `GetEmployeesByDepartment_EmptyDepartment_ReturnsFailureResponse`
- `GetEmployeesByDepartment_NoMatch_ReturnsEmptyList`

Open the generated test file and confirm:
- `applyTo: "**/*Tests.cs"` file-based instructions shaped the test style automatically
- Method names: `MethodName_StateUnderTest_ExpectedBehaviour`
- Assertions use FluentAssertions (`.Should().Be()`, `.Should().BeEquivalentTo()`)
- Mocked `IEmployeeRepository` — no real database access

---

## Phase 4 — Run and Fix Tests (10 min)

In Copilot Chat (Agent mode), type:
```
run the tests and fix any failures
```

**Copilot loads the `run-and-fix-tests` skill automatically** and:
1. Runs `dotnet test tests/LeaveManagement.Application.UnitTests`
2. Parses the output
3. Reports `Build succeeded, X test(s) passed` — or lists failures with root cause + fix

If there are failures, apply the suggested fixes and ask Copilot to re-run.

Keep going until you see all tests passing.

---

## Phase 5 — Refactor (10 min)

Switch to the **Clean Architecture Refactor Expert** agent (`@clean-architecture-refactor-expert`).

Open `GetEmployeesByDepartmentHandler.cs` and ask:
```
Review the handler I just added and refactor it if needed.
```

The agent will:
1. Produce a **Violation Table** — checking for `ApplicationContext` injection, inline mapping, string-interpolation logging, or missing `CancellationToken` threading
2. Show **before/after diffs** for any issues
3. Suggest the handoff: "Run `/run-and-fix-tests` to verify nothing broke"

Follow the handoff to confirm refactoring did not break any tests.

---

## Phase 6 — Code Review (10 min)

Back in default Copilot Chat, open `EmployeesController.cs`.

Type:
```
/review-code
```

Focus on the new `search` endpoint.

**Expected result:** Copilot checks:
- Clean Architecture compliance (controller only calls `_mediator.Send()`, no business logic)
- Security (`department` query param is validated in the handler, not the controller)
- Logging (no `_logger.LogInformation($"...")` with employee PII)
- Response contract (`BaseResponse` returned consistently)

**If issues are found:** fix them and re-run `/review-code` until all categories pass.

---

## Full Primitives Map — What Was Used and When

| Phase | Action | Primitive Used |
|-------|--------|---------------|
| 1 | Architecture context (always active) | Always-On Instructions |
| 1 | `/explain-architecture` invoked | Prompt File |
| 2 | Feature follows CQRS + layering rules | Always-On Instructions |
| 3 | `/generate-feature` invoked | Prompt File |
| 3 | Test style (xUnit, FluentAssertions, AAA naming) applied to test file | File-Based Instructions |
| 4 | "run the tests and fix failures" → `dotnet test` executed | Skill (auto-loaded) |
| 5 | `@clean-architecture-refactor-expert` persona active | Custom Agent |
| 5 | "Run Tests" triggered post-refactor | Agent Handoff |
| 6 | `/review-code` quality gate | Prompt File |

---

## Debrief Questions

Answer these in the group discussion:

1. At which point did you type the fewest words and get the most done? Why?
2. Which primitive saved the most time compared to not having it?
3. Which rule from `copilot-instructions.md` was enforced automatically (without you asking)?
4. What would break if the `tests.instructions.md` file did not exist?
5. What would you add to the instructions file based on what you learned today?

---

## What a Complete Copilot Environment Looks Like

```
.github/
├── copilot-instructions.md                   ← Always-On: universal rules
├── instructions/
│   ├── tests.instructions.md                 ← File-Based: test-specific rules
│   ├── handlers.instructions.md              ← File-Based: handler rules
│   └── migrations.instructions.md            ← File-Based: migration safety rules
├── prompts/
│   ├── review-code.prompt.md                 ← Prompt: /review-code
│   ├── generate-feature.prompt.md            ← Prompt: /generate-feature
│   └── explain-architecture.prompt.md        ← Prompt: /explain-architecture
├── skills/
│   ├── run-and-fix-tests/SKILL.md            ← Skill: auto-runs dotnet test on intent
│   └── ef-core-migration/SKILL.md            ← Skill: auto-runs dotnet ef on intent
└── agents/
    ├── clean-architecture-refactor-expert.agent.md ← Agent: refactoring specialist
    └── dotnet-upgrade-expert.agent.md              ← Agent: .NET migration specialist
```

---

**Previous:** [Lab 05 — Custom Agents](./lab-05.md)
