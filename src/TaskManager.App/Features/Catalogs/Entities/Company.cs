namespace TaskManager.App.Features.Catalogs.Entities;

public class Company
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Project> Projects { get; set; } = [];
}
