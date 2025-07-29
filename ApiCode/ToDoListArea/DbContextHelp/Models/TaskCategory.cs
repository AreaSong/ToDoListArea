using System;
using System.Collections.Generic;

namespace DbContextHelp.Models;

public partial class TaskCategory
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Color { get; set; } = null!;

    public string? Icon { get; set; }

    public string? Description { get; set; }

    public bool IsSystem { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
