using System.ComponentModel.DataAnnotations;

namespace ToDoListArea.Models
{
    /// <summary>
    /// 任务详情DTO
    /// </summary>
    public class TaskDetailDto
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public string DetailType { get; set; } = string.Empty;
        public string DetailKey { get; set; } = string.Empty;
        public string? DetailValue { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// 检查清单项DTO
    /// </summary>
    public class ChecklistItemDto
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// 创建检查清单项DTO
    /// </summary>
    public class CreateChecklistItemDto
    {
        [Required(ErrorMessage = "检查项标题不能为空")]
        [StringLength(200, ErrorMessage = "检查项标题长度不能超过200个字符")]
        public string Title { get; set; } = string.Empty;
    }

    /// <summary>
    /// 更新检查清单项DTO
    /// </summary>
    public class UpdateChecklistItemDto
    {
        [StringLength(200, ErrorMessage = "检查项标题长度不能超过200个字符")]
        public string? Title { get; set; }

        public bool? IsCompleted { get; set; }

        public int? SortOrder { get; set; }
    }

    /// <summary>
    /// 任务笔记DTO
    /// </summary>
    public class TaskNoteDto
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public string NoteType { get; set; } = string.Empty; // "note" 或 "comment"
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// 创建任务笔记DTO
    /// </summary>
    public class CreateTaskNoteDto
    {
        [Required(ErrorMessage = "笔记类型不能为空")]
        [RegularExpression("^(note|comment)$", ErrorMessage = "笔记类型只能是note或comment")]
        public string NoteType { get; set; } = "note";

        [Required(ErrorMessage = "笔记标题不能为空")]
        [StringLength(200, ErrorMessage = "笔记标题长度不能超过200个字符")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "笔记内容不能为空")]
        [StringLength(5000, ErrorMessage = "笔记内容长度不能超过5000个字符")]
        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// 更新任务笔记DTO
    /// </summary>
    public class UpdateTaskNoteDto
    {
        [StringLength(200, ErrorMessage = "笔记标题长度不能超过200个字符")]
        public string? Title { get; set; }

        [StringLength(5000, ErrorMessage = "笔记内容长度不能超过5000个字符")]
        public string? Content { get; set; }
    }

    /// <summary>
    /// 任务链接DTO
    /// </summary>
    public class TaskLinkDto
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// 创建任务链接DTO
    /// </summary>
    public class CreateTaskLinkDto
    {
        [Required(ErrorMessage = "链接标题不能为空")]
        [StringLength(200, ErrorMessage = "链接标题长度不能超过200个字符")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "链接URL不能为空")]
        [Url(ErrorMessage = "请输入有效的URL地址")]
        [StringLength(2000, ErrorMessage = "链接URL长度不能超过2000个字符")]
        public string Url { get; set; } = string.Empty;
    }

    /// <summary>
    /// 更新任务链接DTO
    /// </summary>
    public class UpdateTaskLinkDto
    {
        [StringLength(200, ErrorMessage = "链接标题长度不能超过200个字符")]
        public string? Title { get; set; }

        [Url(ErrorMessage = "请输入有效的URL地址")]
        [StringLength(2000, ErrorMessage = "链接URL长度不能超过2000个字符")]
        public string? Url { get; set; }

        public int? SortOrder { get; set; }
    }

    /// <summary>
    /// 任务详情统计DTO
    /// </summary>
    public class TaskDetailsStatsDto
    {
        public Guid TaskId { get; set; }
        public int ChecklistTotal { get; set; }
        public int ChecklistCompleted { get; set; }
        public double ChecklistCompletionRate { get; set; }
        public int NotesCount { get; set; }
        public int CommentsCount { get; set; }
        public int LinksCount { get; set; }
        public DateTime? LastUpdated { get; set; }
    }

    /// <summary>
    /// 批量操作结果DTO
    /// </summary>
    public class BatchOperationResultDto
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public int TotalCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public double SuccessRate => TotalCount > 0 ? (double)SuccessCount / TotalCount * 100 : 0;
    }
}
