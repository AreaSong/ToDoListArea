-- =============================================
-- ToDoList项目快速数据一致性修复脚本
-- 目标：快速修复Status和Priority字段的中英文混用问题
-- =============================================

-- 检查当前数据状态
PRINT '=== 修复前数据检查 ==='

SELECT 'Status字段当前值' as CheckType, Status, COUNT(*) as Count
FROM Tasks 
GROUP BY Status
ORDER BY Count DESC;

SELECT 'Priority字段当前值' as CheckType, Priority, COUNT(*) as Count
FROM Tasks 
GROUP BY Priority
ORDER BY Count DESC;

-- 开始修复
PRINT '=== 开始数据修复 ==='

BEGIN TRANSACTION QuickFix;

-- 修复Status字段
PRINT '修复Status字段...'

-- 待处理状态标准化
UPDATE Tasks SET Status = 'Pending' 
WHERE Status IN ('待处理', 'pending', 'PENDING', '待办', '未开始', 'todo', 'TODO');

-- 进行中状态标准化  
UPDATE Tasks SET Status = 'InProgress'
WHERE Status IN ('进行中', 'inprogress', 'INPROGRESS', 'in_progress', 'IN_PROGRESS', 
                 '正在进行', '执行中', 'doing', 'DOING', 'active', 'ACTIVE');

-- 已完成状态标准化
UPDATE Tasks SET Status = 'Completed'
WHERE Status IN ('已完成', 'completed', 'COMPLETED', '完成', '已结束', 'done', 'DONE', 
                 'finished', 'FINISHED', 'closed', 'CLOSED');

-- 修复Priority字段
PRINT '修复Priority字段...'

-- 低优先级标准化
UPDATE Tasks SET Priority = 'Low'
WHERE Priority IN ('低', 'low', 'LOW', '低优先级', 'minor', 'MINOR');

-- 中优先级标准化
UPDATE Tasks SET Priority = 'Medium' 
WHERE Priority IN ('中', 'medium', 'MEDIUM', '中等', '普通', 'normal', 'NORMAL', 
                   'standard', 'STANDARD');

-- 高优先级标准化
UPDATE Tasks SET Priority = 'High'
WHERE Priority IN ('高', 'high', 'HIGH', '高优先级', '紧急', 'urgent', 'URGENT', 
                   'critical', 'CRITICAL', 'important', 'IMPORTANT');

-- 处理无效值
PRINT '处理无效值...'

-- 将无效的Status值设为默认值
UPDATE Tasks SET Status = 'Pending' 
WHERE Status NOT IN ('Pending', 'InProgress', 'Completed');

-- 将无效的Priority值设为默认值
UPDATE Tasks SET Priority = 'Medium'
WHERE Priority NOT IN ('Low', 'Medium', 'High');

-- 验证修复结果
PRINT '=== 修复后数据验证 ==='

SELECT 'Status修复后分布' as CheckType, Status, COUNT(*) as Count
FROM Tasks 
GROUP BY Status
ORDER BY Count DESC;

SELECT 'Priority修复后分布' as CheckType, Priority, COUNT(*) as Count
FROM Tasks 
GROUP BY Priority  
ORDER BY Count DESC;

-- 检查是否还有无效值
DECLARE @InvalidStatusCount INT = (SELECT COUNT(*) FROM Tasks WHERE Status NOT IN ('Pending', 'InProgress', 'Completed'));
DECLARE @InvalidPriorityCount INT = (SELECT COUNT(*) FROM Tasks WHERE Priority NOT IN ('Low', 'Medium', 'High'));

PRINT '无效Status数量: ' + CAST(@InvalidStatusCount AS VARCHAR(10));
PRINT '无效Priority数量: ' + CAST(@InvalidPriorityCount AS VARCHAR(10));

IF @InvalidStatusCount = 0 AND @InvalidPriorityCount = 0
BEGIN
    PRINT '✅ 数据一致性修复成功！所有数据已标准化。'
    COMMIT TRANSACTION QuickFix;
END
ELSE
BEGIN
    PRINT '❌ 仍有无效数据，请检查！'
    -- 不回滚，让用户决定
    COMMIT TRANSACTION QuickFix;
END

PRINT '=== 修复完成 ==='
