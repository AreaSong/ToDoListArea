using System.Security.Cryptography;
using System.Text;

namespace ToDoListArea.Utils
{
    /// <summary>
    /// 密码加密工具类
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// 生成密码哈希值
        /// </summary>
        /// <param name="password">原始密码</param>
        /// <returns>哈希后的密码</returns>
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "ToDoListSalt"));
            return Convert.ToBase64String(hashedBytes);
        }

        /// <summary>
        /// 验证密码
        /// </summary>
        /// <param name="password">输入的密码</param>
        /// <param name="hashedPassword">存储的哈希密码</param>
        /// <returns>是否匹配</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            var inputHash = HashPassword(password);
            return inputHash == hashedPassword;
        }
    }
}
