# Training Labs — Advanced GitHub Copilot Mastery

**Project:** Leave Management API (.NET 10 / C# 14)
**Total Duration:** 2 Days — ~7 Hours

---

## Lab Index

| Lab | Primitive | Duration | Day | Module |
|-----|-----------|----------|-----|--------|
| [Lab 01](./lab-01.md) | Always-On Instructions | 45 min | Day 1 | Module 2 |
| [Lab 02](./lab-02.md) | File-Based Instructions | 45 min | Day 1 | Module 3 |
| [Lab 03](./lab-03.md) | Prompt Files (Slash Commands) | 60 min | Day 1 | Module 4 |
| [Lab 04](./lab-04.md) | Skills | 75 min | Day 2 | Module 1 |
| [Lab 05](./lab-05.md) | Custom Agents | 75 min | Day 2 | Module 3 |
| [Lab 06](./lab-06.md) | End-to-End Workflow (Capstone) | 70 min | Day 2 | Module 4 |

---

## Primitives Quick Reference

| Primitive | File/Location | When it activates |
|-----------|--------------|-------------------|
| Always-On Instructions | `.github/copilot-instructions.md` | Every Copilot Chat session |
| File-Based Instructions | `.github/instructions/*.instructions.md` | When `applyTo` pattern matches the active file |
| Prompt Files | `.github/prompts/*.prompt.md` | User types `/name` in chat |
| Skills | `.github/skills/*/SKILL.md` | Copilot matches user's intent to skill's `description` |
| Custom Agents | `.github/agents/*.agent.md` | User selects from agent picker or `@agent-name` in chat |

---

## Prerequisites

Before attending, ensure:

- [ ] **.NET 10 SDK** installed — `dotnet --version` should show `10.x.x`
- [ ] **Docker Desktop** running — required for integration tests (Testcontainers)
- [ ] **VS Code** with **GitHub Copilot Chat** extension (agent mode enabled)
- [ ] **EF Core tools** — `dotnet tool install --global dotnet-ef`

Quick check:
```bash
dotnet --version      # 10.x.x
docker --version      # 20.x or later
dotnet ef --version   # 10.x.x
```

---

## Files Created in This Repo

```
.github/
├── copilot-instructions.md                              ← Lab 01 — Primitive 1
├── instructions/
│   ├── tests.instructions.md                           ← Lab 02 — Primitive 2
│   ├── handlers.instructions.md                        ← Lab 02 — Primitive 2
│   └── migrations.instructions.md                      ← Lab 02 — Primitive 2
├── prompts/
│   ├── review-code.prompt.md                           ← Lab 03 — Primitive 3
│   ├── generate-feature.prompt.md                      ← Lab 03 — Primitive 3
│   └── explain-architecture.prompt.md                  ← Lab 03 — Primitive 3
├── skills/
│   ├── run-and-fix-tests/
│   │   └── SKILL.md                                    ← Lab 04 — Primitive 4
│   └── ef-core-migration/
│       └── SKILL.md                                    ← Lab 04 — Primitive 4
└── agents/
    ├── clean-architecture-refactor-expert.agent.md     ← Lab 05 — Primitive 5
    └── dotnet-upgrade-expert.agent.md                  ← Lab 05 — Primitive 5

docs/training/
├── README.md                                           ← this file
├── lab-01.md                                           ← Always-On Instructions
├── lab-02.md                                           ← File-Based Instructions
├── lab-03.md                                           ← Prompt Files
├── lab-04.md                                           ← Skills
├── lab-05.md                                           ← Custom Agents
└── lab-06.md                                           ← Capstone
```

---

## Suggested Day Schedule

### Day 1 — Foundations (2.5 hours)
| Time | Activity |
|------|----------|
| 0:00–0:15 | Introduction: What are Copilot customization primitives? |
| 0:15–1:00 | Lab 01 — Always-On Instructions |
| 1:00–1:45 | Lab 02 — File-Based Instructions |
| 1:45–2:30 | Lab 03 — Prompt Files |

### Day 2 — Advanced Primitives + Capstone (3.7 hours)
| Time | Activity |
|------|----------|
| 0:00–1:15 | Lab 04 — Skills |
| 1:15–2:30 | Lab 05 — Custom Agents |
| 2:30–3:40 | Lab 06 — End-to-End Capstone |
| 3:40–4:10 | Debrief discussion + "What's next for your team?" |
