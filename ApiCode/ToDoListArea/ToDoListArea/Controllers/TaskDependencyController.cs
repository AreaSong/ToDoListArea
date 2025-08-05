using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DbContextHelp.Models;
using ToDoListArea.DTOs;
using System.Security.Claims;

namespace ToDoListArea.Controllers
{
    /// <summary>
    /// 任务依赖关系管理控制器
    /// 实现任务之间的依赖关系管理功能（简化版）
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskDependencyController : ControllerBase
    {
        private readonly ToDoListAreaDbContext _context;
        private readonly ILogger<TaskDependencyController> _logger;

        public TaskDependencyController(ToDoListAreaDbContext context, ILogger<TaskDependencyController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 获取任务的所有依赖关系
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>依赖关系列表</returns>
        [HttpGet("task/{taskId}/dependencies")]
        public async Task<ActionResult<IEnumerable<TaskDependencyDto>>> GetTaskDependencies(Guid taskId)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // 验证任务是否属于当前用户
                var task = await _context.Tasks
                    .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
                
                if (task == null)
                {
                    return NotFound("任务不存在或无权限访问");
                }

                // 获取任务的依赖关系
                var dependencies = await _context.TaskDependencies
                    .Include(td => td.DependsOnTask)
                    .Where(td => td.TaskId == taskId)
                    .Select(td => new TaskDependencyDto
                    {
                        Id = td.Id,
                        TaskId = td.TaskId,
                        DependsOnTaskId = td.DependsOnTaskId,
                        DependsOnTaskTitle = td.DependsOnTask.Title,
                        DependencyType = td.DependencyType,
                        LagTime = td.LagTime,
                        CreatedAt = td.CreatedAt
                    })
                    .ToListAsync();

                return Ok(dependencies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取任务依赖关系失败，任务ID: {TaskId}", taskId);
                return StatusCode(500, "获取依赖关系失败");
            }
        }

        /// <summary>
        /// 获取依赖于指定任务的所有任务
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>依赖任务列表</returns>
        [HttpGet("task/{taskId}/dependents")]
        public async Task<ActionResult<IEnumerable<TaskDependencyDto>>> GetTaskDependents(Guid taskId)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // 验证任务是否属于当前用户
                var task = await _context.Tasks
                    .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
                
                if (task == null)
                {
                    return NotFound("任务不存在或无权限访问");
                }

                // 获取依赖于此任务的其他任务
                var dependents = await _context.TaskDependencies
                    .Include(td => td.Task)
                    .Where(td => td.DependsOnTaskId == taskId)
                    .Select(td => new TaskDependencyDto
                    {
                        Id = td.Id,
                        TaskId = td.TaskId,
                        TaskTitle = td.Task.Title,
                        DependsOnTaskId = td.DependsOnTaskId,
                        DependencyType = td.DependencyType,
                        LagTime = td.LagTime,
                        CreatedAt = td.CreatedAt
                    })
                    .ToListAsync();

                return Ok(dependents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取任务依赖者失败，任务ID: {TaskId}", taskId);
                return StatusCode(500, "获取依赖者失败");
            }
        }

