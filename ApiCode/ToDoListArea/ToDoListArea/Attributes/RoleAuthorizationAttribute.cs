using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace ToDoListArea.Attributes
{
    /// <summary>
    /// 角色授权属性
    /// </summary>
    public class RoleAuthorizationAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _requiredRoles;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="roles">所需角色</param>
        public RoleAuthorizationAttribute(params string[] roles)
        {
            _requiredRoles = roles ?? throw new ArgumentNullException(nameof(roles));
        }

        /// <summary>
        /// 授权验证
        /// </summary>
        /// <param name="context">授权过滤器上下文</param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // 检查用户是否已认证
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    success = false,
                    message = "用户未登录",
                    code = "UNAUTHORIZED"
                });
                return;
            }

            // 获取用户角色
            var userRoles = context.HttpContext.User.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            // 也检查自定义的role claim
            var customRoles = context.HttpContext.User.FindAll("role")
                .Select(c => c.Value)
                .ToList();

            userRoles.AddRange(customRoles);

            // 检查用户是否具有所需角色
            var hasRequiredRole = _requiredRoles.Any(requiredRole => 
                userRoles.Any(userRole => 
                    string.Equals(userRole, requiredRole, StringComparison.OrdinalIgnoreCase)));

            if (!hasRequiredRole)
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }

    /// <summary>
    /// 管理员角色授权属性
    /// </summary>
    public class AdminOnlyAttribute : RoleAuthorizationAttribute
    {
        public AdminOnlyAttribute() : base("admin")
        {
        }
    }

    /// <summary>
    /// 用户角色授权属性（管理员和普通用户都可以访问）
    /// </summary>
    public class UserRoleAttribute : RoleAuthorizationAttribute
    {
        public UserRoleAttribute() : base("admin", "user")
        {
        }
    }
}

namespace ToDoListArea.Extensions
{
    /// <summary>
    /// 用户扩展方法
    /// </summary>
    public static class UserExtensions
    {
        /// <summary>
        /// 检查用户是否具有指定角色
        /// </summary>
        /// <param name="user">用户主体</param>
        /// <param name="role">角色名称</param>
        /// <returns>是否具有角色</returns>
        public static bool HasRole(this ClaimsPrincipal user, string role)
        {
            if (user == null || string.IsNullOrEmpty(role))
                return false;

            // 检查标准角色声明
            var hasStandardRole = user.IsInRole(role);
            if (hasStandardRole)
                return true;

            // 检查自定义角色声明
            var customRoles = user.FindAll("role").Select(c => c.Value);
            return customRoles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 检查用户是否为管理员
        /// </summary>
        /// <param name="user">用户主体</param>
        /// <returns>是否为管理员</returns>
        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            return user.HasRole("admin");
        }

        /// <summary>
        /// 获取用户ID
        /// </summary>
        /// <param name="user">用户主体</param>
        /// <returns>用户ID</returns>
        public static Guid? GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst("user_id") ?? user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return userId;
            }
            return null;
        }

        /// <summary>
        /// 获取用户邮箱
        /// </summary>
        /// <param name="user">用户主体</param>
        /// <returns>用户邮箱</returns>
        public static string? GetUserEmail(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value ?? user.FindFirst("email")?.Value;
        }

        /// <summary>
        /// 获取用户姓名
        /// </summary>
        /// <param name="user">用户主体</param>
        /// <returns>用户姓名</returns>
        public static string? GetUserName(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value ?? user.FindFirst("name")?.Value;
        }

        /// <summary>
        /// 获取用户角色列表
        /// </summary>
        /// <param name="user">用户主体</param>
        /// <returns>角色列表</returns>
        public static List<string> GetUserRoles(this ClaimsPrincipal user)
        {
            var roles = new List<string>();
            
            // 获取标准角色声明
            roles.AddRange(user.FindAll(ClaimTypes.Role).Select(c => c.Value));
            
            // 获取自定义角色声明
            roles.AddRange(user.FindAll("role").Select(c => c.Value));
            
            return roles.Distinct().ToList();
        }
    }
}
