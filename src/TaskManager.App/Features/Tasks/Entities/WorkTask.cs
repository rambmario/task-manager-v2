using TaskManager.App.Features.Catalogs.Entities;

namespace TaskManager.App.Features.Tasks.Entities;

public enum TaskPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum WorkTaskStatus
{
    Pending = 1,
    InProgress = 2,
    Blocked = 3,
    Done = 4,
    Cancelled = 5
}

public class WorkTask
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int? SubProjectId { get; set; }
    public SubProject? SubProject { get; set; }

    public string? AssignedUserId { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public WorkTaskStatus Status { get; set; } = WorkTaskStatus.Pending;

    public DateOnly RequestedDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public DateOnly? DueDate { get; set; }
    public DateOnly? CompletedDate { get; set; }

    public decimal PlannedHours { get; set; }
    public decimal ActualHours { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<TimeTracking.Entities.TimeEntry> TimeEntries { get; set; } = [];
}
