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

        if (!string.IsNullOrWhiteSpace(query.AssignedUserId))
            q = q.Where(x => x.AssignedUserId == query.AssignedUserId);

        if (query.OpenOnly)
            q = q.Where(x => x.Status == WorkTaskStatus.Pending || x.Status == WorkTaskStatus.InProgress);

        if (query.HighPriorityOnly)
            q = q.Where(x =>
                (x.Priority == TaskPriority.High || x.Priority == TaskPriority.Critical)
                && x.Status != WorkTaskStatus.Done
                && x.Status != WorkTaskStatus.Cancelled);

        var today = DateOnly.FromDateTime(DateTime.Today);
        if (query.OnlyOverdue)
        {
            q = q.Where(x => x.DueDate != null && x.DueDate < today && x.Status != WorkTaskStatus.Done && x.Status != WorkTaskStatus.Cancelled);
        }

        var total = await q.CountAsync(ct);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 5, 100);

        q = ApplySort(q, query, today);

        var items = await q
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

    private static IQueryable<WorkTask> ApplySort(IQueryable<WorkTask> q, TaskQuery query, DateOnly today)
    {
        var desc = query.SortDescending;

        return query.SortBy switch
        {
            TaskSortColumn.Title => desc
                ? q.OrderByDescending(x => x.Title)
                : q.OrderBy(x => x.Title),
            TaskSortColumn.Company => desc
                ? q.OrderByDescending(x => x.Company.Name).ThenBy(x => x.Title)
                : q.OrderBy(x => x.Company.Name).ThenBy(x => x.Title),
            TaskSortColumn.Project => desc
                ? q.OrderByDescending(x => x.Project.Name).ThenBy(x => x.Title)
                : q.OrderBy(x => x.Project.Name).ThenBy(x => x.Title),
            TaskSortColumn.Assignee => desc
                ? q.OrderByDescending(x => x.AssignedUserId).ThenBy(x => x.Title)
                : q.OrderBy(x => x.AssignedUserId).ThenBy(x => x.Title),
            TaskSortColumn.Priority => desc
                ? q.OrderByDescending(x => x.Priority).ThenBy(x => x.DueDate)
                : q.OrderBy(x => x.Priority).ThenBy(x => x.DueDate),
            TaskSortColumn.Status => desc
                ? q.OrderByDescending(x => x.Status).ThenBy(x => x.Title)
                : q.OrderBy(x => x.Status).ThenBy(x => x.Title),
            TaskSortColumn.DueDate => desc
                ? q.OrderByDescending(x => x.DueDate).ThenByDescending(x => x.Priority)
                : q.OrderBy(x => x.DueDate).ThenByDescending(x => x.Priority),
            TaskSortColumn.Hours => desc
                ? q.OrderByDescending(x => x.ActualHours).ThenBy(x => x.Title)
                : q.OrderBy(x => x.ActualHours).ThenBy(x => x.Title),
            _ => q
                .OrderByDescending(x => x.DueDate != null && x.DueDate < today && x.Status != WorkTaskStatus.Done && x.Status != WorkTaskStatus.Cancelled)
                .ThenByDescending(x => x.Priority)
                .ThenBy(x => x.DueDate)
                .ThenBy(x => x.Title)
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
            await tasks.CountAsync(x => (x.Priority == TaskPriority.High || x.Priority == TaskPriority.Critical) && x.Status != WorkTaskStatus.Done && x.Status != WorkTaskStatus.Cancelled, ct),
            await db.TimeEntries.AsNoTracking().SumAsync(x => (decimal?)x.Hours, ct) ?? 0,
            await db.Improvements.AsNoTracking().CountAsync(x => x.Status != ImprovementStatus.Closed, ct));
    }

    /// <summary>
    /// Short action list for the dashboard: overdue first, then priority, then due date.
    /// </summary>
    public Task<List<WorkTask>> GetNextUpAsync(int take = 8, string? assignedUserId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 3, 20);
        var today = DateOnly.FromDateTime(DateTime.Today);

        var q = db.WorkTasks.AsNoTracking()
            .Include(x => x.Company)
            .Include(x => x.Project)
            .Where(x => x.Status != WorkTaskStatus.Done && x.Status != WorkTaskStatus.Cancelled);

        if (!string.IsNullOrWhiteSpace(assignedUserId))
            q = q.Where(x => x.AssignedUserId == assignedUserId);

        return q
            .OrderByDescending(x => x.DueDate != null && x.DueDate < today)
            .ThenByDescending(x => x.Priority)
            .ThenBy(x => x.DueDate)
            .ThenBy(x => x.Title)
            .Take(take)
            .ToListAsync(ct);
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
