# Lab 04 — Primitive 4: Skills

**Duration:** 75 minutes
**Module:** Day 2 — Module 2
**Level:** Intermediate / Advanced

---

## Learning Objectives

By the end of this lab, you will be able to:
- Explain how skills differ from prompts and instructions
- Understand description-matching (how Copilot decides to load a skill)
- Use the two built-in project skills: `run-and-fix-tests` and `ef-core-migration`
- Write a new `SKILL.md` file following the agentskills.io spec
- Decide when to use a skill versus a prompt versus instructions

---

## Key Concept

> **Skills** are on-demand knowledge packages that Copilot loads automatically when your request **semantically matches** the skill's description. Unlike prompts, you never type `/`. Copilot decides to use a skill on its own — just by reading your intent.

### The discovery mechanism

Every Copilot session includes a catalogue of available skills (names + descriptions only).
When you send a message, Copilot matches your intent against those descriptions and loads the full content of the matching skill into context.

```
User: "run the tests and fix the failures"
                    ↓
Copilot checks skill catalogue → matches "run-and-fix-tests"
                    ↓
Loads full SKILL.md into context
                    ↓
Executes: dotnet test → parse output → suggest fixes
```

### File location & structure

```
.github/
└── skills/
    └── {skill-name}/           ← directory name = skill name
        └── SKILL.md            ← required: instructions + frontmatter
```

### SKILL.md frontmatter

```yaml
---
name: run-and-fix-tests          ← lowercase, hyphens only, matches folder name
description: "..."               ← THE most important field — triggers the match
argument-hint: "..."             ← placeholder hint shown in chat
---
```

### Description is everything

The description is what Copilot reads to decide whether to load the skill.

| Poor description | Effective description |
|------------------|-----------------------|
| `"Test runner skill"` | `"Run dotnet test, analyse failures, and suggest targeted code corrections. Use when the user wants to run tests, check test results, investigate test failures, or fix failing unit tests."` |

Rules for a good description:
- State **what** the skill does
- State **when** to use it (trigger phrases)
- Include action keywords users would naturally say: "run", "fix", "analyse", "add migration", "schema change"

---

## Skills in This Project

### Skill 1 — `run-and-fix-tests`

Location: [`.github/skills/run-and-fix-tests/SKILL.md`](../../.github/skills/run-and-fix-tests/SKILL.md)

**What it does:** Runs `dotnet test`, captures output, parses failures against a diagnosis table, and suggests targeted fixes.

**Trigger words:** `run tests`, `fix failing tests`, `dotnet test error`, `xUnit failure`, `Reqnroll failure`

### Skill 2 — `ef-core-migration`

Location: [`.github/skills/ef-core-migration/SKILL.md`](../../.github/skills/ef-core-migration/SKILL.md)

**What it does:** Guides through adding, reviewing, applying, rolling back, and removing EF Core migrations with the correct `--project` / `--startup-project` flags.

**Trigger words:** `add migration`, `EF migration`, `database schema change`, `dotnet ef`, `schema update`, `update database`

---

## Exercise 1 — Trigger a Skill by Intent (15 min)

### Step 1 — Do NOT type `/`

This is important. Skills activate automatically — no slash command needed.

In Copilot Chat (Agent mode), type:
```
run the tests and tell me if anything is failing
```

**Observe:**
1. Copilot recognises the intent matches `run-and-fix-tests`
2. Runs `dotnet test tests/LeaveManagement.Application.UnitTests --no-build --verbosity normal`
3. Parses the output — reports all green or lists failures
4. If failures found: explains root cause using the diagnosis table and suggests a fix

### Step 2 — Try different trigger phrases

```
why are my tests broken?
```
```
dotnet test is failing, help me debug it
```

Both should activate the same skill — demonstrating that the *intent* matches, not a keyword.

---

## Exercise 2 — Use the Migration Skill (15 min)

In Copilot Chat:
```
Check if dotnet ef tools are installed and add a migration called AddDepartmentEntity.
```

**Observe:**
- Copilot loads the `ef-core-migration` skill
- Runs `dotnet ef --version` to verify tools
- Generates the migration command with both flags:
  ```
  dotnet ef migrations add AddDepartmentEntity \
    --project src/LeaveManagement.Infrastructure \
    --startup-project src/LeaveManagement.API
  ```
