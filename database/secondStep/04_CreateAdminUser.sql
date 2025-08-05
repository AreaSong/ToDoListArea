-- =============================================
-- 创建管理员用户和初始邀请码脚本
-- 版本: v1.0
-- 创建时间: 2025-01-21
-- 描述: 创建第一个管理员账户和初始邀请码
-- =============================================

USE ToDoListArea;
GO

-- =============================================
-- 方式一：将现有用户提升为管理员
-- =============================================
PRINT '=== 管理员账户设置 ===';

-- 请根据实际情况修改邮箱地址
DECLARE @AdminEmail VARCHAR(255) = 'admin@AreaSong.com'; -- 请修改为实际的管理员邮箱

-- 检查用户是否存在
IF EXISTS (SELECT * FROM users WHERE email = @AdminEmail)
BEGIN
    -- 将用户角色更新为管理员
    UPDATE users SET 
        role = 'admin',
        updated_at = GETDATE()
    WHERE email = @AdminEmail;
    
    PRINT '用户 ' + @AdminEmail + ' 已设置为管理员';
    
    -- 获取管理员用户ID用于创建邀请码
    DECLARE @AdminUserId UNIQUEIDENTIFIER;
    SELECT @AdminUserId = id FROM users WHERE email = @AdminEmail;
    
    -- 创建初始邀请码
    PRINT '=== 创建初始邀请码 ===';
    
    -- 创建一个永久有效的多次使用邀请码
    INSERT INTO invitation_codes (code, max_uses, used_count, expires_at, status, created_by)
    VALUES ('WELCOME2025', 100, 0, NULL, 'active', @AdminUserId);
    
    -- 创建一个有时间限制的邀请码
    INSERT INTO invitation_codes (code, max_uses, used_count, expires_at, status, created_by)
    VALUES ('TEMP30DAYS', 50, 0, DATEADD(DAY, 30, GETDATE()), 'active', @AdminUserId);
    
    -- 创建一次性邀请码示例
    INSERT INTO invitation_codes (code, max_uses, used_count, expires_at, status, created_by)
    VALUES ('ONETIME001', 1, 0, DATEADD(DAY, 7, GETDATE()), 'active', @AdminUserId);
    
    PRINT '初始邀请码创建完成：';
    PRINT '- WELCOME2025: 永久有效，可使用100次';
    PRINT '- TEMP30DAYS: 30天有效，可使用50次';
    PRINT '- ONETIME001: 7天有效，仅可使用1次';
    
END
ELSE
BEGIN
    PRINT '用户 ' + @AdminEmail + ' 不存在，正在创建新管理员账户...';

    -- 创建新管理员账户
    DECLARE @NewAdminId UNIQUEIDENTIFIER = NEWID();
    DECLARE @NewAdminName VARCHAR(100) = 'AreaSong管理员';
    -- 密码：Admin123! 的SHA256哈希值（与后端PasswordHelper一致）
    DECLARE @NewAdminPassword VARCHAR(255) = 'Dm2CDQx7iQkWgej7FFkXTK+49wCTGyVB5Mq67qY1Cu4=';

    INSERT INTO users (
        id, email, name, password_hash, role, status, phone,
        email_verified, phone_verified, created_at, updated_at
    )
    VALUES (
        @NewAdminId, @AdminEmail, @NewAdminName, @NewAdminPassword, 'admin', 'active',
        '13800000001', -- 使用简短的唯一phone值
        1, 0, GETDATE(), GETDATE()
    );

    -- 验证用户是否创建成功
    IF EXISTS (SELECT * FROM users WHERE id = @NewAdminId)
    BEGIN
        PRINT '新管理员账户创建成功！';
        PRINT '邮箱: ' + @AdminEmail;
        PRINT '姓名: ' + @NewAdminName;
        PRINT '默认密码: Admin123! (请登录后立即修改)';

        -- 创建初始邀请码
        PRINT '=== 创建初始邀请码 ===';

        -- 创建一个永久有效的多次使用邀请码
        INSERT INTO invitation_codes (code, max_uses, used_count, expires_at, status, created_by)
        VALUES ('WELCOME2025', 100, 0, NULL, 'active', @NewAdminId);

        -- 创建一个有时间限制的邀请码
        INSERT INTO invitation_codes (code, max_uses, used_count, expires_at, status, created_by)
        VALUES ('TEMP30DAYS', 50, 0, DATEADD(DAY, 30, GETDATE()), 'active', @NewAdminId);

        -- 创建一次性邀请码示例
        INSERT INTO invitation_codes (code, max_uses, used_count, expires_at, status, created_by)
        VALUES ('ONETIME001', 1, 0, DATEADD(DAY, 7, GETDATE()), 'active', @NewAdminId);

        PRINT '初始邀请码创建完成：';
        PRINT '- WELCOME2025: 永久有效，可使用100次';
        PRINT '- TEMP30DAYS: 30天有效，可使用50次';
        PRINT '- ONETIME001: 7天有效，仅可使用1次';
    END
    ELSE
    BEGIN
        PRINT '错误：管理员账户创建失败！';
        PRINT '请检查数据库约束和数据完整性。';
    END
END
GO

-- =============================================
-- 方式二：直接创建新的管理员账户（可选）
-- =============================================
/*
-- 如果需要直接创建新的管理员账户，请取消注释以下代码并修改相关信息

PRINT '=== 创建新管理员账户 ===';

-- 管理员账户信息（请根据实际情况修改）
DECLARE @NewAdminId UNIQUEIDENTIFIER = NEWID();
DECLARE @NewAdminEmail VARCHAR(255) = 'admin@todolist.com';
DECLARE @NewAdminName VARCHAR(100) = '系统管理员';
DECLARE @NewAdminPassword VARCHAR(255) = '$2a$11$example.hash.here'; -- 请使用实际的密码哈希

-- 检查邮箱是否已被使用
IF NOT EXISTS (SELECT * FROM users WHERE email = @NewAdminEmail)
BEGIN
    INSERT INTO users (
        id, email, name, password_hash, role, status, 
        email_verified, phone_verified, created_at, updated_at
    )
    VALUES (
        @NewAdminId, @NewAdminEmail, @NewAdminName, @NewAdminPassword, 'admin', 'active',
        1, 0, GETDATE(), GETDATE()
    );
    
    PRINT '新管理员账户创建成功：' + @NewAdminEmail;
    
    -- 为新管理员创建邀请码
    INSERT INTO invitation_codes (code, max_uses, used_count, expires_at, status, created_by)
    VALUES ('ADMIN2025', 200, 0, NULL, 'active', @NewAdminId);
    
    PRINT '管理员专用邀请码已创建：ADMIN2025';
END
ELSE
BEGIN
    PRINT '邮箱 ' + @NewAdminEmail + ' 已被使用，无法创建新账户';
END
*/

-- =============================================
-- 验证和统计信息
-- =============================================
PRINT '=== 当前系统状态 ===';

-- 显示管理员列表
PRINT '管理员账户列表：';
SELECT 
    email as '邮箱',
    name as '姓名', 
    status as '状态',
    created_at as '创建时间'
FROM users 
WHERE role = 'admin';

-- 显示邀请码列表
PRINT '当前邀请码列表：';
SELECT 
    code as '邀请码',
    max_uses as '最大使用次数',
    used_count as '已使用次数',
    CASE 
        WHEN expires_at IS NULL THEN '永不过期'
        ELSE CONVERT(VARCHAR, expires_at, 120)
    END as '过期时间',
    status as '状态',
    created_at as '创建时间'
FROM invitation_codes
ORDER BY created_at DESC;

PRINT '管理员设置完成！';
GO
