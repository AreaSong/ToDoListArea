using System;
using System.Collections.Generic;

namespace DbContextHelp.Models;

public partial class ReminderRule
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string RuleName { get; set; } = null!;

    public string RuleType { get; set; } = null!;

    public string Conditions { get; set; } = null!;

    public string Actions { get; set; } = null!;

    public bool IsActive { get; set; }

    public int Priority { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
