using System;
using System.Collections.Generic;

namespace DbContextHelp.Models;

public partial class TaskDetail
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }

    public string DetailType { get; set; } = null!;

    public string DetailKey { get; set; } = null!;

    public string? DetailValue { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Task Task { get; set; } = null!;
}
