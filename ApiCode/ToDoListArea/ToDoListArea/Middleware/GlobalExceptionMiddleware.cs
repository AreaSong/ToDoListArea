using System.Net;
using System.Text.Json;

namespace ToDoListArea.Middleware
{
    /// <summary>
    /// 全局异常处理中间件
    /// 统一处理应用程序中的异常
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// 中间件执行方法
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// 处理异常
        /// </summary>
        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse();
            var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

            switch (exception)
            {
                case ArgumentNullException nullEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "必需参数不能为空";
                    response.Details = isDevelopment ? nullEx.Message : "请检查请求参数";
                    break;

                case ArgumentException argEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "请求参数错误";
                    response.Details = isDevelopment ? argEx.Message : "请求参数格式不正确";
                    break;

                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Message = "未授权访问";
                    response.Details = "您没有权限访问此资源";
                    break;

                case KeyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = "资源未找到";
                    response.Details = isDevelopment ? exception.Message : "请求的资源不存在";
                    break;

                case InvalidOperationException invalidEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "操作无效";
                    response.Details = isDevelopment ? invalidEx.Message : "当前操作不被允许";
                    break;

                case TimeoutException:
                    response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    response.Message = "请求超时";
                    response.Details = "请求处理时间过长，请稍后重试";
                    break;

                case BusinessException businessEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = businessEx.Message;
                    response.Details = businessEx.Details;
                    response.ErrorCode = businessEx.ErrorCode;
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = "服务器内部错误";
                    response.Details = isDevelopment ? exception.Message : "系统发生了未知错误，请联系管理员";
                    // 在生产环境中，不暴露敏感的异常信息
                    if (!isDevelopment)
                    {
                        response.ErrorCode = "INTERNAL_ERROR";
                        response.TraceId = context.TraceIdentifier;
                    }
                    break;
            }

            context.Response.StatusCode = response.StatusCode;

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    /// <summary>
    /// 错误响应模型
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// HTTP状态码
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 错误详情
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// 错误代码
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 请求路径
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// 跟踪ID
        /// </summary>
        public string? TraceId { get; set; }
    }

    /// <summary>
    /// 业务异常类
    /// </summary>
    public class BusinessException : Exception
    {
        /// <summary>
        /// 错误代码
        /// </summary>
        public string? ErrorCode { get; }

        /// <summary>
        /// 错误详情
        /// </summary>
        public string? Details { get; }

        public BusinessException(string message) : base(message)
        {
        }

        public BusinessException(string message, string errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        public BusinessException(string message, string errorCode, string details) : base(message)
        {
            ErrorCode = errorCode;
            Details = details;
        }

        public BusinessException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// 全局异常处理中间件扩展方法
    /// </summary>
    public static class GlobalExceptionMiddlewareExtensions
    {
        /// <summary>
        /// 使用全局异常处理中间件
        /// </summary>
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}
