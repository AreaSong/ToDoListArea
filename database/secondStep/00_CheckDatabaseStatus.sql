-- =============================================
-- 数据库状态检查脚本
-- 用于确认当前数据库结构和需要执行的操作
-- =============================================

USE ToDoListArea;
GO

PRINT '=== ToDoListArea 数据库状态检查 ===';
PRINT '检查时间: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '';

-- 检查用户表是否存在role字段
PRINT '1. 检查用户表结构：';
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'users' AND COLUMN_NAME = 'role')
    PRINT '   ✓ users表已包含role字段'
ELSE
    PRINT '   ✗ users表缺少role字段 - 需要执行升级脚本';

-- 检查邀请码表是否存在
PRINT '2. 检查邀请码相关表：';
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'invitation_codes')
    PRINT '   ✓ invitation_codes表已存在'
ELSE
    PRINT '   ✗ invitation_codes表不存在 - 需要执行升级脚本';

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'invitation_code_usages')
    PRINT '   ✓ invitation_code_usages表已存在'
ELSE
    PRINT '   ✗ invitation_code_usages表不存在 - 需要执行升级脚本';

-- 检查现有用户数据
PRINT '3. 检查现有数据：';
DECLARE @UserCount INT = (SELECT COUNT(*) FROM users);
PRINT '   用户总数: ' + CAST(@UserCount AS VARCHAR);

IF @UserCount > 0
BEGIN
    DECLARE @AdminCount INT = (SELECT COUNT(*) FROM users WHERE role = 'admin');
    PRINT '   管理员数量: ' + CAST(@AdminCount AS VARCHAR);
    
    IF @AdminCount = 0
        PRINT '   ⚠️  警告：没有管理员用户 - 需要创建管理员账户';
END

-- 检查索引
PRINT '4. 检查索引状态：';
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_users_role')
    PRINT '   ✓ users表role字段索引已存在'
ELSE
    PRINT '   ✗ users表role字段索引不存在';

PRINT '';
PRINT '=== 建议的执行步骤 ===';

-- 根据检查结果给出建议
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'users' AND COLUMN_NAME = 'role')
   OR NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'invitation_codes')
   OR NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'invitation_code_usages')
BEGIN
    PRINT '需要执行以下步骤：';
    PRINT '1. 执行安全升级脚本: database/03_SecurityUpgrade_Migration.sql';
    PRINT '2. 创建管理员账户: database/04_CreateAdminUser.sql';
    PRINT '3. 运行安全测试: database/05_SecurityTest.sql';
END
ELSE
BEGIN
    PRINT '数据库结构已是最新状态！';
    PRINT '可以直接启动应用程序。';
END

PRINT '';
GO
