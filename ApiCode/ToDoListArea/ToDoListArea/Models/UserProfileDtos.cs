using System.ComponentModel.DataAnnotations;

namespace ToDoListArea.Models
{
    /// <summary>
    /// 用户详细资料DTO
    /// </summary>
    public class UserProfileDetailDto
    {
        public Guid UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Timezone { get; set; } = "Asia/Shanghai";
        public string Language { get; set; } = "zh-CN";
        public string DateFormat { get; set; } = "YYYY-MM-DD";
        public string TimeFormat { get; set; } = "24h";
        public NotificationPreferencesDto NotificationPreferences { get; set; } = new();
        public ThemePreferencesDto ThemePreferences { get; set; } = new();
    }

    /// <summary>
    /// 用户资料更新DTO
    /// </summary>
    public class UserProfileUpdateDto
    {
        [StringLength(50, ErrorMessage = "姓的长度不能超过50个字符")]
        public string? FirstName { get; set; }

        [StringLength(50, ErrorMessage = "名的长度不能超过50个字符")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "时区不能为空")]
        public string Timezone { get; set; } = "Asia/Shanghai";

        [Required(ErrorMessage = "语言不能为空")]
        public string Language { get; set; } = "zh-CN";

        [Required(ErrorMessage = "日期格式不能为空")]
        public string DateFormat { get; set; } = "YYYY-MM-DD";

        [Required(ErrorMessage = "时间格式不能为空")]
        public string TimeFormat { get; set; } = "24h";

        public NotificationPreferencesDto? NotificationPreferences { get; set; }
        public ThemePreferencesDto? ThemePreferences { get; set; }
    }

    /// <summary>
    /// 通知偏好设置DTO
    /// </summary>
    public class NotificationPreferencesDto
    {
        public bool Email { get; set; } = true;
        public bool Push { get; set; } = true;
        public bool Desktop { get; set; } = true;
        public QuietHoursDto QuietHours { get; set; } = new();
    }

    /// <summary>
    /// 免打扰时间设置DTO
    /// </summary>
    public class QuietHoursDto
    {
        [Required(ErrorMessage = "免打扰开始时间不能为空")]
        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "时间格式不正确，应为HH:mm")]
        public string Start { get; set; } = "22:00";

        [Required(ErrorMessage = "免打扰结束时间不能为空")]
        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "时间格式不正确，应为HH:mm")]
        public string End { get; set; } = "08:00";
    }

    /// <summary>
    /// 主题偏好设置DTO
    /// </summary>
    public class ThemePreferencesDto
    {
        [Required(ErrorMessage = "主题模式不能为空")]
        public string Theme { get; set; } = "light"; // light, dark, auto

        [Required(ErrorMessage = "主色调不能为空")]
        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "颜色格式不正确")]
        public string PrimaryColor { get; set; } = "#1890ff";

        public bool CompactMode { get; set; } = false;
    }
}
