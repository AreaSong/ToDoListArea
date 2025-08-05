namespace ToDoListArea.Middleware
{
    /// <summary>
    /// 安全头中间件
    /// 添加各种安全相关的HTTP头
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityHeadersMiddleware> _logger;

        public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 添加安全头
            AddSecurityHeaders(context);

            await _next(context);
        }

        private static void AddSecurityHeaders(HttpContext context)
        {
            var headers = context.Response.Headers;

            // X-Content-Type-Options: 防止MIME类型嗅探
            headers.Append("X-Content-Type-Options", "nosniff");

            // X-Frame-Options: 防止点击劫持
            headers.Append("X-Frame-Options", "DENY");

            // X-XSS-Protection: XSS保护
            headers.Append("X-XSS-Protection", "1; mode=block");

            // Referrer-Policy: 控制引用信息
            headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

            // Content-Security-Policy: 内容安全策略
            var csp = "default-src 'self'; " +
                     "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                     "style-src 'self' 'unsafe-inline'; " +
                     "img-src 'self' data: https:; " +
                     "font-src 'self' data:; " +
                     "connect-src 'self'; " +
                     "frame-ancestors 'none'; " +
                     "base-uri 'self'; " +
                     "form-action 'self'";
            headers.Append("Content-Security-Policy", csp);

            // Strict-Transport-Security: 强制HTTPS (仅在HTTPS下添加)
            if (context.Request.IsHttps)
            {
                headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
            }

            // Permissions-Policy: 权限策略
            var permissionsPolicy = "camera=(), microphone=(), geolocation=(), " +
                                   "payment=(), usb=(), magnetometer=(), gyroscope=(), " +
                                   "accelerometer=(), ambient-light-sensor=()";
            headers.Append("Permissions-Policy", permissionsPolicy);

            // X-Permitted-Cross-Domain-Policies: 跨域策略
            headers.Append("X-Permitted-Cross-Domain-Policies", "none");

            // Cache-Control: 缓存控制 (对于API响应)
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
                headers.Append("Pragma", "no-cache");
                headers.Append("Expires", "0");
            }

            // 移除可能暴露服务器信息的头
            headers.Remove("Server");
            headers.Remove("X-Powered-By");
            headers.Remove("X-AspNet-Version");
            headers.Remove("X-AspNetMvc-Version");
        }
    }

    /// <summary>
    /// 安全头中间件扩展方法
    /// </summary>
    public static class SecurityHeadersMiddlewareExtensions
    {
        /// <summary>
        /// 使用安全头中间件
        /// </summary>
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
}
