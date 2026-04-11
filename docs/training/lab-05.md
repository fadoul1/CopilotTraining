# Lab 05 — Primitive 5: Custom Agents

**Duration:** 75 minutes
**Module:** Day 2 — Module 3
**Level:** Advanced

---

## Learning Objectives

By the end of this lab, you will be able to:
- Explain what custom agents are and when they outperform default Copilot
- Activate a custom agent and observe how its persona shapes the output
- Use handoffs to chain agents in a workflow
- Create a new custom agent for a targeted engineering role
- Understand the five required sections of an agent persona

---

## Key Concept

> **Custom Agents** are specialized AI personas with defined behaviours and consistent response styles. Where instructions say "how to write code", agents define "how to think and respond". A custom agent is a specialist you can summon — a security reviewer, a refactoring expert, an upgrade guide.

### Instructions vs. Custom Agents

| Always-On Instructions | Custom Agent |
|------------------------|-------------|
| "Use primary constructors" | "I am a refactoring expert. I diagnose violations." |
| Passive style rules | Active persona with methodology |
| Always in context | Activated by user with `@agent-name` |
| No conversation memory | Maintains persona throughout session |

### File location & naming

```
.github/
└── agents/
    └── {name}.agent.md        ← any .md file in agents/ folder
```

### Frontmatter fields

```yaml
---
name: clean-architecture-refactor-expert   ← shown in agent picker
description: '...'                         ← shown as placeholder text in chat
---
```

### The Agent Formula

```
Who You Are      → Persona + background
How You Think    → Methodology + questions you always ask
How You Respond  → Output structure + format
What You Always Do → Consistent behaviours
What You Never Do  → Guardrails
```

---

## Agents in This Project

### Agent 1 — Clean Architecture Refactor Expert

File: [`.github/agents/clean-architecture-refactor-expert.agent.md`](../../.github/agents/clean-architecture-refactor-expert.agent.md)

**Persona:** Senior .NET architect with 20+ years experience  
**Specialty:** Clean Architecture violations, CQRS anti-patterns, EF Core misuse, C# 14 idiom gaps  
**Handoffs:** → Run Tests → Review Code

### Agent 2 — .NET Upgrade Expert

File: [`.github/agents/dotnet-upgrade-expert.agent.md`](../../.github/agents/dotnet-upgrade-expert.agent.md)

**Persona:** Principal .NET engineer specialised in version migrations  
**Specialty:** .NET 6/7/8 → .NET 10, `Startup.cs` → minimal API, `SpecFlow` → Reqnroll, Swashbuckle → Scalar  
**Handoffs:** → Build After Upgrade → Run Tests

---

## Exercise 1 — Activate and Compare Agents (15 min)

### Step 1 — Default Copilot

Open `src/LeaveManagement.Application/Features/Employees/Commands/CreateEmployee/CreateEmployeeHandler.cs`. In the **default** Copilot agent mode, ask:
```
Review this file and suggest improvements.
```

Note the tone, structure, and depth of the response.

### Step 2 — Clean Architecture Refactor Expert

Switch to the **Clean Architecture Refactor Expert** agent (click the agent picker in Copilot Chat, or prefix with `@clean-architecture-refactor-expert`).

Ask the same question:
```
Review this file and suggest improvements.
```

**Observe the difference:**
- The agent starts with a formal **Violation Table** (Location | Violation | Severity | Rule Violated)
- Follows with a numbered **Refactoring Plan** — each step leaves the project compilable
- Ends with **Code Diffs** — before/after for each step
- Handoff suggestion appears: "Run `/run-and-fix-tests` to verify nothing broke"

---

## Exercise 2 — Use Handoffs (10 min)

Still in the **Clean Architecture Refactor Expert** session after Exercise 1:

1. Note the handoff suggestion at the bottom of the response
2. Follow the handoff: ask the agent to verify nothing broke after the suggested refactoring
3. Observe how the agent transitions to a test-verification step

Then test the **"What You Never Do"** guardrails:
```
Can you move the validation logic out of the handler into a static utility class?
```

**Observe:** The agent should refuse `static` dependencies and explain why DI is the correct pattern — even though the user explicitly requested it.

---

## Exercise 3 — Run an Upgrade Assessment with the .NET Expert (15 min)

Switch to the **.NET Upgrade Expert** agent.

Ask:
```
This project is on .NET 10 already, but I want to understand what would need
to change if we were upgrading from .NET 8 to .NET 10. Give me the audit and plan.
```

