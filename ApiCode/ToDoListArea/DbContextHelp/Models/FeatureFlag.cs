using System;
using System.Collections.Generic;

namespace DbContextHelp.Models;

public partial class FeatureFlag
{
    public Guid Id { get; set; }

    public string FlagKey { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public string? Description { get; set; }

    public string? TargetUsers { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
