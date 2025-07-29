using System;
using System.Collections.Generic;

namespace DbContextHelp.Models;

public partial class ReminderHistory
{
    public Guid Id { get; set; }

    public Guid ReminderId { get; set; }

    public Guid UserId { get; set; }

    public DateTime SentAt { get; set; }

    public string Channel { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? ResponseData { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Reminder Reminder { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
