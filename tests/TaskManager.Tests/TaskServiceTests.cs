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
}
