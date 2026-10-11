using TaskManager.App.Features.Catalogs.Entities;

namespace TaskManager.App.Features.Improvements.Entities;

public enum ImprovementStatus
{
    Open = 1,
    InProgress = 2,
    Closed = 3
}

public class Improvement
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Cause { get; set; }
    public string? CorrectiveAction { get; set; }

    public int? CompanyId { get; set; }
    public Company? Company { get; set; }

    public int? ProjectId { get; set; }
    public Project? Project { get; set; }

    public ImprovementStatus Status { get; set; } = ImprovementStatus.Open;
    public decimal ClosureCost { get; set; }
    public DateOnly OpenedDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public DateOnly? ClosedDate { get; set; }

    public string? OwnerUserId { get; set; }

    public ICollection<ImprovementAction> Actions { get; set; } = [];
}

public class ImprovementAction
{
    public int Id { get; set; }
    public int ImprovementId { get; set; }
    public Improvement Improvement { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public DateOnly ActionDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public string? PerformedByUserId { get; set; }
}
