using Microsoft.EntityFrameworkCore;
using TaskManager.App.Data;
using TaskManager.App.Features.Catalogs.Entities;

namespace TaskManager.App.Features.Catalogs;

public sealed class SubProjectService(ApplicationDbContext db)
{
    public Task<List<SubProject>> GetAllAsync(int? projectId = null, CancellationToken ct = default)
    {
        var query = db.SubProjects.AsNoTracking()
            .Include(x => x.Project)
            .ThenInclude(x => x.Company)
            .AsQueryable();

        if (projectId is not null)
            query = query.Where(x => x.ProjectId == projectId);

        return query.OrderBy(x => x.Project.Name).ThenBy(x => x.Name).ToListAsync(ct);
    }

    public Task<SubProject?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.SubProjects.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<SubProject> CreateAsync(SubProject subProject, CancellationToken ct = default)
    {
        subProject.Name = subProject.Name.Trim();
        db.SubProjects.Add(subProject);
        await db.SaveChangesAsync(ct);
        return subProject;
    }

    public async Task UpdateAsync(SubProject subProject, CancellationToken ct = default)
    {
        subProject.Name = subProject.Name.Trim();
        db.SubProjects.Update(subProject);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await db.SubProjects.FindAsync([id], ct);
        if (entity is null) return;
        db.SubProjects.Remove(entity);
        await db.SaveChangesAsync(ct);
    }
}
