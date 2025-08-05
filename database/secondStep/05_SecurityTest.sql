-- =============================================
-- ToDoListArea 安全访问控制系统测试脚本
-- 版本: v1.0
-- 创建时间: 2025-01-21
-- 描述: 测试邀请码系统和角色权限控制
-- =============================================

USE ToDoListArea;
GO

PRINT '开始执行安全访问控制系统测试...';
PRINT '当前数据库: ' + DB_NAME();
PRINT '执行时间: ' + CONVERT(VARCHAR, GETDATE(), 120);
GO

-- =============================================
-- 第一步：验证表结构
-- =============================================
PRINT '步骤 1: 验证表结构...';

-- 验证用户表是否有role字段
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'users' AND COLUMN_NAME = 'role')
    PRINT '✓ 用户表role字段存在';
ELSE
    PRINT '✗ 用户表role字段不存在';

-- 验证邀请码表
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'invitation_codes')
    PRINT '✓ 邀请码表存在';
ELSE
    PRINT '✗ 邀请码表不存在';

-- 验证邀请码使用记录表
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'invitation_code_usages')
    PRINT '✓ 邀请码使用记录表存在';
ELSE
    PRINT '✗ 邀请码使用记录表不存在';

GO

-- =============================================
-- 第二步：创建测试数据
-- =============================================
PRINT '步骤 2: 创建测试数据...';

-- 创建测试管理员用户
DECLARE @AdminUserId UNIQUEIDENTIFIER = NEWID();
DECLARE @TestUserId UNIQUEIDENTIFIER = NEWID();

-- 插入管理员用户（如果不存在）
IF NOT EXISTS (SELECT * FROM users WHERE email = 'admin@test.com')
BEGIN
    INSERT INTO users (id, email, name, password_hash, role, status, email_verified, phone_verified, created_at, updated_at)
    VALUES (@AdminUserId, 'admin@test.com', '测试管理员', '$2a$11$test.hash.for.admin', 'admin', 'active', 1, 0, GETDATE(), GETDATE());
    PRINT '✓ 测试管理员用户创建成功';
END
ELSE
BEGIN
    SELECT @AdminUserId = id FROM users WHERE email = 'admin@test.com';
    PRINT '✓ 测试管理员用户已存在';
END

-- 插入普通用户（如果不存在）
IF NOT EXISTS (SELECT * FROM users WHERE email = 'user@test.com')
BEGIN
    INSERT INTO users (id, email, name, password_hash, role, status, email_verified, phone_verified, created_at, updated_at)
    VALUES (@TestUserId, 'user@test.com', '测试用户', '$2a$11$test.hash.for.user', 'user', 'active', 1, 0, GETDATE(), GETDATE());
    PRINT '✓ 测试普通用户创建成功';
END
ELSE
BEGIN
    SELECT @TestUserId = id FROM users WHERE email = 'user@test.com';
    PRINT '✓ 测试普通用户已存在';
END

-- 创建测试邀请码
IF NOT EXISTS (SELECT * FROM invitation_codes WHERE code = 'TEST2025')
BEGIN
    INSERT INTO invitation_codes (code, max_uses, used_count, expires_at, status, created_by, created_at, updated_at)
    VALUES ('TEST2025', 10, 0, DATEADD(DAY, 30, GETDATE()), 'active', @AdminUserId, GETDATE(), GETDATE());
    PRINT '✓ 测试邀请码创建成功';
END
ELSE
BEGIN
    PRINT '✓ 测试邀请码已存在';
END

-- 创建过期邀请码
IF NOT EXISTS (SELECT * FROM invitation_codes WHERE code = 'EXPIRED2024')
BEGIN
    INSERT INTO invitation_codes (code, max_uses, used_count, expires_at, status, created_by, created_at, updated_at)
    VALUES ('EXPIRED2024', 5, 0, DATEADD(DAY, -1, GETDATE()), 'active', @AdminUserId, GETDATE(), GETDATE());
    PRINT '✓ 过期邀请码创建成功';
