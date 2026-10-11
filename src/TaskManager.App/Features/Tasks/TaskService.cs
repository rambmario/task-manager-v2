using Microsoft.EntityFrameworkCore;
using TaskManager.App.Data;
using TaskManager.App.Features.Improvements.Entities;
using TaskManager.App.Features.Tasks.Entities;
using TaskManager.App.Platform.Paging;

namespace TaskManager.App.Features.Tasks;

public sealed class TaskService(ApplicationDbContext db)
{
    public async Task<PagedResult<WorkTask>> SearchAsync(TaskQuery query, CancellationToken ct = default)
    {
        var q = db.WorkTasks.AsNoTracking()
            .Include(x => x.Company)
            .Include(x => x.Project)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(x => x.Title.Contains(term) || (x.Description != null && x.Description.Contains(term)));
        }

        if (query.CompanyId is not null)
            q = q.Where(x => x.CompanyId == query.CompanyId);

        if (query.ProjectId is not null)
            q = q.Where(x => x.ProjectId == query.ProjectId);

        if (query.Status is not null)
            q = q.Where(x => x.Status == query.Status);

        if (query.Priority is not null)
            q = q.Where(x => x.Priority == query.Priority);

        if (query.OnlyOverdue)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            q = q.Where(x => x.DueDate != null && x.DueDate < today && x.Status != WorkTaskStatus.Done && x.Status != WorkTaskStatus.Cancelled);
        }

        var total = await q.CountAsync(ct);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 5, 100);

        var items = await q
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.DueDate)
            .ThenBy(x => x.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<WorkTask>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public Task<WorkTask?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.WorkTasks
            .Include(x => x.Company)
            .Include(x => x.Project)
            .Include(x => x.SubProject)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<WorkTask> CreateAsync(WorkTask task, CancellationToken ct = default)
    {
        Normalize(task);
        task.CreatedAtUtc = DateTime.UtcNow;
        db.WorkTasks.Add(task);
        await db.SaveChangesAsync(ct);
        return task;
    }

    public async Task UpdateAsync(WorkTask task, CancellationToken ct = default)
    {
        Normalize(task);
        task.UpdatedAtUtc = DateTime.UtcNow;
        if (task.Status == WorkTaskStatus.Done && task.CompletedDate is null)
            task.CompletedDate = DateOnly.FromDateTime(DateTime.Today);

        db.WorkTasks.Update(task);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await db.WorkTasks.FindAsync([id], ct);
        if (entity is null) return;
        db.WorkTasks.Remove(entity);
        await db.SaveChangesAsync(ct);
    }

    public async Task<DashboardStats> GetDashboardAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var tasks = db.WorkTasks.AsNoTracking();

        return new DashboardStats(
            await tasks.CountAsync(ct),
            await tasks.CountAsync(x => x.Status == WorkTaskStatus.Pending || x.Status == WorkTaskStatus.InProgress, ct),
            await tasks.CountAsync(x => x.DueDate != null && x.DueDate < today && x.Status != WorkTaskStatus.Done && x.Status != WorkTaskStatus.Cancelled, ct),
            await tasks.CountAsync(x => (x.Priority == TaskPriority.High || x.Priority == TaskPriority.Critical) && x.Status != WorkTaskStatus.Done, ct),
            await db.TimeEntries.AsNoTracking().SumAsync(x => (decimal?)x.Hours, ct) ?? 0,
            await db.Improvements.AsNoTracking().CountAsync(x => x.Status != ImprovementStatus.Closed, ct));
    }

    private static void Normalize(WorkTask task)
    {
        task.Title = task.Title.Trim();
        task.Description = string.IsNullOrWhiteSpace(task.Description) ? null : task.Description.Trim();
    }
}

public sealed record DashboardStats(
    int TotalTasks,
    int OpenTasks,
    int OverdueTasks,
    int HighPriorityTasks,
    decimal LoggedHours,
    int OpenImprovements);
