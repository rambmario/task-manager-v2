using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaskManager.App.Data;
using TaskManager.App.Features.Catalogs.Entities;
using TaskManager.App.Features.Tasks;
using TaskManager.App.Features.Tasks.Entities;

namespace TaskManager.Tests;

public class TaskServiceTests
{
    [Fact]
    public async Task SearchAsync_FiltersOverdueTasks()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new ApplicationDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var company = new Company { Code = "ACME", Name = "Acme" };
        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var project = new Project { CompanyId = company.Id, Name = "Portal" };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);
        db.WorkTasks.AddRange(
            new WorkTask
            {
                Title = "Overdue",
                CompanyId = company.Id,
                ProjectId = project.Id,
                DueDate = today.AddDays(-1),
                Status = WorkTaskStatus.InProgress,
                Priority = TaskPriority.High
            },
            new WorkTask
            {
                Title = "On track",
                CompanyId = company.Id,
                ProjectId = project.Id,
                DueDate = today.AddDays(5),
                Status = WorkTaskStatus.Pending,
                Priority = TaskPriority.Low
            });
        await db.SaveChangesAsync();

        var service = new TaskService(db);
        var result = await service.SearchAsync(new TaskQuery { OnlyOverdue = true, PageSize = 20 });

        Assert.Single(result.Items);
        Assert.Equal("Overdue", result.Items[0].Title);
    }

    [Fact]
    public async Task SearchAsync_FiltersByAssignee()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new ApplicationDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var company = new Company { Code = "ACME", Name = "Acme" };
        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var project = new Project { CompanyId = company.Id, Name = "Portal" };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        db.WorkTasks.AddRange(
            new WorkTask
            {
                Title = "Mine",
                CompanyId = company.Id,
                ProjectId = project.Id,
                AssignedUserId = "user-a",
                Status = WorkTaskStatus.Pending
            },
            new WorkTask
            {
                Title = "Theirs",
                CompanyId = company.Id,
                ProjectId = project.Id,
                AssignedUserId = "user-b",
                Status = WorkTaskStatus.Pending
            });
        await db.SaveChangesAsync();

        var service = new TaskService(db);
        var result = await service.SearchAsync(new TaskQuery { AssignedUserId = "user-a", PageSize = 20 });

        Assert.Single(result.Items);
        Assert.Equal("Mine", result.Items[0].Title);
    }

    [Fact]
    public async Task SearchAsync_SortsByTitleAscending()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new ApplicationDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var company = new Company { Code = "ACME", Name = "Acme" };
        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var project = new Project { CompanyId = company.Id, Name = "Portal" };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        db.WorkTasks.AddRange(
            new WorkTask { Title = "Zulu", CompanyId = company.Id, ProjectId = project.Id, Status = WorkTaskStatus.Pending },
            new WorkTask { Title = "Alpha", CompanyId = company.Id, ProjectId = project.Id, Status = WorkTaskStatus.Pending });
        await db.SaveChangesAsync();

        var service = new TaskService(db);
        var result = await service.SearchAsync(new TaskQuery
        {
            SortBy = TaskSortColumn.Title,
            SortDescending = false,
            PageSize = 20
        });

        Assert.Equal(["Alpha", "Zulu"], result.Items.Select(x => x.Title).ToArray());
    }

    [Fact]
    public async Task SearchAsync_HighPriorityAndOverdue_DoNotThrow()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new ApplicationDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var company = new Company { Code = "ACME", Name = "Acme" };
        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var project = new Project { CompanyId = company.Id, Name = "Portal" };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);
        db.WorkTasks.Add(new WorkTask
        {
            Title = "Hot",
            CompanyId = company.Id,
            ProjectId = project.Id,
            Priority = TaskPriority.Critical,
            Status = WorkTaskStatus.InProgress,
            DueDate = today.AddDays(-1)
        });
        await db.SaveChangesAsync();

        var service = new TaskService(db);
        var overdue = await service.SearchAsync(new TaskQuery { OnlyOverdue = true, PageSize = 10 });
        var high = await service.SearchAsync(new TaskQuery { HighPriorityOnly = true, PageSize = 10 });

        Assert.NotEmpty(overdue.Items);
        Assert.NotEmpty(high.Items);
    }

    [Fact]
    public async Task GetNextUpAsync_OrdersOverdueBeforePriority()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new ApplicationDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var company = new Company { Code = "ACME", Name = "Acme" };
        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var project = new Project { CompanyId = company.Id, Name = "Portal" };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);
        db.WorkTasks.AddRange(
            new WorkTask
            {
                Title = "Later critical",
                CompanyId = company.Id,
                ProjectId = project.Id,
                Priority = TaskPriority.Critical,
                Status = WorkTaskStatus.Pending,
                DueDate = today.AddDays(5)
            },
            new WorkTask
            {
                Title = "Overdue low",
                CompanyId = company.Id,
                ProjectId = project.Id,
                Priority = TaskPriority.Low,
                Status = WorkTaskStatus.InProgress,
                DueDate = today.AddDays(-1)
            },
            new WorkTask
            {
                Title = "Done ignored",
                CompanyId = company.Id,
                ProjectId = project.Id,
                Priority = TaskPriority.Critical,
                Status = WorkTaskStatus.Done,
                DueDate = today.AddDays(-2)
            });
        await db.SaveChangesAsync();

        var service = new TaskService(db);
        var next = await service.GetNextUpAsync(8);

        Assert.Equal(2, next.Count);
        Assert.Equal("Overdue low", next[0].Title);
        Assert.Equal("Later critical", next[1].Title);
    }
}
