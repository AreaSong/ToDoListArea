-- =============================================
-- 检查users表结构脚本
-- =============================================

USE ToDoListArea;
GO

-- 检查users表的列结构
PRINT '=== Users表列结构 ===';
SELECT 
    COLUMN_NAME as '列名',
    DATA_TYPE as '数据类型',
    IS_NULLABLE as '允许NULL',
    COLUMN_DEFAULT as '默认值',
    CHARACTER_MAXIMUM_LENGTH as '最大长度'
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'users'
ORDER BY ORDINAL_POSITION;

-- 检查表是否存在
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'users')
BEGIN
    PRINT '✓ users表存在';
    
    -- 检查表中的数据
    DECLARE @UserCount INT;
    SELECT @UserCount = COUNT(*) FROM users;
    PRINT '当前用户数量: ' + CAST(@UserCount AS VARCHAR);
    
    -- 显示现有用户（不显示密码）
    IF @UserCount > 0
    BEGIN
        PRINT '现有用户列表:';
        SELECT 
            id,
            email,
            name,
            role,
            status,
            created_at
        FROM users;
    END
END
ELSE
BEGIN
    PRINT '✗ users表不存在';
END

GO
