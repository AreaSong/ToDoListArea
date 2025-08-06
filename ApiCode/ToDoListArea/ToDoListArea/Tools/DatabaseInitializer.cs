using DbContextHelp.Models;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace ToDoListArea.Tools
{
    /// <summary>
    /// 数据库初始化工具
    /// </summary>
    public static class DatabaseInitializer
    {
        /// <summary>
        /// 确保数据库存在（生产环境简化版）
        /// </summary>
        public static async Task<bool> EnsureDatabaseAsync(ToDoListAreaDbContext context, ILogger logger)
        {
            try
            {
                // 简单检查数据库连接并确保创建
                var canConnect = await context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return false;
                }

                await context.Database.EnsureCreatedAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }




    }
}
