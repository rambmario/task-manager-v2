using Microsoft.EntityFrameworkCore;
using TaskManager.App.Data;
using TaskManager.App.Features.Catalogs.Entities;

namespace TaskManager.App.Features.Catalogs;

public sealed class CompanyService(ApplicationDbContext db)
{
    public Task<List<Company>> GetAllAsync(CancellationToken ct = default) =>
        db.Companies.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);

    public Task<Company?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Companies.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Company> CreateAsync(Company company, CancellationToken ct = default)
    {
        Normalize(company);
        db.Companies.Add(company);
        await db.SaveChangesAsync(ct);
        return company;
    }

    public async Task UpdateAsync(Company company, CancellationToken ct = default)
    {
        Normalize(company);
        db.Companies.Update(company);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await db.Companies.FindAsync([id], ct);
        if (entity is null) return;
        db.Companies.Remove(entity);
        await db.SaveChangesAsync(ct);
    }

    private static void Normalize(Company company)
    {
        company.Code = company.Code.Trim().ToUpperInvariant();
        company.Name = company.Name.Trim();
        company.Email = string.IsNullOrWhiteSpace(company.Email) ? null : company.Email.Trim();
    }
}
