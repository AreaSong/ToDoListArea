using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using DbContextHelp.Models;
using ToDoListArea.Models;

namespace ToDoListArea.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 需要JWT认证
    public class TaskController : ControllerBase
    {
        private readonly ToDoListAreaDbContext _context;

        public TaskController(ToDoListAreaDbContext context)
        {
            _context = context;
        }
        /// <summary>
        /// 获取任务列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="query">查询参数</param>
        /// <returns>任务列表</returns>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<PagedResponse<TaskResponseDto>>>> GetTasks(
            Guid userId, 
            [FromQuery] TaskQueryDto query)
        {
            try
            {
                // 构建查询 
                var tasksQuery = _context.Tasks
                    .Where(t => t.UserId == userId)
                    .AsQueryable();

                // 应用筛选条件
                if (!string.IsNullOrEmpty(query.Status))
                    tasksQuery = tasksQuery.Where(t => t.Status == query.Status);

                if (!string.IsNullOrEmpty(query.Priority))
                    tasksQuery = tasksQuery.Where(t => t.Priority == query.Priority);

                if (query.CategoryId.HasValue)
                    tasksQuery = tasksQuery.Where(t => t.CategoryId == query.CategoryId);

                if (query.StartDate.HasValue)
                    tasksQuery = tasksQuery.Where(t => t.StartTime >= query.StartDate);

                if (query.EndDate.HasValue)
                    tasksQuery = tasksQuery.Where(t => t.EndTime <= query.EndDate);

                if (!string.IsNullOrEmpty(query.SearchKeyword))
                    tasksQuery = tasksQuery.Where(t => 
                        t.Title.Contains(query.SearchKeyword) || 
                        (t.Description != null && t.Description.Contains(query.SearchKeyword)));

                // 排序
                tasksQuery = query.SortBy?.ToLower() switch
                {
                    "title" => query.SortOrder?.ToLower() == "asc" 
                        ? tasksQuery.OrderBy(t => t.Title) 
                        : tasksQuery.OrderByDescending(t => t.Title),
                    "priority" => query.SortOrder?.ToLower() == "asc" 
                        ? tasksQuery.OrderBy(t => t.Priority) 
                        : tasksQuery.OrderByDescending(t => t.Priority),
                    "starttime" => query.SortOrder?.ToLower() == "asc" 
                        ? tasksQuery.OrderBy(t => t.StartTime) 
                        : tasksQuery.OrderByDescending(t => t.StartTime),
                    _ => query.SortOrder?.ToLower() == "asc" 
                        ? tasksQuery.OrderBy(t => t.CreatedAt) 
                        : tasksQuery.OrderByDescending(t => t.CreatedAt)
                };

                // 获取总数
                var totalCount = await tasksQuery.CountAsync();

                // 分页
                var tasks = await tasksQuery
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Include(t => t.Category)
                    .ToListAsync();

                // 转换为DTO
                var taskDtos = tasks.Select(t => new TaskResponseDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    ParentTaskId = t.ParentTaskId,
                    CategoryId = t.CategoryId,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    StartTime = t.StartTime,
                    EndTime = t.EndTime,
                    EstimatedDuration = t.EstimatedDuration,
                    ActualDuration = t.ActualDuration,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    CategoryName = t.Category?.Name
                }).ToList();

                var pagedResponse = new PagedResponse<TaskResponseDto>(taskDtos, totalCount, query.PageNumber, query.PageSize);

                return Ok(ApiResponse<PagedResponse<TaskResponseDto>>.SuccessResult(pagedResponse, "获取任务列表成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PagedResponse<TaskResponseDto>>.ErrorResult($"获取任务列表失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 根据ID获取任务详情
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <returns>任务详情</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<TaskResponseDto>>> GetTask(Guid id)
        {
            try
            {
                var task = await _context.Tasks
                    .Include(t => t.Category)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (task == null)
                {
                    return NotFound(ApiResponse<TaskResponseDto>.ErrorResult("任务不存在"));
                }

                var taskDto = new TaskResponseDto
                {
                    Id = task.Id,
                    UserId = task.UserId,
                    ParentTaskId = task.ParentTaskId,
                    CategoryId = task.CategoryId,
                    Title = task.Title,
                    Description = task.Description,
                    Status = task.Status,
                    Priority = task.Priority,
                    StartTime = task.StartTime,
                    EndTime = task.EndTime,
                    EstimatedDuration = task.EstimatedDuration,
                    ActualDuration = task.ActualDuration,
                    CreatedAt = task.CreatedAt,
                    UpdatedAt = task.UpdatedAt,
                    CategoryName = task.Category?.Name
                };

                return Ok(ApiResponse<TaskResponseDto>.SuccessResult(taskDto, "获取任务详情成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<TaskResponseDto>.ErrorResult($"获取任务详情失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 创建任务
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="createDto">任务创建信息</param>
        /// <returns>创建结果</returns>
        [HttpPost("user/{userId}")]
        public async Task<ActionResult<ApiResponse<TaskResponseDto>>> CreateTask(Guid userId, [FromBody] TaskCreateDto createDto)
        {
            try
            {
                // 验证用户是否存在
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                if (!userExists)
                {
                    return BadRequest(ApiResponse<TaskResponseDto>.ErrorResult("用户不存在"));
                }

                // 验证分类是否存在（如果指定了分类）
                if (createDto.CategoryId.HasValue)
                {
                    var categoryExists = await _context.TaskCategories
                        .AnyAsync(c => c.Id == createDto.CategoryId);
                    if (!categoryExists)
                    {
                        return BadRequest(ApiResponse<TaskResponseDto>.ErrorResult("指定的分类不存在"));
                    }
                }

                // 验证父任务是否存在（如果指定了父任务）
                if (createDto.ParentTaskId.HasValue)
                {
                    var parentTaskExists = await _context.Tasks
                        .AnyAsync(t => t.Id == createDto.ParentTaskId && t.UserId == userId);
                    if (!parentTaskExists)
                    {
                        return BadRequest(ApiResponse<TaskResponseDto>.ErrorResult("指定的父任务不存在"));
                    }
                }

                // 创建新任务
                var task = new DbContextHelp.Models.Task
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ParentTaskId = createDto.ParentTaskId,
                    CategoryId = createDto.CategoryId,
                    Title = createDto.Title,
                    Description = createDto.Description,
                    Status = createDto.Status,
                    Priority = createDto.Priority,
                    StartTime = createDto.StartTime,
                    EndTime = createDto.EndTime,
                    EstimatedDuration = createDto.EstimatedDuration,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();

                // 获取创建的任务（包含关联信息）
                var createdTask = await _context.Tasks
                    .Include(t => t.Category)
                    .FirstOrDefaultAsync(t => t.Id == task.Id);

                var taskDto = new TaskResponseDto
                {
                    Id = createdTask!.Id,
                    UserId = createdTask.UserId,
                    ParentTaskId = createdTask.ParentTaskId,
                    CategoryId = createdTask.CategoryId,
                    Title = createdTask.Title,
                    Description = createdTask.Description,
                    Status = createdTask.Status,
                    Priority = createdTask.Priority,
                    StartTime = createdTask.StartTime,
                    EndTime = createdTask.EndTime,
                    EstimatedDuration = createdTask.EstimatedDuration,
                    ActualDuration = createdTask.ActualDuration,
                    CreatedAt = createdTask.CreatedAt,
                    UpdatedAt = createdTask.UpdatedAt,
                    CategoryName = createdTask.Category?.Name
                };

                return CreatedAtAction(nameof(GetTask), new { id = task.Id },
                    ApiResponse<TaskResponseDto>.SuccessResult(taskDto, "任务创建成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<TaskResponseDto>.ErrorResult($"任务创建失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 更新任务
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <param name="updateDto">任务更新信息</param>
        /// <returns>更新结果</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<TaskResponseDto>>> UpdateTask(Guid id, [FromBody] TaskUpdateDto updateDto)
        {
            try
            {
                var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
                if (task == null)
                {
                    return NotFound(ApiResponse<TaskResponseDto>.ErrorResult("任务不存在"));
                }

                // 验证分类是否存在（如果指定了分类）
                if (updateDto.CategoryId.HasValue)
                {
                    var categoryExists = await _context.TaskCategories
                        .AnyAsync(c => c.Id == updateDto.CategoryId);
                    if (!categoryExists)
                    {
                        return BadRequest(ApiResponse<TaskResponseDto>.ErrorResult("指定的分类不存在"));
                    }
                }

                // 验证父任务是否存在（如果指定了父任务）
                if (updateDto.ParentTaskId.HasValue)
                {
                    var parentTaskExists = await _context.Tasks
                        .AnyAsync(t => t.Id == updateDto.ParentTaskId && t.UserId == task.UserId);
                    if (!parentTaskExists)
                    {
                        return BadRequest(ApiResponse<TaskResponseDto>.ErrorResult("指定的父任务不存在"));
                    }
                }

                // 更新任务信息
                if (!string.IsNullOrEmpty(updateDto.Title))
                    task.Title = updateDto.Title;

                if (updateDto.Description != null)
                    task.Description = updateDto.Description;

                if (!string.IsNullOrEmpty(updateDto.Status))
                    task.Status = updateDto.Status;

                if (!string.IsNullOrEmpty(updateDto.Priority))
                    task.Priority = updateDto.Priority;

                if (updateDto.StartTime.HasValue)
                    task.StartTime = updateDto.StartTime;

                if (updateDto.EndTime.HasValue)
                    task.EndTime = updateDto.EndTime;

                if (updateDto.EstimatedDuration.HasValue)
                    task.EstimatedDuration = updateDto.EstimatedDuration;

                if (updateDto.ActualDuration.HasValue)
                    task.ActualDuration = updateDto.ActualDuration;

                if (updateDto.CategoryId.HasValue)
                    task.CategoryId = updateDto.CategoryId;

                if (updateDto.ParentTaskId.HasValue)
                    task.ParentTaskId = updateDto.ParentTaskId;

                task.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // 获取更新后的任务（包含关联信息）
                var updatedTask = await _context.Tasks
                    .Include(t => t.Category)
                    .FirstOrDefaultAsync(t => t.Id == id);

                var taskDto = new TaskResponseDto
                {
                    Id = updatedTask!.Id,
                    UserId = updatedTask.UserId,
                    ParentTaskId = updatedTask.ParentTaskId,
                    CategoryId = updatedTask.CategoryId,
                    Title = updatedTask.Title,
                    Description = updatedTask.Description,
                    Status = updatedTask.Status,
                    Priority = updatedTask.Priority,
                    StartTime = updatedTask.StartTime,
                    EndTime = updatedTask.EndTime,
                    EstimatedDuration = updatedTask.EstimatedDuration,
                    ActualDuration = updatedTask.ActualDuration,
                    CreatedAt = updatedTask.CreatedAt,
                    UpdatedAt = updatedTask.UpdatedAt,
                    CategoryName = updatedTask.Category?.Name
                };

                return Ok(ApiResponse<TaskResponseDto>.SuccessResult(taskDto, "任务更新成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<TaskResponseDto>.ErrorResult($"任务更新失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteTask(Guid id)
        {
            try
            {
                var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
                if (task == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("任务不存在"));
                }

                // 检查是否有子任务
                var hasSubTasks = await _context.Tasks.AnyAsync(t => t.ParentTaskId == id);
                if (hasSubTasks)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("该任务存在子任务，请先删除子任务"));
                }

                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResult(new object(), "任务删除成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"任务删除失败: {ex.Message}"));
            }
        }
    }
}
