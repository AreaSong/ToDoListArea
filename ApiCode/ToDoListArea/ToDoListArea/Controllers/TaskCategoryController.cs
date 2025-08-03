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
    public class TaskCategoryController : ControllerBase
    {
        private readonly ToDoListAreaDbContext _context;

        public TaskCategoryController(ToDoListAreaDbContext context)
        {
            _context = context;
        }
        /// <summary>
        /// 获取所有可用的任务分类
        /// </summary>
        /// <returns>分类列表</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<TaskCategoryDto>>>> GetCategories()
        {
            try
            {
                var categories = await _context.TaskCategories
                    .OrderBy(c => c.SortOrder)
                    .ThenBy(c => c.Name)
                    .ToListAsync();

                var categoryDtos = categories.Select(c => new TaskCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Color = c.Color,
                    Icon = c.Icon,
                    Description = c.Description,
                    IsSystem = c.IsSystem,
                    SortOrder = c.SortOrder,
                    CreatedAt = c.CreatedAt
                }).ToList();

                return Ok(ApiResponse<List<TaskCategoryDto>>.SuccessResult(categoryDtos, "获取分类列表成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<TaskCategoryDto>>.ErrorResult($"获取分类列表失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 根据ID获取分类详情
        /// </summary>
        /// <param name="id">分类ID</param>
        /// <returns>分类详情</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<TaskCategoryDto>>> GetCategory(Guid id)
        {
            try
            {
                var category = await _context.TaskCategories
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return NotFound(ApiResponse<TaskCategoryDto>.ErrorResult("分类不存在"));
                }

                var categoryDto = new TaskCategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Color = category.Color,
                    Icon = category.Icon,
                    Description = category.Description,
                    IsSystem = category.IsSystem,
                    SortOrder = category.SortOrder,
                    CreatedAt = category.CreatedAt
                };

                return Ok(ApiResponse<TaskCategoryDto>.SuccessResult(categoryDto, "获取分类详情成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<TaskCategoryDto>.ErrorResult($"获取分类详情失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 创建新的任务分类（仅管理员）
        /// </summary>
        /// <param name="createDto">分类创建信息</param>
        /// <returns>创建结果</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<TaskCategoryDto>>> CreateCategory([FromBody] TaskCategoryCreateDto createDto)
        {
            try
            {
                // 检查分类名称是否已存在
                var existingCategory = await _context.TaskCategories
                    .FirstOrDefaultAsync(c => c.Name == createDto.Name);

                if (existingCategory != null)
                {
                    return BadRequest(ApiResponse<TaskCategoryDto>.ErrorResult("分类名称已存在"));
                }

                // 创建新分类
                var category = new TaskCategory
                {
                    Id = Guid.NewGuid(),
                    Name = createDto.Name,
                    Color = createDto.Color ?? "#007bff",
                    Icon = createDto.Icon,
                    Description = createDto.Description,
                    IsSystem = false, // 用户创建的分类默认为非系统分类
                    SortOrder = createDto.SortOrder ?? 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.TaskCategories.Add(category);
                await _context.SaveChangesAsync();

                var categoryDto = new TaskCategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Color = category.Color,
                    Icon = category.Icon,
                    Description = category.Description,
                    IsSystem = category.IsSystem,
                    SortOrder = category.SortOrder,
                    CreatedAt = category.CreatedAt
                };

                return CreatedAtAction(nameof(GetCategory), new { id = category.Id },
                    ApiResponse<TaskCategoryDto>.SuccessResult(categoryDto, "分类创建成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<TaskCategoryDto>.ErrorResult($"分类创建失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 更新任务分类（仅管理员，且不能修改系统分类）
        /// </summary>
        /// <param name="id">分类ID</param>
        /// <param name="updateDto">分类更新信息</param>
        /// <returns>更新结果</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<TaskCategoryDto>>> UpdateCategory(Guid id, [FromBody] TaskCategoryUpdateDto updateDto)
        {
            try
            {
                var category = await _context.TaskCategories.FirstOrDefaultAsync(c => c.Id == id);
                if (category == null)
                {
                    return NotFound(ApiResponse<TaskCategoryDto>.ErrorResult("分类不存在"));
                }

                // 系统分类不允许修改
                if (category.IsSystem)
                {
                    return BadRequest(ApiResponse<TaskCategoryDto>.ErrorResult("系统分类不允许修改"));
                }

                // 检查分类名称是否已存在（排除当前分类）
                if (!string.IsNullOrEmpty(updateDto.Name) && updateDto.Name != category.Name)
                {
                    var existingCategory = await _context.TaskCategories
                        .FirstOrDefaultAsync(c => c.Name == updateDto.Name && c.Id != id);

                    if (existingCategory != null)
                    {
                        return BadRequest(ApiResponse<TaskCategoryDto>.ErrorResult("分类名称已存在"));
                    }
                }

                // 更新分类信息
                if (!string.IsNullOrEmpty(updateDto.Name))
                    category.Name = updateDto.Name;

                if (!string.IsNullOrEmpty(updateDto.Color))
                    category.Color = updateDto.Color;

                if (updateDto.Icon != null)
                    category.Icon = updateDto.Icon;

                if (updateDto.Description != null)
                    category.Description = updateDto.Description;

                if (updateDto.SortOrder.HasValue)
                    category.SortOrder = updateDto.SortOrder.Value;

                category.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var categoryDto = new TaskCategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Color = category.Color,
                    Icon = category.Icon,
                    Description = category.Description,
                    IsSystem = category.IsSystem,
                    SortOrder = category.SortOrder,
                    CreatedAt = category.CreatedAt
                };

                return Ok(ApiResponse<TaskCategoryDto>.SuccessResult(categoryDto, "分类更新成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<TaskCategoryDto>.ErrorResult($"分类更新失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 删除任务分类（仅管理员，且不能删除系统分类和有关联任务的分类）
        /// </summary>
        /// <param name="id">分类ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteCategory(Guid id)
        {
            try
            {
                var category = await _context.TaskCategories.FirstOrDefaultAsync(c => c.Id == id);
                if (category == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("分类不存在"));
                }

                // 系统分类不允许删除
                if (category.IsSystem)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("系统分类不允许删除"));
                }

                // 检查是否有关联的任务
                var hasRelatedTasks = await _context.Tasks.AnyAsync(t => t.CategoryId == id);
                if (hasRelatedTasks)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("该分类下存在任务，无法删除"));
                }

                _context.TaskCategories.Remove(category);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResult(new object(), "分类删除成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"分类删除失败: {ex.Message}"));
            }
        }
    }
}