- Instructs you to review the generated file in `src/LeaveManagement.Infrastructure/Migrations/` before applying

Then:
```
now apply the migration to the database
```

Copilot continues with the `dotnet ef database update ...` step — maintaining context from the previous turn.

---

## Exercise 3 — Read a SKILL.md and Understand Its Structure (10 min)

Open [`.github/skills/run-and-fix-tests/SKILL.md`](../../.github/skills/run-and-fix-tests/SKILL.md).

Answer these questions:
1. What is the `name` field? Does it match the folder name?
2. What action keywords appear in the `description`?
3. What are the steps the skill defines (Step 1 → Step 4)?
4. What is the diagnostic table mapping error patterns to likely causes?
5. What command does the skill recommend after a fix is applied?

---

## Exercise 4 — Write a New Skill from Scratch (35 min)

You will create a `setup-local-dev` skill.

### Step 1 — Create the folder and file

```
.github/skills/setup-local-dev/SKILL.md
```

### Step 2 — Write the frontmatter

```yaml
---
name: setup-local-dev
description: "Guide a developer through setting up the Leave Management API locally for the first time. Use when: get started, setup, first time, configure locally, run locally, prerequisites, local environment."
argument-hint: "Optional: specific step to jump to (e.g. 'connection string', 'Docker')"
---
```

### Step 3 — Write the skill body

Your skill should cover:

**Prerequisites:**
- .NET 10 SDK (`dotnet --version` should show `10.x.x`)
- Docker Desktop (required for integration tests)
- PostgreSQL running locally or via Docker

**Step-by-step setup:**
1. Build: `dotnet build`
2. Configure connection string in `src/LeaveManagement.API/appsettings.Development.json`:
   ```json
   "LeaveManagementConnectionString": "Host=localhost;Database=leavemanagement;Username=postgres;Password=yourpassword"
   ```
3. Run the API: `dotnet run --project src/LeaveManagement.API`
4. Verify: `GET /health` returns 200, `https://localhost:<port>/scalar/v1` opens the API docs

**Common setup errors table:**

| Error | Cause | Fix |
|-------|-------|-----|
| `connection refused` on startup | PostgreSQL not running | Start PostgreSQL service or `docker run -e POSTGRES_PASSWORD=pass -p 5432:5432 postgres:16-alpine` |
| `Database.EnsureCreatedAsync() failed` | Wrong connection string | Check username/password/database name in `appsettings.Development.json` |
| Integration tests fail immediately | Docker not running | Start Docker Desktop |

### Step 4 — Test it

In Copilot Chat:
```
How do I set up this project locally for the first time?
```

**Copilot should:**
1. Load the new `setup-local-dev` skill
2. Walk through prerequisites, build, connection string config, run, and verify steps
3. Reference the actual file `appsettings.Development.json` — not a generic path

---

## Key Takeaways

| Insight | Why It Matters |
|---------|---------------|
| Skills activate by intent, not by command | Lower friction than prompts for common workflows |
| Description quality = discoverability | A bad description = skill never loads |
| Skills are self-contained directories | Can include templates, scripts, reference data |
| Skills are portable (agentskills.io spec) | Work in VS Code, GitHub Copilot CLI, and Copilot coding agent |
| Skills vs. always-on instructions | Always-on = rules for everything; Skills = knowledge for specific tasks |

---

## Decision Guide: Skills vs. Prompts vs. Instructions

| Scenario | Use |
|----------|-----|
| "Always follow these coding conventions" | Always-On Instructions |
| "Run handler rules only when editing a handler file" | File-Based Instructions |
| "User explicitly triggers a task with `/`" | Prompt File |
| "User describes a task naturally, Copilot decides what to load" | Skill |
| "Task needs supporting templates or scripts alongside instructions" | Skill (directory structure) |

---

## Quick Reference

```
.github/
└── skills/
    ├── run-and-fix-tests/
    │   └── SKILL.md    ← "run tests", "fix failing tests"
    ├── ef-core-migration/
    │   └── SKILL.md    ← "add migration", "dotnet ef"
    └── setup-local-dev/
        └── SKILL.md    ← created in Exercise 4
```

**Previous:** [Lab 03 — Prompt Files](./lab-03.md)
**Next:** [Lab 05 — Custom Agents](./lab-05.md)
