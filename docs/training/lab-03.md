# Lab 03 — Primitive 3: Prompt Files (Slash Commands)

**Duration:** 60 minutes
**Module:** Day 2 — Module 1
**Level:** Intermediate

---

## Learning Objectives

By the end of this lab, you will be able to:
- Explain what a prompt file is and how it differs from instructions
- Invoke a prompt using the `/` slash command
- Use `${input:variable}` parameters to make prompts reusable
- Create a new prompt file from scratch for a recurring engineering task
- Choose the correct execution mode (`ask` vs `agent`)

---

## Key Concept

> **Prompt Files** are reusable task templates invoked on demand with a `/` slash command. They are macros for workflows you perform repeatedly — code review, feature scaffolding, architecture explanation — eliminating repetitive prompting and producing consistent outputs.

### The critical difference from instructions

| Instructions | Prompts |
|-------------|---------|
| Load automatically | Invoked explicitly with `/` |
| Define **behaviour** | Define a **task** |
| "Always use primary constructors" | "Now, do THIS specific thing" |
| Passive | Active |

### File location & naming

```
.github/
└── prompts/
    └── {name}.prompt.md       ← must end in .prompt.md
```

### Frontmatter fields

```yaml
---
name: review-code               ← appears in the / menu
description: "..."              ← shown as placeholder text
mode: ask | agent               ← execution mode
model: gpt-4o                   ← model to use (optional)
---
```

### Execution modes

| Mode | What it does | Use when |
|------|-------------|----------|
| `ask` | Read-only, conversational | Review, explain, brainstorm |
| `agent` | Creates/edits files, runs commands | Scaffolding, fixing, generating code |

---

## Prompts in This Project

| Slash command | File | Mode | Purpose |
|--------------|------|------|---------|
| `/review-code` | [review-code.prompt.md](../../.github/prompts/review-code.prompt.md) | `ask` | Code review against project standards |
| `/generate-feature` | [generate-feature.prompt.md](../../.github/prompts/generate-feature.prompt.md) | `agent` | Scaffold a full CQRS feature (command/query + handler + validator) |
| `/explain-architecture` | [explain-architecture.prompt.md](../../.github/prompts/explain-architecture.prompt.md) | `ask` | Explain layers, CQRS flow, EF Core model |

---

## Exercise 1 — Invoke an Existing Prompt (10 min)

### Step 1

Open `src/LeaveManagement.Application/Features/Employees/Commands/CreateEmployee/CreateEmployeeHandler.cs`.

### Step 2

In Copilot Chat, type `/` and wait for the menu to appear.

Select `/review-code`.

### Step 3

**Observe the structured output:**
- Severity levels: **Critical** (security/data loss), **Major** (architecture violation), **Minor** (style/idiom), **Info** (suggestion)
- Categories: Clean Architecture boundaries, C# 14 idioms, CQRS pattern, `[LoggerMessage]` vs interpolation, soft-delete safety, test coverage
- A **Summary** at the end with the most important finding

The review is consistent every time because the prompt defines exactly what to check.

---

## Exercise 2 — Scaffold a Feature with a Variable Prompt (15 min)

### Step 1

In Copilot Chat (Agent mode), type `/generate-feature`.

### Step 2

Copilot asks for the input variables. Enter:
- **Entity name**: `Department`
- **Feature type**: `Command`
- **Operation**: `Create`

### Step 3

Watch Copilot:
1. Create the folder `src/LeaveManagement.Application/Features/Departments/Commands/CreateDepartment/`
2. Generate `CreateDepartmentCommand.cs`, `CreateDepartmentHandler.cs`, `CreateDepartmentValidator.cs`
3. Add a mapper extension method stub in `Mappers/DepartmentMapper.cs`
4. Scaffold a unit test class with two `[Fact]` stubs

**Verify the output:**
- Handler class is `partial` and uses a primary constructor
- `[LoggerMessage]` partial method is declared for success logging
- Validation runs before any repository call
- `CancellationToken` is passed through every async call

