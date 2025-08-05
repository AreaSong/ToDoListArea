using System;
using BCrypt.Net;

namespace PasswordHashGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            // 为admin@AreaSong.com生成密码哈希
            string password = "Admin123!";
            string hash = BCrypt.HashPassword(password, 11);
            
            Console.WriteLine("=== 密码哈希生成结果 ===");
            Console.WriteLine($"原始密码: {password}");
            Console.WriteLine($"BCrypt哈希: {hash}");
            Console.WriteLine();
            Console.WriteLine("请将此哈希值复制到SQL脚本中的@NewAdminPassword变量");
            Console.WriteLine();
            
            // 验证哈希是否正确
            bool isValid = BCrypt.Verify(password, hash);
            Console.WriteLine($"哈希验证: {(isValid ? "✓ 成功" : "✗ 失败")}");
        }
    }
}

// 如果您没有安装BCrypt.Net包，请在项目目录中运行：
// dotnet add package BCrypt.Net-Next
