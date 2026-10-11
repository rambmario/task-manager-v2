using Microsoft.EntityFrameworkCore;
using TaskManager.App.Data;
using TaskManager.App.Features.Improvements.Entities;

namespace TaskManager.App.Features.Improvements;

public sealed class ImprovementService(ApplicationDbContext db)
{
    public Task<List<Improvement>> GetAllAsync(CancellationToken ct = default) =>
        db.Improvements.AsNoTracking()
            .Include(x => x.Company)
            .Include(x => x.Project)
            .Include(x => x.Actions)
            .OrderByDescending(x => x.OpenedDate)
            .ToListAsync(ct);

    public Task<Improvement?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Improvements.Include(x => x.Actions).FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Improvement> CreateAsync(Improvement improvement, CancellationToken ct = default)
    {
        improvement.Title = improvement.Title.Trim();
        db.Improvements.Add(improvement);
        await db.SaveChangesAsync(ct);
        return improvement;
    }

    public async Task UpdateAsync(Improvement improvement, CancellationToken ct = default)
    {
        improvement.Title = improvement.Title.Trim();
        if (improvement.Status == ImprovementStatus.Closed && improvement.ClosedDate is null)
            improvement.ClosedDate = DateOnly.FromDateTime(DateTime.Today);

        db.Improvements.Update(improvement);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddActionAsync(ImprovementAction action, CancellationToken ct = default)
    {
        action.Description = action.Description.Trim();
        db.ImprovementActions.Add(action);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await db.Improvements.FindAsync([id], ct);
        if (entity is null) return;
        db.Improvements.Remove(entity);
        await db.SaveChangesAsync(ct);
    }
}