        /// <summary>
        /// 为任务添加依赖关系
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="request">依赖关系创建请求</param>
        /// <returns>创建的依赖关系</returns>
        [HttpPost("task/{taskId}/dependency")]
        public async Task<ActionResult<TaskDependencyDto>> CreateTaskDependency(Guid taskId, [FromBody] CreateTaskDependencyDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // 验证任务是否属于当前用户
                var task = await _context.Tasks
                    .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
                
                if (task == null)
                {
                    return NotFound("任务不存在或无权限访问");
                }

                // 验证依赖任务是否属于当前用户
                var dependsOnTask = await _context.Tasks
                    .FirstOrDefaultAsync(t => t.Id == request.DependsOnTaskId && t.UserId == userId);
                
                if (dependsOnTask == null)
                {
                    return BadRequest("依赖任务不存在或无权限访问");
                }

                // 检查是否会形成循环依赖
                if (await WillCreateCircularDependency(taskId, request.DependsOnTaskId))
                {
                    return BadRequest("添加此依赖关系会形成循环依赖");
                }

                // 检查依赖关系是否已存在
                var existingDependency = await _context.TaskDependencies
                    .FirstOrDefaultAsync(td => td.TaskId == taskId && td.DependsOnTaskId == request.DependsOnTaskId);
                
                if (existingDependency != null)
                {
                    return BadRequest("依赖关系已存在");
                }

                // 创建依赖关系
                var dependency = new TaskDependency
                {
                    Id = Guid.NewGuid(),
                    TaskId = taskId,
                    DependsOnTaskId = request.DependsOnTaskId,
                    DependencyType = "finish_to_start", // 简化版只支持这一种类型
                    LagTime = request.LagTime ?? 0,
                    CreatedAt = DateTime.UtcNow
                };

                _context.TaskDependencies.Add(dependency);
                await _context.SaveChangesAsync();

                // 返回创建的依赖关系
                var result = new TaskDependencyDto
                {
                    Id = dependency.Id,
                    TaskId = dependency.TaskId,
                    DependsOnTaskId = dependency.DependsOnTaskId,
                    DependsOnTaskTitle = dependsOnTask.Title,
                    DependencyType = dependency.DependencyType,
                    LagTime = dependency.LagTime,
                    CreatedAt = dependency.CreatedAt
                };

                _logger.LogInformation("成功创建任务依赖关系，任务ID: {TaskId}, 依赖任务ID: {DependsOnTaskId}", 
                    taskId, request.DependsOnTaskId);

                return CreatedAtAction(nameof(GetTaskDependencies), new { taskId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建任务依赖关系失败，任务ID: {TaskId}", taskId);
                return StatusCode(500, "创建依赖关系失败");
            }
        }

        /// <summary>
        /// 删除依赖关系
        /// </summary>
        /// <param name="dependencyId">依赖关系ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("{dependencyId}")]
        public async Task<ActionResult> DeleteTaskDependency(Guid dependencyId)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // 查找依赖关系并验证权限
                var dependency = await _context.TaskDependencies
                    .Include(td => td.Task)
                    .FirstOrDefaultAsync(td => td.Id == dependencyId && td.Task.UserId == userId);
                
                if (dependency == null)
                {
                    return NotFound("依赖关系不存在或无权限访问");
                }

                _context.TaskDependencies.Remove(dependency);
                await _context.SaveChangesAsync();

                _logger.LogInformation("成功删除任务依赖关系，依赖关系ID: {DependencyId}", dependencyId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除任务依赖关系失败，依赖关系ID: {DependencyId}", dependencyId);
                return StatusCode(500, "删除依赖关系失败");
            }
        }

        /// <summary>
        /// 检查任务的依赖冲突
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>冲突检查结果</returns>
        [HttpGet("task/{taskId}/conflicts")]
        public async Task<ActionResult<TaskConflictCheckDto>> CheckTaskConflicts(Guid taskId)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // 验证任务是否属于当前用户
                var task = await _context.Tasks
                    .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
                
                if (task == null)
                {
                    return NotFound("任务不存在或无权限访问");
                }

                var conflicts = new List<string>();

                // 检查循环依赖
                var circularDependencies = await DetectCircularDependencies(taskId);
                if (circularDependencies.Any())
                {
                    conflicts.Add($"检测到循环依赖：{string.Join(" -> ", circularDependencies)}");
                }

                // 检查时间冲突（如果任务有时间安排）
                if (task.StartTime.HasValue && task.EndTime.HasValue)
                {
                    var timeConflicts = await DetectTimeConflicts(taskId, task.StartTime.Value, task.EndTime.Value);
                    conflicts.AddRange(timeConflicts);
                }

