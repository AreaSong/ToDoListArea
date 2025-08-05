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
    public class GanttDataController : ControllerBase
    {
        private readonly ToDoListAreaDbContext _context;
        private readonly ILogger<GanttDataController> _logger;

        public GanttDataController(ToDoListAreaDbContext context, ILogger<GanttDataController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 获取用户甘特图数据
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>甘特图数据列表</returns>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<List<GanttDataDto>>>> GetGanttData(Guid userId)
        {
            try
            {
                var ganttData = await _context.GanttData
                    .Where(g => g.UserId == userId)
                    .Include(g => g.Task)
                    .ThenInclude(t => t.Category)
                    .OrderBy(g => g.StartDate)
                    .Select(g => new GanttDataDto
                    {
                        Id = g.Id,
                        UserId = g.UserId,
                        TaskId = g.TaskId,
                        TaskTitle = g.Task.Title,
                        TaskDescription = g.Task.Description,
                        StartDate = g.StartDate,
                        EndDate = g.EndDate,
                        Progress = g.Progress,
                        Dependencies = JsonSerializer.Deserialize<List<Guid>>(g.Dependencies ?? "[]", (JsonSerializerOptions?)null),
                        Resources = JsonSerializer.Deserialize<List<string>>(g.Resources ?? "[]", (JsonSerializerOptions?)null),
                        CategoryColor = g.Task.Category != null ? g.Task.Category.Color : "#1890ff",
                        CategoryName = g.Task.Category != null ? g.Task.Category.Name : "默认分类",
                        CreatedAt = g.CreatedAt,
                        UpdatedAt = g.UpdatedAt
                    })
                    .ToListAsync();

                _logger.LogInformation("获取甘特图数据成功，用户ID: {UserId}, 数据条数: {Count}", userId, ganttData.Count);
                return Ok(ApiResponse<List<GanttDataDto>>.SuccessResult(ganttData, $"获取到 {ganttData.Count} 条甘特图数据"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取甘特图数据失败，用户ID: {UserId}", userId);
                return StatusCode(500, ApiResponse<List<GanttDataDto>>.ErrorResult("获取甘特图数据失败"));
            }
        }

        /// <summary>
        /// 更新甘特图数据
        /// </summary>
        /// <param name="id">甘特图数据ID</param>
        /// <param name="updateDto">更新数据</param>
        /// <returns>更新结果</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<GanttDataDto>>> UpdateGanttData(
            Guid id, [FromBody] GanttDataUpdateDto updateDto)
        {
            try
            {
                var ganttItem = await _context.GanttData
                    .Include(g => g.Task)
                    .ThenInclude(t => t.Category)
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (ganttItem == null)
                {
                    return NotFound(ApiResponse<GanttDataDto>.ErrorResult($"甘特图数据不存在，ID: {id}"));
                }

                // 更新甘特图数据
                ganttItem.StartDate = updateDto.StartDate;
                ganttItem.EndDate = updateDto.EndDate;
                ganttItem.Progress = Math.Max(0, Math.Min(100, updateDto.Progress)); // 确保进度在0-100之间
                ganttItem.Dependencies = JsonSerializer.Serialize(updateDto.Dependencies ?? new List<Guid>());
                ganttItem.Resources = JsonSerializer.Serialize(updateDto.Resources ?? new List<string>());
                ganttItem.UpdatedAt = DateTime.UtcNow;

                // 同步更新关联的任务数据
                if (ganttItem.Task != null)
                {
                    ganttItem.Task.StartTime = updateDto.StartDate;
                    ganttItem.Task.EndTime = updateDto.EndDate;
                    ganttItem.Task.CompletionPercentage = (int)updateDto.Progress;
                    ganttItem.Task.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                var updatedDto = new GanttDataDto
                {
                    Id = ganttItem.Id,
                    UserId = ganttItem.UserId,
                    TaskId = ganttItem.TaskId,
                    TaskTitle = ganttItem.Task?.Title ?? "",
                    TaskDescription = ganttItem.Task?.Description ?? "",
                    StartDate = ganttItem.StartDate,
                    EndDate = ganttItem.EndDate,
                    Progress = ganttItem.Progress,
                    Dependencies = JsonSerializer.Deserialize<List<Guid>>(ganttItem.Dependencies ?? "[]"),
                    Resources = JsonSerializer.Deserialize<List<string>>(ganttItem.Resources ?? "[]"),
                    CategoryColor = ganttItem.Task?.Category?.Color ?? "#1890ff",
                    CategoryName = ganttItem.Task?.Category?.Name ?? "默认分类",
                    CreatedAt = ganttItem.CreatedAt,
                    UpdatedAt = ganttItem.UpdatedAt
                };

                _logger.LogInformation("甘特图数据更新成功，ID: {Id}", id);
                return Ok(ApiResponse<GanttDataDto>.SuccessResult(updatedDto, "甘特图数据更新成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新甘特图数据失败，ID: {Id}", id);
                return StatusCode(500, ApiResponse<GanttDataDto>.ErrorResult("更新甘特图数据失败"));
            }
        }

        /// <summary>
        /// 从任务同步到甘特图
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>同步结果</returns>
        [HttpPost("sync/{userId}")]
        public async Task<ActionResult<ApiResponse<GanttSyncResultDto>>> SyncFromTasks(Guid userId)
        {
            try
            {
                // 获取用户的所有任务（有开始和结束时间的）
                var tasks = await _context.Tasks
                    .Where(t => t.UserId == userId && t.StartTime.HasValue && t.EndTime.HasValue)
                    .ToListAsync();

                int syncedCount = 0;
                int updatedCount = 0;
                int skippedCount = 0;

                foreach (var task in tasks)
                {
                    var existingGanttData = await _context.GanttData
                        .FirstOrDefaultAsync(g => g.TaskId == task.Id);

                    if (existingGanttData == null)
                    {
                        // 创建新的甘特图数据
                        var newGanttData = new GanttDatum
                        {
                            UserId = userId,
                            TaskId = task.Id,
                            StartDate = task.StartTime.Value,
                            EndDate = task.EndTime.Value,
                            Progress = task.CompletionPercentage,
                            Dependencies = "[]",
                            Resources = "[]",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.GanttData.Add(newGanttData);
                        syncedCount++;
                    }
                    else
                    {
                        // 更新现有甘特图数据
                        existingGanttData.StartDate = task.StartTime.Value;
                        existingGanttData.EndDate = task.EndTime.Value;
                        existingGanttData.Progress = task.CompletionPercentage;
                        existingGanttData.UpdatedAt = DateTime.UtcNow;
                        updatedCount++;
                    }
                }

                // 处理孤立的甘特图数据（对应的任务已被删除）
                var orphanedGanttData = await _context.GanttData
                    .Where(g => g.UserId == userId && !_context.Tasks.Any(t => t.Id == g.TaskId))
                    .ToListAsync();

                _context.GanttData.RemoveRange(orphanedGanttData);
                int cleanedCount = orphanedGanttData.Count;

                await _context.SaveChangesAsync();

                var result = new GanttSyncResultDto
                {
                    SyncedCount = syncedCount,
                    UpdatedCount = updatedCount,
                    SkippedCount = skippedCount,
                    CleanedCount = cleanedCount,
                    TotalTasks = tasks.Count
                };

                _logger.LogInformation("任务同步完成，用户ID: {UserId}, 新增: {Synced}, 更新: {Updated}, 清理: {Cleaned}",
                    userId, syncedCount, updatedCount, cleanedCount);

                return Ok(ApiResponse<GanttSyncResultDto>.SuccessResult(result, "任务同步完成"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "任务同步失败，用户ID: {UserId}", userId);
                return StatusCode(500, ApiResponse<GanttSyncResultDto>.ErrorResult("任务同步失败"));
            }
        }

        /// <summary>
        /// 删除甘特图数据
        /// </summary>
        /// <param name="id">甘特图数据ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteGanttData(Guid id)
        {
            try
            {
                var ganttItem = await _context.GanttData.FindAsync(id);
                if (ganttItem == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult($"甘特图数据不存在，ID: {id}"));
                }

                _context.GanttData.Remove(ganttItem);
                await _context.SaveChangesAsync();

                _logger.LogInformation("甘特图数据删除成功，ID: {Id}", id);
                return Ok(ApiResponse<object>.SuccessResult(null, "甘特图数据删除成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除甘特图数据失败，ID: {Id}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResult("删除甘特图数据失败"));
            }
        }

        /// <summary>
        /// 检查甘特图数据一致性
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>一致性检查结果</returns>
        [HttpGet("consistency-check/{userId}")]
        public async Task<ActionResult<ApiResponse<GanttConsistencyCheckDto>>> CheckConsistency(Guid userId)
        {
            try
            {
                var inconsistencies = new List<GanttInconsistencyDto>();

                // 获取用户的甘特图数据和对应的任务
                var ganttDataWithTasks = await _context.GanttData
                    .Where(g => g.UserId == userId)
                    .Include(g => g.Task)
                    .ToListAsync();

                foreach (var ganttData in ganttDataWithTasks)
                {
                    if (ganttData.Task == null)
                    {
                        // 甘特图数据对应的任务不存在
                        inconsistencies.Add(new GanttInconsistencyDto
                        {
                            GanttDataId = ganttData.Id,
                            TaskId = ganttData.TaskId,
                            TaskTitle = "任务不存在",
                            InconsistencyType = "MISSING_TASK",
                            Description = "甘特图数据对应的任务已被删除",
                            ExpectedValue = "存在的任务",
                            ActualValue = null
                        });
                        continue;
                    }

                    // 检查日期一致性
                    if (ganttData.Task.StartTime.HasValue && ganttData.StartDate != ganttData.Task.StartTime.Value)
                    {
                        inconsistencies.Add(new GanttInconsistencyDto
                        {
                            GanttDataId = ganttData.Id,
                            TaskId = ganttData.TaskId,
                            TaskTitle = ganttData.Task.Title,
                            InconsistencyType = "START_DATE_MISMATCH",
                            Description = "开始日期不一致",
                            ExpectedValue = ganttData.Task.StartTime.Value,
                            ActualValue = ganttData.StartDate
                        });
                    }

                    if (ganttData.Task.EndTime.HasValue && ganttData.EndDate != ganttData.Task.EndTime.Value)
                    {
                        inconsistencies.Add(new GanttInconsistencyDto
                        {
                            GanttDataId = ganttData.Id,
                            TaskId = ganttData.TaskId,
                            TaskTitle = ganttData.Task.Title,
                            InconsistencyType = "END_DATE_MISMATCH",
                            Description = "结束日期不一致",
                            ExpectedValue = ganttData.Task.EndTime.Value,
                            ActualValue = ganttData.EndDate
                        });
                    }

                    // 检查进度一致性
                    if (ganttData.Progress != ganttData.Task.CompletionPercentage)
                    {
                        inconsistencies.Add(new GanttInconsistencyDto
                        {
                            GanttDataId = ganttData.Id,
                            TaskId = ganttData.TaskId,
                            TaskTitle = ganttData.Task.Title,
                            InconsistencyType = "PROGRESS_MISMATCH",
                            Description = "完成进度不一致",
                            ExpectedValue = ganttData.Task.CompletionPercentage,
                            ActualValue = ganttData.Progress
                        });
                    }
                }

                var result = new GanttConsistencyCheckDto
                {
                    IsConsistent = inconsistencies.Count == 0,
                    Inconsistencies = inconsistencies,
                    CheckTime = DateTime.UtcNow
                };

                _logger.LogInformation("甘特图数据一致性检查完成，用户ID: {UserId}, 不一致项: {Count}",
                    userId, inconsistencies.Count);

                return Ok(ApiResponse<GanttConsistencyCheckDto>.SuccessResult(result,
                    $"一致性检查完成，发现 {inconsistencies.Count} 个不一致项"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "甘特图数据一致性检查失败，用户ID: {UserId}", userId);
                return StatusCode(500, ApiResponse<GanttConsistencyCheckDto>.ErrorResult("一致性检查失败"));
            }
        }
    }
}
