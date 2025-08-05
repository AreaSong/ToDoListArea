using DbContextHelp.Models;
using ToDoListArea.Models;
using System.Security.Claims;
using System.Text.Json;

namespace ToDoListArea.Middleware
{
    /// <summary>
    /// 用户活动追踪中间件
    /// </summary>
    public class UserActivityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserActivityMiddleware> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        // 需要记录的API路径模式（注意：路径大小写敏感）
        private readonly Dictionary<string, string> _trackingPatterns = new()
        {
            { "POST /api/user/login", ActivityTypes.Login },
            { "POST /api/user/logout", ActivityTypes.Logout },
            { "POST /api/task/user/{userId}", ActivityTypes.CreateTask },
            { "PUT /api/task/{id}", ActivityTypes.UpdateTask },
            { "DELETE /api/task/{id}", ActivityTypes.DeleteTask },
            { "GET /api/task/{id}", ActivityTypes.ViewTask },
            { "POST /api/taskdetails/task/{taskId}", ActivityTypes.CreateTaskDetail },
            { "PUT /api/taskdetails/{id}", ActivityTypes.UpdateTaskDetail },
            { "DELETE /api/taskdetails/{id}", ActivityTypes.DeleteTaskDetail },
            { "POST /api/tasktemplate", ActivityTypes.CreateTemplate },
            { "PUT /api/tasktemplate/{id}", ActivityTypes.UpdateTemplate },
            { "DELETE /api/tasktemplate/{id}", ActivityTypes.DeleteTemplate },
            { "POST /api/tasktemplate/use", ActivityTypes.UseTemplate },
            { "PUT /api/userprofile/{userId}", ActivityTypes.UpdateProfile },
            { "GET /api/task/user/{userId}/dashboard", ActivityTypes.ViewDashboard },
            { "GET /api/ganttdata", ActivityTypes.ViewGantt },
            { "GET /api/tasktemplate", ActivityTypes.ViewTemplates },
            { "GET /api/userprofile/{userId}", ActivityTypes.ViewProfile }
        };

        public UserActivityMiddleware(
            RequestDelegate next, 
            ILogger<UserActivityMiddleware> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async System.Threading.Tasks.Task InvokeAsync(HttpContext context)
        {
            // 先执行请求
            await _next(context);

            // 请求完成后记录活动（仅记录成功的请求）
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                _logger.LogInformation("中间件触发: {Method} {Path}, StatusCode: {StatusCode}",
                    context.Request.Method, context.Request.Path, context.Response.StatusCode);
                await RecordUserActivityAsync(context);
            }
        }

