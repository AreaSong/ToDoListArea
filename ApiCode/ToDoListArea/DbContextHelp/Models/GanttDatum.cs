using System;
using System.Collections.Generic;

namespace DbContextHelp.Models;

public partial class GanttDatum
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid TaskId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal Progress { get; set; }

    public string? Dependencies { get; set; }

    public string? Resources { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Task Task { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
