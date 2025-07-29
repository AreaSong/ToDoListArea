using System;
using System.Collections.Generic;

namespace DbContextHelp.Models;

public partial class TaskStatistic
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateOnly Date { get; set; }

    public int TotalTasks { get; set; }

    public int CompletedTasks { get; set; }

    public int PendingTasks { get; set; }

    public int OverdueTasks { get; set; }

    public int TotalTimeSpent { get; set; }

    public decimal? ProductivityScore { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
