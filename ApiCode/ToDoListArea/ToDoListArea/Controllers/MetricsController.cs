using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoListArea.Services;

namespace ToDoListArea.Controllers
{
    /// <summary>
    /// 监控指标控制器
    /// 提供应用程序监控指标的访问接口
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
[Authorize(Roles = "admin")] // 仅管理员可访问（统一为小写，与User.Role默认存储一致）
    public class MetricsController : ControllerBase
    {
        private readonly IMetricsService _metricsService;
        private readonly ILoggingService _loggingService;
        private readonly ILogger<MetricsController> _logger;

        public MetricsController(
            IMetricsService metricsService,
            ILoggingService loggingService,
            ILogger<MetricsController> logger)
        {
            _metricsService = metricsService;
            _loggingService = loggingService;
            _logger = logger;
        }

        /// <summary>
        /// 获取应用程序指标摘要
        /// </summary>
        /// <returns>指标摘要</returns>
        [HttpGet("summary")]
        public ActionResult<MetricsSummary> GetMetricsSummary()
        {
            try
            {
                var summary = _metricsService.GetMetricsSummary();
                _loggingService.LogUserActivity(User.Identity?.Name ?? "Unknown", "查看监控指标");
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取监控指标失败");
                return StatusCode(500, new { error = "获取监控指标失败" });
            }
        }

        /// <summary>
        /// 重置监控指标
        /// </summary>
        /// <returns>操作结果</returns>
        [HttpPost("reset")]
        public IActionResult ResetMetrics()
        {
            try
            {
                _metricsService.ResetMetrics();
                _loggingService.LogUserActivity(User.Identity?.Name ?? "Unknown", "重置监控指标");
                return Ok(new { message = "监控指标已重置", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重置监控指标失败");
                return StatusCode(500, new { error = "重置监控指标失败" });
            }
        }

        /// <summary>
        /// 获取实时系统状态
        /// </summary>
        /// <returns>系统状态</returns>
        [HttpGet("system-status")]
        public ActionResult<object> GetSystemStatus()
        {
            try
            {
                var process = System.Diagnostics.Process.GetCurrentProcess();
                var status = new
                {
                    timestamp = DateTime.UtcNow,
                    uptime = DateTime.UtcNow.Subtract(process.StartTime).ToString(@"dd\.hh\:mm\:ss"),
                    memory = new
                    {
                        working_set = $"{Environment.WorkingSet / 1024 / 1024} MB",
                        gc_memory = $"{GC.GetTotalMemory(false) / 1024 / 1024} MB",
                        gc_collections = new
                        {
                            gen0 = GC.CollectionCount(0),
                            gen1 = GC.CollectionCount(1),
                            gen2 = GC.CollectionCount(2)
                        }
                    },
                    threads = process.Threads.Count,
                    handles = process.HandleCount,
                    processor_count = Environment.ProcessorCount,
                    machine_name = Environment.MachineName,
                    os_version = Environment.OSVersion.ToString(),
                    framework_version = Environment.Version.ToString()
                };

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取系统状态失败");
                return StatusCode(500, new { error = "获取系统状态失败" });
            }
        }

        /// <summary>
        /// 获取应用程序日志（最近的条目）
        /// </summary>
        /// <param name="count">返回的日志条数</param>
        /// <param name="level">日志级别过滤</param>
        /// <returns>日志条目</returns>
        [HttpGet("logs")]
        public ActionResult<object> GetRecentLogs(int count = 100, string? level = null)
        {
            try
            {
                // 注意：这是一个简化的实现
                // 在实际生产环境中，应该从日志存储系统（如文件、数据库或日志服务）中读取
                var logs = new
                {
                    message = "日志查看功能需要集成具体的日志存储系统",
                    suggestion = "建议集成 Serilog、NLog 或 ELK Stack",
                    parameters = new { count, level },
                    timestamp = DateTime.UtcNow
                };

                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取日志失败");
                return StatusCode(500, new { error = "获取日志失败" });
            }
        }



        /// <summary>
        /// 获取性能计数器
        /// </summary>
        /// <returns>性能计数器数据</returns>
        [HttpGet("performance-counters")]
        public ActionResult<object> GetPerformanceCounters()
        {
            try
            {
                var counters = new
                {
                    timestamp = DateTime.UtcNow,
                    note = "性能计数器功能需要根据具体需求实现",
                    available_counters = new[]
                    {
                        "CPU使用率",
                        "内存使用率", 
                        "磁盘I/O",
                        "网络I/O",
                        "数据库连接数",
                        "活跃用户数",
                        "请求队列长度"
                    }
                };

                return Ok(counters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取性能计数器失败");
                return StatusCode(500, new { error = "获取性能计数器失败" });
            }
        }
    }
}
