using System.ComponentModel.DataAnnotations;

namespace ToDoListArea.DTOs
{
    /// <summary>
    /// 管理员用户DTO
    /// </summary>
    public class AdminUserDto
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 手机号
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// 头像URL
        /// </summary>
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 角色
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// 邮箱验证状态
        /// </summary>
        public bool EmailVerified { get; set; }

        /// <summary>
        /// 手机验证状态
        /// </summary>
        public bool PhoneVerified { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 任务数量
        /// </summary>
        public int TaskCount { get; set; }
    }

    /// <summary>
    /// 管理员用户详情DTO
    /// </summary>
    public class AdminUserDetailDto : AdminUserDto
    {
        /// <summary>
        /// 已完成任务数量
        /// </summary>
        public int CompletedTaskCount { get; set; }

        /// <summary>
        /// 使用的邀请码
        /// </summary>
        public string? InvitationCodeUsed { get; set; }

        /// <summary>
        /// 用户权限列表
        /// </summary>
        public List<string> Permissions { get; set; } = new List<string>();
    }

    /// <summary>
    /// 管理员任务DTO
    /// </summary>
    public class AdminTaskDto
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 任务标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 任务描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 优先级
        /// </summary>
        public string Priority { get; set; } = string.Empty;

        /// <summary>
        /// 截止日期
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// 更新用户角色DTO
    /// </summary>
    public class UpdateUserRoleDto
    {
        /// <summary>
        /// 角色
        /// </summary>
        [Required(ErrorMessage = "角色不能为空")]
        [RegularExpression("^(admin|user)$", ErrorMessage = "角色只能是admin或user")]
        public string Role { get; set; } = string.Empty;
    }

    /// <summary>
    /// 更新用户状态DTO
    /// </summary>
    public class UpdateUserStatusDto
    {
        /// <summary>
        /// 状态
        /// </summary>
        [Required(ErrorMessage = "状态不能为空")]
        [RegularExpression("^(active|inactive|banned)$", ErrorMessage = "状态只能是active、inactive或banned")]
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// 管理员统计信息DTO
    /// </summary>
    public class AdminStatsDto
    {
        /// <summary>
        /// 总用户数
        /// </summary>
        public int TotalUsers { get; set; }

        /// <summary>
        /// 活跃用户数
        /// </summary>
        public int ActiveUsers { get; set; }

        /// <summary>
        /// 管理员用户数
        /// </summary>
        public int AdminUsers { get; set; }

        /// <summary>
        /// 总任务数
        /// </summary>
        public int TotalTasks { get; set; }

        /// <summary>
        /// 已完成任务数
        /// </summary>
        public int CompletedTasks { get; set; }

        /// <summary>
        /// 今日注册用户数
        /// </summary>
        public int TodayRegistrations { get; set; }

        /// <summary>
        /// 本周注册用户数
        /// </summary>
        public int WeekRegistrations { get; set; }

        /// <summary>
        /// 本月注册用户数
        /// </summary>
        public int MonthRegistrations { get; set; }

        /// <summary>
        /// 邀请码统计信息
        /// </summary>
        public InvitationCodeStatsDto? InvitationCodeStats { get; set; }
    }

    /// <summary>
    /// 用户查询DTO
    /// </summary>
    public class AdminUserQueryDto
    {
        /// <summary>
        /// 页码
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")]
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每页数量
        /// </summary>
        [Range(1, 100, ErrorMessage = "每页数量必须在1-100之间")]
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// 搜索关键词
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// 角色筛选
        /// </summary>
        public string? Role { get; set; }

        /// <summary>
        /// 状态筛选
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// 排序字段
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// 排序方向
        /// </summary>
        public string? SortDirection { get; set; } = "desc";
    }

    /// <summary>
    /// 批量操作DTO
    /// </summary>
    public class BatchOperationDto
    {
        /// <summary>
        /// 用户ID列表
        /// </summary>
        [Required(ErrorMessage = "用户ID列表不能为空")]
        [MinLength(1, ErrorMessage = "至少选择一个用户")]
        public List<Guid> UserIds { get; set; } = new List<Guid>();

        /// <summary>
        /// 操作类型
        /// </summary>
        [Required(ErrorMessage = "操作类型不能为空")]
        [RegularExpression("^(activate|deactivate|ban|unban|delete)$", ErrorMessage = "无效的操作类型")]
        public string Operation { get; set; } = string.Empty;
    }

    /// <summary>
    /// 系统日志DTO
    /// </summary>
    public class SystemLogDto
    {
        /// <summary>
        /// 日志ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 日志级别
        /// </summary>
        public string Level { get; set; } = string.Empty;

        /// <summary>
        /// 日志消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 异常信息
        /// </summary>
        public string? Exception { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// 用户代理
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
