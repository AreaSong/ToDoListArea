using System.Collections.Concurrent;
using System.Diagnostics;

namespace ToDoListArea.Services
{
    /// <summary>
    /// 监控指标服务接口
    /// </summary>
    public interface IMetricsService
    {
        void IncrementCounter(string name, string[]? tags = null);
        void RecordValue(string name, double value, string[]? tags = null);
        void RecordDuration(string name, TimeSpan duration, string[]? tags = null);
        MetricsSummary GetMetricsSummary();
        void ResetMetrics();
    }

    /// <summary>
    /// 监控指标数据
    /// </summary>
    public class MetricData
    {
        public long Count { get; set; }
        public double Sum { get; set; }
        public double Min { get; set; } = double.MaxValue;
        public double Max { get; set; } = double.MinValue;
        public double Average => Count > 0 ? Sum / Count : 0;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// 指标摘要
    /// </summary>
    public class MetricsSummary
    {
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public Dictionary<string, MetricData> Counters { get; set; } = new();
        public Dictionary<string, MetricData> Values { get; set; } = new();
        public Dictionary<string, MetricData> Durations { get; set; } = new();
        public SystemMetrics System { get; set; } = new();
    }

    /// <summary>
    /// 系统指标
    /// </summary>
    public class SystemMetrics
    {
        public long WorkingSet { get; set; }
        public long GcMemory { get; set; }
        public int ThreadCount { get; set; }
        public TimeSpan Uptime { get; set; }
        public double CpuUsage { get; set; }
    }

    /// <summary>
    /// 监控指标服务实现
    /// </summary>
    public class MetricsService : IMetricsService
    {
        private readonly ConcurrentDictionary<string, MetricData> _counters = new();
        private readonly ConcurrentDictionary<string, MetricData> _values = new();
        private readonly ConcurrentDictionary<string, MetricData> _durations = new();
        private readonly DateTime _startTime = DateTime.UtcNow;
        private readonly ILogger<MetricsService> _logger;

        public MetricsService(ILogger<MetricsService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 增加计数器
        /// </summary>
        public void IncrementCounter(string name, string[]? tags = null)
        {
            var key = BuildKey(name, tags);
            _counters.AddOrUpdate(key, 
                new MetricData { Count = 1, Sum = 1, Min = 1, Max = 1 },
                (k, existing) =>
                {
                    existing.Count++;
                    existing.Sum++;
                    existing.LastUpdated = DateTime.UtcNow;
                    return existing;
                });
        }

        /// <summary>
        /// 记录数值
        /// </summary>
        public void RecordValue(string name, double value, string[]? tags = null)
        {
            var key = BuildKey(name, tags);
            _values.AddOrUpdate(key,
                new MetricData { Count = 1, Sum = value, Min = value, Max = value },
                (k, existing) =>
                {
                    existing.Count++;
                    existing.Sum += value;
                    existing.Min = Math.Min(existing.Min, value);
                    existing.Max = Math.Max(existing.Max, value);
                    existing.LastUpdated = DateTime.UtcNow;
                    return existing;
                });
        }

        /// <summary>
        /// 记录持续时间
        /// </summary>
        public void RecordDuration(string name, TimeSpan duration, string[]? tags = null)
        {
            var milliseconds = duration.TotalMilliseconds;
            var key = BuildKey(name, tags);
            _durations.AddOrUpdate(key,
                new MetricData { Count = 1, Sum = milliseconds, Min = milliseconds, Max = milliseconds },
                (k, existing) =>
                {
                    existing.Count++;
                    existing.Sum += milliseconds;
                    existing.Min = Math.Min(existing.Min, milliseconds);
                    existing.Max = Math.Max(existing.Max, milliseconds);
                    existing.LastUpdated = DateTime.UtcNow;
                    return existing;
                });
        }

        /// <summary>
        /// 获取指标摘要
        /// </summary>
        public MetricsSummary GetMetricsSummary()
        {
            var process = Process.GetCurrentProcess();
            
            return new MetricsSummary
            {
                Counters = new Dictionary<string, MetricData>(_counters),
                Values = new Dictionary<string, MetricData>(_values),
                Durations = new Dictionary<string, MetricData>(_durations),
                System = new SystemMetrics
                {
                    WorkingSet = Environment.WorkingSet,
                    GcMemory = GC.GetTotalMemory(false),
                    ThreadCount = Process.GetCurrentProcess().Threads.Count,
                    Uptime = DateTime.UtcNow - _startTime,
                    CpuUsage = GetCpuUsage()
                }
            };
        }

        /// <summary>
        /// 重置指标
        /// </summary>
        public void ResetMetrics()
        {
            _counters.Clear();
            _values.Clear();
            _durations.Clear();
            _logger.LogInformation("监控指标已重置");
        }

        /// <summary>
        /// 构建指标键
        /// </summary>
        private static string BuildKey(string name, string[]? tags)
        {
            if (tags == null || tags.Length == 0)
                return name;
            
            return $"{name}:{string.Join(",", tags)}";
        }

        /// <summary>
        /// 获取CPU使用率（简化版本）
        /// </summary>
        private static double GetCpuUsage()
        {
            try
            {
                // 这是一个简化的CPU使用率计算
                // 在生产环境中，建议使用更精确的方法
                var process = Process.GetCurrentProcess();
                return process.TotalProcessorTime.TotalMilliseconds / Environment.TickCount * 100;
            }
            catch
            {
                return 0;
            }
        }
    }

    /// <summary>
    /// 指标收集中间件
    /// </summary>
    public class MetricsCollectionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMetricsService _metricsService;

        public MetricsCollectionMiddleware(RequestDelegate next, IMetricsService metricsService)
        {
            _next = next;
            _metricsService = metricsService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                await _next(context);
                
                // 记录成功请求
                _metricsService.IncrementCounter("http_requests_total", 
                    new[] { $"method:{context.Request.Method}", $"status:{context.Response.StatusCode}" });
            }
            catch (Exception)
            {
                // 记录失败请求
                _metricsService.IncrementCounter("http_requests_total", 
                    new[] { $"method:{context.Request.Method}", "status:500" });
                throw;
            }
            finally
            {
                stopwatch.Stop();
                
                // 记录请求持续时间
                _metricsService.RecordDuration("http_request_duration", stopwatch.Elapsed,
                    new[] { $"method:{context.Request.Method}" });
            }
        }
    }

    /// <summary>
    /// 指标收集中间件扩展
    /// </summary>
    public static class MetricsCollectionMiddlewareExtensions
    {
        public static IApplicationBuilder UseMetricsCollection(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MetricsCollectionMiddleware>();
        }
    }
}
