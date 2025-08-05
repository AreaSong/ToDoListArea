using System.Security.Claims;
using System.Text.Json;
using ToDoListArea.Extensions;
using DbContextHelp.Models;
using Microsoft.EntityFrameworkCore;
using TaskModel = DbContextHelp.Models.Task;
using Task = System.Threading.Tasks.Task;

namespace ToDoListArea.Middleware
{
    /// <summary>
    /// 角色授权中间件
    /// </summary>
    public class RoleAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RoleAuthorizationMiddleware> _logger;

        public RoleAuthorizationMiddleware(RequestDelegate next, ILogger<RoleAuthorizationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// 中间件执行方法
        /// </summary>
        /// <param name="context">HTTP上下文</param>
        /// <returns>任务</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            // 记录用户访问信息（仅在用户已认证时）
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.GetUserId();
                var userEmail = context.User.GetUserEmail();
                var userRoles = context.User.GetUserRoles();
                var path = context.Request.Path;
                var method = context.Request.Method;

                _logger.LogInformation(
                    "用户访问: UserId={UserId}, Email={Email}, Roles={Roles}, Method={Method}, Path={Path}",
                    userId, userEmail, string.Join(",", userRoles), method, path);

                // 检查敏感操作的权限
                if (IsSensitiveOperation(context))
                {
                    if (!context.User.IsAdmin())
                    {
                        _logger.LogWarning(
                            "非管理员用户尝试访问敏感操作: UserId={UserId}, Email={Email}, Path={Path}",
                            userId, userEmail, path);

                        await WriteUnauthorizedResponse(context, "权限不足，需要管理员权限");
                        return;
                    }
                }
            }

            await _next(context);
        }

        /// <summary>
        /// 检查是否为敏感操作
        /// </summary>
        /// <param name="context">HTTP上下文</param>
        /// <returns>是否为敏感操作</returns>
        private static bool IsSensitiveOperation(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";
            var method = context.Request.Method.ToUpper();

            // 定义敏感操作路径模式
            var sensitivePatterns = new[]
            {
                "/api/invitationcode", // 邀请码管理
                "/api/admin",          // 管理员专用接口
                "/api/user/admin",     // 用户管理接口
                "/api/system"          // 系统配置接口
            };

            // 检查是否匹配敏感路径
            var isSensitivePath = sensitivePatterns.Any(pattern => path.StartsWith(pattern));

            // 对于用户相关的DELETE和PUT操作也需要特殊权限检查
            if (path.StartsWith("/api/user") && (method == "DELETE" || method == "PUT"))
            {
                // 如果是修改其他用户信息，需要管理员权限
                // 这里可以进一步细化权限控制逻辑
                return true;
            }

            return isSensitivePath;
        }

        /// <summary>
        /// 写入未授权响应
        /// </summary>
        /// <param name="context">HTTP上下文</param>
        /// <param name="message">错误消息</param>
        /// <returns>任务</returns>
        private static async Task WriteUnauthorizedResponse(HttpContext context, string message)
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";

            var response = new
            {
                success = false,
                message = message,
                code = "FORBIDDEN",
                timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }

    /// <summary>
    /// 角色授权中间件扩展方法
    /// </summary>
    public static class RoleAuthorizationMiddlewareExtensions
    {
        /// <summary>
        /// 使用角色授权中间件
        /// </summary>
        /// <param name="builder">应用程序构建器</param>
        /// <returns>应用程序构建器</returns>
        public static IApplicationBuilder UseRoleAuthorization(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RoleAuthorizationMiddleware>();
        }
    }
}

namespace ToDoListArea.Services
{
    /// <summary>
    /// 权限验证服务接口
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// 检查用户是否有权限访问指定资源
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="resource">资源名称</param>
        /// <param name="action">操作类型</param>
        /// <returns>是否有权限</returns>
        Task<bool> HasPermissionAsync(Guid userId, string resource, string action);

        /// <summary>
        /// 检查用户是否有权限访问其他用户的资源
        /// </summary>
        /// <param name="currentUserId">当前用户ID</param>
        /// <param name="targetUserId">目标用户ID</param>
        /// <param name="resource">资源名称</param>
        /// <param name="action">操作类型</param>
        /// <returns>是否有权限</returns>
        Task<bool> CanAccessUserResourceAsync(Guid currentUserId, Guid targetUserId, string resource, string action);

        /// <summary>
        /// 获取用户权限列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>权限列表</returns>
        Task<List<string>> GetUserPermissionsAsync(Guid userId);
    }

    /// <summary>
    /// 权限验证服务实现
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly ToDoListAreaDbContext _context;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(ToDoListAreaDbContext context, ILogger<PermissionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 检查用户是否有权限访问指定资源
        /// </summary>
        public async Task<bool> HasPermissionAsync(Guid userId, string resource, string action)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return false;
                }

                // 管理员拥有所有权限
                if (user.Role == "admin")
                {
                    return true;
                }

                // 根据资源和操作类型进行权限检查
                return CheckResourcePermission(user.Role, resource, action);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查用户权限失败: UserId={UserId}, Resource={Resource}, Action={Action}", 
                    userId, resource, action);
                return false;
            }
        }

        /// <summary>
        /// 检查用户是否有权限访问其他用户的资源
        /// </summary>
        public async Task<bool> CanAccessUserResourceAsync(Guid currentUserId, Guid targetUserId, string resource, string action)
        {
            try
            {
                // 用户可以访问自己的资源
                if (currentUserId == targetUserId)
                {
                    return true;
                }

                // 检查当前用户是否为管理员
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser?.Role == "admin")
                {
                    return true;
                }

                // 其他情况不允许访问
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查用户资源访问权限失败: CurrentUserId={CurrentUserId}, TargetUserId={TargetUserId}, Resource={Resource}, Action={Action}", 
                    currentUserId, targetUserId, resource, action);
                return false;
            }
        }

        /// <summary>
        /// 获取用户权限列表
        /// </summary>
        public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new List<string>();
                }

                var permissions = new List<string>();

                if (user.Role == "admin")
                {
                    permissions.AddRange(new[]
                    {
                        "user:read", "user:write", "user:delete",
                        "invitation:read", "invitation:write", "invitation:delete",
                        "task:read", "task:write", "task:delete",
                        "system:read", "system:write"
                    });
                }
                else if (user.Role == "user")
                {
                    permissions.AddRange(new[]
                    {
                        "task:read", "task:write", "task:delete",
                        "profile:read", "profile:write"
                    });
                }

                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户权限列表失败: UserId={UserId}", userId);
                return new List<string>();
            }
        }

        /// <summary>
        /// 检查资源权限
        /// </summary>
        /// <param name="userRole">用户角色</param>
        /// <param name="resource">资源名称</param>
        /// <param name="action">操作类型</param>
        /// <returns>是否有权限</returns>
        private static bool CheckResourcePermission(string userRole, string resource, string action)
        {
            // 定义角色权限映射
            var rolePermissions = new Dictionary<string, List<string>>
            {
                ["admin"] = new List<string> { "*" }, // 管理员拥有所有权限
                ["user"] = new List<string> 
                { 
                    "task:read", "task:write", "task:delete",
                    "profile:read", "profile:write"
                }
            };

            if (!rolePermissions.ContainsKey(userRole))
            {
                return false;
            }

            var permissions = rolePermissions[userRole];
            var requiredPermission = $"{resource}:{action}";

            return permissions.Contains("*") || permissions.Contains(requiredPermission);
        }
    }
}
