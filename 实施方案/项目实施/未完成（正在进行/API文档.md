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
- [邀请码管理API](#邀请码管理api)
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
- **前端调用约定**: 统一通过 `src/services/api.ts` 的 axios 实例发起请求，模块内仅书写相对路径（不带`/api`前缀），例如：`/Reminder/stats`。

### 🎯 已实现控制器清单（15个）
| 控制器 | 路由前缀 | 功能描述 | 端点数量 |
|--------|---------|----------|----------|
| **HealthController** | `/Health` | 健康检查和系统状态监控 | 5个 |
| **MetricsController** | `/api/Metrics` | 系统指标和性能监控（管理员） | 4个+ |
| **AdminController** | `/api/Admin` | 管理员功能（用户管理、系统统计） | 6个+ |
| **UserController** | `/api/User` | 用户管理和认证系统 | 4个+ |
| **UserProfileController** | `/api/UserProfile` | 用户配置和个人信息管理 | 2个 |
| **InvitationCodeController** | `/api/InvitationCode` | 邀请码管理系统（管理员功能） | 6个+ |
| **TaskController** | `/api/Task` | 任务核心CRUD和管理功能 | 5个 |
| **TaskDetailsController** | `/api/TaskDetails` | 任务详细信息和扩展功能 | 9个+ |
| **TaskCategoryController** | `/api/TaskCategory` | 任务分类管理系统 | 5个 |
| **TaskDependencyController** | `/api/TaskDependency` | 任务依赖关系处理 | 5个 |
| **TaskTemplateController** | `/api/TaskTemplate` | 任务模板和复用系统 | 8个+ |
| **ReminderController** | `/api/Reminder` | 多维度提醒功能管理 | 8个 |
| **GanttDataController** | `/api/GanttData` | 甘特图数据处理和同步 | 5个 |
| **UserActivityController** | `/api/UserActivity` | 用户活动跟踪和日志 | 5个+ |
| **DataConsistencyController** | `/api/DataConsistency` | 数据一致性检查和修复 | 5个 |
| **总计** | - | **15个控制器，企业级API架构** | **100+端点** |

### 认证方式
- **JWT Token**: 在请求头中携带 `Authorization: Bearer <token>`（当前已启用）
- （说明）当前代码未启用 API Key 与 ASP.NET Core Identity；如需启用将另行在`Program.cs`与控制器中补充实现。

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

### 用户认证接口

| 接口 | 方法 | 路径 | 描述 |
|------|------|------|------|
| 用户注册 | POST | `/api/User/register` | 用户注册（需要邀请码） |
| 用户登录 | POST | `/api/User/login` | 用户登录获取JWT令牌 |
| 获取用户信息 | GET | `/api/User/profile/{userId}` | 获取当前用户详细信息 |
| 更新用户信息 | PUT | `/api/User/profile/{userId}` | 更新姓名、头像等信息 |

### 邀请码管理API（管理员专用）

| 接口 | 方法 | 路径 | 描述 |
|------|------|------|------|
| 验证邀请码 | POST | `/api/InvitationCode/validate` | 验证邀请码有效性（公开接口） |
| 创建邀请码 | POST | `/api/InvitationCode` | 创建新的邀请码（管理员） |
| 获取邀请码列表 | GET | `/api/InvitationCode` | 分页获取邀请码列表（管理员） |
| 更新邀请码 | PUT | `/api/InvitationCode/{id}` | 更新邀请码状态（管理员） |
| 删除邀请码 | DELETE | `/api/InvitationCode/{id}` | 删除邀请码（管理员） |
| 获取使用记录 | GET | `/api/InvitationCode/{id}/usages` | 获取邀请码使用记录（管理员） |

> **认证说明**: 除注册、登录、邀请码验证外，其他接口都需要在请求头中携带 `Authorization: Bearer <token>`
> **权限说明**: 邀请码管理接口需要管理员角色权限

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

## 📋 任务管理API（TaskController）

### 核心接口概览（与实际代码一致）

| 接口 | 方法 | 路径 | 描述 |
|------|------|------|------|
| 获取任务列表 | GET | `/api/Task/user/{userId}` | 支持状态、优先级、分类、时间范围、关键词、排序、分页 |
| 获取任务详情 | GET | `/api/Task/{id}` | 获取任务详细信息 |
| 创建任务 | POST | `/api/Task/user/{userId}` | 创建新任务（校验用户/分类/父任务） |
| 更新任务 | PUT | `/api/Task/{id}` | 局部更新任务信息 |
| 删除任务 | DELETE | `/api/Task/{id}` | 删除任务（含子任务检查） |

### 查询参数（GetTasks）
- status, priority, categoryId, startDate, endDate, searchKeyword, sortBy, sortOrder, pageNumber, pageSize

### 关键数据结构（示例）

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

## 📊 时间线/甘特图API（GanttDataController）

### 核心接口概览（与实际代码一致）

| 接口 | 方法 | 路径 | 描述 |
|------|------|------|------|
| 获取甘特图数据 | GET | `/api/GanttData/user/{userId}` | 含任务与分类信息、按开始日期排序 |
| 更新甘特图数据 | PUT | `/api/GanttData/{id}` | 同步更新关联任务开始/结束/进度 |
| 从任务同步 | POST | `/api/GanttData/sync/{userId}` | 根据任务起止时间批量生成/更新甘特条目并清理孤儿数据 |
| 一致性检查 | GET | `/api/GanttData/consistency-check/{userId}` | 检查任务与甘特数据的日期/进度不一致 |

### 说明
- 控制器已实现依赖与资源的JSON序列化/反序列化；进度范围校验已内置

---

## 🔔 提醒系统API（ReminderController）

### 核心接口概览

| 接口 | 方法 | 路径 | 描述 | 状态 |
|------|------|------|------|------|
| 创建提醒 | POST | `/Reminder` | 创建任务提醒 | ✅ 已修复（相对路径，基址`/api`） |
| 获取提醒列表 | GET | `/Reminder` | 获取用户提醒列表 | ✅ 已修复（相对路径，基址`/api`） |
| 获取任务提醒 | GET | `/Reminder/task/{taskId}` | 获取指定任务的提醒列表 | ✅ 已修复（相对路径，基址`/api`） |
| 更新提醒 | PUT | `/Reminder/{reminderId}` | 更新提醒设置 | ✅ 已修复（相对路径，基址`/api`） |
| 删除提醒 | DELETE | `/Reminder/{reminderId}` | 删除提醒 | ✅ 已修复（相对路径，基址`/api`） |
| 完成提醒 | POST | `/Reminder/{reminderId}/complete` | 完成提醒 | ✅ 已修复（相对路径，基址`/api`） |
| 延迟提醒 | POST | `/Reminder/{reminderId}/snooze` | 延迟提醒时间 | ✅ 已修复（相对路径，基址`/api`） |
| 提醒统计 | GET | `/Reminder/stats` | 获取提醒统计信息 | ✅ 已修复（相对路径，基址`/api`） |

### 🔧 API路径修复记录 (2025-08-04)

**修复问题**: 前后端API路径不匹配
- **原问题**: 前端调用未统一，导致与后端`/api/Reminder/*`不一致
- **修复方案**: 统一前端以“相对路径”调用（如`/Reminder/*`），由全局基址`/api`（或`VITE_API_BASE_URL`）拼接
- **涉及文件**: `useReminders.ts`, `reminderApi.ts`, `services/api.ts`
- **修复结果**: 前后端路径对齐，所有提醒API调用正常 ✅

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

## 📊 用户活动与系统统计API（UserActivityController, AdminController）

### 用户活动（UserActivityController）

| 接口 | 方法 | 路径 | 描述 |
|------|------|------|------|
| 获取用户活动列表 | GET | `/api/UserActivity/user/{userId}` | 支持分页与活动类型筛选 |
| 获取用户活动统计 | GET | `/api/UserActivity/user/{userId}/stats` | 每种活动类型数量、今日活动、最近一次活动时间 |
| 记录用户活动 | POST | `/api/UserActivity` | 手动写入活动（记录IP/UA） |
| 获取活动类型列表 | GET | `/api/UserActivity/activity-types` | 去重后的活动类型集合 |
| 删除活动记录 | DELETE | `/api/UserActivity/{id}` | 删除单条活动 |

### 管理员（AdminController，需管理员权限）

| 接口 | 方法 | 路径 | 描述 |
|------|------|------|------|
| 获取用户列表 | GET | `/api/Admin/users` | 支持分页、搜索、角色、状态筛选 |
| 获取用户详情 | GET | `/api/Admin/users/{id}` | 用户概要、任务统计、邀请码使用、权限 |
| 更新用户角色 | PUT | `/api/Admin/users/{id}/role` | 修改为指定角色（禁止修改自身） |
| 更新用户状态 | PUT | `/api/Admin/users/{id}/status` | 统一小写存储，禁止修改自身 |
| 获取系统统计 | GET | `/api/Admin/stats` | 用户/任务关键指标与邀请码统计 |


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