                var result = new TaskConflictCheckDto
                {
                    TaskId = taskId,
                    HasConflicts = conflicts.Any(),
                    Conflicts = conflicts,
                    CheckedAt = DateTime.UtcNow
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查任务冲突失败，任务ID: {TaskId}", taskId);
                return StatusCode(500, "检查冲突失败");
            }
        }

        #region 私有方法

        /// <summary>
        /// 获取当前用户ID
        /// </summary>
        /// <returns>用户ID</returns>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("无法获取用户身份信息");
            }
            return userId;
        }

        /// <summary>
        /// 检查是否会形成循环依赖
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="dependsOnTaskId">依赖任务ID</param>
        /// <returns>是否会形成循环依赖</returns>
        private async Task<bool> WillCreateCircularDependency(Guid taskId, Guid dependsOnTaskId)
        {
            // 如果依赖任务就是自己，直接返回true
            if (taskId == dependsOnTaskId)
            {
                return true;
            }

            // 检查依赖任务是否已经依赖于当前任务（直接或间接）
            var visited = new HashSet<Guid>();
            return await HasDependencyPath(dependsOnTaskId, taskId, visited);
        }

        /// <summary>
        /// 递归检查是否存在依赖路径
        /// </summary>
        /// <param name="fromTaskId">起始任务ID</param>
        /// <param name="toTaskId">目标任务ID</param>
        /// <param name="visited">已访问的任务ID集合</param>
        /// <returns>是否存在依赖路径</returns>
        private async Task<bool> HasDependencyPath(Guid fromTaskId, Guid toTaskId, HashSet<Guid> visited)
        {
            if (visited.Contains(fromTaskId))
            {
                return false; // 避免无限循环
            }

            visited.Add(fromTaskId);

            var dependencies = await _context.TaskDependencies
                .Where(td => td.TaskId == fromTaskId)
                .Select(td => td.DependsOnTaskId)
                .ToListAsync();

            foreach (var dependencyId in dependencies)
            {
                if (dependencyId == toTaskId)
                {
                    return true; // 找到直接依赖
                }

                if (await HasDependencyPath(dependencyId, toTaskId, visited))
                {
                    return true; // 找到间接依赖
                }
            }

            return false;
        }

        /// <summary>
        /// 检测循环依赖
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>循环依赖路径</returns>
        private async Task<List<string>> DetectCircularDependencies(Guid taskId)
        {
            var visited = new HashSet<Guid>();
            var path = new List<Guid>();
            var taskTitles = new Dictionary<Guid, string>();

            // 获取所有相关任务的标题
            var allTasks = await _context.Tasks
                .Where(t => t.UserId == GetCurrentUserId())
                .ToDictionaryAsync(t => t.Id, t => t.Title);

            if (await DetectCycleRecursive(taskId, visited, path, allTasks))
            {
                return path.Select(id => allTasks.GetValueOrDefault(id, "未知任务")).ToList();
            }

            return new List<string>();
        }

        /// <summary>
        /// 递归检测循环依赖
        /// </summary>
        private async Task<bool> DetectCycleRecursive(Guid taskId, HashSet<Guid> visited, List<Guid> path, Dictionary<Guid, string> taskTitles)
        {
            if (path.Contains(taskId))
            {
                // 找到循环，截取循环部分
                var cycleStart = path.IndexOf(taskId);
                path.RemoveRange(0, cycleStart);
                path.Add(taskId); // 添加回到起点
                return true;
            }

            if (visited.Contains(taskId))
            {
                return false;
            }

            visited.Add(taskId);
            path.Add(taskId);

            var dependencies = await _context.TaskDependencies
                .Where(td => td.TaskId == taskId)
                .Select(td => td.DependsOnTaskId)
                .ToListAsync();

            foreach (var dependencyId in dependencies)
            {
                if (await DetectCycleRecursive(dependencyId, visited, path, taskTitles))
                {
                    return true;
                }
            }

            path.RemoveAt(path.Count - 1);
            return false;
        }

        /// <summary>
        /// 检测时间冲突
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns>时间冲突列表</returns>
        private async Task<List<string>> DetectTimeConflicts(Guid taskId, DateTime startTime, DateTime endTime)
        {
            var conflicts = new List<string>();

            // 检查依赖任务的时间安排
            var dependencies = await _context.TaskDependencies
                .Include(td => td.DependsOnTask)
                .Where(td => td.TaskId == taskId)
                .ToListAsync();

            foreach (var dependency in dependencies)
            {
                var dependsOnTask = dependency.DependsOnTask;
                if (dependsOnTask.EndTime.HasValue && dependsOnTask.EndTime.Value > startTime)
                {
                    conflicts.Add($"依赖任务 '{dependsOnTask.Title}' 的结束时间晚于当前任务的开始时间");
                }
            }

            return conflicts;
        }

        #endregion
    }
}
