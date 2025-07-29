using System;
using System.Collections.Generic;

namespace DbContextHelp.Models;

public partial class TimelineEvent
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string EventType { get; set; } = null!;

    public string EventTitle { get; set; } = null!;

    public string? EventDescription { get; set; }

    public string? EventData { get; set; }

    public DateTime OccurredAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
