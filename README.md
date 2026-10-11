# Task Manager v2

Modern rewrite of a **VB.NET WinForms + SQL Server** task / time-tracking / improvement system, rebuilt as a **.NET 10 Blazor Web App** for recruiters and portfolio review.

Legacy desktop app: [rambmario/task-manager](https://github.com/rambmario/task-manager)

## Why this rewrite

| Legacy (v1) | This repo (v2) |
|-------------|----------------|
| WinForms + global module state | Blazor Interactive Server + DI |
| Plaintext passwords | ASP.NET Core Identity (hashed) |
| ADO.NET + ~232 stored procedures | EF Core + focused services |
| Crystal Reports / Excel COM | ClosedXML export endpoint |
| Single VB project | `App` library + thin `Web` host (feature folders) |

## Stack

- .NET 10 / C#
- Blazor Web App (Interactive Server)
- EF Core + SQLite (zero-friction local demo; SQL Server-ready model)
- ASP.NET Core Identity + roles
- ClosedXML (Excel export)
- xUnit

## Features

- **Dashboard** — KPI cards that deep-link into filtered task lists, plus a **Next up** board (overdue → priority → due date)
- **Tasks**
  - CRUD with **assignee** (legacy `id_usuario_asignado`)
  - Filters: search, company, status, assignee / My tasks, open, high priority, overdue
  - **Sortable columns**, **Clear filters**, search with **Enter**
  - Server-side paging, overdue highlighting, Excel export
- **Time tracking** — log hours against tasks and refresh actual hours
- **Improvements (CAPA)** — cause, corrective action, closure cost, action notes
- **Catalogs** — companies, projects and sub-projects

## Quick start

Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/download)

```bash
git clone https://github.com/rambmario/task-manager-v2.git
cd task-manager-v2
dotnet run --project src/TaskManager.Web
```

Open the HTTPS/HTTP URL from the console (see `launchSettings.json`, typically `https://localhost:7108`).

```bash
dotnet test
```

### Demo login

Password for all demo users: `Demo123!`

| Email | Role |
|-------|------|
| `admin@demo.local` | Admin |
| `pm@demo.local` | Member (PM) |
| `dev@demo.local` | Member (Developer) |

The first run (or when the demo task count is low) applies migrations and seeds a rich sample dataset: 5 companies, 9 projects, 11 sub-projects, ~30 tasks, time entries and 5 CAPA-style improvements.

## Solution layout

```
src/
  TaskManager.App/          # domain, EF Core, feature services
    Features/
      Catalogs/
      Tasks/
      TimeTracking/
      Improvements/
      Identity/             # user directory for assignee pickers
    Platform/               # paging helpers
    Data/                   # DbContext, Identity user, seed, migrations
  TaskManager.Web/          # Blazor UI + Identity UI (thin host)
tests/
  TaskManager.Tests/        # unit tests (SQLite in-memory)
```

Architecture ideas (feature folders, thin host, service write-path) are inspired by a larger internal .NET platform refactor — without copying its source-generator stack.

## Docs

- [docs/DOMAIN.md](docs/DOMAIN.md) — legacy → v2 mapping
- [AGENTS.md](AGENTS.md) — conventions for AI/human contributors

## Roadmap

- [ ] SQL Server provider switch + Docker Compose
- [ ] QuestPDF printable reports
- [ ] Google Calendar OAuth (replacing legacy desktop flow)
- [ ] Gantt / capacity view

## License

MIT — see [LICENSE](LICENSE).
