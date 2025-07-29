using System;
using System.Collections.Generic;

namespace DbContextHelp.Models;

public partial class Task
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid? ParentTaskId { get; set; }

    public Guid? CategoryId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public string Priority { get; set; } = null!;

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public int? EstimatedDuration { get; set; }

    public int? ActualDuration { get; set; }

    public decimal CompletionPercentage { get; set; }

    public bool IsRecurring { get; set; }

    public string? RecurrencePattern { get; set; }

    public string? Tags { get; set; }

    public string? Attachments { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual TaskCategory? Category { get; set; }

    public virtual ICollection<GanttDatum> GanttData { get; set; } = new List<GanttDatum>();

    public virtual ICollection<Task> InverseParentTask { get; set; } = new List<Task>();

    public virtual Task? ParentTask { get; set; }

    public virtual ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();

    public virtual ICollection<TaskDependency> TaskDependencyDependsOnTasks { get; set; } = new List<TaskDependency>();

    public virtual ICollection<TaskDependency> TaskDependencyTasks { get; set; } = new List<TaskDependency>();

    public virtual ICollection<TaskDetail> TaskDetails { get; set; } = new List<TaskDetail>();

    public virtual ICollection<TimeBlock> TimeBlocks { get; set; } = new List<TimeBlock>();

    public virtual ICollection<TimelineNode> TimelineNodes { get; set; } = new List<TimelineNode>();

    public virtual User User { get; set; } = null!;
}
