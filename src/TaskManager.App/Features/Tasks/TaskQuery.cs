using TaskManager.App.Features.Tasks.Entities;

namespace TaskManager.App.Features.Tasks;

public sealed class TaskQuery
{
    public string? Search { get; set; }
    public int? CompanyId { get; set; }
    public int? ProjectId { get; set; }
    public WorkTaskStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public bool OnlyOverdue { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
