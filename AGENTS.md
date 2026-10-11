# Agent / contributor rules — Task Manager v2

## Layout

- **`TaskManager.App`**: domain entities, EF Core, feature services, seed data.
- **`TaskManager.Web`**: Blazor UI + Identity endpoints only. Keep it thin.
- Organize by **feature folder** (`Features/Tasks`, `Features/Catalogs`, …), not by technical layer projects.

## Write path

UI components call **feature services** (`TaskService`, `CompanyService`, …).  
Do not run ad-hoc `DbContext` mutations from `.razor` pages except via those services.

## Data

- Default provider: **SQLite** for clone-and-run demos.
- Schema changes: EF migrations in `src/TaskManager.App/Data/Migrations`.
- Never commit real secrets, SMTP passwords, or production dumps.

## Auth

- ASP.NET Core Identity (`ApplicationUser`).
- Demo seed user is created in `DatabaseSeeder` — do not weaken password rules in production configs.

## UI

- Prefer Interactive Server pages under `Components/Features/...`.
- Keep dashboard / CRUD readable for recruiters; avoid demo Counter/Weather leftovers.

## Tests

- Fast tests live in `tests/TaskManager.Tests` (in-memory SQLite).
- Run: `dotnet test`.
