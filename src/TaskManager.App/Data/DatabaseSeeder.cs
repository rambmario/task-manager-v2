using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.App.Features.Catalogs.Entities;
using TaskManager.App.Features.Improvements.Entities;
using TaskManager.App.Features.Tasks.Entities;
using TaskManager.App.Features.TimeTracking.Entities;

namespace TaskManager.App.Data;

public static class DatabaseSeeder
{
    public const string DemoEmail = "admin@demo.local";
    public const string DemoPassword = "Demo123!";
    public const string PmEmail = "pm@demo.local";
    public const string DevEmail = "dev@demo.local";

    private const int MinDemoTasks = 25;

    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in new[] { "Admin", "Member" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var admin = await EnsureUserAsync(userManager, roleManager, DemoEmail, "Demo Admin", "Admin");
        var pm = await EnsureUserAsync(userManager, roleManager, PmEmail, "Pat Manager", "Member");
        var dev = await EnsureUserAsync(userManager, roleManager, DevEmail, "Dana Developer", "Member");

        if (await db.WorkTasks.CountAsync() >= MinDemoTasks)
            return;

        await ClearDomainDataAsync(db);
        await SeedDomainAsync(db, admin.Id, pm.Id, dev.Id);
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        string email,
        string displayName,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = displayName
            };
            var result = await userManager.CreateAsync(user, DemoPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        if (!await userManager.IsInRoleAsync(user, role) && await roleManager.RoleExistsAsync(role))
            await userManager.AddToRoleAsync(user, role);

        return user;
    }

    private static async Task ClearDomainDataAsync(ApplicationDbContext db)
    {
        db.ImprovementActions.RemoveRange(db.ImprovementActions);
        db.Improvements.RemoveRange(db.Improvements);
        db.TimeEntries.RemoveRange(db.TimeEntries);
        db.WorkTasks.RemoveRange(db.WorkTasks);
        db.SubProjects.RemoveRange(db.SubProjects);
        db.Projects.RemoveRange(db.Projects);
        db.Companies.RemoveRange(db.Companies);
        await db.SaveChangesAsync();
    }

    private static async Task SeedDomainAsync(ApplicationDbContext db, string adminId, string pmId, string devId)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var companies = new[]
        {
            new Company { Code = "ACME", Name = "Acme Software", Email = "ops@acme.demo", IsActive = true },
            new Company { Code = "GLX", Name = "Globex Consulting", Email = "pm@globex.demo", IsActive = true },
            new Company { Code = "ING", Name = "Ingesoft Latam", Email = "contacto@ingesoft.demo", IsActive = true },
            new Company { Code = "NORD", Name = "Nordwind Logistics", Email = "it@nordwind.demo", IsActive = true },
            new Company { Code = "HELI", Name = "Helios Retail", Email = "projects@helios.demo", IsActive = false }
        };
        db.Companies.AddRange(companies);
        await db.SaveChangesAsync();

        var projects = new[]
        {
            new Project { CompanyId = companies[0].Id, Name = "Customer Portal", Description = "Rewrite of the legacy client portal", StartDate = today.AddMonths(-3), IsActive = true },
            new Project { CompanyId = companies[0].Id, Name = "Billing API", Description = "Invoicing and payment webhooks", StartDate = today.AddMonths(-1), IsActive = true },
            new Project { CompanyId = companies[1].Id, Name = "ERP Migration", Description = "Data and process migration workstream", StartDate = today.AddMonths(-2), IsActive = true },
            new Project { CompanyId = companies[1].Id, Name = "PMO Dashboard", Description = "Executive KPIs for delivery", StartDate = today.AddDays(-40), IsActive = true },
            new Project { CompanyId = companies[2].Id, Name = "Quality CAPA Suite", Description = "Improvements and corrective actions", StartDate = today.AddMonths(-4), IsActive = true },
            new Project { CompanyId = companies[2].Id, Name = "Field Service App", Description = "Mobile work orders for technicians", StartDate = today.AddDays(-20), IsActive = true },
            new Project { CompanyId = companies[3].Id, Name = "Warehouse WMS", Description = "Inventory and picking optimization", StartDate = today.AddMonths(-2), IsActive = true },
            new Project { CompanyId = companies[3].Id, Name = "Carrier Portal", Description = "Partner self-service for shipments", StartDate = today.AddDays(-15), IsActive = true },
            new Project { CompanyId = companies[4].Id, Name = "Legacy POS Bridge", Description = "Archived bridge (inactive company)", StartDate = today.AddMonths(-12), EndDate = today.AddMonths(-6), IsActive = false }
        };
        db.Projects.AddRange(projects);
        await db.SaveChangesAsync();

