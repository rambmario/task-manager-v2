namespace TaskManager.App.Features.Catalogs.Entities;

public class SubProject
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public decimal PlannedHours { get; set; }
    public bool IsActive { get; set; } = true;
}
