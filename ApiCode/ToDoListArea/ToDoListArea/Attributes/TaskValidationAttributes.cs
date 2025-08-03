using System.ComponentModel.DataAnnotations;
using ToDoListArea.Enums;

namespace ToDoListArea.Attributes
{
    /// <summary>
    /// 任务状态验证特性
    /// </summary>
    public class ValidTaskStatusAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null) return true; // 允许空值，由Required特性处理

            if (value is string status)
            {
                return Enum.TryParse<TodoTaskStatus>(status, true, out _);
            }

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            var validValues = string.Join(", ", Enum.GetNames<TodoTaskStatus>());
            return $"{name} 必须是以下值之一: {validValues}";
        }
    }

    /// <summary>
    /// 任务优先级验证特性
    /// </summary>
    public class ValidTaskPriorityAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null) return true; // 允许空值，由Required特性处理

            if (value is string priority)
            {
                return Enum.TryParse<TodoTaskPriority>(priority, true, out _);
            }

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            var validValues = string.Join(", ", Enum.GetNames<TodoTaskPriority>());
            return $"{name} 必须是以下值之一: {validValues}";
        }
    }
}
