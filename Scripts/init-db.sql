-- ToDoListArea数据库初始化脚本
-- 此脚本用于在Docker容器中初始化数据库

USE master;
GO

-- 检查数据库是否存在，如果不存在则创建
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ToDoListArea')
BEGIN
    CREATE DATABASE ToDoListArea;
    PRINT 'Database ToDoListArea created successfully.';
END
ELSE
BEGIN
    PRINT 'Database ToDoListArea already exists.';
END
GO

USE ToDoListArea;
GO

-- 创建用户表
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE Users (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Username NVARCHAR(50) NOT NULL UNIQUE,
        Email NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL,
        FirstName NVARCHAR(50),
        LastName NVARCHAR(50),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LastLoginAt DATETIME2,
        ProfilePicture NVARCHAR(500),
        TimeZone NVARCHAR(50) DEFAULT 'UTC',
        Language NVARCHAR(10) DEFAULT 'zh-CN',
        Theme NVARCHAR(20) DEFAULT 'light'
    );
    PRINT 'Table Users created successfully.';
END
GO

-- 创建任务分类表
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TaskCategories' AND xtype='U')
BEGIN
    CREATE TABLE TaskCategories (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UserId UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500),
        Color NVARCHAR(7) DEFAULT '#1890ff',
        Icon NVARCHAR(50),
        IsDefault BIT NOT NULL DEFAULT 0,
        SortOrder INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );
    PRINT 'Table TaskCategories created successfully.';
END
GO

-- 创建任务表
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Tasks' AND xtype='U')
BEGIN
    CREATE TABLE Tasks (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UserId UNIQUEIDENTIFIER NOT NULL,
        CategoryId UNIQUEIDENTIFIER,
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX),
        Priority NVARCHAR(20) NOT NULL DEFAULT 'Medium',
        Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        DueDate DATETIME2,
        StartDate DATETIME2,
        CompletedAt DATETIME2,
        EstimatedHours DECIMAL(5,2),
        ActualHours DECIMAL(5,2),
        Tags NVARCHAR(500),
        IsArchived BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
        FOREIGN KEY (CategoryId) REFERENCES TaskCategories(Id) ON DELETE SET NULL
    );
    PRINT 'Table Tasks created successfully.';
END
GO

-- 创建任务依赖关系表
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TaskDependencies' AND xtype='U')
BEGIN
    CREATE TABLE TaskDependencies (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TaskId UNIQUEIDENTIFIER NOT NULL,
        DependsOnTaskId UNIQUEIDENTIFIER NOT NULL,
        DependencyType NVARCHAR(20) NOT NULL DEFAULT 'FinishToStart',
        Description NVARCHAR(500),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        FOREIGN KEY (TaskId) REFERENCES Tasks(Id) ON DELETE CASCADE,
        FOREIGN KEY (DependsOnTaskId) REFERENCES Tasks(Id),
        UNIQUE(TaskId, DependsOnTaskId)
    );
    PRINT 'Table TaskDependencies created successfully.';
END
GO

-- 创建提醒表
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Reminders' AND xtype='U')
BEGIN
    CREATE TABLE Reminders (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UserId UNIQUEIDENTIFIER NOT NULL,
        TaskId UNIQUEIDENTIFIER,
        Title NVARCHAR(200) NOT NULL,
        Message NVARCHAR(1000),
        ReminderTime DATETIME2 NOT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        Channels NVARCHAR(200) NOT NULL DEFAULT 'web',
        RepeatPattern NVARCHAR(MAX),
        SnoozeUntil DATETIME2,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
        FOREIGN KEY (TaskId) REFERENCES Tasks(Id) ON DELETE CASCADE
    );
    PRINT 'Table Reminders created successfully.';
END
GO

-- 创建用户活动记录表
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserActivities' AND xtype='U')
BEGIN
    CREATE TABLE UserActivities (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UserId UNIQUEIDENTIFIER NOT NULL,
        ActivityType NVARCHAR(50) NOT NULL,
        EntityType NVARCHAR(50),
        EntityId UNIQUEIDENTIFIER,
        Description NVARCHAR(500),
        Details NVARCHAR(MAX),
        IpAddress NVARCHAR(45),
        UserAgent NVARCHAR(500),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );
    PRINT 'Table UserActivities created successfully.';
END
GO

-- 创建索引以提高查询性能
CREATE NONCLUSTERED INDEX IX_Tasks_UserId ON Tasks(UserId);
CREATE NONCLUSTERED INDEX IX_Tasks_Status ON Tasks(Status);
CREATE NONCLUSTERED INDEX IX_Tasks_DueDate ON Tasks(DueDate);
CREATE NONCLUSTERED INDEX IX_TaskDependencies_TaskId ON TaskDependencies(TaskId);
CREATE NONCLUSTERED INDEX IX_Reminders_UserId ON Reminders(UserId);
CREATE NONCLUSTERED INDEX IX_Reminders_ReminderTime ON Reminders(ReminderTime);
CREATE NONCLUSTERED INDEX IX_UserActivities_UserId ON UserActivities(UserId);
CREATE NONCLUSTERED INDEX IX_UserActivities_CreatedAt ON UserActivities(CreatedAt);

PRINT 'Database initialization completed successfully.';
GO
