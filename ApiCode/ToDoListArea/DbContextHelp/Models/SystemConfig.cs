using System;
using System.Collections.Generic;

namespace DbContextHelp.Models;

public partial class SystemConfig
{
    public Guid Id { get; set; }

    public string ConfigKey { get; set; } = null!;

    public string ConfigValue { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsEncrypted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
