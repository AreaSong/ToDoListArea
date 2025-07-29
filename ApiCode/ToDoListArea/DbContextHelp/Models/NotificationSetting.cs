using System;
using System.Collections.Generic;

namespace DbContextHelp.Models;

public partial class NotificationSetting
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Channel { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public string? Settings { get; set; }

    public TimeOnly? QuietHoursStart { get; set; }

    public TimeOnly? QuietHoursEnd { get; set; }

    public string Timezone { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
