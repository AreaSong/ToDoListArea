using System;
using System.Collections.Generic;

namespace DbContextHelp.Models;

public partial class Reminder
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid? TaskId { get; set; }

    public string Title { get; set; } = null!;

    public string? Message { get; set; }

    public DateTime ReminderTime { get; set; }

    public string Status { get; set; } = null!;

    public string Channels { get; set; } = null!;

    public string? RepeatPattern { get; set; }

    public DateTime? SnoozeUntil { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<ReminderHistory> ReminderHistories { get; set; } = new List<ReminderHistory>();

    public virtual Task? Task { get; set; }

    public virtual User User { get; set; } = null!;
}
