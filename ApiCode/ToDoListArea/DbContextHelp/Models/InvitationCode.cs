using System;
using System.Collections.Generic;

namespace DbContextHelp.Models;

public partial class InvitationCode
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public int MaxUses { get; set; }

    public int UsedCount { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public string Status { get; set; } = null!;

    public Guid CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<InvitationCodeUsage> InvitationCodeUsages { get; set; } = new List<InvitationCodeUsage>();
}
