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
    public class TaskTemplateController : ControllerBase
    {
        private readonly ToDoListAreaDbContext _context;
        private readonly ILogger<TaskTemplateController> _logger;

        public TaskTemplateController(ToDoListAreaDbContext context, ILogger<TaskTemplateController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 获取用户的任务模板列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="category">分类筛选（可选）</param>
        /// <param name="sortBy">排序方式：usage（使用频率）、created（创建时间）、name（名称）</param>
        /// <returns>模板列表</returns>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<List<TaskTemplateDto>>>> GetUserTemplates(
            Guid userId, 
            [FromQuery] string? category = null,
            [FromQuery] string sortBy = "usage")
        {
            try
            {
                var query = _context.TaskTemplates
                    .Where(t => t.UserId == userId);

                // 分类筛选
                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(t => t.Category == category);
                }

                // 排序
                query = sortBy.ToLower() switch
                {
                    "usage" => query.OrderByDescending(t => t.UsageCount).ThenByDescending(t => t.UpdatedAt),
                    "created" => query.OrderByDescending(t => t.CreatedAt),
                    "name" => query.OrderBy(t => t.Name),
                    _ => query.OrderByDescending(t => t.UsageCount)
                };

                var templates = await query
                    .Select(t => new TaskTemplateDto
                    {
                        Id = t.Id,
                        UserId = t.UserId,
                        Name = t.Name,
                        Description = t.Description,
                        TemplateData = t.TemplateData,
                        Category = t.Category,
                        IsPublic = t.IsPublic,
                        UsageCount = t.UsageCount,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    })
                    .ToListAsync();

                _logger.LogInformation("获取用户模板成功，用户ID: {UserId}, 模板数量: {Count}", userId, templates.Count);
                return Ok(ApiResponse<List<TaskTemplateDto>>.SuccessResult(templates, $"获取到 {templates.Count} 个模板"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户模板失败，用户ID: {UserId}", userId);
                return StatusCode(500, ApiResponse<List<TaskTemplateDto>>.ErrorResult("获取模板列表失败"));
            }
        }

        /// <summary>
        /// 获取模板详情
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <returns>模板详情</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<TaskTemplateDto>>> GetTemplate(Guid id)
        {
            try
            {
                var template = await _context.TaskTemplates
                    .Where(t => t.Id == id)
                    .Select(t => new TaskTemplateDto
                    {
                        Id = t.Id,
                        UserId = t.UserId,
                        Name = t.Name,
                        Description = t.Description,
                        TemplateData = t.TemplateData,
                        Category = t.Category,
                        IsPublic = t.IsPublic,
                        UsageCount = t.UsageCount,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (template == null)
                {
                    return NotFound(ApiResponse<TaskTemplateDto>.ErrorResult($"模板不存在，ID: {id}"));
                }

                _logger.LogInformation("获取模板详情成功，模板ID: {TemplateId}", id);
                return Ok(ApiResponse<TaskTemplateDto>.SuccessResult(template, "获取模板详情成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取模板详情失败，模板ID: {TemplateId}", id);
                return StatusCode(500, ApiResponse<TaskTemplateDto>.ErrorResult("获取模板详情失败"));
            }
        }

        /// <summary>
        /// 创建任务模板
        /// </summary>
        /// <param name="createDto">创建模板数据</param>
        /// <returns>创建的模板</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<TaskTemplateDto>>> CreateTemplate([FromBody] TaskTemplateCreateDto createDto)
        {
            try
            {
                var template = new TaskTemplate
                {
                    UserId = createDto.UserId,
                    Name = createDto.Name,
                    Description = createDto.Description,
                    TemplateData = createDto.TemplateData,
                    Category = createDto.Category,
                    IsPublic = false, // 简化版暂不支持公共模板
                    UsageCount = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.TaskTemplates.Add(template);
                await _context.SaveChangesAsync();

                var templateDto = new TaskTemplateDto
                {
                    Id = template.Id,
                    UserId = template.UserId,
                    Name = template.Name,
                    Description = template.Description,
                    TemplateData = template.TemplateData,
                    Category = template.Category,
                    IsPublic = template.IsPublic,
                    UsageCount = template.UsageCount,
                    CreatedAt = template.CreatedAt,
                    UpdatedAt = template.UpdatedAt
                };

                _logger.LogInformation("创建模板成功，模板ID: {TemplateId}, 用户ID: {UserId}", template.Id, createDto.UserId);
                return Ok(ApiResponse<TaskTemplateDto>.SuccessResult(templateDto, "模板创建成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建模板失败，用户ID: {UserId}", createDto.UserId);
                return StatusCode(500, ApiResponse<TaskTemplateDto>.ErrorResult("创建模板失败"));
            }
        }

        /// <summary>
        /// 更新任务模板
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <param name="updateDto">更新数据</param>
        /// <returns>更新结果</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<TaskTemplateDto>>> UpdateTemplate(
            Guid id, [FromBody] TaskTemplateUpdateDto updateDto)
        {
            try
            {
                var template = await _context.TaskTemplates.FindAsync(id);
                if (template == null)
                {
                    return NotFound(ApiResponse<TaskTemplateDto>.ErrorResult($"模板不存在，ID: {id}"));
                }

                // 更新模板信息
                if (!string.IsNullOrEmpty(updateDto.Name))
                    template.Name = updateDto.Name;

                if (updateDto.Description != null)
                    template.Description = updateDto.Description;

                if (!string.IsNullOrEmpty(updateDto.TemplateData))
                    template.TemplateData = updateDto.TemplateData;

                if (updateDto.Category != null)
                    template.Category = updateDto.Category;

                template.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var templateDto = new TaskTemplateDto
                {
                    Id = template.Id,
                    UserId = template.UserId,
                    Name = template.Name,
                    Description = template.Description,
                    TemplateData = template.TemplateData,
                    Category = template.Category,
                    IsPublic = template.IsPublic,
                    UsageCount = template.UsageCount,
                    CreatedAt = template.CreatedAt,
                    UpdatedAt = template.UpdatedAt
                };

                _logger.LogInformation("更新模板成功，模板ID: {TemplateId}", id);
                return Ok(ApiResponse<TaskTemplateDto>.SuccessResult(templateDto, "模板更新成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新模板失败，模板ID: {TemplateId}", id);
                return StatusCode(500, ApiResponse<TaskTemplateDto>.ErrorResult("更新模板失败"));
            }
        }

        /// <summary>
        /// 删除任务模板
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteTemplate(Guid id)
        {
            try
            {
                var template = await _context.TaskTemplates.FindAsync(id);
                if (template == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult($"模板不存在，ID: {id}"));
                }

                _context.TaskTemplates.Remove(template);
                await _context.SaveChangesAsync();

                _logger.LogInformation("删除模板成功，模板ID: {TemplateId}", id);
                return Ok(ApiResponse<object>.SuccessResult(null, "模板删除成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除模板失败，模板ID: {TemplateId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResult("删除模板失败"));
            }
        }

        /// <summary>
        /// 从模板创建任务
        /// </summary>
        /// <param name="templateId">模板ID</param>
        /// <param name="createTaskDto">任务创建数据</param>
        /// <returns>创建的任务</returns>
        [HttpPost("{templateId}/use")]
        public async Task<ActionResult<ApiResponse<TaskResponseDto>>> CreateTaskFromTemplate(
            Guid templateId, [FromBody] CreateTaskFromTemplateDto createTaskDto)
        {
            try
            {
                var template = await _context.TaskTemplates.FindAsync(templateId);
                if (template == null)
                {
                    return NotFound(ApiResponse<TaskResponseDto>.ErrorResult($"模板不存在，ID: {templateId}"));
                }

                // 解析模板数据
                var templateData = JsonSerializer.Deserialize<TaskTemplateData>(template.TemplateData);
                if (templateData == null)
                {
                    return BadRequest(ApiResponse<TaskResponseDto>.ErrorResult("模板数据格式错误"));
                }

                // 创建任务
                var task = new DbContextHelp.Models.Task
                {
                    UserId = createTaskDto.UserId,
                    Title = createTaskDto.CustomTitle ?? templateData.Title,
                    Description = createTaskDto.CustomDescription ?? templateData.Description,
                    Priority = templateData.Priority,
                    Status = "Pending",
                    CategoryId = templateData.CategoryId,
                    StartTime = createTaskDto.StartTime ?? templateData.StartTime,
                    EndTime = createTaskDto.EndTime ?? templateData.EndTime,
                    CompletionPercentage = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Tasks.Add(task);

                // 增加模板使用次数
                template.UsageCount++;
                template.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // 获取任务详情（包含分类信息）
                var taskWithCategory = await _context.Tasks
                    .Include(t => t.Category)
                    .FirstOrDefaultAsync(t => t.Id == task.Id);

                var taskDto = new TaskResponseDto
                {
                    Id = task.Id,
                    UserId = task.UserId,
                    Title = task.Title,
                    Description = task.Description,
                    Priority = task.Priority,
                    Status = task.Status,
                    CategoryId = task.CategoryId,
                    CategoryName = taskWithCategory?.Category?.Name,
                    StartTime = task.StartTime,
                    EndTime = task.EndTime,
                    CompletionPercentage = (int)task.CompletionPercentage,
                    CreatedAt = task.CreatedAt,
                    UpdatedAt = task.UpdatedAt
                };

                _logger.LogInformation("从模板创建任务成功，模板ID: {TemplateId}, 任务ID: {TaskId}", templateId, task.Id);
                return Ok(ApiResponse<TaskResponseDto>.SuccessResult(taskDto, "从模板创建任务成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "从模板创建任务失败，模板ID: {TemplateId}", templateId);
                return StatusCode(500, ApiResponse<TaskResponseDto>.ErrorResult("从模板创建任务失败"));
            }
        }

        /// <summary>
        /// 获取用户的模板分类列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>分类列表</returns>
        [HttpGet("categories/{userId}")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetTemplateCategories(Guid userId)
        {
            try
            {
                var categories = await _context.TaskTemplates
                    .Where(t => t.UserId == userId && !string.IsNullOrEmpty(t.Category))
                    .Select(t => t.Category!)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                _logger.LogInformation("获取模板分类成功，用户ID: {UserId}, 分类数量: {Count}", userId, categories.Count);
                return Ok(ApiResponse<List<string>>.SuccessResult(categories, $"获取到 {categories.Count} 个分类"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取模板分类失败，用户ID: {UserId}", userId);
                return StatusCode(500, ApiResponse<List<string>>.ErrorResult("获取模板分类失败"));
            }
        }

        /// <summary>
        /// 获取用户的模板使用统计
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>使用统计</returns>
        [HttpGet("stats/{userId}")]
        public async Task<ActionResult<ApiResponse<TemplateUsageStatsDto>>> GetTemplateStats(Guid userId)
        {
            try
            {
                var templates = await _context.TaskTemplates
                    .Where(t => t.UserId == userId)
                    .ToListAsync();

                var stats = new TemplateUsageStatsDto
                {
                    TotalTemplates = templates.Count,
                    TotalUsage = templates.Sum(t => t.UsageCount),
                    MostUsedTemplate = templates.OrderByDescending(t => t.UsageCount).FirstOrDefault()?.Name,
                    AverageUsage = templates.Count > 0 ? (double)templates.Sum(t => t.UsageCount) / templates.Count : 0,
                    CategoriesCount = templates.Where(t => !string.IsNullOrEmpty(t.Category))
                        .Select(t => t.Category).Distinct().Count(),
                    RecentlyCreated = templates.OrderByDescending(t => t.CreatedAt).Take(5)
                        .Select(t => new RecentTemplateDto
                        {
                            Id = t.Id,
                            Name = t.Name,
                            Category = t.Category,
                            UsageCount = t.UsageCount,
                            CreatedAt = t.CreatedAt
                        }).ToList()
                };

                _logger.LogInformation("获取模板统计成功，用户ID: {UserId}", userId);
                return Ok(ApiResponse<TemplateUsageStatsDto>.SuccessResult(stats, "获取模板统计成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取模板统计失败，用户ID: {UserId}", userId);
                return StatusCode(500, ApiResponse<TemplateUsageStatsDto>.ErrorResult("获取模板统计失败"));
            }
        }
    }
}
