using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using DbContextHelp.Models;
using ToDoListArea.Models;
using ToDoListArea.Utils;
using ToDoListArea.Services;

namespace ToDoListArea.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ToDoListAreaDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IUserService _userService;
        private readonly IInvitationCodeService _invitationCodeService;

        public UserController(
            ToDoListAreaDbContext context,
            IJwtService jwtService,
            IUserService userService,
            IInvitationCodeService invitationCodeService)
        {
            _context = context;
            _jwtService = jwtService;
            _userService = userService;
            _invitationCodeService = invitationCodeService;
        }
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
                // 转换为服务层请求模型
                var registerRequest = new UserRegisterRequest
                {
                    Email = registerDto.Email,
                    Password = registerDto.Password,
                    Name = registerDto.Name,
                    Phone = registerDto.Phone,
                    InvitationCode = registerDto.InvitationCode
                };

                // 调用服务层注册方法
                var result = await _userService.RegisterAsync(registerRequest);

                if (!result.IsSuccess)
                {
                    return BadRequest(ApiResponse<UserProfileDto>.ErrorResult(result.ErrorMessage!));
                }

                var user = result.Data!;

                // 使用邀请码（记录IP和User Agent）
                var ipAddress = GetClientIpAddress();
                var userAgent = Request.Headers["User-Agent"].ToString();

                await _invitationCodeService.UseAsync(registerDto.InvitationCode, user.Id, ipAddress, userAgent);

                // 返回用户信息
                var userProfile = new UserProfileDto
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
                // 查找用户
                var user = await _context.Users
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

                // 检查用户状态 - 修复大小写不匹配问题
                if (!user.Status.Equals("active", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(ApiResponse<LoginResponseDto>.ErrorResult("账户已被禁用"));
                }

                // 更新最后登录时间
                user.LastLoginAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // 生成JWT Token
                var token = _jwtService.GenerateToken(user);

                var userProfile = new UserProfileDto
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
                    CreatedAt = user.CreatedAt
                };

                var loginResponse = new LoginResponseDto
                {
                    Token = token,
                    User = userProfile,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60) // 与JWT设置中的ExpiryInMinutes一致
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
        [Authorize] // 需要JWT认证
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetProfile(Guid userId)
        {
            try
            {
                var user = await _context.Users
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
        [Authorize] // 需要JWT认证
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateProfile(Guid userId, [FromBody] UserUpdateDto updateDto)
        {
            try
            {
                var user = await _context.Users
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
                await _context.SaveChangesAsync();

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

        /// <summary>
        /// 获取客户端IP地址
        /// </summary>
        /// <returns>IP地址</returns>
        private string GetClientIpAddress()
        {
            // 检查是否有代理服务器转发的真实IP
            var xForwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                // X-Forwarded-For可能包含多个IP，取第一个
                return xForwardedFor.Split(',')[0].Trim();
            }

            // 检查X-Real-IP头
            var xRealIp = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xRealIp))
            {
                return xRealIp;
            }

            // 使用连接的远程IP地址
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
