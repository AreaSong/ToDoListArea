using System.ComponentModel.DataAnnotations;

namespace ToDoListArea.Models
{
    /// <summary>
    /// 任务分类响应模型
    /// </summary>
    public class TaskCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? Description { get; set; }
        public bool IsSystem { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// 任务分类创建请求模型
    /// </summary>
    public class TaskCategoryCreateDto
    {
        [Required(ErrorMessage = "分类名称不能为空")]
        [StringLength(100, ErrorMessage = "分类名称长度不能超过100个字符")]
        public string Name { get; set; } = string.Empty;

        [StringLength(7, ErrorMessage = "颜色值长度不能超过7个字符")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "颜色值格式不正确，应为十六进制格式如#FF0000")]
        public string? Color { get; set; }

        [StringLength(50, ErrorMessage = "图标名称长度不能超过50个字符")]
        public string? Icon { get; set; }

        [StringLength(500, ErrorMessage = "描述长度不能超过500个字符")]
        public string? Description { get; set; }

        public int? SortOrder { get; set; }
    }

    /// <summary>
    /// 任务分类更新请求模型
    /// </summary>
    public class TaskCategoryUpdateDto
    {
        [StringLength(100, ErrorMessage = "分类名称长度不能超过100个字符")]
        public string? Name { get; set; }

        [StringLength(7, ErrorMessage = "颜色值长度不能超过7个字符")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "颜色值格式不正确，应为十六进制格式如#FF0000")]
        public string? Color { get; set; }

        [StringLength(50, ErrorMessage = "图标名称长度不能超过50个字符")]
        public string? Icon { get; set; }

        [StringLength(500, ErrorMessage = "描述长度不能超过500个字符")]
        public string? Description { get; set; }

        public int? SortOrder { get; set; }
    }
}
