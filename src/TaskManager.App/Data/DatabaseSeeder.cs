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

        var admin = await userManager.FindByEmailAsync(DemoEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = DemoEmail,
                Email = DemoEmail,
                EmailConfirmed = true,
                DisplayName = "Demo Admin"
            };
            var result = await userManager.CreateAsync(admin, DemoPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

            await userManager.AddToRoleAsync(admin, "Admin");
        }

        if (await db.Companies.AnyAsync())
            return;

        var acme = new Company { Code = "ACME", Name = "Acme Software", Email = "ops@acme.demo", IsActive = true };
        var globex = new Company { Code = "GLX", Name = "Globex Consulting", Email = "pm@globex.demo", IsActive = true };
        db.Companies.AddRange(acme, globex);
        await db.SaveChangesAsync();

        var portal = new Project
        {
            CompanyId = acme.Id,
            Name = "Customer Portal",
            Description = "Rewrite of the legacy client portal",
            StartDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)),
            IsActive = true
        };
        var migration = new Project
        {
            CompanyId = globex.Id,
            Name = "ERP Migration",
            Description = "Data and process migration workstream",
            StartDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            IsActive = true
        };
        db.Projects.AddRange(portal, migration);
        await db.SaveChangesAsync();

        db.SubProjects.AddRange(
            new SubProject { ProjectId = portal.Id, Name = "Auth module", PlannedHours = 40 },
            new SubProject { ProjectId = portal.Id, Name = "Billing UI", PlannedHours = 60 },
            new SubProject { ProjectId = migration.Id, Name = "Data cleansing", PlannedHours = 80 });
        await db.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var tasks = new[]
        {
            new WorkTask
            {
                Title = "Implement task filters",
                Description = "Server-side filters for status, priority and overdue items",
                CompanyId = acme.Id,
                ProjectId = portal.Id,
                AssignedUserId = admin.Id,
                Priority = TaskPriority.High,
                Status = WorkTaskStatus.InProgress,
                RequestedDate = today.AddDays(-10),
                DueDate = today.AddDays(3),
                PlannedHours = 16
            },
            new WorkTask
            {
                Title = "Seed demo dashboard",
                CompanyId = acme.Id,
                ProjectId = portal.Id,
                AssignedUserId = admin.Id,
                Priority = TaskPriority.Medium,
                Status = WorkTaskStatus.Done,
                RequestedDate = today.AddDays(-20),
                DueDate = today.AddDays(-5),
                CompletedDate = today.AddDays(-6),
                PlannedHours = 8,
                ActualHours = 7.5m
            },
            new WorkTask
            {
                Title = "Fix overdue report export",
                CompanyId = globex.Id,
                ProjectId = migration.Id,
                AssignedUserId = admin.Id,
                Priority = TaskPriority.Critical,
                Status = WorkTaskStatus.Blocked,
                RequestedDate = today.AddDays(-15),
                DueDate = today.AddDays(-2),
                PlannedHours = 12
            },
            new WorkTask
            {
                Title = "Document CAPA workflow",
                CompanyId = globex.Id,
                ProjectId = migration.Id,
                Priority = TaskPriority.Low,
                Status = WorkTaskStatus.Pending,
                RequestedDate = today.AddDays(-3),
                DueDate = today.AddDays(10),
                PlannedHours = 6
            }
        };
        db.WorkTasks.AddRange(tasks);
        await db.SaveChangesAsync();

        db.TimeEntries.AddRange(
            new TimeEntry
            {
                WorkTaskId = tasks[0].Id,
                UserId = admin.Id,
                WorkDate = today.AddDays(-1),
                Hours = 3.5m,
                Notes = "Filter query + paging"
            },
            new TimeEntry
            {
                WorkTaskId = tasks[1].Id,
                UserId = admin.Id,
                WorkDate = today.AddDays(-6),
                Hours = 7.5m,
                Notes = "Dashboard cards"
            });

        var improvement = new Improvement
        {
            Title = "Reduce delayed task handoffs",
            Cause = "No shared status visibility across teams",
            CorrectiveAction = "Daily board review + overdue alerts",
            CompanyId = acme.Id,
            ProjectId = portal.Id,
            Status = ImprovementStatus.InProgress,
            OwnerUserId = admin.Id,
            ClosureCost = 0,
            OpenedDate = today.AddDays(-12)
        };
        db.Improvements.Add(improvement);
        await db.SaveChangesAsync();

        db.ImprovementActions.Add(new ImprovementAction
        {
            ImprovementId = improvement.Id,
            Description = "Introduced overdue filter on task board",
            ActionDate = today.AddDays(-4),
            PerformedByUserId = admin.Id
        });

        await db.SaveChangesAsync();
    }
}