END
ELSE
BEGIN
    PRINT '✓ 过期邀请码已存在';
END

-- 创建已用完的邀请码
IF NOT EXISTS (SELECT * FROM invitation_codes WHERE code = 'FULL2025')
BEGIN
    INSERT INTO invitation_codes (code, max_uses, used_count, expires_at, status, created_by, created_at, updated_at)
    VALUES ('FULL2025', 1, 1, NULL, 'active', @AdminUserId, GETDATE(), GETDATE());
    PRINT '✓ 已用完邀请码创建成功';
END
ELSE
BEGIN
    PRINT '✓ 已用完邀请码已存在';
END

GO

-- =============================================
-- 第三步：测试邀请码验证逻辑
-- =============================================
PRINT '步骤 3: 测试邀请码验证逻辑...';

-- 测试有效邀请码
DECLARE @ValidCode VARCHAR(32) = 'TEST2025';
DECLARE @ExpiredCode VARCHAR(32) = 'EXPIRED2024';
DECLARE @FullCode VARCHAR(32) = 'FULL2025';
DECLARE @InvalidCode VARCHAR(32) = 'INVALID123';

-- 验证有效邀请码
IF EXISTS (
    SELECT * FROM invitation_codes 
    WHERE code = @ValidCode 
    AND status = 'active' 
    AND (expires_at IS NULL OR expires_at > GETDATE())
    AND used_count < max_uses
)
    PRINT '✓ 有效邀请码验证通过';
ELSE
    PRINT '✗ 有效邀请码验证失败';

-- 验证过期邀请码
IF NOT EXISTS (
    SELECT * FROM invitation_codes 
    WHERE code = @ExpiredCode 
    AND status = 'active' 
    AND (expires_at IS NULL OR expires_at > GETDATE())
    AND used_count < max_uses
)
    PRINT '✓ 过期邀请码验证正确拒绝';
ELSE
    PRINT '✗ 过期邀请码验证失败';

-- 验证已用完邀请码
IF NOT EXISTS (
    SELECT * FROM invitation_codes 
    WHERE code = @FullCode 
    AND status = 'active' 
    AND (expires_at IS NULL OR expires_at > GETDATE())
    AND used_count < max_uses
)
    PRINT '✓ 已用完邀请码验证正确拒绝';
ELSE
    PRINT '✗ 已用完邀请码验证失败';

-- 验证不存在的邀请码
IF NOT EXISTS (
    SELECT * FROM invitation_codes 
    WHERE code = @InvalidCode
)
    PRINT '✓ 不存在邀请码验证正确拒绝';
ELSE
    PRINT '✗ 不存在邀请码验证失败';

GO

-- =============================================
-- 第四步：测试角色权限
-- =============================================
PRINT '步骤 4: 测试角色权限...';

-- 验证管理员角色
IF EXISTS (SELECT * FROM users WHERE email = 'admin@test.com' AND role = 'admin')
    PRINT '✓ 管理员角色设置正确';
ELSE
    PRINT '✗ 管理员角色设置错误';

-- 验证普通用户角色
IF EXISTS (SELECT * FROM users WHERE email = 'user@test.com' AND role = 'user')
    PRINT '✓ 普通用户角色设置正确';
ELSE
    PRINT '✗ 普通用户角色设置错误';

GO

-- =============================================
-- 第五步：测试邀请码使用记录
-- =============================================
PRINT '步骤 5: 测试邀请码使用记录...';

-- 模拟邀请码使用
DECLARE @TestInvitationCodeId UNIQUEIDENTIFIER;
DECLARE @TestUserId2 UNIQUEIDENTIFIER;

SELECT @TestInvitationCodeId = id FROM invitation_codes WHERE code = 'TEST2025';
SELECT @TestUserId2 = id FROM users WHERE email = 'user@test.com';