        var subProjects = new[]
        {
            new SubProject { ProjectId = projects[0].Id, Name = "Auth module", PlannedHours = 40 },
            new SubProject { ProjectId = projects[0].Id, Name = "Billing UI", PlannedHours = 60 },
            new SubProject { ProjectId = projects[0].Id, Name = "Notifications", PlannedHours = 24 },
            new SubProject { ProjectId = projects[1].Id, Name = "Webhook ingress", PlannedHours = 32 },
            new SubProject { ProjectId = projects[2].Id, Name = "Data cleansing", PlannedHours = 80 },
            new SubProject { ProjectId = projects[2].Id, Name = "Cutover rehearsal", PlannedHours = 40 },
            new SubProject { ProjectId = projects[4].Id, Name = "Cause analysis", PlannedHours = 20 },
            new SubProject { ProjectId = projects[4].Id, Name = "Action tracking", PlannedHours = 28 },
            new SubProject { ProjectId = projects[6].Id, Name = "Picking routes", PlannedHours = 50 },
            new SubProject { ProjectId = projects[6].Id, Name = "Cycle count", PlannedHours = 36 },
            new SubProject { ProjectId = projects[7].Id, Name = "Partner onboarding", PlannedHours = 30 }
        };
        db.SubProjects.AddRange(subProjects);
        await db.SaveChangesAsync();

        WorkTask T(
            string title,
            Company company,
            Project project,
            string? userId,
            TaskPriority priority,
            WorkTaskStatus status,
            int requestedDaysAgo,
            int? dueOffsetDays,
            decimal planned,
            decimal actual = 0,
            int? subProjectId = null,
            string? description = null)
        {
            DateOnly? due = dueOffsetDays is null ? null : today.AddDays(dueOffsetDays.Value);
            DateOnly? completed = status == WorkTaskStatus.Done
                ? today.AddDays(Math.Min(-1, (dueOffsetDays ?? -3)))
                : null;

            return new WorkTask
            {
                Title = title,
                Description = description,
                CompanyId = company.Id,
                ProjectId = project.Id,
                SubProjectId = subProjectId,
                AssignedUserId = userId,
                Priority = priority,
                Status = status,
                RequestedDate = today.AddDays(-requestedDaysAgo),
                DueDate = due,
                CompletedDate = completed,
                PlannedHours = planned,
                ActualHours = actual
            };
        }

