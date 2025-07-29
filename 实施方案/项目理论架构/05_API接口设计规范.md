---
**文档类型**: API接口设计规范
**文档版本**: v2.0
**创建日期**: 2025-07-29
**最后更新**: 2025-07-29
**文档所有者**: 后端负责人
**审批状态**: 已审批
**变更说明**: 企业级API接口设计规范重构，完善RESTful API设计和文档标准
---

# 🔌 ToDoListArea API接口设计规范

## 🔗 相关文档链接

- [文档体系主索引](./00_文档体系主索引.md) - 查看完整文档体系
- [技术选型与架构设计](./03_技术选型与架构设计.md) - 查看技术架构
- [详细设计规格书](./04_详细设计规格书.md) - 查看系统设计
- [开发实施指南](./06_开发实施指南.md) - 查看开发步骤

## 📋 目录

- [API设计原则](#API设计原则)
- [接口规范标准](#接口规范标准)
- [认证授权](#认证授权)
- [核心接口设计](#核心接口设计)
- [错误处理](#错误处理)
- [版本控制](#版本控制)

---

## 🎯 API设计原则

### 📐 RESTful设计原则

#### 1. 资源导向设计
- **资源标识**: 使用名词表示资源，避免动词
- **层次结构**: 体现资源间的层次关系
- **统一接口**: 使用标准HTTP方法操作资源

```
✅ 正确示例:
GET    /api/v1/tasks              # 获取任务列表
POST   /api/v1/tasks              # 创建新任务
GET    /api/v1/tasks/{id}         # 获取特定任务
PUT    /api/v1/tasks/{id}         # 更新特定任务
DELETE /api/v1/tasks/{id}         # 删除特定任务

❌ 错误示例:
GET    /api/v1/getTasks           # 使用动词
POST   /api/v1/createTask         # 使用动词
GET    /api/v1/task-list          # 不一致的命名
```

#### 2. HTTP方法语义
| 方法 | 语义 | 幂等性 | 安全性 | 使用场景 |
|------|------|--------|--------|----------|
| **GET** | 获取资源 | ✅ | ✅ | 查询数据 |
| **POST** | 创建资源 | ❌ | ❌ | 创建新资源 |
| **PUT** | 更新资源 | ✅ | ❌ | 完整更新 |
| **PATCH** | 部分更新 | ❌ | ❌ | 部分更新 |
| **DELETE** | 删除资源 | ✅ | ❌ | 删除资源 |

#### 3. 状态码规范
| 状态码 | 含义 | 使用场景 |
|--------|------|----------|
| **200** | OK | 成功获取资源 |
| **201** | Created | 成功创建资源 |
| **204** | No Content | 成功删除资源 |
| **400** | Bad Request | 请求参数错误 |
| **401** | Unauthorized | 未认证 |
| **403** | Forbidden | 无权限 |
| **404** | Not Found | 资源不存在 |
| **409** | Conflict | 资源冲突 |
| **422** | Unprocessable Entity | 业务逻辑错误 |
| **500** | Internal Server Error | 服务器内部错误 |

### 🔧 API设计最佳实践

#### 1. URL设计规范
```
基础URL结构:
https://api.todolistarea.com/v1/{resource}

资源命名规则:
- 使用复数名词: /tasks, /users, /categories
- 使用小写字母: /task-categories (不是 /TaskCategories)
- 使用连字符分隔: /user-profiles (不是 /user_profiles)
- 避免深层嵌套: 最多3层 /users/{id}/tasks/{id}/comments
```

#### 2. 查询参数规范
```
分页参数:
?page=1&pageSize=20&sort=createdAt&order=desc

筛选参数:
?status=pending&priority=high&categoryId=123

搜索参数:
?search=关键词&searchFields=title,description

日期范围:
?startDate=2025-01-01&endDate=2025-12-31
```

#### 3. 响应格式标准
```json
{
  "success": true,
  "code": "SUCCESS",
  "message": "操作成功",
  "data": {
    // 实际数据
  },
  "timestamp": "2025-07-29T10:30:00Z",
  "requestId": "req-123456789"
}
```

---

## 🔒 认证授权

### 🎫 JWT认证机制

#### 认证流程设计
```mermaid
sequenceDiagram
    participant C as 客户端
    participant A as 认证服务
    participant R as 资源服务
    
    C->>A: POST /auth/login (用户名/密码)
    A->>A: 验证凭据
    A->>C: 返回JWT令牌
    C->>R: GET /api/v1/tasks (携带JWT)
    R->>R: 验证JWT令牌
    R->>C: 返回资源数据
```

#### JWT令牌结构
```json
{
  "header": {
    "alg": "HS256",
    "typ": "JWT"
  },
  "payload": {
    "sub": "user-id-123",
    "email": "user@example.com",
    "name": "张三",
    "iat": 1643723400,
    "exp": 1643809800,
    "iss": "todolistarea.com",
    "aud": "todolistarea-api"
  }
}
```

### 🔐 认证接口设计

#### 1. 用户注册
```http
POST /api/v1/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "firstName": "张",
  "lastName": "三",
  "timezone": "Asia/Shanghai"
}
```

**响应示例**:
```json
{
  "success": true,
  "code": "REGISTER_SUCCESS",
  "message": "注册成功，请查收验证邮件",
  "data": {
    "userId": "123e4567-e89b-12d3-a456-426614174000",
    "email": "user@example.com",
    "emailVerificationRequired": true
  }
}
```

#### 2. 用户登录
```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "rememberMe": true
}
```

**响应示例**:
```json
{
  "success": true,
  "code": "LOGIN_SUCCESS",
  "message": "登录成功",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "def50200e54b...",
    "expiresIn": 86400,
    "tokenType": "Bearer",
    "user": {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "email": "user@example.com",
      "name": "张三",
      "avatar": "https://cdn.example.com/avatar.jpg"
    }
  }
}
```

#### 3. 令牌刷新
```http
POST /api/v1/auth/refresh
Content-Type: application/json

{
  "refreshToken": "def50200e54b..."
}
```

#### 4. 用户登出
```http
POST /api/v1/auth/logout
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 📋 核心接口设计

### 🎯 任务管理接口

#### 1. 获取任务列表
```http
GET /api/v1/tasks?page=1&pageSize=20&status=pending&priority=high
Authorization: Bearer {token}
```

**查询参数**:
| 参数 | 类型 | 必需 | 说明 |
|------|------|------|------|
| page | integer | 否 | 页码，默认1 |
| pageSize | integer | 否 | 每页大小，默认20，最大100 |
| status | string | 否 | 任务状态：pending/in_progress/completed |
| priority | string | 否 | 优先级：low/medium/high/urgent |
| categoryId | string | 否 | 分类ID |
| search | string | 否 | 搜索关键词 |
| startDate | string | 否 | 开始日期 (ISO 8601) |
| endDate | string | 否 | 结束日期 (ISO 8601) |
| sort | string | 否 | 排序字段：createdAt/updatedAt/priority |
| order | string | 否 | 排序方向：asc/desc |

**响应示例**:
```json
{
  "success": true,
  "code": "SUCCESS",
  "message": "获取成功",
  "data": {
    "items": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "title": "完成项目文档",
        "description": "编写技术文档和用户手册",
        "status": "in_progress",
        "priority": "high",
        "startTime": "2025-07-29T09:00:00Z",
        "endTime": "2025-07-29T17:00:00Z",
        "estimatedDuration": 480,
        "completionPercentage": 60.5,
        "category": {
          "id": "cat-123",
          "name": "工作",
          "color": "#FF5722"
        },
        "tags": ["文档", "项目"],
        "createdAt": "2025-07-28T10:00:00Z",
        "updatedAt": "2025-07-29T08:30:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 20,
      "totalCount": 156,
      "totalPages": 8,
      "hasNext": true,
      "hasPrevious": false
    }
  }
}
```

#### 2. 创建任务
```http
POST /api/v1/tasks
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "完成项目文档",
  "description": "编写技术文档和用户手册",
  "priority": "high",
  "startTime": "2025-07-29T09:00:00Z",
  "endTime": "2025-07-29T17:00:00Z",
  "estimatedDuration": 480,
  "categoryId": "cat-123",
  "tags": ["文档", "项目"],
  "isRecurring": false,
  "parentTaskId": null
}
```

**字段验证规则**:
| 字段 | 规则 | 说明 |
|------|------|------|
| title | 必需，1-255字符 | 任务标题 |
| description | 可选，最大5000字符 | 任务描述 |
| priority | 可选，枚举值 | low/medium/high/urgent |
| startTime | 可选，ISO 8601格式 | 开始时间 |
| endTime | 可选，ISO 8601格式 | 结束时间，必须晚于开始时间 |
| estimatedDuration | 可选，正整数 | 预估时长（分钟） |

#### 3. 更新任务
```http
PUT /api/v1/tasks/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "完成项目文档（已更新）",
  "status": "completed",
  "completionPercentage": 100,
  "actualDuration": 420
}
```

#### 4. 删除任务
```http
DELETE /api/v1/tasks/{id}
Authorization: Bearer {token}
```

**响应示例**:
```json
{
  "success": true,
  "code": "DELETE_SUCCESS",
  "message": "任务删除成功"
}
```

### 📊 甘特图接口

#### 1. 获取甘特图数据
```http
GET /api/v1/gantt?startDate=2025-07-01&endDate=2025-07-31&viewType=month
Authorization: Bearer {token}
```

**响应示例**:
```json
{
  "success": true,
  "code": "SUCCESS",
  "message": "获取成功",
  "data": {
    "timeline": {
      "startDate": "2025-07-01T00:00:00Z",
      "endDate": "2025-07-31T23:59:59Z",
      "viewType": "month",
      "timeUnits": [
        {
          "date": "2025-07-01",
          "label": "7月1日",
          "isWeekend": false,
          "isHoliday": false
        }
      ]
    },
    "tasks": [
      {
        "id": "task-123",
        "title": "项目开发",
        "startDate": "2025-07-01T09:00:00Z",
        "endDate": "2025-07-15T18:00:00Z",
        "progress": 75.5,
        "dependencies": ["task-456"],
        "resources": ["user-123", "user-456"],
        "color": "#2196F3",
        "position": {
          "x": 0,
          "y": 0,
          "width": 300,
          "height": 24
        }
      }
    ],
    "dependencies": [
      {
        "from": "task-456",
        "to": "task-123",
        "type": "finish_to_start",
        "lag": 0
      }
    ]
  }
}
```

#### 2. 更新任务时间
```http
PATCH /api/v1/gantt/tasks/{id}/time
Authorization: Bearer {token}
Content-Type: application/json

{
  "startTime": "2025-07-29T10:00:00Z",
  "endTime": "2025-07-29T18:00:00Z"
}
```

### 🔔 提醒系统接口

#### 1. 创建提醒
```http
POST /api/v1/reminders
Authorization: Bearer {token}
Content-Type: application/json

{
  "taskId": "task-123",
  "type": "email",
  "triggerTime": "2025-07-29T08:00:00Z",
  "message": "项目会议即将开始",
  "isRecurring": true,
  "recurrencePattern": "daily"
}
```

#### 2. 获取提醒列表
```http
GET /api/v1/reminders?taskId=task-123&type=email&status=active
Authorization: Bearer {token}
```

---

## ❌ 错误处理

### 🎯 错误响应格式

#### 标准错误响应
```json
{
  "success": false,
  "code": "VALIDATION_ERROR",
  "message": "请求参数验证失败",
  "errors": [
    {
      "field": "title",
      "code": "REQUIRED",
      "message": "任务标题不能为空"
    },
    {
      "field": "endTime",
      "code": "INVALID_DATE_RANGE",
      "message": "结束时间必须晚于开始时间"
    }
  ],
  "timestamp": "2025-07-29T10:30:00Z",
  "requestId": "req-123456789",
  "path": "/api/v1/tasks"
}
```

### 📋 错误代码规范

#### 业务错误代码
| 错误代码 | HTTP状态码 | 说明 |
|----------|------------|------|
| **VALIDATION_ERROR** | 400 | 请求参数验证失败 |
| **UNAUTHORIZED** | 401 | 未认证或令牌无效 |
| **FORBIDDEN** | 403 | 无权限访问资源 |
| **RESOURCE_NOT_FOUND** | 404 | 资源不存在 |
| **RESOURCE_CONFLICT** | 409 | 资源冲突 |
| **BUSINESS_ERROR** | 422 | 业务逻辑错误 |
| **RATE_LIMIT_EXCEEDED** | 429 | 请求频率超限 |
| **INTERNAL_ERROR** | 500 | 服务器内部错误 |

#### 具体业务错误
| 错误代码 | 说明 |
|----------|------|
| **TASK_TIME_CONFLICT** | 任务时间冲突 |
| **INVALID_TASK_DEPENDENCY** | 无效的任务依赖关系 |
| **TASK_ALREADY_COMPLETED** | 任务已完成 |
| **CATEGORY_NOT_FOUND** | 分类不存在 |
| **REMINDER_TIME_PAST** | 提醒时间已过期 |

---

## 🔄 版本控制

### 📋 版本控制策略

#### 1. URL版本控制
```
当前版本: /api/v1/tasks
下个版本: /api/v2/tasks

版本号规则:
- v1, v2, v3... (主版本号)
- 向后兼容的更改不增加版本号
- 破坏性更改增加主版本号
```

#### 2. 版本兼容性
| 变更类型 | 是否兼容 | 版本策略 |
|----------|----------|----------|
| 新增字段 | ✅ | 当前版本 |
| 新增接口 | ✅ | 当前版本 |
| 修改字段类型 | ❌ | 新版本 |
| 删除字段 | ❌ | 新版本 |
| 修改接口行为 | ❌ | 新版本 |

#### 3. 版本废弃策略
```
版本生命周期:
1. 发布新版本
2. 标记旧版本为废弃 (6个月过渡期)
3. 停止支持旧版本

废弃通知:
- API响应头: X-API-Deprecated: true
- 文档标注: [DEPRECATED]
- 邮件通知开发者
```

---

## 📊 API监控与限流

### 🎯 性能指标

| 指标 | 目标值 | 监控方法 |
|------|--------|----------|
| **响应时间** | P95 < 300ms | APM监控 |
| **可用性** | > 99.9% | 健康检查 |
| **错误率** | < 0.1% | 错误日志统计 |
| **吞吐量** | > 1000 RPS | 负载测试 |

### 🚦 限流策略

#### 限流规则
```
用户级别限流:
- 普通用户: 1000 请求/小时
- 高级用户: 5000 请求/小时
- 企业用户: 10000 请求/小时

接口级别限流:
- 查询接口: 100 请求/分钟
- 创建接口: 50 请求/分钟
- 批量接口: 10 请求/分钟
```

#### 限流响应
```json
{
  "success": false,
  "code": "RATE_LIMIT_EXCEEDED",
  "message": "请求频率超限，请稍后重试",
  "retryAfter": 60,
  "limit": 1000,
  "remaining": 0,
  "resetTime": "2025-07-29T11:00:00Z"
}
```

---

## 📝 变更记录

| 版本 | 日期 | 变更内容 | 变更人 | 审批人 |
|------|------|----------|--------|--------|
| v2.0 | 2025-07-29 | 企业级API接口设计规范重构，完善RESTful API设计和文档标准 | 后端团队 | 架构师 |
| v1.0 | 2025-07-26 | 初始API接口设计文档创建 | 后端负责人 | 技术负责人 |

### 🔄 下次更新计划
- **计划日期**: 2025-08-12
- **更新内容**: 根据开发进展完善接口设计
- **负责人**: 后端负责人

---

**文档维护**: 本文档由后端团队维护，接口变更时及时更新
**接口测试**: 所有接口必须通过自动化测试验证
**版权声明**: 本文档为ToDoListArea项目内部文档，未经授权不得外传
