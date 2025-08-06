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
    public class UserProfileController : ControllerBase
    {
        private readonly ToDoListAreaDbContext _context;
        private readonly ILogger<UserProfileController> _logger;

        public UserProfileController(ToDoListAreaDbContext context, ILogger<UserProfileController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 获取用户详细资料
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户详细资料</returns>
        [HttpGet("{userId}")]
        public async Task<ActionResult<ApiResponse<UserProfileDetailDto>>> GetProfile(Guid userId)
        {
            try
            {
                var profile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (profile == null)
                {
                    // 如果用户资料不存在，创建默认资料
                    profile = new UserProfile
                    {
                        UserId = userId,
                        Timezone = "Asia/Shanghai",
                        Language = "zh-CN",
                        DateFormat = "YYYY-MM-DD",
                        TimeFormat = "24h",
                        NotificationPreferences = JsonSerializer.Serialize(new
                        {
                            email = true,
                            push = true,
                            desktop = true,
                            quietHours = new { start = "22:00", end = "08:00" }
                        }),
                        ThemePreferences = JsonSerializer.Serialize(new
                        {
                            theme = "light",
                            primaryColor = "#1890ff",
                            compactMode = false
                        }),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.UserProfiles.Add(profile);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("为用户 {UserId} 创建了默认资料", userId);
                }

                var profileDto = new UserProfileDetailDto
                {
                    UserId = profile.UserId,
                    FirstName = profile.FirstName,
                    LastName = profile.LastName,
                    Timezone = profile.Timezone,
                    Language = profile.Language,
                    DateFormat = profile.DateFormat,
                    TimeFormat = profile.TimeFormat,
                    NotificationPreferences = JsonSerializer.Deserialize<NotificationPreferencesDto>(
                        profile.NotificationPreferences ?? "{}") ?? new NotificationPreferencesDto(),
                    ThemePreferences = JsonSerializer.Deserialize<ThemePreferencesDto>(
                        profile.ThemePreferences ?? "{}") ?? new ThemePreferencesDto()
                };

                return Ok(ApiResponse<UserProfileDetailDto>.SuccessResult(profileDto, "获取用户资料成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户资料失败，用户ID: {UserId}", userId);
                return StatusCode(500, ApiResponse<UserProfileDetailDto>.ErrorResult("获取用户资料失败"));
            }
        }

        /// <summary>
        /// 更新用户详细资料
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="updateDto">更新数据</param>
        /// <returns>更新结果</returns>
        [HttpPut("{userId}")]
        public async Task<ActionResult<ApiResponse<UserProfileDetailDto>>> UpdateProfile(
            Guid userId, [FromBody] UserProfileUpdateDto updateDto)
        {
            try
            {
                var profile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (profile == null)
                {
                    return NotFound(ApiResponse<UserProfileDetailDto>.ErrorResult("用户资料不存在"));
                }

                // 更新基本信息
                if (!string.IsNullOrEmpty(updateDto.FirstName))
                    profile.FirstName = updateDto.FirstName;

                if (!string.IsNullOrEmpty(updateDto.LastName))
                    profile.LastName = updateDto.LastName;

                if (!string.IsNullOrEmpty(updateDto.Timezone))
                    profile.Timezone = updateDto.Timezone;

                if (!string.IsNullOrEmpty(updateDto.Language))
                    profile.Language = updateDto.Language;

                if (!string.IsNullOrEmpty(updateDto.DateFormat))
                    profile.DateFormat = updateDto.DateFormat;

                if (!string.IsNullOrEmpty(updateDto.TimeFormat))
                    profile.TimeFormat = updateDto.TimeFormat;

                // 更新通知偏好
                if (updateDto.NotificationPreferences != null)
                {
                    profile.NotificationPreferences = JsonSerializer.Serialize(updateDto.NotificationPreferences);
                }

                // 更新主题偏好
                if (updateDto.ThemePreferences != null)
                {
                    profile.ThemePreferences = JsonSerializer.Serialize(updateDto.ThemePreferences);
                }

                profile.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("用户资料更新成功，用户ID: {UserId}", userId);

                // 返回更新后的资料
                var updatedProfileDto = new UserProfileDetailDto
                {
                    UserId = profile.UserId,
                    FirstName = profile.FirstName,
                    LastName = profile.LastName,
                    Timezone = profile.Timezone,
                    Language = profile.Language,
                    DateFormat = profile.DateFormat,
                    TimeFormat = profile.TimeFormat,
                    NotificationPreferences = JsonSerializer.Deserialize<NotificationPreferencesDto>(
                        profile.NotificationPreferences ?? "{}") ?? new NotificationPreferencesDto(),
                    ThemePreferences = JsonSerializer.Deserialize<ThemePreferencesDto>(
                        profile.ThemePreferences ?? "{}") ?? new ThemePreferencesDto()
                };

                return Ok(ApiResponse<UserProfileDetailDto>.SuccessResult(updatedProfileDto, "用户资料更新成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户资料失败，用户ID: {UserId}", userId);
                return StatusCode(500, ApiResponse<UserProfileDetailDto>.ErrorResult("更新用户资料失败"));
            }
        }
    }
}
