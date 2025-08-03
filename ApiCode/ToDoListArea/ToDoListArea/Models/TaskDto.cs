using System.ComponentModel.DataAnnotations;
using ToDoListArea.Enums;
using ToDoListArea.Attributes;

namespace ToDoListArea.Models
{
    /// <summary>
    /// 任务创建请求模型
    /// </summary>
    public class TaskCreateDto
    {
        [Required(ErrorMessage = "任务标题不能为空")]
        [StringLength(200, ErrorMessage = "任务标题长度不能超过200个字符")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "任务描述长度不能超过1000个字符")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "任务状态不能为空")]
        [ValidTaskStatus]
        public string Status { get; set; } = "Pending";

        [Required(ErrorMessage = "任务优先级不能为空")]
        [ValidTaskPriority]
        public string Priority { get; set; } = "Medium";

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? EstimatedDuration { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? ParentTaskId { get; set; }
    }

    /// <summary>
    /// 任务更新请求模型
    /// </summary>
    public class TaskUpdateDto
    {
        [StringLength(200, ErrorMessage = "任务标题长度不能超过200个字符")]
        public string? Title { get; set; }

        [StringLength(1000, ErrorMessage = "任务描述长度不能超过1000个字符")]
        public string? Description { get; set; }

        [ValidTaskStatus]
        public string? Status { get; set; }

        [ValidTaskPriority]
        public string? Priority { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? EstimatedDuration { get; set; }
        public int? ActualDuration { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? ParentTaskId { get; set; }
    }

    /// <summary>
    /// 任务响应模型
    /// </summary>
    public class TaskResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? ParentTaskId { get; set; }
        public Guid? CategoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? EstimatedDuration { get; set; }
        public int? ActualDuration { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // 关联信息
        public string? CategoryName { get; set; }
        public string? ParentTaskTitle { get; set; }
        public List<TaskResponseDto> SubTasks { get; set; } = new();
    }

    /// <summary>
    /// 任务查询参数模型
    /// </summary>
    public class TaskQueryDto
    {
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public Guid? CategoryId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SearchKeyword { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "CreatedAt";
        public string? SortOrder { get; set; } = "desc";
    }

    /// <summary>
    /// 分页响应模型
    /// </summary>
    public class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }

        public PagedResponse(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            HasPreviousPage = pageNumber > 1;
            HasNextPage = pageNumber < TotalPages;
        }
    }
}
