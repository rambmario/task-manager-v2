using Microsoft.EntityFrameworkCore;
using TaskManager.App.Data;

namespace TaskManager.App.Features.Identity;

public sealed record UserOption(string Id, string DisplayLabel);

public sealed class UserDirectoryService(ApplicationDbContext db)
{
    public Task<List<UserOption>> GetAssignableUsersAsync(CancellationToken ct = default) =>
        db.Users.AsNoTracking()
            .OrderBy(u => u.DisplayName ?? u.Email)
            .Select(u => new UserOption(
                u.Id,
                u.DisplayName ?? u.Email ?? u.UserName ?? u.Id))
            .ToListAsync(ct);

    public async Task<IReadOnlyDictionary<string, string>> GetDisplayMapAsync(
        IEnumerable<string?> userIds,
        CancellationToken ct = default)
    {
        var ids = userIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().Cast<string>().ToList();
        if (ids.Count == 0)
            return new Dictionary<string, string>();

        return await db.Users.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(
                u => u.Id,
                u => u.DisplayName ?? u.Email ?? u.UserName ?? u.Id,
                ct);
    }
}