        var tasks = new List<WorkTask>
        {
            T("Implement task filters", companies[0], projects[0], adminId, TaskPriority.High, WorkTaskStatus.InProgress, 12, 3, 16, 6, subProjects[0].Id, "Server-side filters for status, priority and overdue"),
            T("Seed recruiter dashboard", companies[0], projects[0], adminId, TaskPriority.Medium, WorkTaskStatus.Done, 25, -5, 8, 7.5m, subProjects[0].Id),
            T("Portal login polish", companies[0], projects[0], devId, TaskPriority.Medium, WorkTaskStatus.InProgress, 8, 5, 10, 4, subProjects[0].Id),
            T("Billing invoice PDF", companies[0], projects[0], pmId, TaskPriority.High, WorkTaskStatus.Pending, 6, 10, 14, 0, subProjects[1].Id),
            T("Email digest for overdue", companies[0], projects[0], devId, TaskPriority.Low, WorkTaskStatus.Pending, 4, 14, 6, 0, subProjects[2].Id),
            T("Stripe webhook retries", companies[0], projects[1], devId, TaskPriority.Critical, WorkTaskStatus.Blocked, 15, -1, 12, 3, subProjects[3].Id, "Waiting on vendor sandbox keys"),
            T("Payment reconciliation job", companies[0], projects[1], adminId, TaskPriority.High, WorkTaskStatus.InProgress, 9, 4, 20, 8, subProjects[3].Id),
            T("API rate-limit docs", companies[0], projects[1], pmId, TaskPriority.Low, WorkTaskStatus.Done, 18, -3, 4, 3.5m),

            T("Fix overdue report export", companies[1], projects[2], adminId, TaskPriority.Critical, WorkTaskStatus.Blocked, 16, -2, 12, 2, subProjects[4].Id),
            T("Document CAPA workflow", companies[1], projects[2], pmId, TaskPriority.Low, WorkTaskStatus.Pending, 5, 10, 6),
            T("Customer master mapping", companies[1], projects[2], devId, TaskPriority.High, WorkTaskStatus.InProgress, 11, 2, 24, 10, subProjects[4].Id),
            T("Dry-run cutover checklist", companies[1], projects[2], pmId, TaskPriority.High, WorkTaskStatus.Pending, 3, 7, 16, 0, subProjects[5].Id),
            T("Rollback runbook", companies[1], projects[2], adminId, TaskPriority.Medium, WorkTaskStatus.Done, 20, -6, 8, 9, subProjects[5].Id),
            T("KPI tile: cycle time", companies[1], projects[3], devId, TaskPriority.Medium, WorkTaskStatus.InProgress, 7, 6, 12, 5),
            T("Executive weekly PDF", companies[1], projects[3], pmId, TaskPriority.High, WorkTaskStatus.Pending, 2, 8, 10),
            T("Broken drill-down filter", companies[1], projects[3], adminId, TaskPriority.Critical, WorkTaskStatus.InProgress, 4, -1, 6, 1),

            T("Root-cause template library", companies[2], projects[4], pmId, TaskPriority.Medium, WorkTaskStatus.Done, 30, -10, 12, 11, subProjects[6].Id),
            T("Action due-date reminders", companies[2], projects[4], devId, TaskPriority.High, WorkTaskStatus.InProgress, 9, 5, 14, 6, subProjects[7].Id),
            T("Closure cost report", companies[2], projects[4], adminId, TaskPriority.Medium, WorkTaskStatus.Pending, 6, 12, 8, 0, subProjects[7].Id),
            T("Offline sync for work orders", companies[2], projects[5], devId, TaskPriority.Critical, WorkTaskStatus.InProgress, 14, 1, 30, 12),
            T("Photo attach compression", companies[2], projects[5], adminId, TaskPriority.Low, WorkTaskStatus.Pending, 3, 15, 5),
            T("Technician role matrix", companies[2], projects[5], pmId, TaskPriority.Medium, WorkTaskStatus.Blocked, 10, -3, 7, 1),

            T("Optimize picking routes", companies[3], projects[6], devId, TaskPriority.High, WorkTaskStatus.InProgress, 13, 4, 22, 9, subProjects[8].Id),
            T("Cycle count mobile UI", companies[3], projects[6], adminId, TaskPriority.Medium, WorkTaskStatus.Pending, 5, 9, 18, 0, subProjects[9].Id),
            T("ASN import validation", companies[3], projects[6], pmId, TaskPriority.High, WorkTaskStatus.Done, 22, -4, 10, 9.5m, subProjects[9].Id),
            T("Partner self-registration", companies[3], projects[7], devId, TaskPriority.Medium, WorkTaskStatus.InProgress, 8, 6, 16, 5, subProjects[10].Id),
            T("Shipment status webhooks", companies[3], projects[7], adminId, TaskPriority.High, WorkTaskStatus.Pending, 4, 11, 14, 0, subProjects[10].Id),
            T("SLA breach alerts", companies[3], projects[7], pmId, TaskPriority.Critical, WorkTaskStatus.Pending, 2, 3, 9),

            T("Archive POS bridge docs", companies[4], projects[8], adminId, TaskPriority.Low, WorkTaskStatus.Cancelled, 60, -40, 4, 2),
            T("Final data extract from POS", companies[4], projects[8], pmId, TaskPriority.Medium, WorkTaskStatus.Done, 90, -50, 20, 18)
        };

        db.WorkTasks.AddRange(tasks);
        await db.SaveChangesAsync();

        var timeEntries = new List<TimeEntry>();
        void Log(WorkTask task, string userId, int daysAgo, decimal hours, string notes)
        {
            timeEntries.Add(new TimeEntry
            {
                WorkTaskId = task.Id,
                UserId = userId,
                WorkDate = today.AddDays(-daysAgo),
                Hours = hours,
                Notes = notes
            });
        }

        Log(tasks[0], adminId, 1, 3.5m, "Filter query + paging");
        Log(tasks[0], adminId, 2, 2.5m, "Overdue highlighting");
        Log(tasks[1], adminId, 6, 7.5m, "Dashboard cards");
        Log(tasks[2], devId, 1, 4m, "Login UX polish");
        Log(tasks[5], devId, 3, 3m, "Webhook retry spikes");
        Log(tasks[6], adminId, 1, 4m, "Reconciliation batch");
        Log(tasks[6], adminId, 2, 4m, "Edge cases for refunds");
        Log(tasks[8], adminId, 4, 2m, "Export repro");
        Log(tasks[10], devId, 1, 5m, "Mapping rules");
        Log(tasks[10], devId, 2, 5m, "Customer duplicates");
        Log(tasks[13], devId, 1, 3m, "Cycle time SQL");
        Log(tasks[13], devId, 3, 2m, "Chart wiring");
        Log(tasks[17], devId, 1, 3m, "Reminder scheduler");
        Log(tasks[17], devId, 2, 3m, "Email template");
        Log(tasks[19], devId, 1, 6m, "Offline queue");
        Log(tasks[19], adminId, 2, 6m, "Conflict resolution");
        Log(tasks[22], devId, 1, 4.5m, "Route heuristics");
        Log(tasks[22], devId, 3, 4.5m, "Benchmarks");
        Log(tasks[24], pmId, 5, 4m, "Validation rules");
        Log(tasks[24], pmId, 6, 5.5m, "UAT with warehouse");
        Log(tasks[25], devId, 1, 5m, "Registration flow");

