-- 用户状态标准化迁移脚本
-- 将所有用户状态统一为小写格式

-- 更新用户状态为标准格式
UPDATE Users 
SET Status = LOWER(Status), 
    UpdatedAt = GETUTCDATE()
WHERE Status != LOWER(Status);

-- 验证更新结果
SELECT 
    Status,
    COUNT(*) as UserCount
FROM Users 
GROUP BY Status
ORDER BY Status;

-- 添加约束确保状态值只能是指定的小写值
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Users_Status_Values')
BEGIN
    ALTER TABLE Users 
    ADD CONSTRAINT CK_Users_Status_Values 
    CHECK (Status IN ('active', 'inactive', 'banned'));
END

PRINT '用户状态标准化完成';
