using DbContextHelp.Models;
using Microsoft.EntityFrameworkCore;
using ToDoListArea.Utils;

namespace ToDoListArea.Services
{
    /// <summary>
    /// 用户服务实现
    /// </summary>
    public class UserService : IUserService
    {
        private readonly ToDoListAreaDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IInvitationCodeService _invitationCodeService;

        public UserService(ToDoListAreaDbContext context, IJwtService jwtService, IInvitationCodeService invitationCodeService)
        {
            _context = context;
            _jwtService = jwtService;
            _invitationCodeService = invitationCodeService;
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        public async Task<ServiceResult<User>> RegisterAsync(UserRegisterRequest request)
        {
            try
            {
                // 验证邀请码
                var invitationValidation = await _invitationCodeService.ValidateAsync(request.InvitationCode);
                if (!invitationValidation.IsSuccess || !invitationValidation.Data!.IsValid)
                {
                    return ServiceResult<User>.Failure(invitationValidation.Data?.Message ?? "邀请码无效", "INVALID_INVITATION_CODE");
                }

                // 检查邮箱是否已存在
                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
                {
                    return ServiceResult<User>.Failure("邮箱已被注册", "EMAIL_EXISTS");
                }

                // 创建用户
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email.ToLower(),
                    Name = request.Name,
                    Phone = request.Phone,
                    PasswordHash = PasswordHelper.HashPassword(request.Password),
                    Status = "active",
                    Role = "user", // 默认角色为普通用户
                    EmailVerified = false,
                    PhoneVerified = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // 使用邀请码
                var useResult = await _invitationCodeService.UseAsync(
                    request.InvitationCode,
                    user.Id,
                    null, // IP地址在控制器层获取
                    null  // User Agent在控制器层获取
                );

                if (!useResult.IsSuccess)
                {
                    // 如果邀请码使用失败，记录日志但不影响注册流程
                    // 因为用户已经创建成功
                    // 可以考虑在这里添加日志记录
                }

                return ServiceResult<User>.Success(user);
            }
            catch (Exception ex)
            {
                return ServiceResult<User>.Failure($"注册失败：{ex.Message}", "REGISTER_ERROR");
            }
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        public async Task<ServiceResult<UserLoginResponse>> LoginAsync(UserLoginRequest request)
        {
            try
            {
                // 根据邮箱获取用户
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
                if (user == null)
                {
                    return ServiceResult<UserLoginResponse>.Failure("用户不存在", "USER_NOT_FOUND");
                }

                // 验证密码
                if (!PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
                {
                    return ServiceResult<UserLoginResponse>.Failure("密码错误", "INVALID_PASSWORD");
                }

                // 检查用户状态 - 使用不区分大小写的比较
                if (!string.Equals(user.Status, "active", StringComparison.OrdinalIgnoreCase))
                {
                    return ServiceResult<UserLoginResponse>.Failure("账户已被禁用", "ACCOUNT_DISABLED");
                }

                // 生成JWT Token
                var token = _jwtService.GenerateToken(user);
                var expiresAt = DateTime.Now.AddMinutes(60);

                // 更新最后登录时间
                user.LastLoginAt = DateTime.Now;
                user.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                var response = new UserLoginResponse
                {
                    User = user,
                    Token = token,
                    ExpiresAt = expiresAt
                };

                return ServiceResult<UserLoginResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return ServiceResult<UserLoginResponse>.Failure($"登录失败：{ex.Message}", "LOGIN_ERROR");
            }
        }

        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        public async Task<ServiceResult<User>> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return ServiceResult<User>.Failure("用户不存在", "USER_NOT_FOUND");
                }

                return ServiceResult<User>.Success(user);
            }
            catch (Exception ex)
            {
                return ServiceResult<User>.Failure($"获取用户失败：{ex.Message}", "GET_USER_ERROR");
            }
        }

        /// <summary>
        /// 根据邮箱获取用户
        /// </summary>
        public async Task<ServiceResult<User>> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                if (user == null)
                {
                    return ServiceResult<User>.Failure("用户不存在", "USER_NOT_FOUND");
                }

                return ServiceResult<User>.Success(user);
            }
            catch (Exception ex)
            {
                return ServiceResult<User>.Failure($"获取用户失败：{ex.Message}", "GET_USER_ERROR");
            }
        }
    }
}
