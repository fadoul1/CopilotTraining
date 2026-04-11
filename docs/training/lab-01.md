# Lab 01 — Primitive 1: Always-On Instructions

**Duration:** 45 minutes
**Module:** Day 1 — Module 2
**Level:** Foundational

---

## Learning Objectives

By the end of this lab, you will be able to:
- Explain what the `copilot-instructions.md` file does and when it loads
- Create a repository-level instructions file that shapes Copilot's behaviour
- Verify that Copilot is reading and applying the instructions
- Iterate and refine instructions to improve output quality

---

## Key Concept

> **Always-On Instructions** are the foundation of Copilot customisation. They are loaded automatically at the start of **every** Copilot Chat session in the repository. You never have to reference them manually.

### File location

```
.github/copilot-instructions.md    ← Shared with the team (committed to Git)
```

### What it does

Without `copilot-instructions.md`, Copilot treats every repository identically. It does not know:
- That you use **MediatR** (not service classes calling repositories directly)
- That your exception handling returns `BaseResponse { Success = false }` (not thrown exceptions)
- That constructor injection via primary constructors is required
- That tests must follow the `MethodName_StateUnderTest_ExpectedBehaviour` naming pattern

With the file, all of this is **permanent context** — Copilot knows it before you type a single word.

### What it does NOT affect

Inline suggestions (ghost text as you type) are **not** affected. The file only influences Copilot **Chat** interactions.

---

## The Instructions File in This Project

Open [`.github/copilot-instructions.md`](../../.github/copilot-instructions.md).

Notice the structure:

| Section | Purpose |
|---------|---------|
| **Project Overview** | Tells Copilot what this project is |
| **Tech Stack** | Prevents wrong framework or package suggestions |
| **Architecture & Layering** | Enforces Clean Architecture dependency boundaries |
| **C# 14 & .NET 10 Conventions** | Primary constructors, collection expressions, nullable types |
| **CQRS & MediatR Conventions** | Naming, handler structure, validator injection |
| **EF Core Conventions** | Soft deletes, audit timestamps, migration approach |
| **Logging Standards** | `[LoggerMessage]` source-generated vs string interpolation |
| **Security Requirements** | Guardrails around secrets, queries, and PII |
| **Testing Requirements** | Test framework, naming, AAA pattern |
| **What NOT to Do** | Explicit anti-patterns to avoid |
| **Why These Rules Exist** | Rationale — helps Copilot make better edge-case decisions |

---

## Exercise 1 — Verify the Instructions Load (10 min)

### Step 1 — Open Copilot Chat

Press `Ctrl+Alt+I` (or click the Copilot icon in the sidebar).

### Step 2 — Test without context

Ask Copilot:
```
What are the coding standards for this project?
```

**Expected result:** Copilot lists the conventions from `copilot-instructions.md` — primary constructors, `[LoggerMessage]`, soft deletes, test naming, etc.

If it doesn't, check:
- The file is named exactly `copilot-instructions.md`
- It is in the `.github/` folder at the workspace root
- VS Code setting `chat.includeApplyingInstructions` is `true` (default)

---

## Exercise 2 — See Copilot Enforce the Rules (15 min)

### Scenario: New handler method

Ask Copilot in Agent mode:
```
Add a method to get an employee by email address. It should search by email,
return an EmployeeResponse, and return a failure response if not found.
```

**Observe:**
- Does Copilot use a primary constructor (not `private readonly` + constructor body)?
- Does it use the mapper extension method `employee.ToEmployeeResponse()` (not inline mapping)?
- Does it add a `[LoggerMessage]` partial method for logging?
- Does it return `BaseResponse { Success = false }` for not-found (not throw an exception)?

If yes — the instructions are working.

---

## Exercise 3 — Break a Rule Deliberately (10 min)

Ask Copilot:
```
Show me how to inject IEmployeeRepository using a private readonly field and
a traditional constructor body.
```

**Expected result:** Copilot refuses or immediately flags this as violating the project standard and offers the primary constructor alternative instead.

Now ask:
```
Write a handler that logs the employee's full name and email address on every request.
```

**Expected result:** Copilot flags the PII logging anti-pattern (from the Security Requirements section) and suggests logging a non-sensitive identifier such as the employee ID instead.

---

## Exercise 4 — Iterate on the Instructions (10 min)

The instructions file is a living document. Add a new rule together:

1. Open `.github/copilot-instructions.md`
2. Add this section at the bottom:

```markdown
## Department Rules (Added in Lab)
- Employees must belong to a department.
- Department names must come from the `DepartmentType` enum — never accept a raw string.
- `GetEmployeesByDepartment` queries must filter by `DeletedAt == default` (soft-delete rule applies).
```

3. Ask Copilot again:
```
Add a department filter to the GetEmployeesList query.
```

**Observe** how the suggestion changes compared to before the instruction was added — Copilot should now reference the enum constraint and respect the soft-delete filter automatically.

---

## Key Takeaways

| Insight | Why It Matters |
|---------|---------------|
| Instructions load automatically | No prompting overhead for every request |
| Include rationale (not just rules) | Copilot makes better edge-case decisions with context |
| Use ✅/❌ examples | Concrete patterns outperform abstract rules |
| Keep it under ~2000 words | Longer files dilute the important rules |
| Review quarterly | Instructions drift from reality if not maintained |

---

## Quick Reference

```
.github/
└── copilot-instructions.md   ← Primitive 1 (always-on, every session)
```

**Next:** [Lab 02 — File-Based Instructions](./lab-02.md)
