using Microsoft.EntityFrameworkCore;
using TaskManager.App.Data;
using TaskManager.App.Features.TimeTracking.Entities;

namespace TaskManager.App.Features.TimeTracking;

public sealed class TimeEntryService(ApplicationDbContext db)
{
    public Task<List<TimeEntry>> GetRecentAsync(int take = 50, CancellationToken ct = default) =>
        db.TimeEntries.AsNoTracking()
            .Include(x => x.WorkTask)
            .OrderByDescending(x => x.WorkDate)
            .ThenByDescending(x => x.Id)
            .Take(take)
            .ToListAsync(ct);

    public async Task<TimeEntry> CreateAsync(TimeEntry entry, CancellationToken ct = default)
    {
        if (entry.Hours <= 0)
            throw new InvalidOperationException("Hours must be greater than zero.");

        db.TimeEntries.Add(entry);

        var task = await db.WorkTasks.FirstOrDefaultAsync(x => x.Id == entry.WorkTaskId, ct)
            ?? throw new InvalidOperationException("Task not found.");

        task.ActualHours = await db.TimeEntries
            .Where(x => x.WorkTaskId == entry.WorkTaskId)
            .SumAsync(x => x.Hours, ct) + entry.Hours;
        task.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return entry;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entry = await db.TimeEntries.FindAsync([id], ct);
        if (entry is null) return;

        var taskId = entry.WorkTaskId;
        db.TimeEntries.Remove(entry);
        await db.SaveChangesAsync(ct);

        var task = await db.WorkTasks.FirstOrDefaultAsync(x => x.Id == taskId, ct);
        if (task is not null)
        {
            task.ActualHours = await db.TimeEntries.Where(x => x.WorkTaskId == taskId).SumAsync(x => (decimal?)x.Hours, ct) ?? 0;
            task.UpdatedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
    }
}
