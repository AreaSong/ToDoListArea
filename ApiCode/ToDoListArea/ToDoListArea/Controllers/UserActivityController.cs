using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DbContextHelp.Models;
using ToDoListArea.Models;
using System.Security.Claims;

namespace ToDoListArea.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserActivityController : ControllerBase
    {
        private readonly ToDoListAreaDbContext _context;
        private readonly ILogger<UserActivityController> _logger;

        public UserActivityController(ToDoListAreaDbContext context, ILogger<UserActivityController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 获取用户活动列表
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<List<UserActivityDto>>>> GetUserActivities(
            Guid userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? activityType = null)
        {
            try
            {
                var query = _context.UserActivities
                    .Where(a => a.UserId == userId);

                // 按活动类型筛选
                if (!string.IsNullOrEmpty(activityType))
                {
                    query = query.Where(a => a.ActivityType == activityType);
                }

                // 分页和排序
                var activities = await query
                    .OrderByDescending(a => a.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new UserActivityDto
                    {
                        Id = a.Id,
                        UserId = a.UserId,
                        ActivityType = a.ActivityType,
                        ActivityDescription = a.ActivityDescription,
                        EntityType = a.EntityType,
                        EntityId = a.EntityId,
                        Metadata = a.Metadata,
                        IpAddress = a.IpAddress,
                        UserAgent = a.UserAgent,
                        CreatedAt = a.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<UserActivityDto>>
                {
                    Success = true,
                    Data = activities,
                    Message = "获取用户活动列表成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户活动列表失败: UserId={UserId}", userId);
                return StatusCode(500, new ApiResponse<List<UserActivityDto>>
                {
                    Success = false,
                    Message = "获取用户活动列表失败"
                });
            }
        }

        /// <summary>
        /// 获取用户活动统计
        /// </summary>
        [HttpGet("user/{userId}/stats")]
        public async Task<ActionResult<ApiResponse<UserActivityStatsDto>>> GetUserActivityStats(
            Guid userId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = _context.UserActivities
                    .Where(a => a.UserId == userId);

                // 时间范围筛选
                if (startDate.HasValue)
                {
                    query = query.Where(a => a.CreatedAt >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    query = query.Where(a => a.CreatedAt <= endDate.Value);
                }

                // 统计各种活动类型的数量
                var stats = await query
                    .GroupBy(a => a.ActivityType)
                    .Select(g => new ActivityTypeCount
                    {
                        ActivityType = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                // 总活动数
                var totalActivities = stats.Sum(s => s.Count);

                // 最近活动时间
                var lastActivityTime = await query
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => a.CreatedAt)
                    .FirstOrDefaultAsync();

                // 今日活动数
                var today = DateTime.Today;
                var todayActivities = await query
                    .Where(a => a.CreatedAt >= today && a.CreatedAt < today.AddDays(1))
                    .CountAsync();

                var result = new UserActivityStatsDto
                {
                    TotalActivities = totalActivities,
                    TodayActivities = todayActivities,
                    LastActivityTime = lastActivityTime,
                    ActivityTypeStats = stats
                };

                return Ok(new ApiResponse<UserActivityStatsDto>
                {
                    Success = true,
                    Data = result,
                    Message = "获取用户活动统计成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户活动统计失败: UserId={UserId}", userId);
                return StatusCode(500, new ApiResponse<UserActivityStatsDto>
                {
                    Success = false,
                    Message = "获取用户活动统计失败"
                });
            }
        }

        /// <summary>
        /// 手动记录用户活动
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserActivityDto>>> CreateUserActivity(
            [FromBody] CreateUserActivityDto createDto)
        {
            try
            {
                var activity = new UserActivity
                {
                    Id = Guid.NewGuid(),
                    UserId = createDto.UserId,
                    ActivityType = createDto.ActivityType,
                    ActivityDescription = createDto.ActivityDescription,
                    EntityType = createDto.EntityType,
                    EntityId = createDto.EntityId,
                    Metadata = createDto.Metadata,
                    IpAddress = GetClientIpAddress(),
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.UserActivities.Add(activity);
                await _context.SaveChangesAsync();

                var result = new UserActivityDto
                {
                    Id = activity.Id,
                    UserId = activity.UserId,
                    ActivityType = activity.ActivityType,
                    ActivityDescription = activity.ActivityDescription,
                    EntityType = activity.EntityType,
                    EntityId = activity.EntityId,
                    Metadata = activity.Metadata,
                    IpAddress = activity.IpAddress,
                    UserAgent = activity.UserAgent,
                    CreatedAt = activity.CreatedAt
                };

                return Ok(new ApiResponse<UserActivityDto>
                {
                    Success = true,
                    Data = result,
                    Message = "记录用户活动成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "记录用户活动失败: {@CreateDto}", createDto);
                return StatusCode(500, new ApiResponse<UserActivityDto>
                {
                    Success = false,
                    Message = "记录用户活动失败"
                });
            }
        }

        /// <summary>
        /// 获取活动类型列表
        /// </summary>
        [HttpGet("activity-types")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetActivityTypes()
        {
            try
            {
                var activityTypes = await _context.UserActivities
                    .Select(a => a.ActivityType)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync();

                return Ok(new ApiResponse<List<string>>
                {
                    Success = true,
                    Data = activityTypes,
                    Message = "获取活动类型列表成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取活动类型列表失败");
                return StatusCode(500, new ApiResponse<List<string>>
                {
                    Success = false,
                    Message = "获取活动类型列表失败"
                });
            }
        }

        /// <summary>
        /// 删除用户活动记录
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteUserActivity(Guid id)
        {
            try
            {
                var activity = await _context.UserActivities.FindAsync(id);
                if (activity == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "活动记录不存在"
                    });
                }

                _context.UserActivities.Remove(activity);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "删除活动记录成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除活动记录失败: Id={Id}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "删除活动记录失败"
                });
            }
        }

        /// <summary>
        /// 获取客户端IP地址
        /// </summary>
        private string GetClientIpAddress()
        {
            var ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = Request.Headers["X-Real-IP"].FirstOrDefault();
            }
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            }
            return ipAddress ?? "Unknown";
        }
    }
}
