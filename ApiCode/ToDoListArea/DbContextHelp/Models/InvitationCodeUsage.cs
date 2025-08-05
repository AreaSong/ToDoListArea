using System;
using System.Collections.Generic;

namespace DbContextHelp.Models;

public partial class InvitationCodeUsage
{
    public Guid Id { get; set; }

    public Guid InvitationCodeId { get; set; }

    public Guid UserId { get; set; }

    public DateTime UsedAt { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public virtual InvitationCode InvitationCode { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
