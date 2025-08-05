using System.ComponentModel.DataAnnotations;

namespace ToDoListArea.DTOs
{
    /// <summary>
    /// 邀请码创建请求DTO
    /// </summary>
    public class CreateInvitationCodeDto
    {
        /// <summary>
        /// 邀请码（可选，不提供则自动生成）
        /// </summary>
        [StringLength(32, MinimumLength = 6, ErrorMessage = "邀请码长度必须在6-32个字符之间")]
        public string? Code { get; set; }

        /// <summary>
        /// 最大使用次数
        /// </summary>
        [Range(1, 10000, ErrorMessage = "最大使用次数必须在1-10000之间")]
        public int MaxUses { get; set; } = 1;

        /// <summary>
        /// 过期时间（可选，不提供则永不过期）
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// 描述信息（可选）
        /// </summary>
        [StringLength(500, ErrorMessage = "描述信息不能超过500个字符")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// 邀请码响应DTO
    /// </summary>
    public class InvitationCodeDto
    {
        /// <summary>
        /// 邀请码ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 邀请码
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 最大使用次数
        /// </summary>
        public int MaxUses { get; set; }

        /// <summary>
        /// 已使用次数
        /// </summary>
        public int UsedCount { get; set; }

        /// <summary>
        /// 剩余使用次数
        /// </summary>
        public int RemainingUses => MaxUses - UsedCount;

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// 是否已过期
        /// </summary>
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;

        /// <summary>
        /// 是否可用
        /// </summary>
        public bool IsAvailable => Status == "active" && !IsExpired && RemainingUses > 0;

        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 创建者ID
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// 创建者姓名
        /// </summary>
        public string CreatedByName { get; set; } = string.Empty;

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
    /// 邀请码验证请求DTO
    /// </summary>
    public class ValidateInvitationCodeDto
    {
        /// <summary>
        /// 邀请码
        /// </summary>
        [Required(ErrorMessage = "邀请码不能为空")]
        [StringLength(32, MinimumLength = 6, ErrorMessage = "邀请码长度必须在6-32个字符之间")]
        public string Code { get; set; } = string.Empty;
    }

    /// <summary>
    /// 邀请码验证响应DTO
    /// </summary>
    public class InvitationCodeValidationDto
    {
        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 验证消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 邀请码信息（仅在有效时返回）
        /// </summary>
        public InvitationCodeDto? InvitationCode { get; set; }
    }

    /// <summary>
    /// 邀请码使用记录DTO
    /// </summary>
    public class InvitationCodeUsageDto
    {
        /// <summary>
        /// 记录ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 邀请码
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 使用者ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 使用者姓名
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 使用者邮箱
        /// </summary>
        public string UserEmail { get; set; } = string.Empty;

        /// <summary>
        /// 使用时间
        /// </summary>
        public DateTime UsedAt { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// 用户代理
        /// </summary>
        public string? UserAgent { get; set; }
    }

    /// <summary>
    /// 邀请码更新请求DTO
    /// </summary>
    public class UpdateInvitationCodeDto
    {
        /// <summary>
        /// 最大使用次数
        /// </summary>
        [Range(1, 10000, ErrorMessage = "最大使用次数必须在1-10000之间")]
        public int? MaxUses { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [RegularExpression("^(active|disabled)$", ErrorMessage = "状态只能是active或disabled")]
        public string? Status { get; set; }
    }

    /// <summary>
    /// 邀请码列表查询DTO
    /// </summary>
    public class InvitationCodeQueryDto
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
        /// 状态筛选
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// 创建者筛选
        /// </summary>
        public Guid? CreatedBy { get; set; }

        /// <summary>
        /// 搜索关键词（邀请码）
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// 是否包含已过期的
        /// </summary>
        public bool IncludeExpired { get; set; } = true;
    }

    /// <summary>
    /// 分页响应DTO
    /// </summary>
    public class PagedResultDto<T>
    {
        /// <summary>
        /// 数据列表
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// 总数量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 当前页码
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool HasNextPage => Page < TotalPages;

        /// <summary>
        /// 是否有上一页
        /// </summary>
        public bool HasPreviousPage => Page > 1;
    }

    /// <summary>
    /// 邀请码统计信息DTO
    /// </summary>
    public class InvitationCodeStatsDto
    {
        /// <summary>
        /// 总邀请码数量
        /// </summary>
        public int TotalCodes { get; set; }

        /// <summary>
        /// 活跃邀请码数量
        /// </summary>
        public int ActiveCodes { get; set; }

        /// <summary>
        /// 已过期邀请码数量
        /// </summary>
        public int ExpiredCodes { get; set; }

        /// <summary>
        /// 已禁用邀请码数量
        /// </summary>
        public int DisabledCodes { get; set; }

        /// <summary>
        /// 总使用次数
        /// </summary>
        public int TotalUsages { get; set; }

        /// <summary>
        /// 今日使用次数
        /// </summary>
        public int TodayUsages { get; set; }

        /// <summary>
        /// 本周使用次数
        /// </summary>
        public int WeekUsages { get; set; }

        /// <summary>
        /// 本月使用次数
        /// </summary>
        public int MonthUsages { get; set; }

        /// <summary>
        /// 总可用次数
        /// </summary>
        public int TotalMaxUses { get; set; }

        /// <summary>
        /// 使用率（百分比）
        /// </summary>
        public decimal UsageRate { get; set; }

        /// <summary>
        /// 最近7天新增邀请码数量
        /// </summary>
        public int RecentCodesCount { get; set; }

        /// <summary>
        /// 最近7天使用次数
        /// </summary>
        public int RecentUsagesCount { get; set; }

        /// <summary>
        /// 统计时间
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
