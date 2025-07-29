using System;
using System.Collections.Generic;

namespace DbContextHelp.Models;

public partial class UserProfile
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string Timezone { get; set; } = null!;

    public string Language { get; set; } = null!;

    public string DateFormat { get; set; } = null!;

    public string TimeFormat { get; set; } = null!;

    public string? NotificationPreferences { get; set; }

    public string? ThemePreferences { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
