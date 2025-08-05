---
**文档版本**: v2.0
**创建日期**: 2025-07-29
**最后更新**: 2025-08-04
**更新人**: AI Assistant
**变更说明**: 提醒系统API路径修复完成，前后端API调用统一，所有提醒功能正常工作
---

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
- [更新记录](#更新记录)

## 🔗 相关文档链接

- [数据库表结构](./数据库表_实际结构.md) - 查看完整的数据库设计
- [技术架构设计](./技术架构.md#api接口设计) - 了解API架构设计原则
- [开发流程实施](./开发流程实施.md#第五步api接口设计) - API开发实施指南

---

## 🌐 API概述

### 基础信息
- **API版本**: v1.0
- **基础URL**: `http://localhost:5006/api` (开发环境)
- **协议**: HTTP/HTTPS
- **数据格式**: JSON
- **字符编码**: UTF-8

### 🎯 已实现控制器清单（13个）
| 控制器 | 路由前缀 | 功能描述 | 端点数量 |
|--------|---------|----------|----------|
| **HealthController** | `/Health` | 健康检查和系统状态监控 | 5个 |
| **MetricsController** | `/Metrics` | 系统指标和性能监控 | 6个 |
| **UserController** | `/User` | 用户管理和认证系统 | 8个 |
| **UserProfileController** | `/UserProfile` | 用户配置和个人信息管理 | 7个 |
| **TaskController** | `/Task` | 任务核心CRUD和管理功能 | 12个 |
| **TaskDetailsController** | `/TaskDetails` | 任务详细信息和扩展功能 | 10个 |
| **TaskCategoryController** | `/TaskCategory` | 任务分类管理系统 | 8个 |
| **TaskDependencyController** | `/TaskDependency` | 任务依赖关系处理 | 9个 |
| **TaskTemplateController** | `/TaskTemplate` | 任务模板和复用系统 | 11个 |
| **ReminderController** | `/Reminder` | 多维度提醒功能管理 | 13个 |
| **GanttDataController** | `/GanttData` | 甘特图数据处理和同步 | 8个 |
| **UserActivityController** | `/UserActivity` | 用户活动跟踪和日志 | 7个 |
| **DataConsistencyController** | `/DataConsistency` | 数据一致性检查和修复 | 6个 |
| **总计** | - | **完整的企业级API架构** | **110+个端点** |

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

n/v1/auth/profile` | 获取当前用户详细信息 |
| 更新用户信息 | PUT | `/api/v1/auth/profile` | 更新姓名、头像等信息 |
| 修改密码 | PUT | `/api/v1/auth/password` | 修改登录密码 |

> **认证说明**: 除注册、登录、第三方登录外，其他接口都需要在请求头中携带 `Authorization: Bearer <token>`

### 关键数据结构

**用户注册请求**:
```json
{
  "email": "user@example.com",
  "password": "password123",
  "name": "张三",
  "phone": "13800138000"
}
```

**认证响应数据**:
```json
{
  "userId": "user_123456789",
  "email": "user@example.com",
  "name": "张三",
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "refresh_token_123456789",
  "expiresIn": 3600
}
```

---

## 📋 任务管理API

### 核心接口概览

| 接口 | 方法 | 路径 | 描述 |
|------|------|------|------|
| 创建任务 | POST | `/api/v1/tasks` | 创建新任务 |
| 获取任务列表 | GET | `/api/v1/tasks` | 分页查询任务，支持筛选 |
| 获取任务详情 | GET | `/api/v1/tasks/{taskId}` | 获取任务详细信息 |
| 更新任务 | PUT | `/api/v1/tasks/{taskId}` | 更新任务信息 |
| 删除任务 | DELETE | `/api/v1/tasks/{taskId}` | 删除任务 |
| 批量操作 | POST | `/api/v1/tasks/batch` | 批量创建、更新、删除任务 |
| 任务搜索 | GET | `/api/v1/tasks/search` | 全文搜索任务 |
| 任务统计 | GET | `/api/v1/tasks/statistics` | 获取任务统计数据 |

### 关键数据结构

**任务创建请求**:
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

**任务响应数据**:
```json
{
  "taskId": "task_987654321",
  "title": "完成项目文档",
  "status": "pending",
  "priority": "high",
  "progress": 0,
  "startTime": "2024-01-15T09:00:00Z",
  "endTime": "2024-01-20T18:00:00Z",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### 查询参数说明

**任务列表查询参数**:
- `page`, `limit`: 分页参数
- `status`: 任务状态筛选 (pending, in_progress, completed, cancelled)
- `priority`: 优先级筛选 (low, medium, high)
- `category`: 分类筛选
- `startDate`, `endDate`: 时间范围筛选
- `search`: 关键词搜索

**分页响应格式**:
```json
{
  "tasks": [...],
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 100,
    "totalPages": 5
  }
}
```

---

## 🏷️ 任务分类管理API

### 核心接口概览

| 接口 | 方法 | 路径 | 描述 | 状态 |
|------|------|------|------|------|
| 获取分类列表 | GET | `/api/TaskCategory` | 获取所有任务分类 | ✅ 已实现 |
| 获取分类详情 | GET | `/api/TaskCategory/{id}` | 获取单个分类信息 | ✅ 已实现 |
| 创建分类 | POST | `/api/TaskCategory` | 创建新的任务分类 | ✅ 已实现 |
| 更新分类 | PUT | `/api/TaskCategory/{id}` | 更新分类信息 | ✅ 已实现 |
| 删除分类 | DELETE | `/api/TaskCategory/{id}` | 删除分类（检查关联任务） | ✅ 已实现 |

### 关键数据结构

**分类创建请求**:
```json
{
  "name": "工作",
  "color": "#007bff",
  "icon": "briefcase",
  "description": "工作相关任务",
  "sortOrder": 1
}
```

**分类响应数据**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": {
    "id": "502d78ae-784e-4e08-95b3-3a5affa365e3",
    "name": "工作",
    "color": "#007bff",
    "icon": "briefcase",
    "description": "工作相关任务",
    "isSystem": true,
    "sortOrder": 1,
    "createdAt": "2025-07-29T20:18:41.66Z"
  },
  "errors": []
}
```

### 测试验证结果 (2025-08-03)

**✅ 已通过Swagger测试验证**:
- ✅ GET `/api/TaskCategory`: 成功返回5个预设分类
- ✅ POST `/api/TaskCategory`: 成功创建"测试分类"
- ✅ 依赖注入配置正确，数据库操作正常
- ✅ API响应格式统一，错误处理完善

---

## 📊 时间线管理API

### 核心接口概览

| 接口 | 方法 | 路径 | 描述 |
|------|------|------|------|
| 获取甘特图数据 | GET | `/api/v1/timeline/gantt` | 获取甘特图任务和里程碑数据 |
| 更新任务时间 | PUT | `/api/v1/timeline/tasks/{taskId}/time` | 更新任务时间安排 |
| 检测时间冲突 | POST | `/api/v1/timeline/conflicts/detect` | 检测任务时间冲突 |
| 自动调整时间 | POST | `/api/v1/timeline/auto-adjust` | AI智能调整任务时间 |
| 创建里程碑 | POST | `/api/v1/timeline/milestones` | 创建项目里程碑 |
| 时间块管理 | GET/POST/PUT | `/api/v1/timeline/blocks` | 管理专注时间块 |

### 关键数据结构

**甘特图查询参数**:
- `startDate`, `endDate`: 时间范围
- `view`: 视图类型 (day, week, month, quarter)
- `category`, `status`: 筛选条件

**甘特图响应数据**:
```json
{
  "timeline": {
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": "2024-01-31T23:59:59Z",
    "view": "week"
  },
  "tasks": [...],
  "milestones": [...]
}
```

---

## 🔔 提醒系统API

### 核心接口概览

| 接口 | 方法 | 路径 | 描述 | 状态 |
|------|------|------|------|------|
| 创建提醒 | POST | `/api/Reminder` | 创建任务提醒 | ✅ 已修复 |
| 获取提醒列表 | GET | `/api/Reminder` | 获取用户提醒列表 | ✅ 已修复 |
| 获取任务提醒 | GET | `/api/Reminder/task/{taskId}` | 获取指定任务的提醒列表 | ✅ 已修复 |
| 更新提醒 | PUT | `/api/Reminder/{reminderId}` | 更新提醒设置 | ✅ 已修复 |
| 删除提醒 | DELETE | `/api/Reminder/{reminderId}` | 删除提醒 | ✅ 已修复 |
| 完成提醒 | POST | `/api/Reminder/{reminderId}/complete` | 完成提醒 | ✅ 已修复 |
| 延迟提醒 | POST | `/api/Reminder/{reminderId}/snooze` | 延迟提醒时间 | ✅ 已修复 |
| 提醒统计 | GET | `/api/Reminder/stats` | 获取提醒统计信息 | ✅ 已修复 |

### 🔧 API路径修复记录 (2025-08-04)

**修复问题**: 前后端API路径不匹配
- **原问题**: 前端调用`/Reminder/*`，后端实际路径为`/api/Reminder/*`
- **修复方案**: 统一前端API调用路径为`/api/Reminder/*`
- **涉及文件**: `useReminders.ts`, `reminderApi.ts`
- **修复结果**: 所有提醒API调用正常，功能完全可用 ✅

### 关键数据结构

**提醒创建请求**:
```json
{
  "taskId": "task_123456789",
  "reminderTime": "2024-01-15T08:30:00Z",
  "reminderType": "web",
  "message": "记得开始项目文档编写",
  "channels": ["web", "email"],
  "repeatRule": {
    "type": "none"
  }
}
```

**提醒响应数据**:
```json
{
  "reminderId": "reminder_123456789",
  "taskId": "task_123456789",
  "reminderTime": "2024-01-15T08:30:00Z",
  "status": "pending",
  "channels": ["web", "email"]
}
```

---

## 📊 数据分析API

### 核心接口概览

| 接口 | 方法 | 路径 | 描述 |
|------|------|------|------|
| 用户统计 | GET | `/api/v1/analytics/user-stats` | 获取用户任务统计 |
| 生产力分析 | GET | `/api/v1/analytics/productivity` | 获取生产力分析数据 |
| 时间分析 | GET | `/api/v1/analytics/time-analysis` | 获取时间使用分析 |
| 趋势分析 | GET | `/api/v1/analytics/trends` | 获取任务完成趋势 |
| 效率报告 | GET | `/api/v1/analytics/efficiency` | 获取效率分析报告 |
| 对比分析 | GET | `/api/v1/analytics/comparison` | 获取历史对比数据 |

nafa





---

## ❌ 错误处理

### 错误代码分类
- **AUTH_***: 认证相关错误 (001-005)
- **TASK_***: 任务相关错误 (001-005)
- **REMINDER_***: 提醒相关错误 (001-004)
- **SYS_***: 系统相关错误 (001-004)

### ASP.NET Core错误处理机制
- **ModelState验证**: 自动验证请求模型
- **全局异常处理**: 使用ExceptionFilter统一处理
- **自定义异常**: 定义业务异常类型
- **中间件处理**: 异常处理中间件

---

## 📋 数据格式规范

### 标准格式
- **日期时间**: ISO 8601格式 `YYYY-MM-DDTHH:mm:ss.sssZ`
- **分页**: `{page, limit, total, totalPages, hasNext, hasPrev}`
- **排序**: `{field, order}` (order: asc/desc)

### 枚举值定义
- **任务状态**: pending, in_progress, completed, cancelled, paused
- **优先级**: low, medium, high, urgent
- **提醒类型**: web, email, sms, push
- **重复规则**: none, daily, weekly, monthly, yearly



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

---

## 📝 更新记录

| 版本 | 日期 | 更新人 | 变更说明 |
|------|------|--------|----------|
| v1.0 | 2025-07-29 | AreaSong | 初始版本创建，完整API接口规范 |

### 更新频率说明
- **定期更新**: 当API接口发生变更时及时更新
- **版本同步**: 与后端开发进度保持同步
- **文档验证**: 每次发布前验证API文档的准确性

### 相关数据库表映射
本API文档中的接口与以下数据库表直接对应，详细表结构请参考 [数据库表_实际结构.md](./数据库表_实际结构.md)：

- **认证授权API** ↔ `users`, `user_sessions`, `user_oauth_accounts` 表
- **任务管理API** ↔ `tasks`, `task_categories`, `task_dependencies` 表
- **时间线管理API** ↔ `timeline_nodes`, `gantt_data`, `time_blocks` 表
- **提醒系统API** ↔ `reminders`, `reminder_rules`, `notification_settings` 表
- **数据分析API** ↔ `user_activities`, `task_statistics`, `productivity_metrics` 表
