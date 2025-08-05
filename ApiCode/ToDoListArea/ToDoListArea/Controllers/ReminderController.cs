using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using DbContextHelp.Models;
using ToDoListArea.DTOs;

namespace ToDoListArea.Controllers
{
    /// <summary>
    /// 提醒管理控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReminderController : ControllerBase
    {
        private readonly ToDoListAreaDbContext _context;
        private readonly ILogger<ReminderController> _logger;

        public ReminderController(ToDoListAreaDbContext context, ILogger<ReminderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 获取当前用户ID
        /// </summary>
        /// <returns>用户ID</returns>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("无效的用户身份");
            }
            return userId;
        }

        /// <summary>
        /// 获取用户的提醒列表
        /// </summary>
        /// <param name="query">查询参数</param>
        /// <returns>提醒列表</returns>
        [HttpGet]
        public async Task<ActionResult<object>> GetReminders([FromQuery] ReminderQueryDto query)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                var queryable = _context.Reminders
                    .Include(r => r.Task)
                    .Where(r => r.UserId == userId);

                // 应用过滤条件
                if (!string.IsNullOrEmpty(query.Status))
                {
                    queryable = queryable.Where(r => r.Status == query.Status);
                }

                if (query.TaskId.HasValue)
                {
                    queryable = queryable.Where(r => r.TaskId == query.TaskId);
                }

                if (query.StartTime.HasValue)
                {
                    queryable = queryable.Where(r => r.ReminderTime >= query.StartTime);
                }

                if (query.EndTime.HasValue)
                {
                    queryable = queryable.Where(r => r.ReminderTime <= query.EndTime);
                }

                if (!string.IsNullOrEmpty(query.SearchKeyword))
                {
                    queryable = queryable.Where(r => 
                        r.Title.Contains(query.SearchKeyword) || 
                        (r.Message != null && r.Message.Contains(query.SearchKeyword)));
                }

                // 应用排序
                queryable = query.SortBy.ToLower() switch
                {
                    "title" => query.SortOrder.ToLower() == "desc" 
                        ? queryable.OrderByDescending(r => r.Title)
                        : queryable.OrderBy(r => r.Title),
                    "status" => query.SortOrder.ToLower() == "desc"
                        ? queryable.OrderByDescending(r => r.Status)
                        : queryable.OrderBy(r => r.Status),
                    "createdat" => query.SortOrder.ToLower() == "desc"
                        ? queryable.OrderByDescending(r => r.CreatedAt)
                        : queryable.OrderBy(r => r.CreatedAt),
                    _ => query.SortOrder.ToLower() == "desc"
                        ? queryable.OrderByDescending(r => r.ReminderTime)
                        : queryable.OrderBy(r => r.ReminderTime)
                };

