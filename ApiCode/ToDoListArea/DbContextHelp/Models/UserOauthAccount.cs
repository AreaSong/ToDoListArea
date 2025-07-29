using System;
using System.Collections.Generic;

namespace DbContextHelp.Models;

public partial class UserOauthAccount
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Provider { get; set; } = null!;

    public string ProviderUserId { get; set; } = null!;

    public string? ProviderEmail { get; set; }

    public string? ProviderData { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