        db.TimeEntries.AddRange(timeEntries);

        foreach (var group in timeEntries.GroupBy(x => x.WorkTaskId))
        {
            var task = tasks.First(t => t.Id == group.Key);
            task.ActualHours = group.Sum(x => x.Hours);
            task.UpdatedAtUtc = DateTime.UtcNow;
        }

        var improvements = new[]
        {
            new Improvement
            {
                Title = "Reduce delayed task handoffs",
                Cause = "No shared status visibility across teams",
                CorrectiveAction = "Daily board review + overdue alerts",
                CompanyId = companies[0].Id,
                ProjectId = projects[0].Id,
                Status = ImprovementStatus.InProgress,
                OwnerUserId = adminId,
                ClosureCost = 0,
                OpenedDate = today.AddDays(-12)
            },
            new Improvement
            {
                Title = "Stabilize webhook retries",
                Cause = "Transient provider outages without backoff",
                CorrectiveAction = "Exponential backoff + dead-letter queue",
                CompanyId = companies[0].Id,
                ProjectId = projects[1].Id,
                Status = ImprovementStatus.Open,
                OwnerUserId = devId,
                ClosureCost = 0,
                OpenedDate = today.AddDays(-5)
            },
            new Improvement
            {
                Title = "Cutover checklist gaps",
                Cause = "Manual steps omitted during rehearsal",
                CorrectiveAction = "Automated checklist with owners",
                CompanyId = companies[1].Id,
                ProjectId = projects[2].Id,
                Status = ImprovementStatus.InProgress,
                OwnerUserId = pmId,
                ClosureCost = 1200,
                OpenedDate = today.AddDays(-18)
            },
            new Improvement
            {
                Title = "Incomplete CAPA evidence",
                Cause = "Actions closed without attached proof",
                CorrectiveAction = "Mandatory evidence upload before close",
                CompanyId = companies[2].Id,
                ProjectId = projects[4].Id,
                Status = ImprovementStatus.Closed,
                OwnerUserId = pmId,
                ClosureCost = 850,
                OpenedDate = today.AddDays(-45),
                ClosedDate = today.AddDays(-10)
            },
            new Improvement
            {
                Title = "Picking route inefficiencies",
                Cause = "Static aisle order ignoring SKU velocity",
                CorrectiveAction = "Velocity-based route planner",
                CompanyId = companies[3].Id,
                ProjectId = projects[6].Id,
                Status = ImprovementStatus.Open,
                OwnerUserId = adminId,
                ClosureCost = 0,
                OpenedDate = today.AddDays(-7)
            }
        };
        db.Improvements.AddRange(improvements);
        await db.SaveChangesAsync();

        db.ImprovementActions.AddRange(
            new ImprovementAction { ImprovementId = improvements[0].Id, Description = "Introduced overdue filter on task board", ActionDate = today.AddDays(-4), PerformedByUserId = adminId },
            new ImprovementAction { ImprovementId = improvements[0].Id, Description = "Added dashboard overdue card", ActionDate = today.AddDays(-2), PerformedByUserId = adminId },
            new ImprovementAction { ImprovementId = improvements[1].Id, Description = "Spike: compare Polly vs custom retry", ActionDate = today.AddDays(-3), PerformedByUserId = devId },
            new ImprovementAction { ImprovementId = improvements[2].Id, Description = "Drafted checklist with RACI", ActionDate = today.AddDays(-8), PerformedByUserId = pmId },
            new ImprovementAction { ImprovementId = improvements[2].Id, Description = "Reviewed with Ops lead", ActionDate = today.AddDays(-5), PerformedByUserId = pmId },
            new ImprovementAction { ImprovementId = improvements[3].Id, Description = "Shipped evidence gate in CAPA close", ActionDate = today.AddDays(-12), PerformedByUserId = pmId },
            new ImprovementAction { ImprovementId = improvements[3].Id, Description = "Closed after audit sample passed", ActionDate = today.AddDays(-10), PerformedByUserId = adminId },
            new ImprovementAction { ImprovementId = improvements[4].Id, Description = "Collected SKU velocity dataset", ActionDate = today.AddDays(-2), PerformedByUserId = adminId });

        await db.SaveChangesAsync();
    }
}
