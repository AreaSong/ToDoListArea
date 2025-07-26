# 🔌 智能提醒事项Web App - API接口文档

## 📋 目录

- [API概述](#api概述)
- [认证授权API](#认证授权api)
- [任务管理API](#任务管理api)
- [时间线管理API](#时间线管理api)
- [提醒系统API](#提醒系统api)
- [数据分析API](#数据分析api)
- [错误处理](#错误处理)
- [数据格式规范](#数据格式规范)

---

## 🌐 API概述

### 基础信息
- **API版本**: v1.0
- **基础URL**: `https://api.todoapp.com/api/v1`
- **协议**: HTTPS
- **数据格式**: JSON
- **字符编码**: UTF-8

### 认证方式
- **JWT Token**: 在请求头中携带 `Authorization: Bearer <token>`
- **API Key**: 用于第三方集成，在请求头中携带 `X-API-Key: <key>`
- **ASP.NET Core Identity**: 内置身份认证系统

### 通用响应格式
```json
{
  "success": true,
  "data": {},
  "message": "操作成功",
  "timestamp": "2024-01-01T00:00:00Z",
  "requestId": "req_123456789"
}
```

### 错误响应格式
```json
{
  "success": false,
  "error": {
    "code": "AUTH_001",
    "message": "认证失败",
    "details": "Token已过期"
  },
  "timestamp": "2024-01-01T00:00:00Z",
  "requestId": "req_123456789"
}
```

---

## 🔐 认证授权API

### 用户注册

#### POST /api/v1/auth/register
**描述**: 用户注册接口

**请求参数**:
```json
{
  "email": "user@example.com",
  "password": "password123",
  "name": "张三",
  "phone": "13800138000"
}
```

**响应示例**:
```json
{
  "success": true,
  "data": {
    "userId": "user_123456789",
    "email": "user@example.com",
    "name": "张三",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_123456789"
  },
  "message": "注册成功"
}
```

### 用户登录

#### POST /api/v1/auth/login
**描述**: 用户登录接口

**请求参数**:
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**响应示例**:
```json
{
  "success": true,
  "data": {
    "userId": "user_123456789",
    "email": "user@example.com",
    "name": "张三",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_123456789",
    "expiresIn": 3600
  },
  "message": "登录成功"
}
```

### 第三方登录

#### POST /api/v1/auth/oauth/{provider}
**描述**: 第三方登录接口

**路径参数**:
- `provider`: 第三方提供商（wechat, qq, github, google）

**请求参数**:
```json
{
  "code": "oauth_code_123456789",
  "redirectUri": "https://app.todoapp.com/callback"
}
```

**响应示例**:
```json
{
  "success": true,
  "data": {
    "userId": "user_123456789",
    "email": "user@example.com",
    "name": "张三",
    "avatar": "https://example.com/avatar.jpg",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_123456789"
  },
  "message": "登录成功"
}
```

### 刷新令牌

#### POST /api/v1/auth/refresh
**描述**: 刷新访问令牌

**请求参数**:
```json
{
  "refreshToken": "refresh_token_123456789"
}
```

**响应示例**:
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "new_refresh_token_123456789",
    "expiresIn": 3600
  },
  "message": "令牌刷新成功"
}
```

### 用户登出

#### POST /api/v1/auth/logout
**描述**: 用户登出接口

**请求头**:
```
Authorization: Bearer <token>
```

**响应示例**:
```json
{
  "success": true,
  "data": null,
  "message": "登出成功"
}
```

---

## 📋 任务管理API

### 创建任务

#### POST /api/v1/tasks
**描述**: 创建新任务

**请求头**:
```
Authorization: Bearer <token>
```

**请求参数**:
```json
{
  "title": "完成项目文档",
  "description": "编写项目技术文档和用户手册",
  "startTime": "2024-01-15T09:00:00Z",
  "endTime": "2024-01-20T18:00:00Z",
  "priority": "high",
  "category": "work",
  "tags": ["文档", "项目"],
  "estimatedHours": 16,
  "dependencies": ["task_123456789"]
}
```

**响应示例**:
```json
{
  "success": true,
  "data": {
    "taskId": "task_987654321",
    "title": "完成项目文档",
    "description": "编写项目技术文档和用户手册",
    "startTime": "2024-01-15T09:00:00Z",
    "endTime": "2024-01-20T18:00:00Z",
    "priority": "high",
    "category": "work",
    "status": "pending",
    "progress": 0,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  },
  "message": "任务创建成功"
}
```

### 获取任务列表

#### GET /api/v1/tasks
**描述**: 获取用户的任务列表

**请求头**:
```
Authorization: Bearer <token>
```

**查询参数**:
- `page`: 页码（默认1）
- `limit`: 每页数量（默认20）
- `status`: 任务状态（pending, in_progress, completed, cancelled）
- `priority`: 优先级（low, medium, high）
- `category`: 分类
- `startDate`: 开始日期
- `endDate`: 结束日期
- `search`: 搜索关键词

**响应示例**:
```json
{
  "success": true,
  "data": {
    "tasks": [
      {
        "taskId": "task_123456789",
        "title": "完成项目文档",
        "description": "编写项目技术文档和用户手册",
        "startTime": "2024-01-15T09:00:00Z",
        "endTime": "2024-01-20T18:00:00Z",
        "priority": "high",
        "category": "work",
        "status": "in_progress",
        "progress": 60,
        "tags": ["文档", "项目"],
        "createdAt": "2024-01-01T00:00:00Z",
        "updatedAt": "2024-01-01T00:00:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "total": 100,
      "totalPages": 5
    }
  },
  "message": "获取任务列表成功"
}
```

### 获取任务详情

#### GET /api/v1/tasks/{taskId}
**描述**: 获取指定任务的详细信息

**路径参数**:
- `taskId`: 任务ID

**请求头**:
```
Authorization: Bearer <token>
```

**响应示例**:
```json
{
  "success": true,
  "data": {
    "taskId": "task_123456789",
    "title": "完成项目文档",
    "description": "编写项目技术文档和用户手册",
    "startTime": "2024-01-15T09:00:00Z",
    "endTime": "2024-01-20T18:00:00Z",
    "priority": "high",
    "category": "work",
    "status": "in_progress",
    "progress": 60,
    "tags": ["文档", "项目"],
    "estimatedHours": 16,
    "actualHours": 9.5,
    "dependencies": [
      {
        "taskId": "task_987654321",
        "title": "需求分析",
        "status": "completed"
      }
    ],
    "subtasks": [
      {
        "subtaskId": "subtask_123456789",
        "title": "技术架构文档",
        "status": "completed",
        "progress": 100
      }
    ],
    "comments": [
      {
        "commentId": "comment_123456789",
        "content": "文档结构已确定",
        "author": "张三",
        "createdAt": "2024-01-01T00:00:00Z"
      }
    ],
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  },
  "message": "获取任务详情成功"
}
```

### 更新任务

#### PUT /api/v1/tasks/{taskId}
**描述**: 更新任务信息

**路径参数**:
- `taskId`: 任务ID

**请求头**:
```
Authorization: Bearer <token>
```

**请求参数**:
```json
{
  "title": "完成项目文档（更新）",
  "description": "编写项目技术文档和用户手册，包含API文档",
  "startTime": "2024-01-15T09:00:00Z",
  "endTime": "2024-01-22T18:00:00Z",
  "priority": "high",
  "category": "work",
  "status": "in_progress",
  "progress": 75,
  "tags": ["文档", "项目", "API"]
}
```

**响应示例**:
```json
{
  "success": true,
  "data": {
    "taskId": "task_123456789",
    "title": "完成项目文档（更新）",
    "description": "编写项目技术文档和用户手册，包含API文档",
    "startTime": "2024-01-15T09:00:00Z",
    "endTime": "2024-01-22T18:00:00Z",
    "priority": "high",
    "category": "work",
    "status": "in_progress",
    "progress": 75,
    "updatedAt": "2024-01-01T00:00:00Z"
  },
  "message": "任务更新成功"
}
```

### 删除任务

#### DELETE /api/v1/tasks/{taskId}
**描述**: 删除指定任务

**路径参数**:
- `taskId`: 任务ID

**请求头**:
```
Authorization: Bearer <token>
```

**响应示例**:
```json
{
  "success": true,
  "data": null,
  "message": "任务删除成功"
}
```

### 批量操作

#### POST /api/v1/tasks/batch
**描述**: 批量操作任务

**请求头**:
```
Authorization: Bearer <token>
```

**请求参数**:
```json
{
  "action": "update_status",
  "taskIds": ["task_123456789", "task_987654321"],
  "data": {
    "status": "completed",
    "progress": 100
  }
}
```

**响应示例**:
```json
{
  "success": true,
  "data": {
    "updatedCount": 2,
    "failedCount": 0,
    "results": [
      {
        "taskId": "task_123456789",
        "success": true
      },
      {
        "taskId": "task_987654321",
        "success": true
      }
    ]
  },
  "message": "批量操作成功"
}
```

---

## 📊 时间线管理API

### 获取甘特图数据

#### GET /api/v1/timeline/gantt
**描述**: 获取甘特图数据

**请求头**:
```
Authorization: Bearer <token>
```

**查询参数**:
- `startDate`: 开始日期
- `endDate`: 结束日期
- `view`: 视图类型（day, week, month, quarter）
- `category`: 分类过滤
- `status`: 状态过滤

**响应示例**:
```json
{
  "success": true,
  "data": {
    "timeline": {
      "startDate": "2024-01-01T00:00:00Z",
      "endDate": "2024-01-31T23:59:59Z",
      "view": "week"
    },
    "tasks": [
      {
        "taskId": "task_123456789",
        "title": "完成项目文档",
        "startTime": "2024-01-15T09:00:00Z",
        "endTime": "2024-01-20T18:00:00Z",
        "status": "in_progress",
        "progress": 60,
        "priority": "high",
        "category": "work",
        "dependencies": ["task_987654321"],
        "assignee": "张三",
        "color": "#1890ff"
      }
    ],
    "milestones": [
      {
        "milestoneId": "milestone_123456789",
        "title": "项目启动",
        "date": "2024-01-10T00:00:00Z",
        "type": "start"
      }
    ]
  },
  "message": "获取甘特图数据成功"
}
```

### 更新任务时间

#### PUT /api/v1/timeline/tasks/{taskId}/time
**描述**: 更新任务的时间安排

**路径参数**:
- `taskId`: 任务ID

**请求头**:
```
Authorization: Bearer <token>
```

**请求参数**:
```json
{
  "startTime": "2024-01-16T09:00:00Z",
  "endTime": "2024-01-21T18:00:00Z"
}
```

**响应示例**:
```json
{
  "success": true,
  "data": {
    "taskId": "task_123456789",
    "startTime": "2024-01-16T09:00:00Z",
    "endTime": "2024-01-21T18:00:00Z",
    "conflicts": [],
    "updatedAt": "2024-01-01T00:00:00Z"
  },
  "message": "任务时间更新成功"
}
```

### 检测时间冲突

#### POST /api/v1/timeline/conflicts/detect
**描述**: 检测时间冲突

**请求头**:
```
Authorization: Bearer <token>
```

**请求参数**:
```json
{
  "taskId": "task_123456789",
  "startTime": "2024-01-16T09:00:00Z",
  "endTime": "2024-01-21T18:00:00Z"
}
```

**响应示例**:
```json
{
  "success": true,
  "data": {
    "hasConflicts": true,
    "conflicts": [
      {
        "conflictId": "conflict_123456789",
        "type": "time_overlap",
        "severity": "high",
        "description": "与任务'需求分析'时间重叠",
        "conflictingTask": {
          "taskId": "task_987654321",
          "title": "需求分析",
          "startTime": "2024-01-15T14:00:00Z",
          "endTime": "2024-01-18T18:00:00Z"
        },
        "suggestions": [
          {
            "type": "delay",
            "description": "延迟开始时间到2024-01-19",
            "newStartTime": "2024-01-19T09:00:00Z",
            "newEndTime": "2024-01-24T18:00:00Z"
          }
        ]
      }
    ]
  },
  "message": "冲突检测完成"
}
```

### AI智能调整

#### POST /api/v1/timeline/ai/adjust
**描述**: AI智能调整任务安排

**请求头**:
```
Authorization: Bearer <token>
```

**请求参数**:
```json
{
  "taskId": "task_123456789",
  "reason": "昨晚加班到很晚，今天需要调整时间",
  "constraints": {
    "mustCompleteBy": "2024-01-25T18:00:00Z",
    "preferredTimeSlots": [
      {
        "startTime": "2024-01-16T10:00:00Z",
        "endTime": "2024-01-16T18:00:00Z"
      }
    ]
  }
}
```

**响应示例**:
```json
{
  "success": true,
  "data": {
    "adjustmentId": "adjustment_123456789",
    "originalSchedule": {
      "startTime": "2024-01-15T09:00:00Z",
      "endTime": "2024-01-20T18:00:00Z"
    },
    "newSchedule": {
      "startTime": "2024-01-16T10:00:00Z",
      "endTime": "2024-01-22T18:00:00Z"
    },
    "reason": "考虑到您昨晚加班的情况，建议将任务延后一天开始，并适当延长工作时间以确保质量",
    "impact": {
      "affectedTasks": 2,
      "totalDelay": "1 day",
      "riskLevel": "low"
    },
    "alternatives": [
      {
        "option": "split_task",
        "description": "将任务拆分为两个阶段",
        "schedule": {
          "phase1": {
            "startTime": "2024-01-16T10:00:00Z",
            "endTime": "2024-01-19T18:00:00Z"
          },
          "phase2": {
            "startTime": "2024-01-22T09:00:00Z",
            "endTime": "2024-01-24T18:00:00Z"
          }
        }
      }
    ]
  },
  "message": "AI调整建议生成成功"
}
```

---

## 🔔 提醒系统API

### 创建提醒

#### POST /api/v1/reminders
**描述**: 创建任务提醒

**请求头**:
```
Authorization: Bearer <token>
```

**请求参数**:
```json
{
  "taskId": "task_123456789",
  "reminderTime": "2024-01-15T08:30:00Z",
  "reminderType": "web",
  "message": "记得开始项目文档编写",
  "channels": ["web", "email"],
  "repeatRule": {
    "type": "none",
    "interval": null,
    "endDate": null
  }
}
```

**响应示例**:
```json
{
  "success": true,
  "data": {
    "reminderId": "reminder_123456789",
    "taskId": "task_123456789",
    "reminderTime": "2024-01-15T08:30:00Z",
    "reminderType": "web",
    "status": "pending",
    "channels": ["web", "email"],
    "createdAt": "2024-01-01T00:00:00Z"
  },
  "message": "提醒创建成功"
}
```

### 获取提醒列表

#### GET /api/v1/reminders
**描述**: 获取用户的提醒列表

**请求头**:
```
Authorization: Bearer <token>
```

**查询参数**:
- `page`: 页码
- `limit`: 每页数量
- `status`: 状态（pending, sent, cancelled）
- `startDate`: 开始日期
- `endDate`: 结束日期

**响应示例**:
```json
{
  "success": true,
  "data": {
    "reminders": [
      {
        "reminderId": "reminder_123456789",
        "taskId": "task_123456789",
        "taskTitle": "完成项目文档",
        "reminderTime": "2024-01-15T08:30:00Z",
        "reminderType": "web",
        "status": "pending",
        "message": "记得开始项目文档编写",
        "channels": ["web", "email"],
        "createdAt": "2024-01-01T00:00:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "total": 50,
      "totalPages": 3
    }
  },
  "message": "获取提醒列表成功"
}
```

### 更新提醒

#### PUT /api/v1/reminders/{reminderId}
**描述**: 更新提醒设置

**路径参数**:
- `reminderId`: 提醒ID

**请求头**:
```
Authorization: Bearer <token>
```

**请求参数**:
```json
{
  "reminderTime": "2024-01-15T09:00:00Z",
  "message": "记得开始项目文档编写（更新）",
  "channels": ["web", "email", "sms"]
}
```

**响应示例**:
```json
{
  "success": true,
  "data": {
    "reminderId": "reminder_123456789",
    "reminderTime": "2024-01-15T09:00:00Z",
    "message": "记得开始项目文档编写（更新）",
    "channels": ["web", "email", "sms"],
    "updatedAt": "2024-01-01T00:00:00Z"
  },
  "message": "提醒更新成功"
}
```

### 删除提醒

#### DELETE /api/v1/reminders/{reminderId}
**描述**: 删除指定提醒

**路径参数**:
- `reminderId`: 提醒ID

**请求头**:
```
Authorization: Bearer <token>
```

**响应示例**:
```json
{
  "success": true,
  "data": null,
  "message": "提醒删除成功"
}
```

### 获取通知设置

#### GET /api/v1/notifications/settings
**描述**: 获取用户的通知设置

**请求头**:
```
Authorization: Bearer <token>
```

**响应示例**:
```json
{
  "success": true,
  "data": {
    "channels": {
      "web": {
        "enabled": true,
        "sound": true,
        "desktop": true
      },
      "email": {
        "enabled": true,
        "frequency": "immediate",
        "digest": false
      },
      "sms": {
        "enabled": false,
        "phone": "13800138000"
      },
      "push": {
        "enabled": true,
        "categories": ["urgent", "daily", "weekly"]
      }
    },
    "preferences": {
      "quietHours": {
        "enabled": true,
        "startTime": "22:00",
        "endTime": "08:00"
      },
      "timezone": "Asia/Shanghai"
    }
  },
  "message": "获取通知设置成功"
}
```

### 更新通知设置

#### PUT /api/v1/notifications/settings
**描述**: 更新用户的通知设置

**请求头**:
```
Authorization: Bearer <token>
```

**请求参数**:
```json
{
  "channels": {
    "web": {
      "enabled": true,
      "sound": true,
      "desktop": true
    },
    "email": {
      "enabled": true,
      "frequency": "daily",
      "digest": true
    }
  },
  "preferences": {
    "quietHours": {
      "enabled": true,
      "startTime": "23:00",
      "endTime": "07:00"
    }
  }
}
```

**响应示例**:
```json
{
  "success": true,
  "data": {
    "updatedAt": "2024-01-01T00:00:00Z"
  },
  "message": "通知设置更新成功"
}
```

---

## 📊 数据分析API

### 获取用户统计

#### GET /api/v1/analytics/user
**描述**: 获取用户使用统计

**请求头**:
```
Authorization: Bearer <token>
```

**查询参数**:
- `period`: 统计周期（day, week, month, year）
- `startDate`: 开始日期
- `endDate`: 结束日期

**响应示例**:
```json
{
  "success": true,
  "data": {
    "period": "month",
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": "2024-01-31T23:59:59Z",
    "metrics": {
      "tasks": {
        "total": 45,
        "completed": 38,
        "inProgress": 5,
        "overdue": 2,
        "completionRate": 84.4
      },
      "time": {
        "totalPlannedHours": 120,
        "totalActualHours": 108,
        "efficiency": 90.0,
        "averageTaskDuration": 2.4
      },
      "productivity": {
        "focusTime": 85.5,
        "interruptions": 12,
        "productivityScore": 87.2
      }
    },
    "trends": {
      "dailyActivity": [
        {
          "date": "2024-01-01",
          "tasksCreated": 3,
          "tasksCompleted": 2,
          "focusHours": 6.5
        }
      ]
    }
  },
  "message": "获取用户统计成功"
}
```

### 获取任务分析

#### GET /api/v1/analytics/tasks
**描述**: 获取任务分析数据

**请求头**:
```
Authorization: Bearer <token>
```

**查询参数**:
- `category`: 分类过滤
- `priority`: 优先级过滤
- `startDate`: 开始日期
- `endDate`: 结束日期

**响应示例**:
```json
{
  "success": true,
  "data": {
    "summary": {
      "totalTasks": 45,
      "completedTasks": 38,
      "overdueTasks": 2,
      "averageCompletionTime": "2.4 days"
    },
    "byCategory": [
      {
        "category": "work",
        "total": 25,
        "completed": 22,
        "completionRate": 88.0,
        "averageTime": "2.1 days"
      },
      {
        "category": "personal",
        "total": 20,
        "completed": 16,
        "completionRate": 80.0,
        "averageTime": "2.8 days"
      }
    ],
    "byPriority": [
      {
        "priority": "high",
        "total": 15,
        "completed": 13,
        "completionRate": 86.7
      },
      {
        "priority": "medium",
        "total": 20,
        "completed": 18,
        "completionRate": 90.0
      },
      {
        "priority": "low",
        "total": 10,
        "completed": 7,
        "completionRate": 70.0
      }
    ],
    "timeline": {
      "dailyProgress": [
        {
          "date": "2024-01-01",
          "tasksCreated": 3,
          "tasksCompleted": 2,
          "progress": 66.7
        }
      ]
    }
  },
  "message": "获取任务分析成功"
}
```

### 获取效率报告

#### GET /api/v1/analytics/efficiency
**描述**: 获取效率分析报告

**请求头**:
```
Authorization: Bearer <token>
```

**查询参数**:
- `period`: 报告周期（week, month, quarter）
- `startDate`: 开始日期
- `endDate`: 结束日期

**响应示例**:
```json
{
  "success": true,
  "data": {
    "period": "month",
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": "2024-01-31T23:59:59Z",
    "overview": {
      "productivityScore": 87.2,
      "efficiencyTrend": "improving",
      "focusTime": 85.5,
      "interruptions": 12
    },
    "timeAnalysis": {
      "totalWorkTime": 160,
      "productiveTime": 140,
      "distractionTime": 20,
      "efficiencyRatio": 87.5
    },
    "taskAnalysis": {
      "tasksCompleted": 38,
      "tasksOverdue": 2,
      "averageCompletionTime": "2.4 days",
      "completionRate": 84.4
    },
    "insights": [
      {
        "type": "positive",
        "title": "效率提升",
        "description": "相比上月，您的任务完成率提升了15%",
        "metric": "completion_rate",
        "change": "+15%"
      },
      {
        "type": "suggestion",
        "title": "时间管理建议",
        "description": "建议在上午9-11点安排重要任务，此时效率最高",
        "metric": "peak_hours",
        "recommendation": "schedule_important_tasks"
      }
    ],
    "recommendations": [
      {
        "category": "time_management",
        "title": "优化工作时间",
        "description": "根据您的效率曲线，建议将重要任务安排在上午",
        "priority": "high",
        "expectedImpact": "提升15%效率"
      }
    ]
  },
  "message": "获取效率报告成功"
}
```

---

## ❌ 错误处理

### 错误代码说明

#### 认证错误 (AUTH_*)
- `AUTH_001`: 认证失败
- `AUTH_002`: Token已过期
- `AUTH_003`: 权限不足
- `AUTH_004`: 用户不存在
- `AUTH_005`: 密码错误

#### 任务错误 (TASK_*)
- `TASK_001`: 任务不存在
- `TASK_002`: 任务创建失败
- `TASK_003`: 任务更新失败
- `TASK_004`: 任务删除失败
- `TASK_005`: 时间冲突

#### 提醒错误 (REMINDER_*)
- `REMINDER_001`: 提醒不存在
- `REMINDER_002`: 提醒创建失败
- `REMINDER_003`: 提醒时间无效
- `REMINDER_004`: 提醒渠道不支持

#### 系统错误 (SYS_*)
- `SYS_001`: 服务器内部错误
- `SYS_002`: 数据库连接失败
- `SYS_003`: 外部服务调用失败
- `SYS_004`: 数据验证失败

### ASP.NET Core错误处理
- **ModelState验证**: 自动验证请求模型
- **全局异常处理**: 使用ExceptionFilter
- **日志记录**: 使用ILogger记录错误
- **自定义异常**: 定义业务异常类型
- **中间件处理**: 使用中间件统一处理异常

### 错误响应示例

#### 认证失败
```json
{
  "success": false,
  "error": {
    "code": "AUTH_001",
    "message": "认证失败",
    "details": "Token无效或已过期"
  },
  "timestamp": "2024-01-01T00:00:00Z",
  "requestId": "req_123456789"
}
```

#### 任务不存在
```json
{
  "success": false,
  "error": {
    "code": "TASK_001",
    "message": "任务不存在",
    "details": "指定的任务ID不存在或已被删除"
  },
  "timestamp": "2024-01-01T00:00:00Z",
  "requestId": "req_123456789"
}
```

#### 时间冲突
```json
{
  "success": false,
  "error": {
    "code": "TASK_005",
    "message": "时间冲突",
    "details": "任务时间与现有任务冲突",
    "conflicts": [
      {
        "taskId": "task_987654321",
        "title": "需求分析",
        "conflictType": "overlap"
      }
    ]
  },
  "timestamp": "2024-01-01T00:00:00Z",
  "requestId": "req_123456789"
}
```

---

## 📋 数据格式规范

### 日期时间格式
- **ISO 8601**: `YYYY-MM-DDTHH:mm:ss.sssZ`
- **示例**: `2024-01-15T09:00:00.000Z`

### 枚举值

#### 任务状态
- `pending`: 待开始
- `in_progress`: 进行中
- `completed`: 已完成
- `cancelled`: 已取消
- `paused`: 已暂停

#### 任务优先级
- `low`: 低
- `medium`: 中
- `high`: 高
- `urgent`: 紧急

#### 提醒类型
- `web`: 网页提醒
- `email`: 邮件提醒
- `sms`: 短信提醒
- `push`: 推送提醒

#### 重复规则类型
- `none`: 不重复
- `daily`: 每天
- `weekly`: 每周
- `monthly`: 每月
- `yearly`: 每年

### 分页格式
```json
{
  "page": 1,
  "limit": 20,
  "total": 100,
  "totalPages": 5,
  "hasNext": true,
  "hasPrev": false
}
```

### 排序格式
```json
{
  "field": "createdAt",
  "order": "desc"
}
```

---

## 📝 总结

这个API文档提供了智能提醒事项Web App的完整接口规范，包括：

1. **认证授权API**: 用户注册、登录、第三方登录、令牌管理
2. **任务管理API**: 任务的增删改查、批量操作、搜索筛选
3. **时间线管理API**: 甘特图数据、时间调整、冲突检测、AI智能调整
4. **提醒系统API**: 提醒创建、管理、通知设置
5. **数据分析API**: 用户统计、任务分析、效率报告
6. **错误处理**: 完整的错误代码和响应格式
7. **数据格式规范**: 统一的日期时间、枚举值、分页格式

所有接口都遵循RESTful设计原则，使用JSON格式进行数据交换，支持JWT认证，并提供了详细的请求参数和响应示例。
