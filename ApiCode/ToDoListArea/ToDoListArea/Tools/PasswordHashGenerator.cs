using System.Security.Cryptography;
using System.Text;
using ToDoListArea.Utils;

namespace ToDoListArea.Tools
{
    /// <summary>
    /// 密码哈希生成工具 - 用于生成管理员密码哈希
    /// </summary>
    public static class PasswordHashGenerator
    {
        /// <summary>
        /// 生成密码哈希值（用于数据库更新）
        /// </summary>
        /// <param name="password">原始密码</param>
        /// <returns>哈希后的密码</returns>
        public static string GenerateHash(string password)
        {
            return PasswordHelper.HashPassword(password);
        }

        /// <summary>
        /// 验证密码是否正确
        /// </summary>
        /// <param name="password">原始密码</param>
        /// <param name="hash">哈希值</param>
        /// <returns>是否匹配</returns>
        public static bool VerifyHash(string password, string hash)
        {
            return PasswordHelper.VerifyPassword(password, hash);
        }


    }
}
