using DbContextHelp.Models;
using System.Security.Claims;

namespace ToDoListArea.Services
{
    /// <summary>
    /// JWT服务接口
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// 生成JWT Token
        /// </summary>
        string GenerateToken(User user);

        /// <summary>
        /// 验证JWT Token
        /// </summary>
        ClaimsPrincipal? ValidateToken(string token);

        /// <summary>
        /// 从Token中获取用户ID
        /// </summary>
        Guid? GetUserIdFromToken(string token);
    }
}
