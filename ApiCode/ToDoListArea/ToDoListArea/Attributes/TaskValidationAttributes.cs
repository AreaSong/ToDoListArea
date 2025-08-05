using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
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

    /// <summary>
    /// 安全字符串验证属性 - 防止XSS和SQL注入
    /// </summary>
    public class SafeStringAttribute : ValidationAttribute
    {
        private readonly bool _allowHtml;
        private readonly int _maxLength;

        public SafeStringAttribute(int maxLength = 1000, bool allowHtml = false)
        {
            _maxLength = maxLength;
            _allowHtml = allowHtml;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            string input = value.ToString()!;

            // 检查长度
            if (input.Length > _maxLength)
            {
                return new ValidationResult($"字符串长度不能超过 {_maxLength} 个字符");
            }

            // 检查危险字符
            if (!_allowHtml)
            {
                var dangerousPatterns = new[]
                {
                    @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>",
                    @"javascript:",
                    @"vbscript:",
                    @"onload\s*=",
                    @"onerror\s*=",
                    @"onclick\s*=",
                    @"<iframe\b",
                    @"<object\b",
                    @"<embed\b"
                };

                foreach (var pattern in dangerousPatterns)
                {
                    if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                    {
                        return new ValidationResult("输入包含潜在危险内容");
                    }
                }
            }

            // 检查SQL注入模式
            var sqlPatterns = new[]
            {
                @"(\b(ALTER|CREATE|DELETE|DROP|EXEC(UTE)?|INSERT( +INTO)?|MERGE|SELECT|UPDATE|UNION( +ALL)?)\b)",
                @"(\b(AND|OR)\b.{1,6}?(=|>|<|\!=|<>|<=|>=))",
                @"(\bCAST\s*\()",
                @"(\bCONVERT\s*\()"
            };

            foreach (var pattern in sqlPatterns)
            {
                if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                {
                    return new ValidationResult("输入包含潜在危险的SQL模式");
                }
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// 强密码验证属性
    /// </summary>
    public class StrongPasswordAttribute : ValidationAttribute
    {
        private readonly int _minLength;
        private readonly bool _requireUppercase;
        private readonly bool _requireLowercase;
        private readonly bool _requireDigit;
        private readonly bool _requireSpecialChar;

        public StrongPasswordAttribute(int minLength = 8, bool requireUppercase = true,
            bool requireLowercase = true, bool requireDigit = true, bool requireSpecialChar = true)
        {
            _minLength = minLength;
            _requireUppercase = requireUppercase;
            _requireLowercase = requireLowercase;
            _requireDigit = requireDigit;
            _requireSpecialChar = requireSpecialChar;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            string password = value.ToString()!;

            var errors = new List<string>();

            if (password.Length < _minLength)
                errors.Add($"密码长度至少需要 {_minLength} 个字符");

            if (_requireUppercase && !password.Any(char.IsUpper))
                errors.Add("密码必须包含至少一个大写字母");

            if (_requireLowercase && !password.Any(char.IsLower))
                errors.Add("密码必须包含至少一个小写字母");

            if (_requireDigit && !password.Any(char.IsDigit))
                errors.Add("密码必须包含至少一个数字");

            if (_requireSpecialChar && !password.Any(ch => !char.IsLetterOrDigit(ch)))
                errors.Add("密码必须包含至少一个特殊字符");

            // 检查常见弱密码
            var commonPasswords = new[] { "password", "123456", "qwerty", "admin", "letmein" };
            if (commonPasswords.Any(cp => password.ToLower().Contains(cp)))
                errors.Add("密码包含常见的弱密码模式");

            if (errors.Any())
                return new ValidationResult(string.Join("; ", errors));

            return ValidationResult.Success;
        }
    }
}
