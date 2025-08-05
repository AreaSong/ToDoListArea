using System.ComponentModel.DataAnnotations;

namespace ToDoListArea.Models
{
    /// <summary>
    /// 任务模板DTO
    /// </summary>
    public class TaskTemplateDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string TemplateData { get; set; } = string.Empty;
        public string? Category { get; set; }
        public bool IsPublic { get; set; }
        public int UsageCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 创建任务模板DTO
    /// </summary>
    public class TaskTemplateCreateDto
    {
        [Required(ErrorMessage = "用户ID不能为空")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "模板名称不能为空")]
        [StringLength(100, ErrorMessage = "模板名称长度不能超过100个字符")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "模板描述长度不能超过500个字符")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "模板数据不能为空")]
        public string TemplateData { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "分类名称长度不能超过50个字符")]
        public string? Category { get; set; }
    }

    /// <summary>
    /// 更新任务模板DTO
    /// </summary>
    public class TaskTemplateUpdateDto
    {
        [StringLength(100, ErrorMessage = "模板名称长度不能超过100个字符")]
        public string? Name { get; set; }

        [StringLength(500, ErrorMessage = "模板描述长度不能超过500个字符")]
        public string? Description { get; set; }

        public string? TemplateData { get; set; }

        [StringLength(50, ErrorMessage = "分类名称长度不能超过50个字符")]
        public string? Category { get; set; }
    }

    /// <summary>
    /// 从模板创建任务DTO
    /// </summary>
    public class CreateTaskFromTemplateDto
    {
        [Required(ErrorMessage = "用户ID不能为空")]
        public Guid UserId { get; set; }

        [StringLength(200, ErrorMessage = "自定义标题长度不能超过200个字符")]
        public string? CustomTitle { get; set; }

        [StringLength(1000, ErrorMessage = "自定义描述长度不能超过1000个字符")]
        public string? CustomDescription { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

    /// <summary>
    /// 模板使用统计DTO
    /// </summary>
    public class TemplateUsageStatsDto
    {
        /// <summary>
        /// 总模板数量
        /// </summary>
        public int TotalTemplates { get; set; }

        /// <summary>
        /// 总使用次数
        /// </summary>
        public int TotalUsage { get; set; }

        /// <summary>
        /// 最常用的模板名称
        /// </summary>
        public string? MostUsedTemplate { get; set; }

        /// <summary>
        /// 平均使用次数
        /// </summary>
        public double AverageUsage { get; set; }

        /// <summary>
        /// 分类数量
        /// </summary>
        public int CategoriesCount { get; set; }

        /// <summary>
        /// 最近创建的模板
        /// </summary>
        public List<RecentTemplateDto> RecentlyCreated { get; set; } = new();
    }

    /// <summary>
    /// 最近模板DTO
    /// </summary>
    public class RecentTemplateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int UsageCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// 模板数据结构（用于JSON序列化）
    /// </summary>
    public class TaskTemplateData
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Priority { get; set; } = "Medium";
        public Guid? CategoryId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<string> Tags { get; set; } = new();
        public Dictionary<string, object> CustomFields { get; set; } = new();
    }
}
