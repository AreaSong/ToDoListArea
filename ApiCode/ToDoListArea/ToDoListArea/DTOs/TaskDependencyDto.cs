using System.ComponentModel.DataAnnotations;

namespace ToDoListArea.DTOs
{
    /// <summary>
    /// 任务依赖关系DTO
    /// </summary>
    public class TaskDependencyDto
    {
        /// <summary>
        /// 依赖关系ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 任务ID
        /// </summary>
        public Guid TaskId { get; set; }

        /// <summary>
        /// 任务标题
        /// </summary>
        public string? TaskTitle { get; set; }

        /// <summary>
        /// 依赖任务ID
        /// </summary>
        public Guid DependsOnTaskId { get; set; }

        /// <summary>
        /// 依赖任务标题
        /// </summary>
        public string? DependsOnTaskTitle { get; set; }

        /// <summary>
        /// 依赖类型
        /// </summary>
        public string DependencyType { get; set; } = "finish_to_start";

        /// <summary>
        /// 延迟时间（分钟）
        /// </summary>
        public int LagTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// 创建任务依赖关系DTO
    /// </summary>
    public class CreateTaskDependencyDto
    {
        /// <summary>
        /// 依赖任务ID
        /// </summary>
        [Required(ErrorMessage = "依赖任务ID不能为空")]
        public Guid DependsOnTaskId { get; set; }

        /// <summary>
        /// 延迟时间（分钟）
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "延迟时间不能为负数")]
        public int? LagTime { get; set; }
    }

    /// <summary>
    /// 任务冲突检查DTO
    /// </summary>
    public class TaskConflictCheckDto
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public Guid TaskId { get; set; }

        /// <summary>
        /// 是否有冲突
        /// </summary>
        public bool HasConflicts { get; set; }

        /// <summary>
        /// 冲突列表
        /// </summary>
        public List<string> Conflicts { get; set; } = new List<string>();

        /// <summary>
        /// 检查时间
        /// </summary>
        public DateTime CheckedAt { get; set; }
    }

    /// <summary>
    /// 任务依赖关系摘要DTO
    /// </summary>
    public class TaskDependencySummaryDto
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public Guid TaskId { get; set; }

        /// <summary>
        /// 任务标题
        /// </summary>
        public string TaskTitle { get; set; } = string.Empty;

        /// <summary>
        /// 依赖的任务数量
        /// </summary>
        public int DependenciesCount { get; set; }

        /// <summary>
        /// 被依赖的任务数量
        /// </summary>
        public int DependentsCount { get; set; }

        /// <summary>
        /// 是否有循环依赖
        /// </summary>
        public bool HasCircularDependency { get; set; }

        /// <summary>
        /// 是否有时间冲突
        /// </summary>
        public bool HasTimeConflict { get; set; }
    }
}