**The agent will:**
1. Produce a **Current State Audit** table with package versions vs. target versions
2. Produce an **Upgrade Plan** using the .NET 8→10 migration cheat sheet:
   - `Startup.cs` → `Program.cs` minimal API
   - `SpecFlow` → `Reqnroll`
   - `Swashbuckle` UI → Scalar
   - `<Nullable>enable</Nullable>` flag
3. Produce a **Verification Checklist** with `dotnet build` and `dotnet test` steps

**Key observation:** The agent works **incrementally** — it warns against upgrading all packages in one commit. Ask it: "What would happen if I upgraded everything at once?" and observe the guardrail response.

---

## Exercise 4 — Create a New Custom Agent (35 min)

You will create a **Security Reviewer** agent.

### Step 1 — Create the file

Create `.github/agents/security-reviewer.agent.md`:

```markdown
---
name: security-reviewer
description: "Reviews code for security vulnerabilities — OWASP Top 10, input validation, hardcoded secrets, logging of PII, EF Core parameterisation. Use when you want a security-focused code review."
---

# Who You Are

You are a senior application security engineer with expertise in .NET and ASP.NET Core.
You have performed security reviews on 50+ enterprise applications.
You think in terms of **attack vectors**, not just code style.

# How You Think

You always ask:
1. Can an attacker control this input and reach a database query?
2. Can this log statement leak PII (name, email, employee data)?
3. Is there a hardcoded secret or connection string in this file?
4. Does this endpoint require authentication?
5. Does the CORS policy restrict origins appropriately?

# How You Respond

1. **Risk Assessment** — start with overall `CRITICAL / HIGH / MEDIUM / LOW`
2. **Finding List** — for each issue:
   - Vulnerability type (e.g. "PII in logs", "Hardcoded secret")
   - Affected file + line
   - Attack scenario: "An attacker could..."
   - Recommended fix with C# code example
3. **Positive Notes** — acknowledge what is done well
4. **Summary** — overall security posture + top priority to fix

# What You Always Do

- Provide a concrete fix for every finding — not just a description
- Check that `"LeaveManagementConnectionString"` comes from environment variables, not hardcode
- Verify no `ExecuteSqlRaw()` with unsanitised input
- Verify `[LoggerMessage]` is used (not string interpolation that could leak PII)
- Flag `AllowAny*` CORS on non-development configurations

# What You Never Do

- Never mark something as a vulnerability without evidence in the code
- Never suggest adding security logic to controllers — it belongs in middleware or the application layer
- Never recommend disabling validation or CORS without documenting the risk
- Never overlook the logging rules — PII in logs is a compliance violation
```

### Step 2 — Test it

Switch to your new **Security Reviewer** agent. Open `src/LeaveManagement.API/StartupExtensions.cs` and ask:
```
Review this file for security issues.
```

**The agent should check:**
- Is the CORS policy `AllowAny*` (flag it)?
- Are connection strings coming from configuration (not hardcoded)?
- Are health check endpoints unrestricted?
- Any middleware ordering problems that bypass auth?

---

## Key Takeaways

| Insight | Why It Matters |
|---------|---------------|
| Persona specificity produces consistency | Vague agents produce generic answers |
| "What You Never Do" prevents persona drift | Guards are as important as capabilities |
| Handoffs chain specialists together | Avoids context bloat — each agent stays focused |
| `@agent-name` maintains persona for the full session | Context accumulates — later questions benefit from earlier turns |
| Agents vs. instructions: agents are active, instructions are passive | Use agents for multi-turn expert sessions; instructions for ambient rules |

---

## Agents vs. Prompts — Quick Decision Guide

| Use a **Prompt** when | Use a **Custom Agent** when |
|-----------------------|-----------------------------|
| One-shot specific task (scaffold a feature) | Entire conversation needs a persona (security review) |
| You need fill-in-the-blank variables | Persona is fixed at session start |
| The task is the same every time | The expert needs to ask questions and iterate |
| Short execution (single response) | Long, multi-step engagement |

---

## Quick Reference

```
.github/
└── agents/
    ├── clean-architecture-refactor-expert.agent.md  ← @clean-architecture-refactor-expert
    ├── dotnet-upgrade-expert.agent.md               ← @dotnet-upgrade-expert
    └── security-reviewer.agent.md                   ← created in Exercise 4
```

**Previous:** [Lab 04 — Skills](./lab-04.md)
**Next:** [Lab 06 — End-To-End Workflow](./lab-06.md)
