using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using DbContextHelp.Models;
using ToDoListArea.Models;
using System.Text.Json;

namespace ToDoListArea.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 所有接口都需要JWT认证
    public class TaskDetailsController : ControllerBase
    {
        private readonly ToDoListAreaDbContext _context;
        private readonly ILogger<TaskDetailsController> _logger;

        public TaskDetailsController(ToDoListAreaDbContext context, ILogger<TaskDetailsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 获取任务的所有详情
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="detailType">详情类型筛选（可选）</param>
        /// <returns>任务详情列表</returns>
        [HttpGet("task/{taskId}")]
        public async Task<ActionResult<ApiResponse<List<TaskDetailDto>>>> GetTaskDetails(
            Guid taskId, 
            [FromQuery] string? detailType = null)
        {
            try
            {
                var query = _context.TaskDetails
                    .Where(td => td.TaskId == taskId);

                // 详情类型筛选
                if (!string.IsNullOrEmpty(detailType))
                {
                    query = query.Where(td => td.DetailType == detailType);
                }

                var details = await query
                    .OrderBy(td => td.SortOrder)
                    .ThenBy(td => td.CreatedAt)
                    .Select(td => new TaskDetailDto
                    {
                        Id = td.Id,
                        TaskId = td.TaskId,
                        DetailType = td.DetailType,
                        DetailKey = td.DetailKey,
                        DetailValue = td.DetailValue,
                        SortOrder = td.SortOrder,
                        CreatedAt = td.CreatedAt,
                        UpdatedAt = td.UpdatedAt
                    })
                    .ToListAsync();

                _logger.LogInformation("获取任务详情成功，任务ID: {TaskId}, 详情数量: {Count}", taskId, details.Count);
                return Ok(ApiResponse<List<TaskDetailDto>>.SuccessResult(details, $"获取到 {details.Count} 个任务详情"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取任务详情失败，任务ID: {TaskId}", taskId);
                return StatusCode(500, ApiResponse<List<TaskDetailDto>>.ErrorResult("获取任务详情失败"));
            }
        }

        /// <summary>
        /// 获取任务的检查清单
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>检查清单</returns>
        [HttpGet("task/{taskId}/checklist")]
        public async Task<ActionResult<ApiResponse<List<ChecklistItemDto>>>> GetTaskChecklist(Guid taskId)
        {
            try
            {
                var checklistDetails = await _context.TaskDetails
                    .Where(td => td.TaskId == taskId && td.DetailType == "checklist")
                    .OrderBy(td => td.SortOrder)
                    .ToListAsync();

                var checklist = checklistDetails.Select(td => new ChecklistItemDto
                {
                    Id = td.Id,
                    TaskId = td.TaskId,
                    Title = td.DetailKey,
                    IsCompleted = td.DetailValue == "true",
                    SortOrder = td.SortOrder,
                    CreatedAt = td.CreatedAt,
                    UpdatedAt = td.UpdatedAt
                }).ToList();

                _logger.LogInformation("获取任务检查清单成功，任务ID: {TaskId}, 项目数量: {Count}", taskId, checklist.Count);
                return Ok(ApiResponse<List<ChecklistItemDto>>.SuccessResult(checklist, $"获取到 {checklist.Count} 个检查项"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取任务检查清单失败，任务ID: {TaskId}", taskId);
                return StatusCode(500, ApiResponse<List<ChecklistItemDto>>.ErrorResult("获取任务检查清单失败"));
            }
        }

        /// <summary>
        /// 获取任务的笔记和评论
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>笔记和评论列表</returns>
        [HttpGet("task/{taskId}/notes")]
        public async Task<ActionResult<ApiResponse<List<TaskNoteDto>>>> GetTaskNotes(Guid taskId)
        {
            try
            {
                var noteDetails = await _context.TaskDetails
                    .Where(td => td.TaskId == taskId && (td.DetailType == "note" || td.DetailType == "comment"))
                    .OrderByDescending(td => td.CreatedAt)
                    .ToListAsync();

                var notes = noteDetails.Select(td => new TaskNoteDto
                {
                    Id = td.Id,
                    TaskId = td.TaskId,
                    NoteType = td.DetailType,
                    Title = td.DetailKey,
                    Content = td.DetailValue ?? "",
                    CreatedAt = td.CreatedAt,
                    UpdatedAt = td.UpdatedAt
                }).ToList();

                _logger.LogInformation("获取任务笔记成功，任务ID: {TaskId}, 笔记数量: {Count}", taskId, notes.Count);
                return Ok(ApiResponse<List<TaskNoteDto>>.SuccessResult(notes, $"获取到 {notes.Count} 个笔记"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取任务笔记失败，任务ID: {TaskId}", taskId);
                return StatusCode(500, ApiResponse<List<TaskNoteDto>>.ErrorResult("获取任务笔记失败"));
            }
        }

        /// <summary>
        /// 获取任务的链接和引用
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>链接列表</returns>
        [HttpGet("task/{taskId}/links")]
        public async Task<ActionResult<ApiResponse<List<TaskLinkDto>>>> GetTaskLinks(Guid taskId)
        {
            try
            {
                var linkDetails = await _context.TaskDetails
                    .Where(td => td.TaskId == taskId && td.DetailType == "link")
                    .OrderBy(td => td.SortOrder)
                    .ThenBy(td => td.CreatedAt)
                    .ToListAsync();

                var links = linkDetails.Select(td => new TaskLinkDto
                {
                    Id = td.Id,
                    TaskId = td.TaskId,
                    Title = td.DetailKey,
                    Url = td.DetailValue ?? "",
                    SortOrder = td.SortOrder,
                    CreatedAt = td.CreatedAt,
                    UpdatedAt = td.UpdatedAt
                }).ToList();

                _logger.LogInformation("获取任务链接成功，任务ID: {TaskId}, 链接数量: {Count}", taskId, links.Count);
                return Ok(ApiResponse<List<TaskLinkDto>>.SuccessResult(links, $"获取到 {links.Count} 个链接"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取任务链接失败，任务ID: {TaskId}", taskId);
                return StatusCode(500, ApiResponse<List<TaskLinkDto>>.ErrorResult("获取任务链接失败"));
            }
        }

        /// <summary>
        /// 添加检查清单项
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="createDto">检查清单项数据</param>
        /// <returns>创建的检查清单项</returns>
        [HttpPost("task/{taskId}/checklist")]
        public async Task<ActionResult<ApiResponse<ChecklistItemDto>>> AddChecklistItem(
            Guid taskId, [FromBody] CreateChecklistItemDto createDto)
        {
            try
            {
                // 验证任务是否存在
                var taskExists = await _context.Tasks.AnyAsync(t => t.Id == taskId);
                if (!taskExists)
                {
                    return NotFound(ApiResponse<ChecklistItemDto>.ErrorResult($"任务不存在，ID: {taskId}"));
                }

                // 获取当前最大排序号
                var maxSortOrder = await _context.TaskDetails
                    .Where(td => td.TaskId == taskId && td.DetailType == "checklist")
                    .MaxAsync(td => (int?)td.SortOrder) ?? 0;

                var taskDetail = new TaskDetail
                {
                    TaskId = taskId,
                    DetailType = "checklist",
                    DetailKey = createDto.Title,
                    DetailValue = "false", // 默认未完成
                    SortOrder = maxSortOrder + 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.TaskDetails.Add(taskDetail);
                await _context.SaveChangesAsync();

                var checklistItem = new ChecklistItemDto
                {
                    Id = taskDetail.Id,
                    TaskId = taskDetail.TaskId,
                    Title = taskDetail.DetailKey,
                    IsCompleted = false,
                    SortOrder = taskDetail.SortOrder,
                    CreatedAt = taskDetail.CreatedAt,
                    UpdatedAt = taskDetail.UpdatedAt
                };

                _logger.LogInformation("添加检查清单项成功，任务ID: {TaskId}, 项目ID: {ItemId}", taskId, taskDetail.Id);
                return Ok(ApiResponse<ChecklistItemDto>.SuccessResult(checklistItem, "检查清单项添加成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加检查清单项失败，任务ID: {TaskId}", taskId);
                return StatusCode(500, ApiResponse<ChecklistItemDto>.ErrorResult("添加检查清单项失败"));
            }
        }

        /// <summary>
        /// 更新检查清单项
        /// </summary>
        /// <param name="id">检查清单项ID</param>
        /// <param name="updateDto">更新数据</param>
        /// <returns>更新结果</returns>
        [HttpPut("checklist/{id}")]
        public async Task<ActionResult<ApiResponse<ChecklistItemDto>>> UpdateChecklistItem(
            Guid id, [FromBody] UpdateChecklistItemDto updateDto)
        {
            try
            {
                var taskDetail = await _context.TaskDetails
                    .FirstOrDefaultAsync(td => td.Id == id && td.DetailType == "checklist");

                if (taskDetail == null)
                {
                    return NotFound(ApiResponse<ChecklistItemDto>.ErrorResult($"检查清单项不存在，ID: {id}"));
                }

                // 更新数据
                if (!string.IsNullOrEmpty(updateDto.Title))
                    taskDetail.DetailKey = updateDto.Title;

                if (updateDto.IsCompleted.HasValue)
                    taskDetail.DetailValue = updateDto.IsCompleted.Value ? "true" : "false";

                if (updateDto.SortOrder.HasValue)
                    taskDetail.SortOrder = updateDto.SortOrder.Value;

                taskDetail.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var checklistItem = new ChecklistItemDto
                {
                    Id = taskDetail.Id,
                    TaskId = taskDetail.TaskId,
                    Title = taskDetail.DetailKey,
                    IsCompleted = taskDetail.DetailValue == "true",
                    SortOrder = taskDetail.SortOrder,
                    CreatedAt = taskDetail.CreatedAt,
                    UpdatedAt = taskDetail.UpdatedAt
                };

                _logger.LogInformation("更新检查清单项成功，项目ID: {ItemId}", id);
                return Ok(ApiResponse<ChecklistItemDto>.SuccessResult(checklistItem, "检查清单项更新成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新检查清单项失败，项目ID: {ItemId}", id);
                return StatusCode(500, ApiResponse<ChecklistItemDto>.ErrorResult("更新检查清单项失败"));
            }
        }

        /// <summary>
        /// 添加任务笔记
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="createDto">笔记数据</param>
        /// <returns>创建的笔记</returns>
        [HttpPost("task/{taskId}/notes")]
        public async Task<ActionResult<ApiResponse<TaskNoteDto>>> AddTaskNote(
            Guid taskId, [FromBody] CreateTaskNoteDto createDto)
        {
            try
            {
                // 验证任务是否存在
                var taskExists = await _context.Tasks.AnyAsync(t => t.Id == taskId);
                if (!taskExists)
                {
                    return NotFound(ApiResponse<TaskNoteDto>.ErrorResult($"任务不存在，ID: {taskId}"));
                }

                var taskDetail = new TaskDetail
                {
                    TaskId = taskId,
                    DetailType = createDto.NoteType, // "note" 或 "comment"
                    DetailKey = createDto.Title,
                    DetailValue = createDto.Content,
                    SortOrder = 0, // 笔记不需要排序
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.TaskDetails.Add(taskDetail);
                await _context.SaveChangesAsync();

                var taskNote = new TaskNoteDto
                {
                    Id = taskDetail.Id,
                    TaskId = taskDetail.TaskId,
                    NoteType = taskDetail.DetailType,
                    Title = taskDetail.DetailKey,
                    Content = taskDetail.DetailValue ?? "",
                    CreatedAt = taskDetail.CreatedAt,
                    UpdatedAt = taskDetail.UpdatedAt
                };

                _logger.LogInformation("添加任务笔记成功，任务ID: {TaskId}, 笔记ID: {NoteId}", taskId, taskDetail.Id);
                return Ok(ApiResponse<TaskNoteDto>.SuccessResult(taskNote, "任务笔记添加成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加任务笔记失败，任务ID: {TaskId}", taskId);
                return StatusCode(500, ApiResponse<TaskNoteDto>.ErrorResult("添加任务笔记失败"));
            }
        }

        /// <summary>
        /// 添加任务链接
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="createDto">链接数据</param>
        /// <returns>创建的链接</returns>
        [HttpPost("task/{taskId}/links")]
        public async Task<ActionResult<ApiResponse<TaskLinkDto>>> AddTaskLink(
            Guid taskId, [FromBody] CreateTaskLinkDto createDto)
        {
            try
            {
                // 验证任务是否存在
                var taskExists = await _context.Tasks.AnyAsync(t => t.Id == taskId);
                if (!taskExists)
                {
                    return NotFound(ApiResponse<TaskLinkDto>.ErrorResult($"任务不存在，ID: {taskId}"));
                }

                // 获取当前最大排序号
                var maxSortOrder = await _context.TaskDetails
                    .Where(td => td.TaskId == taskId && td.DetailType == "link")
                    .MaxAsync(td => (int?)td.SortOrder) ?? 0;

                var taskDetail = new TaskDetail
                {
                    TaskId = taskId,
                    DetailType = "link",
                    DetailKey = createDto.Title,
                    DetailValue = createDto.Url,
                    SortOrder = maxSortOrder + 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.TaskDetails.Add(taskDetail);
                await _context.SaveChangesAsync();

                var taskLink = new TaskLinkDto
                {
                    Id = taskDetail.Id,
                    TaskId = taskDetail.TaskId,
                    Title = taskDetail.DetailKey,
                    Url = taskDetail.DetailValue ?? "",
                    SortOrder = taskDetail.SortOrder,
                    CreatedAt = taskDetail.CreatedAt,
                    UpdatedAt = taskDetail.UpdatedAt
                };

                _logger.LogInformation("添加任务链接成功，任务ID: {TaskId}, 链接ID: {LinkId}", taskId, taskDetail.Id);
                return Ok(ApiResponse<TaskLinkDto>.SuccessResult(taskLink, "任务链接添加成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加任务链接失败，任务ID: {TaskId}", taskId);
                return StatusCode(500, ApiResponse<TaskLinkDto>.ErrorResult("添加任务链接失败"));
            }
        }

        /// <summary>
        /// 删除任务详情项
        /// </summary>
        /// <param name="id">详情项ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteTaskDetail(Guid id)
        {
            try
            {
                var taskDetail = await _context.TaskDetails.FindAsync(id);
                if (taskDetail == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult($"任务详情项不存在，ID: {id}"));
                }

                _context.TaskDetails.Remove(taskDetail);
                await _context.SaveChangesAsync();

                _logger.LogInformation("删除任务详情项成功，详情项ID: {DetailId}", id);
                return Ok(ApiResponse<object>.SuccessResult(null, "任务详情项删除成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除任务详情项失败，详情项ID: {DetailId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResult("删除任务详情项失败"));
            }
        }

        /// <summary>
        /// 获取任务详情统计
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>任务详情统计</returns>
        [HttpGet("task/{taskId}/stats")]
        public async Task<ActionResult<ApiResponse<TaskDetailsStatsDto>>> GetTaskDetailsStats(Guid taskId)
        {
            try
            {
                var details = await _context.TaskDetails
                    .Where(td => td.TaskId == taskId)
                    .ToListAsync();

                var checklistItems = details.Where(d => d.DetailType == "checklist").ToList();
                var notes = details.Where(d => d.DetailType == "note").ToList();
                var comments = details.Where(d => d.DetailType == "comment").ToList();
                var links = details.Where(d => d.DetailType == "link").ToList();

                var stats = new TaskDetailsStatsDto
                {
                    TaskId = taskId,
                    ChecklistTotal = checklistItems.Count,
                    ChecklistCompleted = checklistItems.Count(item => item.DetailValue == "true"),
                    ChecklistCompletionRate = checklistItems.Count > 0
                        ? (double)checklistItems.Count(item => item.DetailValue == "true") / checklistItems.Count * 100
                        : 0,
                    NotesCount = notes.Count,
                    CommentsCount = comments.Count,
                    LinksCount = links.Count,
                    LastUpdated = details.Any() ? details.Max(d => d.UpdatedAt) : null
                };

                _logger.LogInformation("获取任务详情统计成功，任务ID: {TaskId}", taskId);
                return Ok(ApiResponse<TaskDetailsStatsDto>.SuccessResult(stats, "获取任务详情统计成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取任务详情统计失败，任务ID: {TaskId}", taskId);
                return StatusCode(500, ApiResponse<TaskDetailsStatsDto>.ErrorResult("获取任务详情统计失败"));
            }
        }

        /// <summary>
        /// 批量更新检查清单项状态
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="isCompleted">完成状态</param>
        /// <returns>批量操作结果</returns>
        [HttpPut("task/{taskId}/checklist/batch-status")]
        public async Task<ActionResult<ApiResponse<BatchOperationResultDto>>> BatchUpdateChecklistStatus(
            Guid taskId, [FromQuery] bool isCompleted)
        {
            try
            {
                var checklistItems = await _context.TaskDetails
                    .Where(td => td.TaskId == taskId && td.DetailType == "checklist")
                    .ToListAsync();

                var result = new BatchOperationResultDto
                {
                    TotalCount = checklistItems.Count
                };

                foreach (var item in checklistItems)
                {
                    try
                    {
                        item.DetailValue = isCompleted ? "true" : "false";
                        item.UpdatedAt = DateTime.UtcNow;
                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailureCount++;
                        result.Errors.Add($"更新项目 {item.DetailKey} 失败: {ex.Message}");
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("批量更新检查清单状态成功，任务ID: {TaskId}, 成功: {Success}, 失败: {Failure}",
                    taskId, result.SuccessCount, result.FailureCount);

                return Ok(ApiResponse<BatchOperationResultDto>.SuccessResult(result,
                    $"批量操作完成，成功: {result.SuccessCount}, 失败: {result.FailureCount}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量更新检查清单状态失败，任务ID: {TaskId}", taskId);
                return StatusCode(500, ApiResponse<BatchOperationResultDto>.ErrorResult("批量更新检查清单状态失败"));
            }
        }
    }
}
