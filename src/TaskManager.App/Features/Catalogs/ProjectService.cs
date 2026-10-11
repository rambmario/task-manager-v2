using Microsoft.EntityFrameworkCore;
using TaskManager.App.Data;
using TaskManager.App.Features.Catalogs.Entities;

namespace TaskManager.App.Features.Catalogs;

public sealed class ProjectService(ApplicationDbContext db)
{
    public Task<List<Project>> GetAllAsync(int? companyId = null, CancellationToken ct = default)
    {
        var query = db.Projects.AsNoTracking().Include(x => x.Company).AsQueryable();
        if (companyId is not null)
            query = query.Where(x => x.CompanyId == companyId);
        return query.OrderBy(x => x.Name).ToListAsync(ct);
    }

    public Task<Project?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Projects.Include(x => x.SubProjects).FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Project> CreateAsync(Project project, CancellationToken ct = default)
    {
        project.Name = project.Name.Trim();
        db.Projects.Add(project);
        await db.SaveChangesAsync(ct);
        return project;
    }

    public async Task UpdateAsync(Project project, CancellationToken ct = default)
    {
        project.Name = project.Name.Trim();
        db.Projects.Update(project);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await db.Projects.FindAsync([id], ct);
        if (entity is null) return;
        db.Projects.Remove(entity);
        await db.SaveChangesAsync(ct);
    }
}