        private async System.Threading.Tasks.Task RecordUserActivityAsync(HttpContext context)
        {
            try
            {
                // 获取用户ID
                var userId = GetUserIdFromContext(context);
                _logger.LogInformation("获取用户ID: {UserId}", userId);
                if (userId == null)
                {
                    _logger.LogInformation("未登录用户，跳过记录");
                    return; // 未登录用户不记录
                }

                // 检查是否需要记录此请求
                var activityType = GetActivityType(context);
                _logger.LogInformation("获取活动类型: {ActivityType}, 路径: {Method} {Path}",
                    activityType, context.Request.Method, context.Request.Path);
                if (string.IsNullOrEmpty(activityType))
                {
                    _logger.LogInformation("不需要记录的请求，跳过");
                    return; // 不需要记录的请求
                }

                // 获取实体信息
                var (entityType, entityId) = GetEntityInfo(context);

                // 生成活动描述
                var description = GenerateActivityDescription(context, activityType);

                // 生成元数据
                var metadata = GenerateMetadata(context);

                // 记录到数据库
                using var scope = _serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ToDoListAreaDbContext>();

                var activity = new UserActivity
                {
                    Id = Guid.NewGuid(),
                    UserId = userId.Value,
                    ActivityType = activityType,
                    ActivityDescription = description,
                    EntityType = entityType,
                    EntityId = entityId,
                    Metadata = metadata,
                    IpAddress = GetClientIpAddress(context),
                    UserAgent = context.Request.Headers["User-Agent"].ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.UserActivities.Add(activity);
                await dbContext.SaveChangesAsync();

                _logger.LogInformation("记录用户活动: UserId={UserId}, ActivityType={ActivityType}, Description={Description}", 
                    userId, activityType, description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "记录用户活动失败");
                // 不抛出异常，避免影响正常请求
            }
        }

        private Guid? GetUserIdFromContext(HttpContext context)
        {
            var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }

        private string? GetActivityType(HttpContext context)
        {
            var method = context.Request.Method;
            var path = context.Request.Path.Value ?? "";

            // 精确匹配
            var key = $"{method} {path}";
            if (_trackingPatterns.TryGetValue(key, out var activityType))
            {
                return activityType;
            }

            // 模式匹配
            foreach (var pattern in _trackingPatterns)
            {
                if (IsPathMatch(pattern.Key, key))
                {
                    return pattern.Value;
                }
            }

            return null;
        }

        private bool IsPathMatch(string pattern, string actual)
        {
            // 简单的路径匹配，支持参数
            // 例如: "GET /api/Task/{id}" 匹配 "GET /api/Task/123"
            var patternParts = pattern.Split(' ');
            var actualParts = actual.Split(' ');

            if (patternParts.Length != 2 || actualParts.Length != 2)
                return false;

            if (patternParts[0] != actualParts[0]) // HTTP方法必须匹配
                return false;

            var patternPath = patternParts[1].Split('/');
            var actualPath = actualParts[1].Split('/');

            if (patternPath.Length != actualPath.Length)
                return false;

            for (int i = 0; i < patternPath.Length; i++)
            {
                if (patternPath[i].StartsWith("{") && patternPath[i].EndsWith("}"))
                {
                    continue; // 参数段，跳过
                }
                if (patternPath[i] != actualPath[i])
                {
                    return false;
                }
            }

            return true;
        }

        private (string? entityType, Guid? entityId) GetEntityInfo(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";
            
            // 从路径中提取实体信息
            if (path.Contains("/api/Task/"))
            {
                var segments = path.Split('/');
                if (segments.Length > 3 && Guid.TryParse(segments[3], out var taskId))
                {
                    return (EntityTypes.Task, taskId);
                }
                return (EntityTypes.Task, null);
            }
            
            if (path.Contains("/api/TaskDetails/"))
            {
                return (EntityTypes.TaskDetail, null);
            }
            
            if (path.Contains("/api/TaskTemplate/"))
            {
                return (EntityTypes.TaskTemplate, null);
            }
            
            if (path.Contains("/api/UserProfile/"))
            {
                return (EntityTypes.UserProfile, null);
            }

            return (null, null);
        }

        private string GenerateActivityDescription(HttpContext context, string activityType)
        {
            var method = context.Request.Method;
            var path = context.Request.Path.Value ?? "";

            return activityType switch
            {
                ActivityTypes.Login => "用户登录系统",
                ActivityTypes.Logout => "用户退出系统",
                ActivityTypes.CreateTask => "创建新任务",
                ActivityTypes.UpdateTask => "更新任务信息",
                ActivityTypes.DeleteTask => "删除任务",
                ActivityTypes.ViewTask => "查看任务详情",
                ActivityTypes.CreateTaskDetail => "添加任务详情",
                ActivityTypes.UpdateTaskDetail => "更新任务详情",
                ActivityTypes.DeleteTaskDetail => "删除任务详情",
                ActivityTypes.CreateTemplate => "创建任务模板",
                ActivityTypes.UpdateTemplate => "更新任务模板",
                ActivityTypes.DeleteTemplate => "删除任务模板",
                ActivityTypes.UseTemplate => "使用任务模板",
                ActivityTypes.UpdateProfile => "更新个人资料",
                ActivityTypes.ViewDashboard => "访问控制台",
                ActivityTypes.ViewGantt => "查看甘特图",
                ActivityTypes.ViewTemplates => "查看模板列表",
                ActivityTypes.ViewProfile => "查看个人资料",
                _ => $"执行操作: {method} {path}"
            };
        }

        private string? GenerateMetadata(HttpContext context)
        {
            var metadata = new Dictionary<string, object>
            {
                { "method", context.Request.Method },
                { "path", context.Request.Path.Value ?? "" },
                { "statusCode", context.Response.StatusCode },
                { "timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
            };

            // 添加查询参数
            if (context.Request.Query.Any())
            {
                metadata["queryParams"] = context.Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
            }

            return JsonSerializer.Serialize(metadata);
        }

        private string GetClientIpAddress(HttpContext context)
        {
            var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            }
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = context.Connection.RemoteIpAddress?.ToString();
            }
            return ipAddress ?? "Unknown";
        }
    }

    /// <summary>
    /// 中间件扩展方法
    /// </summary>
    public static class UserActivityMiddlewareExtensions
    {
        public static IApplicationBuilder UseUserActivityTracking(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserActivityMiddleware>();
        }
    }
}
