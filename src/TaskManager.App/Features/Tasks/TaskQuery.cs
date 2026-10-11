using TaskManager.App.Features.Tasks.Entities;

namespace TaskManager.App.Features.Tasks;

public sealed class TaskQuery
{
    public string? Search { get; set; }
    public int? CompanyId { get; set; }
    public int? ProjectId { get; set; }
    public WorkTaskStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    /// <summary>Filter by assignee (legacy id_usuario_asignado).</summary>
    public string? AssignedUserId { get; set; }
    public bool OnlyOverdue { get; set; }
    /// <summary>Pending or InProgress only.</summary>
    public bool OpenOnly { get; set; }
    /// <summary>High or Critical, excluding Done.</summary>
    public bool HighPriorityOnly { get; set; }
    public TaskSortColumn SortBy { get; set; } = TaskSortColumn.Default;
    public bool SortDescending { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
