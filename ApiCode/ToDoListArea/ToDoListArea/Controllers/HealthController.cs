using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoListArea.Data;

namespace ToDoListArea.Controllers
{
    /// <summary>
    /// 健康检查控制器
    /// 用于监控应用程序和依赖服务的健康状态
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ToDoListDbContext _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(ToDoListDbContext context, ILogger<HealthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 基础健康检查
        /// </summary>
        /// <returns>健康状态</returns>
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var healthStatus = new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    version = "1.0.0",
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
                };

                return Ok(healthStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(503, new { status = "unhealthy", error = ex.Message });
            }
        }

        /// <summary>
        /// 详细健康检查
        /// 包括数据库连接状态
        /// </summary>
        /// <returns>详细健康状态</returns>
        [HttpGet("detailed")]
        public async Task<IActionResult> GetDetailed()
        {
            var healthChecks = new Dictionary<string, object>();

            try
            {
                // 检查应用程序状态
                healthChecks["application"] = new
                {
                    status = "healthy",
                    uptime = DateTime.UtcNow.Subtract(Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss"),
                    memory_usage = $"{GC.GetTotalMemory(false) / 1024 / 1024} MB"
                };

                // 检查数据库连接
                try
                {
                    var canConnect = await _context.Database.CanConnectAsync();
                    if (canConnect)
                    {
                        // 执行简单查询测试
                        var userCount = await _context.Users.CountAsync();
                        healthChecks["database"] = new
                        {
                            status = "healthy",
                            connection = "ok",
                            user_count = userCount
                        };
                    }
                    else
                    {
                        healthChecks["database"] = new
                        {
                            status = "unhealthy",
                            connection = "failed"
                        };
                    }
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database health check failed");
                    healthChecks["database"] = new
                    {
                        status = "unhealthy",
                        connection = "error",
                        error = dbEx.Message
                    };
                }

                // 检查系统资源
                healthChecks["system"] = new
                {
                    status = "healthy",
                    processor_count = Environment.ProcessorCount,
                    working_set = $"{Environment.WorkingSet / 1024 / 1024} MB",
                    machine_name = Environment.MachineName
                };

                var overallStatus = healthChecks.Values.All(hc => 
                    hc.GetType().GetProperty("status")?.GetValue(hc)?.ToString() == "healthy") 
                    ? "healthy" : "unhealthy";

                var result = new
                {
                    status = overallStatus,
                    timestamp = DateTime.UtcNow,
                    version = "1.0.0",
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    checks = healthChecks
                };

                return overallStatus == "healthy" ? Ok(result) : StatusCode(503, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Detailed health check failed");
                return StatusCode(503, new 
                { 
                    status = "unhealthy", 
                    timestamp = DateTime.UtcNow,
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// 就绪检查
        /// 检查应用程序是否准备好接收请求
        /// </summary>
        /// <returns>就绪状态</returns>
        [HttpGet("ready")]
        public async Task<IActionResult> Ready()
        {
            try
            {
                // 检查数据库是否可用
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return StatusCode(503, new { status = "not_ready", reason = "database_unavailable" });
                }

                // 检查关键服务是否初始化完成
                // 这里可以添加其他必要的检查

                return Ok(new 
                { 
                    status = "ready", 
                    timestamp = DateTime.UtcNow 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Readiness check failed");
                return StatusCode(503, new 
                { 
                    status = "not_ready", 
                    reason = "check_failed",
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// 存活检查
        /// 检查应用程序是否仍在运行
        /// </summary>
        /// <returns>存活状态</returns>
        [HttpGet("live")]
        public IActionResult Live()
        {
            return Ok(new 
            { 
                status = "alive", 
                timestamp = DateTime.UtcNow,
                process_id = Environment.ProcessId
            });
        }
    }
}
