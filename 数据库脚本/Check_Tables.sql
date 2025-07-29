-- =============================================
-- 检查数据库中实际创建的表
-- =============================================

USE ToDoListArea;
GO

-- 查看所有创建的表
SELECT 
    TABLE_NAME as '表名',
    TABLE_TYPE as '类型'
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- 查看缺少的表（应该有的23个表）
DECLARE @ExpectedTables TABLE (TableName VARCHAR(100));
INSERT INTO @ExpectedTables VALUES 
('users'), ('user_profiles'), ('user_sessions'), ('user_oauth_accounts'),
('tasks'), ('task_details'), ('task_dependencies'), ('task_categories'), ('task_templates'),
('timeline_nodes'), ('timeline_events'), ('gantt_data'), ('time_blocks'),
('reminders'), ('reminder_rules'), ('reminder_history'), ('notification_settings'),
('user_activities'), ('task_statistics'), ('productivity_metrics'),
('system_configs'), ('feature_flags'), ('system_logs');

-- 显示缺少的表
SELECT 
    et.TableName as '缺少的表'
FROM @ExpectedTables et
LEFT JOIN INFORMATION_SCHEMA.TABLES t ON et.TableName = t.TABLE_NAME
WHERE t.TABLE_NAME IS NULL;

-- 查看外键约束
SELECT 
    fk.name as '外键名称',
    tp.name as '父表',
    cp.name as '父列',
    tr.name as '子表',
    cr.name as '子列'
FROM sys.foreign_keys fk
INNER JOIN sys.tables tp ON fk.referenced_object_id = tp.object_id
INNER JOIN sys.tables tr ON fk.parent_object_id = tr.object_id
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns cp ON fkc.referenced_column_id = cp.column_id AND fkc.referenced_object_id = cp.object_id
INNER JOIN sys.columns cr ON fkc.parent_column_id = cr.column_id AND fkc.parent_object_id = cr.object_id
ORDER BY tr.name, fk.name;
