using System.ComponentModel;

namespace ToDoListArea.Enums
{
    /// <summary>
    /// 任务状态枚举
    /// </summary>
    public enum TodoTaskStatus
    {
        [Description("待处理")]
        Pending = 0,

        [Description("进行中")]
        InProgress = 1,

        [Description("已完成")]
        Completed = 2
    }

    /// <summary>
    /// 任务优先级枚举
    /// </summary>
    public enum TodoTaskPriority
    {
        [Description("低")]
        Low = 0,

        [Description("中")]
        Medium = 1,

        [Description("高")]
        High = 2
    }

    /// <summary>
    /// 枚举扩展方法
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// 获取枚举的描述信息
        /// </summary>
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            if (field != null)
            {
                var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
                return attribute?.Description ?? value.ToString();
            }
            return value.ToString();
        }

        /// <summary>
        /// 将字符串转换为TodoTaskStatus枚举
        /// </summary>
        public static TodoTaskStatus ToTodoTaskStatus(this string status)
        {
            return status?.ToLower() switch
            {
                "pending" => TodoTaskStatus.Pending,
                "inprogress" => TodoTaskStatus.InProgress,
                "completed" => TodoTaskStatus.Completed,
                _ => TodoTaskStatus.Pending
            };
        }

        /// <summary>
        /// 将字符串转换为TodoTaskPriority枚举
        /// </summary>
        public static TodoTaskPriority ToTodoTaskPriority(this string priority)
        {
            return priority?.ToLower() switch
            {
                "low" => TodoTaskPriority.Low,
                "medium" => TodoTaskPriority.Medium,
                "high" => TodoTaskPriority.High,
                _ => TodoTaskPriority.Medium
            };
        }

        /// <summary>
        /// 获取所有任务状态选项
        /// </summary>
        public static Dictionary<string, string> GetTaskStatusOptions()
        {
            return new Dictionary<string, string>
            {
                { TodoTaskStatus.Pending.ToString(), TodoTaskStatus.Pending.GetDescription() },
                { TodoTaskStatus.InProgress.ToString(), TodoTaskStatus.InProgress.GetDescription() },
                { TodoTaskStatus.Completed.ToString(), TodoTaskStatus.Completed.GetDescription() }
            };
        }

        /// <summary>
        /// 获取所有任务优先级选项
        /// </summary>
        public static Dictionary<string, string> GetTaskPriorityOptions()
        {
            return new Dictionary<string, string>
            {
                { TodoTaskPriority.Low.ToString(), TodoTaskPriority.Low.GetDescription() },
                { TodoTaskPriority.Medium.ToString(), TodoTaskPriority.Medium.GetDescription() },
                { TodoTaskPriority.High.ToString(), TodoTaskPriority.High.GetDescription() }
            };
        }
    }
}
