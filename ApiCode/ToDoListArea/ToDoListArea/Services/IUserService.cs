using DbContextHelp.Models;

namespace ToDoListArea.Services
{
    /// <summary>
    /// 用户服务接口
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// 用户注册
        /// </summary>
        Task<ServiceResult<User>> RegisterAsync(UserRegisterRequest request);

        /// <summary>
        /// 用户登录
        /// </summary>
        Task<ServiceResult<UserLoginResponse>> LoginAsync(UserLoginRequest request);

        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        Task<ServiceResult<User>> GetUserByIdAsync(Guid userId);

        /// <summary>
        /// 根据邮箱获取用户
        /// </summary>
        Task<ServiceResult<User>> GetUserByEmailAsync(string email);
    }

    /// <summary>
    /// 服务结果包装类
    /// </summary>
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }

        public static ServiceResult<T> Success(T data) => new() { IsSuccess = true, Data = data };
        public static ServiceResult<T> Failure(string message, string? code = null) => new() { IsSuccess = false, ErrorMessage = message, ErrorCode = code };
    }

    /// <summary>
    /// 用户注册请求
    /// </summary>
    public class UserRegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string InvitationCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// 用户登录请求
    /// </summary>
    public class UserLoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// 用户登录响应
    /// </summary>
    public class UserLoginResponse
    {
        public User User { get; set; } = null!;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
