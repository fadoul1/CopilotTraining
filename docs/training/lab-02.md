# Lab 02 — Primitive 2: File-Based Instructions

**Duration:** 45 minutes
**Module:** Day 1 — Module 3
**Level:** Foundational

---

## Learning Objectives

By the end of this lab, you will be able to:
- Explain the difference between always-on instructions and file-based instructions
- Create a `*.instructions.md` file with an `applyTo` glob pattern
- Observe context-specific activation when editing targeted files
- Design a scoped instruction strategy for different layers of an application

---

## Key Concept

> **File-Based Instructions** are like always-on instructions — but they only load when Copilot is working on files that **match a glob pattern**. They solve the "context pollution" problem: rules for test files don't need to load when you're writing production code.

### File location & naming

```
.github/
└── instructions/
    └── {name}.instructions.md     ← must end in .instructions.md
```

### Frontmatter

```yaml
---
applyTo: "**/*Tests.cs"               ← glob pattern (required)
---
```

### How it activates

| Always-On | File-Based |
|-----------|-----------|
| Loaded every session | Loaded when active file matches `applyTo` |
| Global rules | Scoped rules |
| `copilot-instructions.md` | `*.instructions.md` files |
| One file | Multiple files (one per concern) |

---

## The Instructions Files in This Project

Three instruction files are active:

| File | `applyTo` | Purpose |
|------|-----------|---------|
| [tests.instructions.md](../../.github/instructions/tests.instructions.md) | `**/*Tests.cs,**/*Test.cs` | xUnit + FluentAssertions + Reqnroll patterns |
| [handlers.instructions.md](../../.github/instructions/handlers.instructions.md) | `**/Features/**/*Handler.cs` | MediatR handler structure — primary constructor, `[LoggerMessage]`, validation-first |
| [migrations.instructions.md](../../.github/instructions/migrations.instructions.md) | `**/Migrations/**/*.cs` | EF Core migration safety — never hand-edit, descriptive names |

Open [`.github/instructions/tests.instructions.md`](../../.github/instructions/tests.instructions.md).

This file loads **only** when you are working on a file that ends in `Tests.cs` or `Test.cs`.
It adds test-specific rules on top of the always-on instructions:
- xUnit `[Fact]` / `[Theory]` usage and `[InlineData]` patterns
- `MethodName_StateUnderTest_ExpectedBehaviour` naming convention
- AAA pattern with `// Arrange / // Act / // Assert` comments
- FluentAssertions over raw `Assert.*` calls
- Explicit list of what NOT to do in tests

---

## Exercise 1 — Observe Scoped Activation (10 min)

### Step 1 — Open a production file

Open `src/LeaveManagement.Application/Features/Employees/Commands/CreateEmployee/CreateEmployeeHandler.cs`.

Ask Copilot:
```
What testing rules apply to this codebase?
```

Note the answer — it mentions the testing rules from the always-on instructions.

### Step 2 — Open a test file

Open `tests/LeaveManagement.Application.UnitTests/` (or any file ending in `Tests.cs`).

Ask the same question:
```
What testing rules apply to this file?
```

**Expected result:** Copilot now gives a richer, more detailed answer that includes the test-specific rules from `tests.instructions.md` — because the active file matches `**/*Tests.cs`.

---

## Exercise 2 — Generate a Test with Scoped Rules Active (15 min)

While a `*Tests.cs` file is the active file, ask:
```
Add a test for the GetEmployeeById handler — test the case where the employee does not exist.
```

**Observe that Copilot automatically:**
- Names the method `HandleAsync_EmployeeNotFound_ReturnsFailureResponse` (or equivalent `MethodName_StateUnderTest_ExpectedBehaviour` form)
- Uses `[Fact]` (xUnit), not `[Test]` or `[TestMethod]`
- Asserts with FluentAssertions: `result.Should().NotBeNull()` — not `Assert.NotNull(result)`
- Follows the AAA pattern with `// Arrange / // Act / // Assert` comments

**Now switch back** to a production handler file and ask for the same thing. Observe the difference — the test-specific rules no longer activate.

---

## Exercise 3 — Create a New File-Based Instruction (20 min)

You will create a new scoped instruction for the **validator layer**.

### Step 1 — Create the file

Ask Copilot:

```
Create a file-based instruction at .github/instructions/validators.instructions.md
that applies to **/Features/**/*Validator.cs files. It should enforce:
- AbstractValidator<T> as the base class
- RuleFor() for every required property in the command/query
- Custom error messages on every rule — never rely on auto-generated messages
- No async rules unless the validation genuinely requires a database lookup
Include a ✅ correct and ❌ incorrect example for the custom message rule.
```

### Step 2 — Test it

Open `src/LeaveManagement.Application/Features/Employees/Commands/CreateEmployee/CreateEmployeeValidator.cs`. Ask:
```
Add a validation rule that ensures the Email property is a valid email address and is required.
```

**Observe:** Copilot includes a custom error message (e.g., `"Email must be a valid email address."`) and does not use async validation — all from the scoped instruction.

---

## Key Takeaways

| Insight | Why It Matters |
|---------|---------------|
| `applyTo` is a glob pattern | Precise control over when rules activate |
| Keeps always-on instructions lean | Only universal rules go in `copilot-instructions.md` |
| Multiple files per concern | Separate files for tests, handlers, migrations, validators |
| Additive, not replacing | File-based rules stack on top of always-on rules |

---

## Quick Reference

```
.github/
├── copilot-instructions.md                  ← Primitive 1 (always-on)
└── instructions/
    ├── tests.instructions.md                ← Primitive 2 (applyTo: **/*Tests.cs)
    ├── handlers.instructions.md             ← Primitive 2 (applyTo: **/Features/**/*Handler.cs)
    ├── migrations.instructions.md           ← Primitive 2 (applyTo: **/Migrations/**/*.cs)
    └── validators.instructions.md           ← Primitive 2 (created in Exercise 3)
```

---

## Decision Guide: Always-On vs. File-Based

| Rule type | Where to put it |
|-----------|----------------|
| Applies to ALL C# code | `copilot-instructions.md` |
| Applies only to test files | `tests.instructions.md` with `applyTo: "**/*Tests.cs"` |
| Applies only to MediatR handlers | `handlers.instructions.md` with `applyTo: "**/Features/**/*Handler.cs"` |
| Applies only to EF Core migrations | `migrations.instructions.md` with `applyTo: "**/Migrations/**/*.cs"` |
| Applies only to FluentValidation validators | `validators.instructions.md` with `applyTo: "**/Features/**/*Validator.cs"` |

**Previous:** [Lab 01 — Always-On Instructions](./lab-01.md)
**Next:** [Lab 03 — Prompt Files](./lab-03.md)
