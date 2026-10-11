namespace TaskManager.App.Features.Catalogs.Entities;

public class Project
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<SubProject> SubProjects { get; set; } = [];
    public ICollection<Tasks.Entities.WorkTask> Tasks { get; set; } = [];
}
