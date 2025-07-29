using System;
using System.Collections.Generic;

namespace DbContextHelp.Models;

public partial class TimelineNode
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid? TaskId { get; set; }

    public string NodeType { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string Color { get; set; } = null!;

    public string? PositionData { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Task? Task { get; set; }

    public virtual User User { get; set; } = null!;
}
