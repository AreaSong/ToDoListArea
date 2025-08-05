using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DbContextHelp.Models;
using ToDoListArea.Attributes;
using ToDoListArea.DTOs;
using ToDoListArea.Extensions;
using ToDoListArea.Models;
using ToDoListArea.Services;

namespace ToDoListArea.Controllers
{
    /// <summary>
    /// 管理员控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [AdminOnly]
    public class AdminController : ControllerBase
    {
        private readonly ToDoListAreaDbContext _context;
        private readonly IInvitationCodeService _invitationCodeService;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ToDoListAreaDbContext context,
            IInvitationCodeService invitationCodeService,
            IPermissionService permissionService,
            ILogger<AdminController> logger)
        {
            _context = context;
            _invitationCodeService = invitationCodeService;
            _permissionService = permissionService;
            _logger = logger;
        }

        #region 用户管理

        /// <summary>
        /// 获取所有用户列表
        /// </summary>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="search">搜索关键词</param>
        /// <param name="role">角色筛选</param>
        /// <param name="status">状态筛选</param>
        /// <returns>用户列表</returns>
        [HttpGet("users")]
        public async Task<ActionResult<ApiResponse<PagedResultDto<AdminUserDto>>>> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? role = null,
            [FromQuery] string? status = null)
        {
            try
            {
                var query = _context.Users.AsQueryable();

                // 搜索筛选
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(u => u.Name.Contains(search) || u.Email.Contains(search));
                }

                // 角色筛选
                if (!string.IsNullOrEmpty(role))
                {
                    query = query.Where(u => u.Role == role);
                }

                // 状态筛选 - 使用不区分大小写的比较
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(u => string.Equals(u.Status, status, StringComparison.OrdinalIgnoreCase));
                }

                var totalCount = await query.CountAsync();

                var users = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new AdminUserDto
                    {
                        Id = u.Id,
                        Email = u.Email,
                        Name = u.Name,
                        Phone = u.Phone,
                        AvatarUrl = u.AvatarUrl,
                        Status = u.Status,
                        Role = u.Role,
                        EmailVerified = u.EmailVerified,
                        PhoneVerified = u.PhoneVerified,
                        LastLoginAt = u.LastLoginAt,
                        CreatedAt = u.CreatedAt,
                        UpdatedAt = u.UpdatedAt,
                        TaskCount = u.Tasks.Count()
                    })
                    .ToListAsync();

                var result = new PagedResultDto<AdminUserDto>
                {
                    Items = users,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };

                return Ok(ApiResponse<PagedResultDto<AdminUserDto>>.SuccessResult(result, "获取用户列表成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户列表失败");
                return StatusCode(500, ApiResponse<PagedResultDto<AdminUserDto>>.ErrorResult("获取用户列表失败"));
            }
        }

        /// <summary>
        /// 获取用户详情
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>用户详情</returns>
        [HttpGet("users/{id}")]
        public async Task<ActionResult<ApiResponse<AdminUserDetailDto>>> GetUserDetail(Guid id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Tasks)
                    .Include(u => u.InvitationCodeUsages)
                        .ThenInclude(icu => icu.InvitationCode)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound(ApiResponse<AdminUserDetailDto>.ErrorResult("用户不存在"));
                }

                var userDetail = new AdminUserDetailDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    Phone = user.Phone,
                    AvatarUrl = user.AvatarUrl,
                    Status = user.Status,
                    Role = user.Role,
                    EmailVerified = user.EmailVerified,
                    PhoneVerified = user.PhoneVerified,
                    LastLoginAt = user.LastLoginAt,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    TaskCount = user.Tasks.Count,
                    CompletedTaskCount = user.Tasks.Count(t => t.Status == "completed"),
                    InvitationCodeUsed = user.InvitationCodeUsages.FirstOrDefault()?.InvitationCode.Code,
                    Permissions = await _permissionService.GetUserPermissionsAsync(user.Id)
                };

                return Ok(ApiResponse<AdminUserDetailDto>.SuccessResult(userDetail, "获取用户详情成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户详情失败: {UserId}", id);
                return StatusCode(500, ApiResponse<AdminUserDetailDto>.ErrorResult("获取用户详情失败"));
            }
        }

        /// <summary>
        /// 更新用户角色
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="request">角色更新请求</param>
        /// <returns>更新结果</returns>
        [HttpPut("users/{id}/role")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleDto request)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResult("用户不存在"));
                }

                var currentUserId = User.GetUserId();
                if (currentUserId == id)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("不能修改自己的角色"));
                }

                user.Role = request.Role;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("管理员 {AdminId} 将用户 {UserId} 的角色更新为 {Role}", 
                    currentUserId, id, request.Role);

                return Ok(ApiResponse<bool>.SuccessResult(true, "用户角色更新成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户角色失败: {UserId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("更新用户角色失败"));
            }
        }

        /// <summary>
        /// 更新用户状态
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="request">状态更新请求</param>
        /// <returns>更新结果</returns>
        [HttpPut("users/{id}/status")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusDto request)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResult("用户不存在"));
                }

                var currentUserId = User.GetUserId();
                if (currentUserId == id)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("不能修改自己的状态"));
                }

                // 统一状态格式为小写
                user.Status = request.Status.ToLower();
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("管理员 {AdminId} 将用户 {UserId} 的状态更新为 {Status}", 
                    currentUserId, id, request.Status);

                return Ok(ApiResponse<bool>.SuccessResult(true, "用户状态更新成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户状态失败: {UserId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("更新用户状态失败"));
            }
        }

        /// <summary>
        /// 获取用户的待办事项
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <returns>用户待办事项列表</returns>
        [HttpGet("users/{id}/tasks")]
        public async Task<ActionResult<ApiResponse<PagedResultDto<AdminTaskDto>>>> GetUserTasks(
            Guid id, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse<PagedResultDto<AdminTaskDto>>.ErrorResult("用户不存在"));
                }

                var query = _context.Tasks.Where(t => t.UserId == id);
                var totalCount = await query.CountAsync();

                var tasks = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new AdminTaskDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        Status = t.Status,
                        Priority = t.Priority,
                        DueDate = t.EndTime, // 使用 EndTime 作为截止日期
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    })
                    .ToListAsync();

                var result = new PagedResultDto<AdminTaskDto>
                {
                    Items = tasks,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };

                return Ok(ApiResponse<PagedResultDto<AdminTaskDto>>.SuccessResult(result, "获取用户待办事项成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户待办事项失败: {UserId}", id);
                return StatusCode(500, ApiResponse<PagedResultDto<AdminTaskDto>>.ErrorResult("获取用户待办事项失败"));
            }
        }

        #endregion

        #region 系统统计

        /// <summary>
        /// 获取系统统计信息
        /// </summary>
        /// <returns>统计信息</returns>
        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponse<AdminStatsDto>>> GetSystemStats()
        {
            try
            {
                var now = DateTime.UtcNow;
                var today = now.Date;
                var weekStart = today.AddDays(-(int)today.DayOfWeek);
                var monthStart = new DateTime(today.Year, today.Month, 1);

                var stats = new AdminStatsDto
                {
                    TotalUsers = await _context.Users.CountAsync(),
                    ActiveUsers = await _context.Users.CountAsync(u => string.Equals(u.Status, "active", StringComparison.OrdinalIgnoreCase)),
                    AdminUsers = await _context.Users.CountAsync(u => u.Role == "admin"),
                    TotalTasks = await _context.Tasks.CountAsync(),
                    CompletedTasks = await _context.Tasks.CountAsync(t => t.Status == "completed"),
                    TodayRegistrations = await _context.Users.CountAsync(u => u.CreatedAt >= today),
                    WeekRegistrations = await _context.Users.CountAsync(u => u.CreatedAt >= weekStart),
                    MonthRegistrations = await _context.Users.CountAsync(u => u.CreatedAt >= monthStart)
                };

                // 获取邀请码统计
                var invitationStats = await _invitationCodeService.GetStatsAsync();
                if (invitationStats.IsSuccess)
                {
                    stats.InvitationCodeStats = invitationStats.Data!;
                }

                return Ok(ApiResponse<AdminStatsDto>.SuccessResult(stats, "获取系统统计信息成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取系统统计信息失败");
                return StatusCode(500, ApiResponse<AdminStatsDto>.ErrorResult("获取系统统计信息失败"));
            }
        }

        #endregion
    }
}