-- 检查是否已有使用记录
IF NOT EXISTS (
    SELECT * FROM invitation_code_usages 
    WHERE invitation_code_id = @TestInvitationCodeId 
    AND user_id = @TestUserId2
)
BEGIN
    -- 插入使用记录
    INSERT INTO invitation_code_usages (invitation_code_id, user_id, used_at, ip_address, user_agent)
    VALUES (@TestInvitationCodeId, @TestUserId2, GETDATE(), '127.0.0.1', 'Test User Agent');
    
    -- 更新邀请码使用次数
    UPDATE invitation_codes 
    SET used_count = used_count + 1, updated_at = GETDATE()
    WHERE id = @TestInvitationCodeId;
    
    PRINT '✓ 邀请码使用记录创建成功';
END
ELSE
BEGIN
    PRINT '✓ 邀请码使用记录已存在';
END

-- 验证唯一约束（同一用户不能重复使用同一邀请码）
BEGIN TRY
    INSERT INTO invitation_code_usages (invitation_code_id, user_id, used_at, ip_address, user_agent)
    VALUES (@TestInvitationCodeId, @TestUserId2, GETDATE(), '127.0.0.1', 'Test User Agent 2');
    PRINT '✗ 唯一约束验证失败 - 允许了重复使用';
END TRY
BEGIN CATCH
    PRINT '✓ 唯一约束验证成功 - 正确阻止了重复使用';
END CATCH

GO

-- =============================================
-- 第六步：生成测试报告
-- =============================================
PRINT '步骤 6: 生成测试报告...';

PRINT '=== 安全访问控制系统测试报告 ===';

-- 用户统计
PRINT '用户统计：';

-- 使用变量存储统计数据
DECLARE @TotalUsers INT, @AdminUsers INT, @RegularUsers INT;
DECLARE @TotalCodes INT, @ActiveCodes INT, @ExpiredCodes INT, @TotalUsages INT;

SELECT @TotalUsers = COUNT(*) FROM users;
SELECT @AdminUsers = COUNT(*) FROM users WHERE role = 'admin';
SELECT @RegularUsers = COUNT(*) FROM users WHERE role = 'user';

PRINT '总用户数: ' + CAST(@TotalUsers AS VARCHAR);
PRINT '管理员数量: ' + CAST(@AdminUsers AS VARCHAR);
PRINT '普通用户数量: ' + CAST(@RegularUsers AS VARCHAR);

-- 邀请码统计
PRINT '邀请码统计：';

SELECT @TotalCodes = COUNT(*) FROM invitation_codes;
SELECT @ActiveCodes = COUNT(*) FROM invitation_codes WHERE status = 'active';
SELECT @ExpiredCodes = COUNT(*) FROM invitation_codes WHERE expires_at IS NOT NULL AND expires_at < GETDATE();
SELECT @TotalUsages = COUNT(*) FROM invitation_code_usages;

PRINT '总邀请码数: ' + CAST(@TotalCodes AS VARCHAR);
PRINT '活跃邀请码数: ' + CAST(@ActiveCodes AS VARCHAR);
PRINT '已过期邀请码数: ' + CAST(@ExpiredCodes AS VARCHAR);
PRINT '总使用次数: ' + CAST(@TotalUsages AS VARCHAR);

-- 显示测试邀请码信息
PRINT '测试邀请码信息：';
SELECT 
    code as '邀请码',
    max_uses as '最大使用次数',
    used_count as '已使用次数',
    CASE 
        WHEN expires_at IS NULL THEN '永不过期'
        ELSE CONVERT(VARCHAR, expires_at, 120)
    END as '过期时间',
    status as '状态'
FROM invitation_codes 
WHERE code IN ('TEST2025', 'EXPIRED2024', 'FULL2025')
ORDER BY created_at;

PRINT '安全访问控制系统测试完成！';
PRINT '完成时间: ' + CONVERT(VARCHAR, GETDATE(), 120);
GO