**Verification:**
```bash
dotnet build
```
Build must succeed. If there are compile errors, ask Copilot to fix them in the same session.

---

## Exercise 3 — Explain the Architecture (10 min)

Type `/explain-architecture`.

When prompted for `scope`, enter: `CQRS flow`

**Observe:**
- Copilot traces a request from the HTTP call through the controller → `_mediator.Send()` → handler → repository → `ApplicationContext.SaveChangesAsync()`
- Produces a Mermaid sequence diagram
- References actual class names: `CreateEmployeeCommand`, `CreateEmployeeHandler`, `IEmployeeRepository`
- Explains why `BaseResponse` is returned instead of throwing exceptions

---

## Exercise 4 — Create a New Prompt from Scratch (25 min)

You will create an `/add-integration-scenario` prompt that scaffolds a Reqnroll BDD scenario.

### Step 1 — Create the file

Create `.github/prompts/add-integration-scenario.prompt.md`:

````markdown
---
name: add-integration-scenario
description: Scaffold a Reqnroll .feature file and matching step definitions for a given entity and HTTP operation
mode: agent
model: gpt-4o
---

# Add Integration Test Scenario — Leave Management API

Scaffold a complete BDD integration test for: **${input:entityName}** — **${input:httpMethod}** operation

## Step 1 — Plan the Scenario

Identify:
- The endpoint path (e.g. `POST /api/employees`)
- The happy-path Given/When/Then steps
- At least one error-path scenario (e.g. not found, validation failure)

## Step 2 — Generate the Feature File

Create or update `tests/LeaveManagement.API.IntegrationTests/Features/${entityName}API.feature`.
Include a happy-path scenario and one error scenario.

## Step 3 — Generate Step Definitions

Create or update `tests/LeaveManagement.API.IntegrationTests/StepDefinitions/${entityName}Steps.cs`:
- `[Binding]` attribute on the class
- Primary constructor injecting `ScenarioContext` and `CustomWebApplicationFactory`
- Use `ScenarioContext` to share state between steps — never static fields
- Assertions with FluentAssertions

## Constraints
- Follow the Reqnroll conventions in `tests.instructions.md`
- Step definitions must use `ScenarioContext` — no static state
````

### Step 2 — Test it

In Copilot Chat (Agent mode), type `/add-integration-scenario`.

Enter `Employee` and `POST` when prompted.

**Observe** how Copilot generates both the `.feature` file and the step definitions in one go, following the project's Reqnroll conventions.

---

## Key Takeaways

| Insight | Why It Matters |
|---------|---------------|
| Use `ask` for read-only tasks | No file changes — safe for review and explanation |
| Use `agent` for code generation | Copilot can create/edit files autonomously |
| `${input:variable}` makes prompts reusable | One prompt, many entities and operations |
| Prompts encode team workflows | Onboarding is faster when workflows are codified |
| Prompt + Instructions = consistent output | The prompt defines the task; instructions define the style |

---

## Comparison: Instructions vs. Prompts vs. Skills

| | Instructions | Prompts | Skills |
|-|-------------|---------|--------|
| Invocation | Automatic | `/name` | Automatic (intent match) |
| Purpose | Rules & conventions | Specific task templates | Procedural workflows with steps |
| Parameterised | No | Yes (`${input:x}`) | No |
| File type | `.instructions.md` | `.prompt.md` | `SKILL.md` |

---

## Quick Reference

```
.github/
└── prompts/
    ├── review-code.prompt.md                  ← /review-code (ask)
    ├── generate-feature.prompt.md             ← /generate-feature (agent)
    ├── explain-architecture.prompt.md         ← /explain-architecture (ask)
    └── add-integration-scenario.prompt.md     ← /add-integration-scenario (created in Exercise 4)
```

**Previous:** [Lab 02 — File-Based Instructions](./lab-02.md)
**Next:** [Lab 04 — Skills](./lab-04.md)