                // 分页
                var totalCount = await queryable.CountAsync();
                var reminders = await queryable
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                // 转换为DTO
                var reminderDtos = reminders.Select(r => new ReminderDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    TaskId = r.TaskId,
                    TaskTitle = r.Task?.Title,
                    Title = r.Title,
                    Message = r.Message,
                    ReminderTime = r.ReminderTime,
                    Status = r.Status,
                    Channels = ParseChannels(r.Channels),
                    RepeatPattern = ParseRepeatPattern(r.RepeatPattern),
                    SnoozeUntil = r.SnoozeUntil,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                }).ToList();

                return Ok(new
                {
                    data = new
                    {
                        items = reminderDtos,
                        totalCount = totalCount,
                        pageNumber = query.PageNumber,
                        pageSize = query.PageSize,
                        totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取提醒列表失败");
                return StatusCode(500, "获取提醒列表失败");
            }
        }

        /// <summary>
        /// 根据ID获取提醒详情
        /// </summary>
        /// <param name="id">提醒ID</param>
        /// <returns>提醒详情</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ReminderDto>> GetReminder(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                var reminder = await _context.Reminders
                    .Include(r => r.Task)
                    .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

                if (reminder == null)
                {
                    return NotFound("提醒不存在或无权限访问");
                }

                var reminderDto = new ReminderDto
                {
                    Id = reminder.Id,
                    UserId = reminder.UserId,
                    TaskId = reminder.TaskId,
                    TaskTitle = reminder.Task?.Title,
                    Title = reminder.Title,
                    Message = reminder.Message,
                    ReminderTime = reminder.ReminderTime,
                    Status = reminder.Status,
                    Channels = ParseChannels(reminder.Channels),
                    RepeatPattern = ParseRepeatPattern(reminder.RepeatPattern),
                    SnoozeUntil = reminder.SnoozeUntil,
                    CreatedAt = reminder.CreatedAt,
                    UpdatedAt = reminder.UpdatedAt
                };

                return Ok(reminderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取提醒详情失败，ID: {ReminderId}", id);
                return StatusCode(500, "获取提醒详情失败");
            }
        }

        /// <summary>
        /// 创建新提醒
        /// </summary>
        /// <param name="createDto">创建提醒DTO</param>
        /// <returns>创建的提醒</returns>
        [HttpPost]
        public async Task<ActionResult<ReminderDto>> CreateReminder([FromBody] CreateReminderDto createDto)
        {
            try
            {
                var userId = GetCurrentUserId();

                // 验证关联任务是否存在且属于当前用户
                if (createDto.TaskId.HasValue)
                {
                    var task = await _context.Tasks
                        .FirstOrDefaultAsync(t => t.Id == createDto.TaskId && t.UserId == userId);
                    
                    if (task == null)
                    {
                        return BadRequest("关联任务不存在或无权限访问");
                    }
                }

                // 验证提醒时间不能是过去时间
                if (createDto.ReminderTime <= DateTime.Now)
                {
                    return BadRequest("提醒时间不能是过去时间");
                }

                var reminder = new Reminder
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    TaskId = createDto.TaskId,
                    Title = createDto.Title,
                    Message = createDto.Message,
                    ReminderTime = createDto.ReminderTime,
                    Status = "pending",
                    Channels = JsonSerializer.Serialize(createDto.Channels),
                    RepeatPattern = createDto.RepeatPattern != null 
                        ? JsonSerializer.Serialize(createDto.RepeatPattern) 
                        : null,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Reminders.Add(reminder);
                await _context.SaveChangesAsync();

                // 获取创建的提醒（包含关联数据）
                var createdReminder = await _context.Reminders
                    .Include(r => r.Task)
                    .FirstOrDefaultAsync(r => r.Id == reminder.Id);

                var reminderDto = new ReminderDto
                {
                    Id = createdReminder!.Id,
                    UserId = createdReminder.UserId,
                    TaskId = createdReminder.TaskId,
                    TaskTitle = createdReminder.Task?.Title,
                    Title = createdReminder.Title,
                    Message = createdReminder.Message,
                    ReminderTime = createdReminder.ReminderTime,
                    Status = createdReminder.Status,
                    Channels = ParseChannels(createdReminder.Channels),
                    RepeatPattern = ParseRepeatPattern(createdReminder.RepeatPattern),
                    SnoozeUntil = createdReminder.SnoozeUntil,
                    CreatedAt = createdReminder.CreatedAt,
                    UpdatedAt = createdReminder.UpdatedAt
                };

                _logger.LogInformation("用户 {UserId} 创建了新提醒 {ReminderId}", userId, reminder.Id);
                return CreatedAtAction(nameof(GetReminder), new { id = reminder.Id }, reminderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建提醒失败");
                return StatusCode(500, "创建提醒失败");
            }
        }

        /// <summary>
        /// 更新提醒
        /// </summary>
        /// <param name="id">提醒ID</param>
        /// <param name="updateDto">更新提醒DTO</param>
        /// <returns>更新后的提醒</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ReminderDto>> UpdateReminder(Guid id, [FromBody] UpdateReminderDto updateDto)
        {
            try
            {
                var userId = GetCurrentUserId();

                var reminder = await _context.Reminders
                    .Include(r => r.Task)
                    .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

                if (reminder == null)
                {
                    return NotFound("提醒不存在或无权限访问");
                }

                // 更新字段
                if (!string.IsNullOrEmpty(updateDto.Title))
                {
                    reminder.Title = updateDto.Title;
                }

                if (updateDto.Message != null)
                {
                    reminder.Message = updateDto.Message;
                }

                if (updateDto.ReminderTime.HasValue)
                {
                    if (updateDto.ReminderTime <= DateTime.Now)
                    {
                        return BadRequest("提醒时间不能是过去时间");
                    }
                    reminder.ReminderTime = updateDto.ReminderTime.Value;
                }

                if (!string.IsNullOrEmpty(updateDto.Status))
                {
                    reminder.Status = updateDto.Status;
                }

                if (updateDto.Channels != null)
                {
                    reminder.Channels = JsonSerializer.Serialize(updateDto.Channels);
                }

                if (updateDto.RepeatPattern != null)
                {
                    reminder.RepeatPattern = JsonSerializer.Serialize(updateDto.RepeatPattern);
                }

                if (updateDto.SnoozeUntil.HasValue)
                {
                    reminder.SnoozeUntil = updateDto.SnoozeUntil;
                }

                reminder.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                var reminderDto = new ReminderDto
                {
                    Id = reminder.Id,
                    UserId = reminder.UserId,
                    TaskId = reminder.TaskId,
                    TaskTitle = reminder.Task?.Title,
                    Title = reminder.Title,
                    Message = reminder.Message,
                    ReminderTime = reminder.ReminderTime,
                    Status = reminder.Status,
                    Channels = ParseChannels(reminder.Channels),
                    RepeatPattern = ParseRepeatPattern(reminder.RepeatPattern),
                    SnoozeUntil = reminder.SnoozeUntil,
                    CreatedAt = reminder.CreatedAt,
                    UpdatedAt = reminder.UpdatedAt
                };

                _logger.LogInformation("用户 {UserId} 更新了提醒 {ReminderId}", userId, id);
                return Ok(reminderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新提醒失败，ID: {ReminderId}", id);
                return StatusCode(500, "更新提醒失败");
            }
        }

        /// <summary>
        /// 删除提醒
        /// </summary>
        /// <param name="id">提醒ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteReminder(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var reminder = await _context.Reminders
                    .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

                if (reminder == null)
                {
                    return NotFound("提醒不存在或无权限访问");
                }

                _context.Reminders.Remove(reminder);
                await _context.SaveChangesAsync();

                _logger.LogInformation("用户 {UserId} 删除了提醒 {ReminderId}", userId, id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除提醒失败，ID: {ReminderId}", id);
                return StatusCode(500, "删除提醒失败");
            }
        }

        /// <summary>
        /// 延迟提醒
        /// </summary>
        /// <param name="id">提醒ID</param>
        /// <param name="snoozeDto">延迟DTO</param>
        /// <returns>延迟结果</returns>
        [HttpPost("{id}/snooze")]
        public async Task<ActionResult> SnoozeReminder(Guid id, [FromBody] SnoozeReminderDto snoozeDto)
        {
            try
            {
                var userId = GetCurrentUserId();

                var reminder = await _context.Reminders
                    .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

                if (reminder == null)
                {
                    return NotFound("提醒不存在或无权限访问");
                }

                if (snoozeDto.SnoozeUntil <= DateTime.Now)
                {
                    return BadRequest("延迟时间不能是过去时间");
                }

                reminder.SnoozeUntil = snoozeDto.SnoozeUntil;
                reminder.Status = "snoozed";
                reminder.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("用户 {UserId} 延迟了提醒 {ReminderId} 到 {SnoozeUntil}",
                    userId, id, snoozeDto.SnoozeUntil);
                return Ok(new { message = "提醒已延迟", snoozeUntil = snoozeDto.SnoozeUntil });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "延迟提醒失败，ID: {ReminderId}", id);
                return StatusCode(500, "延迟提醒失败");
            }
        }

        /// <summary>
        /// 标记提醒为已完成
        /// </summary>
        /// <param name="id">提醒ID</param>
        /// <returns>完成结果</returns>
        [HttpPost("{id}/complete")]
        public async Task<ActionResult> CompleteReminder(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var reminder = await _context.Reminders
                    .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

                if (reminder == null)
                {
                    return NotFound("提醒不存在或无权限访问");
                }

                reminder.Status = "completed";
                reminder.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("用户 {UserId} 完成了提醒 {ReminderId}", userId, id);
                return Ok(new { message = "提醒已标记为完成" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "完成提醒失败，ID: {ReminderId}", id);
                return StatusCode(500, "完成提醒失败");
            }
        }

        /// <summary>
        /// 获取提醒统计信息
        /// </summary>
        /// <returns>统计信息</returns>
        [HttpGet("stats")]
        public async Task<ActionResult<ReminderStatsDto>> GetReminderStats()
        {
            try
            {
                var userId = GetCurrentUserId();
                var now = DateTime.Now;
                var today = now.Date;
                var weekStart = today.AddDays(-(int)today.DayOfWeek);

                var reminders = await _context.Reminders
                    .Where(r => r.UserId == userId)
                    .ToListAsync();

                var stats = new ReminderStatsDto
                {
                    TotalReminders = reminders.Count,
                    PendingReminders = reminders.Count(r => r.Status == "pending"),
                    CompletedReminders = reminders.Count(r => r.Status == "completed"),
                    SnoozedReminders = reminders.Count(r => r.Status == "snoozed"),
                    TodayReminders = reminders.Count(r => r.ReminderTime.Date == today),
                    WeekReminders = reminders.Count(r => r.ReminderTime.Date >= weekStart && r.ReminderTime.Date < weekStart.AddDays(7))
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取提醒统计失败");
                return StatusCode(500, "获取提醒统计失败");
            }
        }

        /// <summary>
        /// 获取任务的所有提醒
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>任务提醒列表</returns>
        [HttpGet("task/{taskId}")]
        public async Task<ActionResult<List<ReminderDto>>> GetTaskReminders(Guid taskId)
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

                var reminders = await _context.Reminders
                    .Include(r => r.Task)
                    .Where(r => r.TaskId == taskId && r.UserId == userId)
                    .OrderBy(r => r.ReminderTime)
                    .ToListAsync();

                var reminderDtos = reminders.Select(r => new ReminderDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    TaskId = r.TaskId,
                    TaskTitle = r.Task?.Title,
                    Title = r.Title,
                    Message = r.Message,
                    ReminderTime = r.ReminderTime,
                    Status = r.Status,
                    Channels = ParseChannels(r.Channels),
                    RepeatPattern = ParseRepeatPattern(r.RepeatPattern),
                    SnoozeUntil = r.SnoozeUntil,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                }).ToList();

                return Ok(reminderDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取任务提醒失败，TaskId: {TaskId}", taskId);
                return StatusCode(500, "获取任务提醒失败");
            }
        }

        /// <summary>
        /// 解析渠道JSON字符串
        /// </summary>
        /// <param name="channelsJson">渠道JSON字符串</param>
        /// <returns>渠道列表</returns>
        private List<string> ParseChannels(string channelsJson)
        {
            try
            {
                if (string.IsNullOrEmpty(channelsJson))
                {
                    return new List<string> { "web" };
                }

                var channels = JsonSerializer.Deserialize<List<string>>(channelsJson);
                return channels ?? new List<string> { "web" };
            }
            catch
            {
                return new List<string> { "web" };
            }
        }

        /// <summary>
        /// 解析重复模式JSON字符串
        /// </summary>
        /// <param name="repeatPatternJson">重复模式JSON字符串</param>
        /// <returns>重复模式对象</returns>
        private ReminderRepeatPattern? ParseRepeatPattern(string? repeatPatternJson)
        {
            try
            {
                if (string.IsNullOrEmpty(repeatPatternJson))
                {
                    return null;
                }

                return JsonSerializer.Deserialize<ReminderRepeatPattern>(repeatPatternJson);
            }
            catch
            {
                return null;
            }
        }
    }
}
