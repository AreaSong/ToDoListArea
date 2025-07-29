using System;
using System.Collections.Generic;

namespace DbContextHelp.Models;

public partial class SystemLog
{
    public Guid Id { get; set; }

    public string LogLevel { get; set; } = null!;

    public string LoggerName { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string? Exception { get; set; }

    public string? Properties { get; set; }

    public Guid? UserId { get; set; }

    public string? RequestId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
