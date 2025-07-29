-- =============================================
-- 智能提醒事项Web App - 数据库修复版v3创建脚本
-- 解决级联删除冲突问题
-- 数据库名称: ToDoListArea
-- 创建时间: 2025-07-29
-- 版本: 3.0 (级联删除修复版)
-- =============================================

USE master;
GO

-- 如果数据库已存在，则删除
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'ToDoListArea')
BEGIN
    ALTER DATABASE ToDoListArea SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE ToDoListArea;
END
GO

-- 创建数据库（不指定具体文件路径，使用默认路径）
CREATE DATABASE ToDoListArea;
GO

-- 使用新创建的数据库
USE ToDoListArea;
GO

-- =============================================
-- 第一批：基础表（无依赖）
-- =============================================

-- 1. 用户表
CREATE TABLE users (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    email VARCHAR(255) UNIQUE NOT NULL,
    phone VARCHAR(20) UNIQUE NULL,
    password_hash VARCHAR(255) NOT NULL,
    name VARCHAR(100) NOT NULL,
    avatar_url NVARCHAR(MAX) NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'active',
    email_verified BIT NOT NULL DEFAULT 0,
    phone_verified BIT NOT NULL DEFAULT 0,
    last_login_at DATETIME2 NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- 2. 任务分类表
CREATE TABLE task_categories (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    name VARCHAR(100) NOT NULL,
    color VARCHAR(7) NOT NULL DEFAULT '#007bff',
    icon VARCHAR(50) NULL,
    description NVARCHAR(500) NULL,
    is_system BIT NOT NULL DEFAULT 0,
    sort_order INTEGER NOT NULL DEFAULT 0,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- 3. 系统配置表
CREATE TABLE system_configs (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    config_key VARCHAR(100) UNIQUE NOT NULL,
    config_value NVARCHAR(MAX) NOT NULL,
    description NVARCHAR(500) NULL,
    is_encrypted BIT NOT NULL DEFAULT 0,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- 4. 功能开关表
CREATE TABLE feature_flags (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    flag_key VARCHAR(100) UNIQUE NOT NULL,
    is_enabled BIT NOT NULL DEFAULT 0,
    description NVARCHAR(500) NULL,
    target_users NVARCHAR(MAX) NULL,
    start_date DATETIME2 NULL,
    end_date DATETIME2 NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- =============================================
-- 第二批：用户相关表
-- =============================================

-- 5. 用户详细资料表
CREATE TABLE user_profiles (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    user_id UNIQUEIDENTIFIER NOT NULL,
    first_name VARCHAR(100) NULL,
    last_name VARCHAR(100) NULL,
    timezone VARCHAR(50) NOT NULL DEFAULT 'UTC',
    language VARCHAR(10) NOT NULL DEFAULT 'zh-CN',
    date_format VARCHAR(20) NOT NULL DEFAULT 'YYYY-MM-DD',
    time_format VARCHAR(10) NOT NULL DEFAULT '24h',
    notification_preferences NVARCHAR(MAX) NULL,
    theme_preferences NVARCHAR(MAX) NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);
GO

-- 6. 用户会话表
CREATE TABLE user_sessions (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    user_id UNIQUEIDENTIFIER NOT NULL,
    session_token VARCHAR(255) UNIQUE NOT NULL,
    refresh_token VARCHAR(255) UNIQUE NULL,
    device_info NVARCHAR(500) NULL,
    ip_address VARCHAR(45) NULL,
    user_agent NVARCHAR(MAX) NULL,
    is_active BIT NOT NULL DEFAULT 1,
    expires_at DATETIME2 NOT NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);
GO

-- 7. 第三方登录账户表
CREATE TABLE user_oauth_accounts (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    user_id UNIQUEIDENTIFIER NOT NULL,
    provider VARCHAR(50) NOT NULL,
    provider_user_id VARCHAR(255) NOT NULL,
    provider_email VARCHAR(255) NULL,
    provider_data NVARCHAR(MAX) NULL,
    is_active BIT NOT NULL DEFAULT 1,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    UNIQUE (provider, provider_user_id)
);
GO

-- =============================================
-- 第三批：任务相关表（核心表）
-- =============================================

-- 8. 任务主表
CREATE TABLE tasks (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    user_id UNIQUEIDENTIFIER NOT NULL,
    parent_task_id UNIQUEIDENTIFIER NULL,
    category_id UNIQUEIDENTIFIER NULL,
    title VARCHAR(255) NOT NULL,
    description NVARCHAR(MAX) NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'pending',
    priority VARCHAR(10) NOT NULL DEFAULT 'medium',
    start_time DATETIME2 NULL,
    end_time DATETIME2 NULL,
    estimated_duration INTEGER NULL,
    actual_duration INTEGER NULL,
    completion_percentage DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    is_recurring BIT NOT NULL DEFAULT 0,
    recurrence_pattern NVARCHAR(MAX) NULL,
    tags NVARCHAR(MAX) NULL,
    attachments NVARCHAR(MAX) NULL,
    completed_at DATETIME2 NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (parent_task_id) REFERENCES tasks(id) ON DELETE NO ACTION,
    FOREIGN KEY (category_id) REFERENCES task_categories(id) ON DELETE SET NULL
);
GO

-- 9. 任务详情表
CREATE TABLE task_details (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    task_id UNIQUEIDENTIFIER NOT NULL,
    detail_type VARCHAR(50) NOT NULL,
    detail_key VARCHAR(100) NOT NULL,
    detail_value NVARCHAR(MAX) NULL,
    sort_order INTEGER NOT NULL DEFAULT 0,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (task_id) REFERENCES tasks(id) ON DELETE CASCADE
);
GO

-- 10. 任务依赖关系表
CREATE TABLE task_dependencies (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    task_id UNIQUEIDENTIFIER NOT NULL,
    depends_on_task_id UNIQUEIDENTIFIER NOT NULL,
    dependency_type VARCHAR(20) NOT NULL DEFAULT 'finish_to_start',
    lag_time INTEGER NOT NULL DEFAULT 0,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (task_id) REFERENCES tasks(id) ON DELETE CASCADE,
    FOREIGN KEY (depends_on_task_id) REFERENCES tasks(id) ON DELETE NO ACTION,
    UNIQUE (task_id, depends_on_task_id)
);
GO

-- 11. 任务模板表
CREATE TABLE task_templates (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    user_id UNIQUEIDENTIFIER NOT NULL,
    name VARCHAR(255) NOT NULL,
    description NVARCHAR(MAX) NULL,
    template_data NVARCHAR(MAX) NOT NULL,
    category VARCHAR(100) NULL,
    is_public BIT NOT NULL DEFAULT 0,
    usage_count INTEGER NOT NULL DEFAULT 0,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);
GO

-- =============================================
-- 第四批：时间线相关表（避免级联冲突）
-- =============================================

-- 12. 时间线节点表
CREATE TABLE timeline_nodes (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    user_id UNIQUEIDENTIFIER NOT NULL,
    task_id UNIQUEIDENTIFIER NULL,
    node_type VARCHAR(20) NOT NULL DEFAULT 'task',
    title VARCHAR(255) NOT NULL,
    description NVARCHAR(MAX) NULL,
    start_time DATETIME2 NOT NULL,
    end_time DATETIME2 NULL,
    color VARCHAR(7) NOT NULL DEFAULT '#007bff',
    position_data NVARCHAR(MAX) NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (task_id) REFERENCES tasks(id) ON DELETE NO ACTION
);
GO

-- 13. 时间线事件表
CREATE TABLE timeline_events (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    user_id UNIQUEIDENTIFIER NOT NULL,
    event_type VARCHAR(50) NOT NULL,
    event_title VARCHAR(255) NOT NULL,
    event_description NVARCHAR(MAX) NULL,
    event_data NVARCHAR(MAX) NULL,
    occurred_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);
GO

-- 14. 甘特图数据表
CREATE TABLE gantt_data (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    user_id UNIQUEIDENTIFIER NOT NULL,
    task_id UNIQUEIDENTIFIER NOT NULL,
    start_date DATETIME2 NOT NULL,
    end_date DATETIME2 NOT NULL,
    progress DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    dependencies NVARCHAR(MAX) NULL,
    resources NVARCHAR(MAX) NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (task_id) REFERENCES tasks(id) ON DELETE NO ACTION
);
GO

-- 15. 时间块表
CREATE TABLE time_blocks (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    user_id UNIQUEIDENTIFIER NOT NULL,
    task_id UNIQUEIDENTIFIER NULL,
    title VARCHAR(255) NOT NULL,
    description NVARCHAR(MAX) NULL,
    start_time DATETIME2 NOT NULL,
    end_time DATETIME2 NOT NULL,
    block_type VARCHAR(20) NOT NULL DEFAULT 'work',
    color VARCHAR(7) NOT NULL DEFAULT '#007bff',
    is_locked BIT NOT NULL DEFAULT 0,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (task_id) REFERENCES tasks(id) ON DELETE NO ACTION
);
GO

-- =============================================
-- 第五批：提醒相关表（避免级联冲突）
-- =============================================

-- 16. 提醒主表
CREATE TABLE reminders (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    user_id UNIQUEIDENTIFIER NOT NULL,
    task_id UNIQUEIDENTIFIER NULL,
    title VARCHAR(255) NOT NULL,
    message NVARCHAR(MAX) NULL,
    reminder_time DATETIME2 NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'pending',
    channels NVARCHAR(MAX) NOT NULL DEFAULT '["web"]',
    repeat_pattern NVARCHAR(MAX) NULL,
    snooze_until DATETIME2 NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (task_id) REFERENCES tasks(id) ON DELETE NO ACTION
);
GO

-- 17. 提醒规则表
CREATE TABLE reminder_rules (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    user_id UNIQUEIDENTIFIER NOT NULL,
    rule_name VARCHAR(255) NOT NULL,
    rule_type VARCHAR(50) NOT NULL,
    conditions NVARCHAR(MAX) NOT NULL,
    actions NVARCHAR(MAX) NOT NULL,
    is_active BIT NOT NULL DEFAULT 1,
    priority INTEGER NOT NULL DEFAULT 0,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);
GO

-- 18. 提醒历史表
CREATE TABLE reminder_history (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    reminder_id UNIQUEIDENTIFIER NOT NULL,
    user_id UNIQUEIDENTIFIER NOT NULL,
    sent_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    channel VARCHAR(50) NOT NULL,
    status VARCHAR(20) NOT NULL,
    response_data NVARCHAR(MAX) NULL,
    error_message NVARCHAR(MAX) NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (reminder_id) REFERENCES reminders(id) ON DELETE NO ACTION,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);
GO

-- 19. 通知设置表
CREATE TABLE notification_settings (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    user_id UNIQUEIDENTIFIER NOT NULL,
    channel VARCHAR(50) NOT NULL,
    is_enabled BIT NOT NULL DEFAULT 1,
    settings NVARCHAR(MAX) NULL,
    quiet_hours_start TIME NULL,
    quiet_hours_end TIME NULL,
    timezone VARCHAR(50) NOT NULL DEFAULT 'UTC',
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    UNIQUE (user_id, channel)
);
GO

-- =============================================
-- 第六批：数据分析和系统表
-- =============================================

-- 20. 用户活动表
CREATE TABLE user_activities (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    user_id UNIQUEIDENTIFIER NOT NULL,
    activity_type VARCHAR(50) NOT NULL,
    activity_description NVARCHAR(500) NULL,
    entity_type VARCHAR(50) NULL,
    entity_id UNIQUEIDENTIFIER NULL,
    metadata NVARCHAR(MAX) NULL,
    ip_address VARCHAR(45) NULL,
    user_agent NVARCHAR(MAX) NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);
GO

-- 21. 任务统计表
CREATE TABLE task_statistics (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    user_id UNIQUEIDENTIFIER NOT NULL,
    date DATE NOT NULL,
    total_tasks INTEGER NOT NULL DEFAULT 0,
    completed_tasks INTEGER NOT NULL DEFAULT 0,
    pending_tasks INTEGER NOT NULL DEFAULT 0,
    overdue_tasks INTEGER NOT NULL DEFAULT 0,
    total_time_spent INTEGER NOT NULL DEFAULT 0,
    productivity_score DECIMAL(5,2) NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    UNIQUE (user_id, date)
);
GO

-- 22. 生产力指标表
CREATE TABLE productivity_metrics (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    user_id UNIQUEIDENTIFIER NOT NULL,
    metric_type VARCHAR(50) NOT NULL,
    metric_value DECIMAL(10,2) NOT NULL,
    metric_unit VARCHAR(20) NULL,
    period_start DATETIME2 NOT NULL,
    period_end DATETIME2 NOT NULL,
    metadata NVARCHAR(MAX) NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);
GO

-- 23. 系统日志表
CREATE TABLE system_logs (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    log_level VARCHAR(20) NOT NULL,
    logger_name VARCHAR(255) NOT NULL,
    message NVARCHAR(MAX) NOT NULL,
    exception NVARCHAR(MAX) NULL,
    properties NVARCHAR(MAX) NULL,
    user_id UNIQUEIDENTIFIER NULL,
    request_id VARCHAR(100) NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE SET NULL
);
GO

-- =============================================
-- 创建索引
-- =============================================

-- 用户表索引
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_status ON users(status);
CREATE INDEX idx_users_created_at ON users(created_at);
GO

-- 任务表索引
CREATE INDEX idx_tasks_user_id ON tasks(user_id);
CREATE INDEX idx_tasks_status ON tasks(status);
CREATE INDEX idx_tasks_priority ON tasks(priority);
CREATE INDEX idx_tasks_start_time ON tasks(start_time);
CREATE INDEX idx_tasks_end_time ON tasks(end_time);
CREATE INDEX idx_tasks_parent_task_id ON tasks(parent_task_id);
CREATE INDEX idx_tasks_category_id ON tasks(category_id);
CREATE INDEX idx_tasks_user_status_priority ON tasks(user_id, status, priority);
GO

-- 提醒表索引
CREATE INDEX idx_reminders_user_id ON reminders(user_id);
CREATE INDEX idx_reminders_task_id ON reminders(task_id);
CREATE INDEX idx_reminders_reminder_time ON reminders(reminder_time);
CREATE INDEX idx_reminders_status ON reminders(status);
GO

-- 用户活动表索引
CREATE INDEX idx_user_activities_user_id ON user_activities(user_id);
CREATE INDEX idx_user_activities_activity_type ON user_activities(activity_type);
CREATE INDEX idx_user_activities_created_at ON user_activities(created_at);
GO

-- 会话表索引
CREATE INDEX idx_user_sessions_user_id ON user_sessions(user_id);
CREATE INDEX idx_user_sessions_session_token ON user_sessions(session_token);
CREATE INDEX idx_user_sessions_expires_at ON user_sessions(expires_at);
GO

-- =============================================
-- 插入默认数据
-- =============================================

-- 插入默认系统配置
INSERT INTO system_configs (config_key, config_value, description) VALUES
('app_name', '智能提醒事项管理系统', '应用名称'),
('app_version', '1.0.0', '应用版本'),
('default_timezone', 'UTC', '默认时区'),
('default_language', 'zh-CN', '默认语言'),
('max_file_size', '10485760', '最大文件大小（字节）'),
('session_timeout', '3600', '会话超时时间（秒）'),
('password_min_length', '8', '密码最小长度'),
('max_login_attempts', '5', '最大登录尝试次数');
GO

-- 插入默认功能开关
INSERT INTO feature_flags (flag_key, is_enabled, description) VALUES
('ai_task_optimization', 0, 'AI任务优化功能'),
('advanced_gantt_chart', 0, '高级甘特图功能'),
('team_collaboration', 0, '团队协作功能'),
('data_analytics', 0, '数据分析功能'),
('mobile_app', 0, '移动应用功能'),
('api_rate_limiting', 1, 'API速率限制'),
('email_notifications', 1, '邮件通知功能'),
('sms_notifications', 0, '短信通知功能');
GO

-- 插入默认任务分类
INSERT INTO task_categories (name, color, icon, description, is_system, sort_order) VALUES
('工作', '#007bff', 'briefcase', '工作相关任务', 1, 1),
('个人', '#28a745', 'user', '个人事务任务', 1, 2),
('学习', '#ffc107', 'book', '学习相关任务', 1, 3),
('健康', '#dc3545', 'heart', '健康相关任务', 1, 4),
('娱乐', '#6f42c1', 'gamepad', '娱乐休闲任务', 1, 5);
GO

-- =============================================
-- 数据库完整性验证
-- =============================================

-- 验证所有表是否创建成功
DECLARE @TableCount INT;
SELECT @TableCount = COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';
PRINT '已创建表数量: ' + CAST(@TableCount AS VARCHAR(10));

-- 验证所有索引是否创建成功
DECLARE @IndexCount INT;
SELECT @IndexCount = COUNT(*) FROM sys.indexes WHERE is_primary_key = 0 AND is_unique_constraint = 0;
PRINT '已创建索引数量: ' + CAST(@IndexCount AS VARCHAR(10));

-- 验证所有外键约束是否创建成功
DECLARE @FKCount INT;
SELECT @FKCount = COUNT(*) FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS;
PRINT '已创建外键约束数量: ' + CAST(@FKCount AS VARCHAR(10));

-- =============================================
-- 脚本完成
-- =============================================

PRINT '========================================';
PRINT '数据库 ToDoListArea v3 修复版创建完成！';
PRINT '解决了级联删除冲突问题';
PRINT '包含 23 个表，完整的索引和约束。';
PRINT '创建时间: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';

-- 设置数据库选项
ALTER DATABASE ToDoListArea SET RECOVERY FULL;
ALTER DATABASE ToDoListArea SET AUTO_CLOSE OFF;
ALTER DATABASE ToDoListArea SET AUTO_SHRINK OFF;
ALTER DATABASE ToDoListArea SET AUTO_CREATE_STATISTICS ON;
ALTER DATABASE ToDoListArea SET AUTO_UPDATE_STATISTICS ON;
GO
