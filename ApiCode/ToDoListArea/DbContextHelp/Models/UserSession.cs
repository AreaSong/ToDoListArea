using System;
using System.Collections.Generic;

namespace DbContextHelp.Models;

public partial class UserSession
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string SessionToken { get; set; } = null!;

    public string? RefreshToken { get; set; }

    public string? DeviceInfo { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public bool IsActive { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
