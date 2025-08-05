using System.Diagnostics;

namespace ToDoListArea.Services
{
    /// <summary>
    /// 结构化日志服务
    /// 提供统一的日志记录接口
    /// </summary>
    public interface ILoggingService
    {
        void LogUserActivity(string userId, string activity, string? details = null);
        void LogApiCall(string endpoint, string method, int statusCode, long responseTime);
        void LogSecurityEvent(string eventType, string? details = null, string? userId = null);
        void LogPerformanceMetric(string metricName, double value, string? unit = null);
        void LogBusinessEvent(string eventType, object data);
    }

    /// <summary>
    /// 结构化日志服务实现
    /// </summary>
    public class LoggingService : ILoggingService
    {
        private readonly ILogger<LoggingService> _logger;

        public LoggingService(ILogger<LoggingService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 记录用户活动
        /// </summary>
        public void LogUserActivity(string userId, string activity, string? details = null)
        {
            _logger.LogInformation("用户活动: {UserId} 执行了 {Activity}. 详情: {Details}",
                userId, activity, details ?? "无");
        }

        /// <summary>
        /// 记录API调用
        /// </summary>
        public void LogApiCall(string endpoint, string method, int statusCode, long responseTime)
        {
            var logLevel = statusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
            
            _logger.Log(logLevel, "API调用: {Method} {Endpoint} 返回 {StatusCode}, 耗时 {ResponseTime}ms",
                method, endpoint, statusCode, responseTime);
        }

        /// <summary>
        /// 记录安全事件
        /// </summary>
        public void LogSecurityEvent(string eventType, string? details = null, string? userId = null)
        {
            _logger.LogWarning("安全事件: {EventType}. 用户: {UserId}. 详情: {Details}",
                eventType, userId ?? "未知", details ?? "无");
        }

        /// <summary>
        /// 记录性能指标
        /// </summary>
        public void LogPerformanceMetric(string metricName, double value, string? unit = null)
        {
            _logger.LogInformation("性能指标: {MetricName} = {Value} {Unit}",
                metricName, value, unit ?? "");
        }

        /// <summary>
        /// 记录业务事件
        /// </summary>
        public void LogBusinessEvent(string eventType, object data)
        {
            _logger.LogInformation("业务事件: {EventType}. 数据: {@Data}",
                eventType, data);
        }
    }

    /// <summary>
    /// 性能监控中间件
    /// </summary>
    public class PerformanceMonitoringMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggingService _loggingService;
        private readonly ILogger<PerformanceMonitoringMiddleware> _logger;

        public PerformanceMonitoringMiddleware(
            RequestDelegate next, 
            ILoggingService loggingService,
            ILogger<PerformanceMonitoringMiddleware> logger)
        {
            _next = next;
            _loggingService = loggingService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var originalBodyStream = context.Response.Body;

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                
                // 记录API调用性能
                _loggingService.LogApiCall(
                    context.Request.Path,
                    context.Request.Method,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds
                );

                // 记录慢请求
                if (stopwatch.ElapsedMilliseconds > 1000) // 超过1秒的请求
                {
                    _logger.LogWarning("慢请求检测: {Method} {Path} 耗时 {ElapsedMs}ms",
                        context.Request.Method,
                        context.Request.Path,
                        stopwatch.ElapsedMilliseconds);
                }

                // 记录性能指标
                _loggingService.LogPerformanceMetric("request_duration", stopwatch.ElapsedMilliseconds, "ms");
                _loggingService.LogPerformanceMetric("response_status", context.Response.StatusCode);
            }
        }
    }

    /// <summary>
    /// 性能监控中间件扩展
    /// </summary>
    public static class PerformanceMonitoringMiddlewareExtensions
    {
        public static IApplicationBuilder UsePerformanceMonitoring(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PerformanceMonitoringMiddleware>();
        }
    }
}
