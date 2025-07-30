using System.ComponentModel.DataAnnotations;

namespace ToDoListArea.Models
{
    /// <summary>
    /// 用户注册请求模型
    /// </summary>
    public class UserRegisterDto
    {
        [Required(ErrorMessage = "邮箱不能为空")]
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "密码不能为空")]
        [MinLength(6, ErrorMessage = "密码长度不能少于6位")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "用户名不能为空")]
        [StringLength(50, ErrorMessage = "用户名长度不能超过50个字符")]
        public string Name { get; set; } = string.Empty;

        [Phone(ErrorMessage = "手机号格式不正确")]
        public string? Phone { get; set; }
    }

    /// <summary>
    /// 用户登录请求模型
    /// </summary>
    public class UserLoginDto
    {
        [Required(ErrorMessage = "邮箱不能为空")]
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "密码不能为空")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// 用户信息更新请求模型
    /// </summary>
    public class UserUpdateDto
    {
        [StringLength(50, ErrorMessage = "用户名长度不能超过50个字符")]
        public string? Name { get; set; }

        [Phone(ErrorMessage = "手机号格式不正确")]
        public string? Phone { get; set; }

        public string? AvatarUrl { get; set; }
    }

    /// <summary>
    /// 用户信息响应模型
    /// </summary>
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool EmailVerified { get; set; }
        public bool PhoneVerified { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// 登录成功响应模型
    /// </summary>
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserProfileDto User { get; set; } = new();
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// API响应基础模型
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();

        public static ApiResponse<T> SuccessResult(T data, string message = "操作成功")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}
