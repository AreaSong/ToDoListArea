-- =============================================
-- 密码哈希生成脚本
-- 用于生成BCrypt密码哈希
-- =============================================

-- 注意：此脚本仅用于演示，实际生产环境中应使用应用程序生成密码哈希

PRINT '=== 密码哈希生成指南 ===';
PRINT '';
PRINT '由于SQL Server不支持BCrypt，请使用以下方式生成密码哈希：';
PRINT '';
PRINT '方式一：使用在线BCrypt生成器';
PRINT '1. 访问：https://bcrypt-generator.com/';
PRINT '2. 输入您的密码';
PRINT '3. 选择rounds: 11（推荐）';
PRINT '4. 复制生成的哈希值';
PRINT '';
PRINT '方式二：使用.NET应用程序生成';
PRINT '在您的.NET项目中运行以下代码：';
PRINT '';
PRINT 'using BCrypt.Net;';
PRINT 'string password = "your-password";';
PRINT 'string hash = BCrypt.HashPassword(password, 11);';
PRINT 'Console.WriteLine(hash);';
PRINT '';
PRINT '方式三：使用PowerShell（如果安装了BCrypt模块）';
PRINT 'Install-Module -Name BCrypt';
PRINT '$hash = ConvertTo-BCryptHash -Password "your-password"';
PRINT 'Write-Output $hash';
PRINT '';
PRINT '示例哈希值（密码：Admin123!）：';
PRINT '$2a$11$example.hash.for.Admin123.password.here';
PRINT '';
PRINT '⚠️ 重要提醒：';
PRINT '- 请使用强密码（至少8位，包含大小写字母、数字、特殊字符）';
PRINT '- 不要在生产环境中使用示例密码';
PRINT '- 生成的哈希值应该以 $2a$11$ 开头';
GO
