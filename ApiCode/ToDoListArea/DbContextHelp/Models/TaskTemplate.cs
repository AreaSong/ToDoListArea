using System;
using System.Collections.Generic;

namespace DbContextHelp.Models;

public partial class TaskTemplate
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string TemplateData { get; set; } = null!;

    public string? Category { get; set; }

    public bool IsPublic { get; set; }

    public int UsageCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
