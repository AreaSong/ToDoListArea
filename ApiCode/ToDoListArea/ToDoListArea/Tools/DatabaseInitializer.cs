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
        /// 检查并初始化数据库
        /// </summary>
        public static async Task<bool> EnsureDatabaseAsync(ToDoListAreaDbContext context, ILogger logger)
        {
            try
            {
                logger.LogInformation("开始检查数据库连接...");

                // 1. 检查数据库连接
                var canConnect = await context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    logger.LogError("无法连接到数据库服务器");
                    return false;
                }

                logger.LogInformation("数据库服务器连接成功");

                // 2. 检查数据库是否存在
                var databaseExists = await context.Database.EnsureCreatedAsync();
                if (databaseExists)
                {
                    logger.LogInformation("数据库已创建");
                }
                else
                {
                    logger.LogInformation("数据库已存在");
                }

                // 3. 检查关键表是否存在
                await CheckCriticalTablesAsync(context, logger);

                // 4. 检查是否有初始数据
                await CheckInitialDataAsync(context, logger);

                logger.LogInformation("数据库初始化检查完成");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "数据库初始化检查失败: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 检查关键表是否存在
        /// </summary>
        private static async Task CheckCriticalTablesAsync(ToDoListAreaDbContext context, ILogger logger)
        {
            try
            {
                // 检查用户表
                var userCount = await context.Users.CountAsync();
                logger.LogInformation("用户表检查完成，当前用户数: {Count}", userCount);

                // 检查邀请码表
                var invitationCodeCount = await context.InvitationCodes.CountAsync();
                logger.LogInformation("邀请码表检查完成，当前邀请码数: {Count}", invitationCodeCount);

                // 检查任务表
                var taskCount = await context.Tasks.CountAsync();
                logger.LogInformation("任务表检查完成，当前任务数: {Count}", taskCount);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "检查关键表时出现问题: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 检查初始数据
        /// </summary>
        private static async Task CheckInitialDataAsync(ToDoListAreaDbContext context, ILogger logger)
        {
            try
            {
                // 检查是否有管理员用户
                var adminExists = await context.Users.AnyAsync(u => u.Role == "admin");
                if (!adminExists)
                {
                    logger.LogWarning("未找到管理员用户，建议执行管理员创建脚本");
                }
                else
                {
                    logger.LogInformation("管理员用户已存在");
                }

                // 检查是否有初始邀请码
                var invitationCodeExists = await context.InvitationCodes.AnyAsync();
                if (!invitationCodeExists)
                {
                    logger.LogWarning("未找到邀请码，建议执行初始化脚本");
                }
                else
                {
                    var activeCodesCount = await context.InvitationCodes
                        .CountAsync(ic => ic.Status == "active");
                    logger.LogInformation("邀请码已存在，活跃邀请码数: {Count}", activeCodesCount);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "检查初始数据时出现问题: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// 获取数据库连接诊断信息
        /// </summary>
        public static async Task<string> GetDatabaseDiagnosticsAsync(ToDoListAreaDbContext context)
        {
            var diagnostics = new List<string>();

            try
            {
                // 连接字符串信息（隐藏敏感信息）
                var connectionString = context.Database.GetConnectionString();
                var safeConnectionString = connectionString?.Replace("Password=", "Password=***");
                diagnostics.Add($"连接字符串: {safeConnectionString}");

                // 数据库提供程序
                diagnostics.Add($"数据库提供程序: {context.Database.ProviderName}");

                // 连接状态
                var canConnect = await context.Database.CanConnectAsync();
                diagnostics.Add($"连接状态: {(canConnect ? "成功" : "失败")}");

                if (canConnect)
                {
                    // 数据库版本信息
                    try
                    {
                        using var command = context.Database.GetDbConnection().CreateCommand();
                        command.CommandText = "SELECT @@VERSION";
                        await context.Database.OpenConnectionAsync();
                        var result = await command.ExecuteScalarAsync();
                        var version = result?.ToString();
                        diagnostics.Add($"数据库版本: {version?.Split('\n')[0]}");
                    }
                    catch (Exception ex)
                    {
                        diagnostics.Add($"获取数据库版本失败: {ex.Message}");
                    }
                    finally
                    {
                        await context.Database.CloseConnectionAsync();
                    }

                    // 表统计信息
                    try
                    {
                        var tableStats = new Dictionary<string, int>
                        {
                            ["用户"] = await context.Users.CountAsync(),
                            ["邀请码"] = await context.InvitationCodes.CountAsync(),
                            ["任务"] = await context.Tasks.CountAsync(),
                            ["任务分类"] = await context.TaskCategories.CountAsync()
                        };

                        diagnostics.Add("表统计信息:");
                        foreach (var stat in tableStats)
                        {
                            diagnostics.Add($"  {stat.Key}: {stat.Value} 条记录");
                        }
                    }
                    catch (Exception ex)
                    {
                        diagnostics.Add($"获取表统计信息失败: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                diagnostics.Add($"诊断过程中出现错误: {ex.Message}");
            }

            return string.Join("\n", diagnostics);
        }
    }
}
