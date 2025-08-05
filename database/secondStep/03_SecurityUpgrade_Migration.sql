-- =============================================
-- ToDoListArea 安全访问控制系统升级脚本
-- 版本: v1.0
-- 创建时间: 2025-01-21
-- 描述: 添加用户角色、邀请码系统相关表结构
-- =============================================

USE ToDoListArea;
GO

-- 检查数据库连接
PRINT '开始执行安全访问控制系统升级...';
PRINT '当前数据库: ' + DB_NAME();
PRINT '执行时间: ' + CONVERT(VARCHAR, GETDATE(), 120);
GO

-- =============================================
-- 第一步：为用户表添加角色字段
-- =============================================
PRINT '步骤 1: 检查并添加用户角色字段...';

-- 检查role字段是否已存在
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'users' AND COLUMN_NAME = 'role'
)
BEGIN
    PRINT '添加用户角色字段...';
    ALTER TABLE users ADD role VARCHAR(20) NOT NULL DEFAULT 'user';
    
    -- 创建角色字段的索引
    CREATE INDEX idx_users_role ON users(role);
    
    PRINT '用户角色字段添加完成';
END
ELSE
BEGIN
    PRINT '用户角色字段已存在，跳过添加';
END
GO

-- =============================================
-- 第二步：创建邀请码表
-- =============================================
PRINT '步骤 2: 创建邀请码表...';

-- 检查邀请码表是否已存在
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'invitation_codes')
BEGIN
    PRINT '创建邀请码表...';
    
    CREATE TABLE invitation_codes (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        code VARCHAR(32) UNIQUE NOT NULL, -- 邀请码
        max_uses INTEGER NOT NULL DEFAULT 1, -- 最大使用次数
        used_count INTEGER NOT NULL DEFAULT 0, -- 已使用次数
        expires_at DATETIME2 NULL, -- 过期时间，NULL表示永不过期
        status VARCHAR(20) NOT NULL DEFAULT 'active', -- 状态：active, disabled
        created_by UNIQUEIDENTIFIER NOT NULL, -- 创建者ID
        created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        
        -- 外键约束
        CONSTRAINT FK_invitation_codes_created_by FOREIGN KEY (created_by) REFERENCES users(id)
    );
    
    -- 创建索引
    CREATE INDEX idx_invitation_codes_code ON invitation_codes(code);
    CREATE INDEX idx_invitation_codes_status ON invitation_codes(status);
    CREATE INDEX idx_invitation_codes_expires_at ON invitation_codes(expires_at);
    CREATE INDEX idx_invitation_codes_created_by ON invitation_codes(created_by);
    
    PRINT '邀请码表创建完成';
END
ELSE
BEGIN
    PRINT '邀请码表已存在，跳过创建';
END
GO

-- =============================================
-- 第三步：创建邀请码使用记录表
-- =============================================
PRINT '步骤 3: 创建邀请码使用记录表...';

-- 检查邀请码使用记录表是否已存在
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'invitation_code_usages')
BEGIN
    PRINT '创建邀请码使用记录表...';
    
    CREATE TABLE invitation_code_usages (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        invitation_code_id UNIQUEIDENTIFIER NOT NULL, -- 邀请码ID
        user_id UNIQUEIDENTIFIER NOT NULL, -- 使用者ID
        used_at DATETIME2 NOT NULL DEFAULT GETDATE(), -- 使用时间
        ip_address VARCHAR(45) NULL, -- 使用者IP地址
        user_agent NVARCHAR(500) NULL, -- 用户代理信息
        
        -- 外键约束
        CONSTRAINT FK_invitation_code_usages_invitation_code FOREIGN KEY (invitation_code_id) REFERENCES invitation_codes(id),
        CONSTRAINT FK_invitation_code_usages_user FOREIGN KEY (user_id) REFERENCES users(id),
        
        -- 唯一约束：同一用户不能重复使用同一邀请码
        CONSTRAINT UK_invitation_code_usages_code_user UNIQUE (invitation_code_id, user_id)
    );
    
    -- 创建索引
    CREATE INDEX idx_invitation_code_usages_invitation_code ON invitation_code_usages(invitation_code_id);
    CREATE INDEX idx_invitation_code_usages_user ON invitation_code_usages(user_id);
    CREATE INDEX idx_invitation_code_usages_used_at ON invitation_code_usages(used_at);
    
    PRINT '邀请码使用记录表创建完成';
END
ELSE
BEGIN
    PRINT '邀请码使用记录表已存在，跳过创建';
END
GO

-- =============================================
-- 第四步：创建默认管理员账户和初始邀请码
-- =============================================
PRINT '步骤 4: 创建默认数据...';

-- 检查是否已有管理员账户
IF NOT EXISTS (SELECT * FROM users WHERE role = 'admin')
BEGIN
    PRINT '未找到管理员账户，需要手动创建第一个管理员账户';
    PRINT '请使用现有账户登录后，手动将其角色更新为admin';
    PRINT '示例SQL: UPDATE users SET role = ''admin'' WHERE email = ''your-admin-email@example.com'';';
END
ELSE
BEGIN
    PRINT '已存在管理员账户';
END
GO

-- =============================================
-- 第五步：验证升级结果
-- =============================================
PRINT '步骤 5: 验证升级结果...';

-- 验证用户表结构
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'users' AND COLUMN_NAME = 'role')
    PRINT '✓ 用户表role字段验证通过';
ELSE
    PRINT '✗ 用户表role字段验证失败';

-- 验证邀请码表
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'invitation_codes')
    PRINT '✓ 邀请码表验证通过';
ELSE
    PRINT '✗ 邀请码表验证失败';

-- 验证邀请码使用记录表
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'invitation_code_usages')
    PRINT '✓ 邀请码使用记录表验证通过';
ELSE
    PRINT '✗ 邀请码使用记录表验证失败';

-- 显示统计信息
PRINT '=== 升级完成统计 ===';

-- 使用变量存储统计数据
DECLARE @UserCount INT, @AdminCount INT, @RegularUserCount INT, @InvitationCodeCount INT;

SELECT @UserCount = COUNT(*) FROM users;
SELECT @AdminCount = COUNT(*) FROM users WHERE role = 'admin';
SELECT @RegularUserCount = COUNT(*) FROM users WHERE role = 'user';
SELECT @InvitationCodeCount = COUNT(*) FROM invitation_codes;

PRINT '用户总数: ' + CAST(@UserCount AS VARCHAR);
PRINT '管理员数量: ' + CAST(@AdminCount AS VARCHAR);
PRINT '普通用户数量: ' + CAST(@RegularUserCount AS VARCHAR);
PRINT '邀请码总数: ' + CAST(@InvitationCodeCount AS VARCHAR);

PRINT '安全访问控制系统升级完成！';
PRINT '完成时间: ' + CONVERT(VARCHAR, GETDATE(), 120);
GO
