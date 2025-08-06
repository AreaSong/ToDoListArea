using Microsoft.EntityFrameworkCore;
using ToDoListArea.Enums;
using DbContextHelp.Models;
using DbContextHelp;

namespace ToDoListArea.Services
{
    /// <summary>
    /// 数据一致性检查和修复服务
    /// </summary>
    public class DataConsistencyService
    {
        private readonly ToDoListAreaDbContext _context;
        private readonly ILogger<DataConsistencyService> _logger;

        public DataConsistencyService(ToDoListAreaDbContext context, ILogger<DataConsistencyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 检查数据一致性问题
        /// </summary>
        public async Task<DataConsistencyReport> CheckDataConsistencyAsync()
        {
            var report = new DataConsistencyReport();

            // 检查Status字段
            var invalidStatusTasks = await _context.Tasks
                .Where(t => !Enum.GetNames<TodoTaskStatus>().Contains(t.Status))
                .Select(t => new { t.Id, t.Title, t.Status })
                .ToListAsync();

            report.InvalidStatusTasks = invalidStatusTasks.Select(t => new InvalidDataItem
            {
                Id = t.Id.ToString(),
                Title = t.Title,
                CurrentValue = t.Status,
                FieldName = "Status"
            }).ToList();

            // 检查Priority字段
            var invalidPriorityTasks = await _context.Tasks
                .Where(t => !Enum.GetNames<TodoTaskPriority>().Contains(t.Priority))
                .Select(t => new { t.Id, t.Title, t.Priority })
                .ToListAsync();

            report.InvalidPriorityTasks = invalidPriorityTasks.Select(t => new InvalidDataItem
            {
                Id = t.Id.ToString(),
                Title = t.Title,
                CurrentValue = t.Priority,
                FieldName = "Priority"
            }).ToList();

            // 统计信息
            report.TotalTasks = await _context.Tasks.CountAsync();
            report.InvalidStatusCount = report.InvalidStatusTasks.Count;
            report.InvalidPriorityCount = report.InvalidPriorityTasks.Count;

            // 数据一致性检查完成

            return report;
        }

        /// <summary>
        /// 修复数据一致性问题
        /// </summary>
        public async Task<DataFixResult> FixDataConsistencyAsync()
        {
            var result = new DataFixResult();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 修复Status字段
                var tasksWithInvalidStatus = await _context.Tasks
                    .Where(t => !Enum.GetNames<TodoTaskStatus>().Contains(t.Status))
                    .ToListAsync();

                foreach (var task in tasksWithInvalidStatus)
                {
                    var oldValue = task.Status;
                    task.Status = NormalizeStatus(task.Status);
                    task.UpdatedAt = DateTime.UtcNow;

                    result.StatusFixes.Add(new DataFix
                    {
                        TaskId = task.Id.ToString(),
                        TaskTitle = task.Title,
                        OldValue = oldValue,
                        NewValue = task.Status
                    });
                }

                // 修复Priority字段
                var tasksWithInvalidPriority = await _context.Tasks
                    .Where(t => !Enum.GetNames<TodoTaskPriority>().Contains(t.Priority))
                    .ToListAsync();

                foreach (var task in tasksWithInvalidPriority)
                {
                    var oldValue = task.Priority;
                    task.Priority = NormalizePriority(task.Priority);
                    task.UpdatedAt = DateTime.UtcNow;

                    result.PriorityFixes.Add(new DataFix
                    {
                        TaskId = task.Id.ToString(),
                        TaskTitle = task.Title,
                        OldValue = oldValue,
                        NewValue = task.Priority
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                result.Success = true;
                result.TotalFixed = result.StatusFixes.Count + result.PriorityFixes.Count;

                // 数据一致性修复完成
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result.Success = false;
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "数据一致性修复失败");
            }

            return result;
        }

        /// <summary>
        /// 标准化状态值
        /// </summary>
        private string NormalizeStatus(string status)
        {
            return status?.ToLower() switch
            {
                "pending" or "待处理" or "待办" or "未开始" or "todo" => TodoTaskStatus.Pending.ToString(),
                "inprogress" or "in_progress" or "进行中" or "正在进行" or "执行中" or "doing" or "active" => TodoTaskStatus.InProgress.ToString(),
                "completed" or "已完成" or "完成" or "已结束" or "done" or "finished" or "closed" => TodoTaskStatus.Completed.ToString(),
                _ => TodoTaskStatus.Pending.ToString() // 默认值
            };
        }

        /// <summary>
        /// 标准化优先级值
        /// </summary>
        private string NormalizePriority(string priority)
        {
            return priority?.ToLower() switch
            {
                "low" or "低" or "低优先级" or "minor" => TodoTaskPriority.Low.ToString(),
                "medium" or "中" or "中等" or "普通" or "normal" or "standard" => TodoTaskPriority.Medium.ToString(),
                "high" or "高" or "高优先级" or "紧急" or "urgent" or "critical" or "important" => TodoTaskPriority.High.ToString(),
                _ => TodoTaskPriority.Medium.ToString() // 默认值
            };
        }
    }

    /// <summary>
    /// 数据一致性检查报告
    /// </summary>
    public class DataConsistencyReport
    {
        public int TotalTasks { get; set; }
        public int InvalidStatusCount { get; set; }
        public int InvalidPriorityCount { get; set; }
        public List<InvalidDataItem> InvalidStatusTasks { get; set; } = new();
        public List<InvalidDataItem> InvalidPriorityTasks { get; set; } = new();
    }

    /// <summary>
    /// 无效数据项
    /// </summary>
    public class InvalidDataItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string CurrentValue { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
    }

    /// <summary>
    /// 数据修复结果
    /// </summary>
    public class DataFixResult
    {
        public bool Success { get; set; }
        public int TotalFixed { get; set; }
        public string? ErrorMessage { get; set; }
        public List<DataFix> StatusFixes { get; set; } = new();
        public List<DataFix> PriorityFixes { get; set; } = new();
    }

    /// <summary>
    /// 数据修复记录
    /// </summary>
    public class DataFix
    {
        public string TaskId { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
    }
}
