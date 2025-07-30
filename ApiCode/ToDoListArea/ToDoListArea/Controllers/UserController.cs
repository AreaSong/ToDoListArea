using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DbContextHelp.Models;
using ToDoListArea.Models;
using ToDoListArea.Utils;

namespace ToDoListArea.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="registerDto">注册信息</param>
        /// <returns>注册结果</returns>
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> Register([FromBody] UserRegisterDto registerDto)
        {
            try
            {
                using var context = new ToDoListAreaDbContext();

                // 检查邮箱是否已存在
                var existingUser = await context.Users
                    .FirstOrDefaultAsync(u => u.Email == registerDto.Email);

                if (existingUser != null)
                {
                    return BadRequest(ApiResponse<UserProfileDto>.ErrorResult("邮箱已被注册"));
                }

                // 创建新用户
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = registerDto.Email,
                    Name = registerDto.Name,
                    Phone = registerDto.Phone,
                    PasswordHash = PasswordHelper.HashPassword(registerDto.Password),
                    Status = "Active",
                    EmailVerified = false,
                    PhoneVerified = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.Users.Add(user);
                await context.SaveChangesAsync();

                // 返回用户信息
                var userProfile = new UserProfileDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    Phone = user.Phone,
                    AvatarUrl = user.AvatarUrl,
                    Status = user.Status,
                    EmailVerified = user.EmailVerified,
                    PhoneVerified = user.PhoneVerified,
                    CreatedAt = user.CreatedAt
                };

                return Ok(ApiResponse<UserProfileDto>.SuccessResult(userProfile, "注册成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserProfileDto>.ErrorResult($"注册失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="loginDto">登录信息</param>
        /// <returns>登录结果</returns>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] UserLoginDto loginDto)
        {
            try
            {
                using var context = new ToDoListAreaDbContext();

                // 查找用户
                var user = await context.Users
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

                if (user == null)
                {
                    return BadRequest(ApiResponse<LoginResponseDto>.ErrorResult("邮箱或密码错误"));
                }

                // 验证密码
                if (!PasswordHelper.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    return BadRequest(ApiResponse<LoginResponseDto>.ErrorResult("邮箱或密码错误"));
                }

                // 检查用户状态
                if (user.Status != "Active")
                {
                    return BadRequest(ApiResponse<LoginResponseDto>.ErrorResult("账户已被禁用"));
                }

                // 更新最后登录时间
                user.LastLoginAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();

                // 生成简单的Token（实际项目中应使用JWT）
                var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{user.Id}:{DateTime.UtcNow.Ticks}"));

                var userProfile = new UserProfileDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    Phone = user.Phone,
                    AvatarUrl = user.AvatarUrl,
                    Status = user.Status,
                    EmailVerified = user.EmailVerified,
                    PhoneVerified = user.PhoneVerified,
                    LastLoginAt = user.LastLoginAt,
                    CreatedAt = user.CreatedAt
                };

                var loginResponse = new LoginResponseDto
                {
                    Token = token,
                    User = userProfile,
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                };

                return Ok(ApiResponse<LoginResponseDto>.SuccessResult(loginResponse, "登录成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<LoginResponseDto>.ErrorResult($"登录失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户信息</returns>
        [HttpGet("profile/{userId}")]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetProfile(Guid userId)
        {
            try
            {
                using var context = new ToDoListAreaDbContext();

                var user = await context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound(ApiResponse<UserProfileDto>.ErrorResult("用户不存在"));
                }

                var userProfile = new UserProfileDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    Phone = user.Phone,
                    AvatarUrl = user.AvatarUrl,
                    Status = user.Status,
                    EmailVerified = user.EmailVerified,
                    PhoneVerified = user.PhoneVerified,
                    LastLoginAt = user.LastLoginAt,
                    CreatedAt = user.CreatedAt
                };

                return Ok(ApiResponse<UserProfileDto>.SuccessResult(userProfile, "获取用户信息成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserProfileDto>.ErrorResult($"获取用户信息失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="updateDto">更新信息</param>
        /// <returns>更新结果</returns>
        [HttpPut("profile/{userId}")]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateProfile(Guid userId, [FromBody] UserUpdateDto updateDto)
        {
            try
            {
                using var context = new ToDoListAreaDbContext();

                var user = await context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound(ApiResponse<UserProfileDto>.ErrorResult("用户不存在"));
                }

                // 更新用户信息
                if (!string.IsNullOrEmpty(updateDto.Name))
                    user.Name = updateDto.Name;

                if (!string.IsNullOrEmpty(updateDto.Phone))
                    user.Phone = updateDto.Phone;

                if (!string.IsNullOrEmpty(updateDto.AvatarUrl))
                    user.AvatarUrl = updateDto.AvatarUrl;

                user.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();

                var userProfile = new UserProfileDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    Phone = user.Phone,
                    AvatarUrl = user.AvatarUrl,
                    Status = user.Status,
                    EmailVerified = user.EmailVerified,
                    PhoneVerified = user.PhoneVerified,
                    LastLoginAt = user.LastLoginAt,
                    CreatedAt = user.CreatedAt
                };

                return Ok(ApiResponse<UserProfileDto>.SuccessResult(userProfile, "更新用户信息成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserProfileDto>.ErrorResult($"更新用户信息失败: {ex.Message}"));
            }
        }
    }
}
