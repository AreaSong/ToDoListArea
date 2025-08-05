using System.ComponentModel.DataAnnotations;

namespace ToDoListArea.Models
{
    /// <summary>
    /// 用户活动DTO
    /// </summary>
    public class UserActivityDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ActivityType { get; set; } = null!;
        public string? ActivityDescription { get; set; }
        public string? EntityType { get; set; }
        public Guid? EntityId { get; set; }
        public string? Metadata { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// 创建用户活动请求DTO
    /// </summary>
    public class CreateUserActivityDto
    {
        [Required(ErrorMessage = "用户ID不能为空")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "活动类型不能为空")]
        [StringLength(50, ErrorMessage = "活动类型长度不能超过50个字符")]
        public string ActivityType { get; set; } = null!;

        [StringLength(500, ErrorMessage = "活动描述长度不能超过500个字符")]
        public string? ActivityDescription { get; set; }

        [StringLength(50, ErrorMessage = "实体类型长度不能超过50个字符")]
        public string? EntityType { get; set; }

        public Guid? EntityId { get; set; }

        [StringLength(2000, ErrorMessage = "元数据长度不能超过2000个字符")]
        public string? Metadata { get; set; }
    }

    /// <summary>
    /// 用户活动统计DTO
    /// </summary>
    public class UserActivityStatsDto
    {
        public int TotalActivities { get; set; }
        public int TodayActivities { get; set; }
        public DateTime? LastActivityTime { get; set; }
        public List<ActivityTypeCount> ActivityTypeStats { get; set; } = new();
    }

    /// <summary>
    /// 活动类型统计
    /// </summary>
    public class ActivityTypeCount
    {
        public string ActivityType { get; set; } = null!;
        public int Count { get; set; }
    }

    /// <summary>
    /// 用户活动查询参数DTO
    /// </summary>
    public class UserActivityQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? ActivityType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// 活动类型常量
    /// </summary>
    public static class ActivityTypes
    {
        public const string Login = "login";
        public const string Logout = "logout";
        public const string CreateTask = "create_task";
        public const string UpdateTask = "update_task";
        public const string DeleteTask = "delete_task";
        public const string ViewTask = "view_task";
        public const string CreateTaskDetail = "create_task_detail";
        public const string UpdateTaskDetail = "update_task_detail";
        public const string DeleteTaskDetail = "delete_task_detail";
        public const string CreateTemplate = "create_template";
        public const string UpdateTemplate = "update_template";
        public const string DeleteTemplate = "delete_template";
        public const string UseTemplate = "use_template";
        public const string UpdateProfile = "update_profile";
        public const string ViewDashboard = "view_dashboard";
        public const string ViewGantt = "view_gantt";
        public const string ViewTemplates = "view_templates";
        public const string ViewProfile = "view_profile";
        public const string Search = "search";
        public const string Export = "export";
        public const string Import = "import";
    }

    /// <summary>
    /// 实体类型常量
    /// </summary>
    public static class EntityTypes
    {
        public const string Task = "task";
        public const string TaskDetail = "task_detail";
        public const string TaskTemplate = "task_template";
        public const string TaskCategory = "task_category";
        public const string User = "user";
        public const string UserProfile = "user_profile";
        public const string GanttData = "gantt_data";
    }
}
