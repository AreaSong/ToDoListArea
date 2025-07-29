using System;
using System.Collections.Generic;

namespace DbContextHelp.Models;

public partial class TaskDependency
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }

    public Guid DependsOnTaskId { get; set; }

    public string DependencyType { get; set; } = null!;

    public int LagTime { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Task DependsOnTask { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
