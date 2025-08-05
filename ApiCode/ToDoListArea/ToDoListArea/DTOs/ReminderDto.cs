using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace ToDoListArea.DTOs
{
    /// <summary>
    /// 提醒DTO
    /// </summary>
    public class ReminderDto
    {
        /// <summary>
        /// 提醒ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 关联任务ID
        /// </summary>
        public Guid? TaskId { get; set; }

        /// <summary>
        /// 关联任务标题
        /// </summary>
        public string? TaskTitle { get; set; }

        /// <summary>
        /// 提醒标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 提醒消息
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// 提醒时间
        /// </summary>
        public DateTime ReminderTime { get; set; }

        /// <summary>
        /// 提醒状态
        /// </summary>
        public string Status { get; set; } = "pending";

        /// <summary>
        /// 提醒渠道列表
        /// </summary>
        public List<string> Channels { get; set; } = new List<string> { "web" };

        /// <summary>
        /// 重复模式
        /// </summary>
        public ReminderRepeatPattern? RepeatPattern { get; set; }

        /// <summary>
        /// 延迟到时间
        /// </summary>
        public DateTime? SnoozeUntil { get; set; }

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
    /// 创建提醒DTO
    /// </summary>
    public class CreateReminderDto
    {
        /// <summary>
        /// 关联任务ID（可选）
        /// </summary>
        public Guid? TaskId { get; set; }

        /// <summary>
        /// 提醒标题
        /// </summary>
        [Required(ErrorMessage = "提醒标题不能为空")]
        [StringLength(255, ErrorMessage = "提醒标题长度不能超过255个字符")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 提醒消息
        /// </summary>
        [StringLength(1000, ErrorMessage = "提醒消息长度不能超过1000个字符")]
        public string? Message { get; set; }

        /// <summary>
        /// 提醒时间
        /// </summary>
        [Required(ErrorMessage = "提醒时间不能为空")]
        public DateTime ReminderTime { get; set; }

        /// <summary>
        /// 提醒渠道列表
        /// </summary>
        public List<string> Channels { get; set; } = new List<string> { "web" };

        /// <summary>
        /// 重复模式
        /// </summary>
        public ReminderRepeatPattern? RepeatPattern { get; set; }
    }

    /// <summary>
    /// 更新提醒DTO
    /// </summary>
    public class UpdateReminderDto
    {
        /// <summary>
        /// 提醒标题
        /// </summary>
        [StringLength(255, ErrorMessage = "提醒标题长度不能超过255个字符")]
        public string? Title { get; set; }

        /// <summary>
        /// 提醒消息
        /// </summary>
        [StringLength(1000, ErrorMessage = "提醒消息长度不能超过1000个字符")]
        public string? Message { get; set; }

        /// <summary>
        /// 提醒时间
        /// </summary>
        public DateTime? ReminderTime { get; set; }

        /// <summary>
        /// 提醒状态
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// 提醒渠道列表
        /// </summary>
        public List<string>? Channels { get; set; }

        /// <summary>
        /// 重复模式
        /// </summary>
        public ReminderRepeatPattern? RepeatPattern { get; set; }

        /// <summary>
        /// 延迟到时间
        /// </summary>
        public DateTime? SnoozeUntil { get; set; }
    }

    /// <summary>
    /// 提醒重复模式
    /// </summary>
    public class ReminderRepeatPattern
    {
        /// <summary>
        /// 重复类型：none, daily, weekly, monthly, yearly
        /// </summary>
        public string Type { get; set; } = "none";

        /// <summary>
        /// 重复间隔
        /// </summary>
        public int Interval { get; set; } = 1;

        /// <summary>
        /// 重复结束日期
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 最大重复次数
        /// </summary>
        public int? MaxOccurrences { get; set; }

        /// <summary>
        /// 星期几重复（用于weekly类型）
        /// </summary>
        public List<int>? DaysOfWeek { get; set; }

        /// <summary>
        /// 月份中的第几天（用于monthly类型）
        /// </summary>
        public int? DayOfMonth { get; set; }
    }

    /// <summary>
    /// 提醒查询DTO
    /// </summary>
    public class ReminderQueryDto
    {
        /// <summary>
        /// 提醒状态
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// 关联任务ID
        /// </summary>
        public Guid? TaskId { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 搜索关键词
        /// </summary>
        public string? SearchKeyword { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// 页面大小
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// 排序字段
        /// </summary>
        public string SortBy { get; set; } = "ReminderTime";

        /// <summary>
        /// 排序方向
        /// </summary>
        public string SortOrder { get; set; } = "asc";
    }

    /// <summary>
    /// 延迟提醒DTO
    /// </summary>
    public class SnoozeReminderDto
    {
        /// <summary>
        /// 延迟到的时间
        /// </summary>
        [Required(ErrorMessage = "延迟时间不能为空")]
        public DateTime SnoozeUntil { get; set; }
    }

    /// <summary>
    /// 提醒统计DTO
    /// </summary>
    public class ReminderStatsDto
    {
        /// <summary>
        /// 总提醒数
        /// </summary>
        public int TotalReminders { get; set; }

        /// <summary>
        /// 待处理提醒数
        /// </summary>
        public int PendingReminders { get; set; }

        /// <summary>
        /// 已完成提醒数
        /// </summary>
        public int CompletedReminders { get; set; }

        /// <summary>
        /// 已延迟提醒数
        /// </summary>
        public int SnoozedReminders { get; set; }

        /// <summary>
        /// 今日提醒数
        /// </summary>
        public int TodayReminders { get; set; }

        /// <summary>
        /// 本周提醒数
        /// </summary>
        public int WeekReminders { get; set; }
    }
}
