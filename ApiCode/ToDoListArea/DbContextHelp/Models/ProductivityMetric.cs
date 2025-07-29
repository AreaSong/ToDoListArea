using System;
using System.Collections.Generic;

namespace DbContextHelp.Models;

public partial class ProductivityMetric
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string MetricType { get; set; } = null!;

    public decimal MetricValue { get; set; }

    public string? MetricUnit { get; set; }

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd { get; set; }

    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
