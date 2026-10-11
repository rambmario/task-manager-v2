# Domain mapping — legacy → v2

| Legacy (VB.NET / SQL Server) | v2 entity / feature |
|------------------------------|---------------------|
| `Empresa` | `Company` / Catalogs |
| `PROYECTO` | `Project` / Catalogs |
| `Sub_proyecto` | `SubProject` / Catalogs |
| `TAREA` | `WorkTask` / Tasks |
| `Estado_tarea` / prioridad | `WorkTaskStatus` / `TaskPriority` enums |
| `Hora_real` | `TimeEntry` / TimeTracking |
| `Mejora` | `Improvement` / Improvements |
| `Mejora_accion` | `ImprovementAction` |
| `Usuario` + plaintext `clave` | ASP.NET Identity `ApplicationUser` |

## Intentionally deferred

- Google Calendar sync (`IDCalendar`)
- Crystal Reports
- Full Gantt planner tables
- Mailing / SMTP helpers from the WinForms `Inicio` module
