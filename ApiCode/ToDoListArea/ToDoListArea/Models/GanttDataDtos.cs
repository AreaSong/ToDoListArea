using System.ComponentModel.DataAnnotations;

namespace ToDoListArea.Models
{
    /// <summary>
    /// 甘特图数据DTO
    /// </summary>
    public class GanttDataDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TaskId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public string TaskDescription { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Progress { get; set; }
        public List<Guid> Dependencies { get; set; } = new();
        public List<string> Resources { get; set; } = new();
        public string CategoryColor { get; set; } = "#1890ff";
        public string CategoryName { get; set; } = "默认分类";
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 甘特图数据更新DTO
    /// </summary>
    public class GanttDataUpdateDto
    {
        [Required(ErrorMessage = "开始日期不能为空")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "结束日期不能为空")]
        public DateTime EndDate { get; set; }

        [Range(0, 100, ErrorMessage = "进度必须在0-100之间")]
        public decimal Progress { get; set; }

        public List<Guid>? Dependencies { get; set; }
        public List<string>? Resources { get; set; }

        /// <summary>
        /// 验证结束日期必须大于开始日期
        /// </summary>
        public bool IsValid => EndDate > StartDate;
    }

    /// <summary>
    /// 甘特图数据创建DTO
    /// </summary>
    public class GanttDataCreateDto
    {
        [Required(ErrorMessage = "任务ID不能为空")]
        public Guid TaskId { get; set; }

        [Required(ErrorMessage = "开始日期不能为空")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "结束日期不能为空")]
        public DateTime EndDate { get; set; }

        [Range(0, 100, ErrorMessage = "进度必须在0-100之间")]
        public decimal Progress { get; set; } = 0;

        public List<Guid>? Dependencies { get; set; }
        public List<string>? Resources { get; set; }

        /// <summary>
        /// 验证结束日期必须大于开始日期
        /// </summary>
        public bool IsValid => EndDate > StartDate;
    }

    /// <summary>
    /// 甘特图同步结果DTO
    /// </summary>
    public class GanttSyncResultDto
    {
        /// <summary>
        /// 新增的甘特图数据数量
        /// </summary>
        public int SyncedCount { get; set; }

        /// <summary>
        /// 更新的甘特图数据数量
        /// </summary>
        public int UpdatedCount { get; set; }

        /// <summary>
        /// 跳过的任务数量（没有时间信息）
        /// </summary>
        public int SkippedCount { get; set; }

        /// <summary>
        /// 清理的孤立数据数量
        /// </summary>
        public int CleanedCount { get; set; }

        /// <summary>
        /// 总任务数量
        /// </summary>
        public int TotalTasks { get; set; }

        /// <summary>
        /// 同步成功率
        /// </summary>
        public double SuccessRate => TotalTasks > 0 ? (double)(SyncedCount + UpdatedCount) / TotalTasks * 100 : 0;
    }

    /// <summary>
    /// 甘特图数据一致性检查结果DTO
    /// </summary>
    public class GanttConsistencyCheckDto
    {
        /// <summary>
        /// 检查是否通过
        /// </summary>
        public bool IsConsistent { get; set; }

        /// <summary>
        /// 不一致的数据项
        /// </summary>
        public List<GanttInconsistencyDto> Inconsistencies { get; set; } = new();

        /// <summary>
        /// 检查时间
        /// </summary>
        public DateTime CheckTime { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// 甘特图数据不一致项DTO
    /// </summary>
    public class GanttInconsistencyDto
    {
        public Guid GanttDataId { get; set; }
        public Guid TaskId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public string InconsistencyType { get; set; } = string.Empty; // "DATE_MISMATCH", "PROGRESS_MISMATCH", "MISSING_TASK"
        public string Description { get; set; } = string.Empty;
        public object? ExpectedValue { get; set; }
        public object? ActualValue { get; set; }
    }
}
