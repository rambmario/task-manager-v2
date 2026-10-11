using TaskManager.App.Features.Tasks.Entities;

namespace TaskManager.App.Features.TimeTracking.Entities;

public class TimeEntry
{
    public int Id { get; set; }
    public int WorkTaskId { get; set; }
    public WorkTask WorkTask { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public DateOnly WorkDate { get; set; }
    public decimal Hours { get; set; }
    public string? Notes { get; set; }
    public decimal? HourlyCost { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
