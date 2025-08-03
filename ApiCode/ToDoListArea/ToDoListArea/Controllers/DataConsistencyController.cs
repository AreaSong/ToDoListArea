using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ToDoListArea.Services;
using ToDoListArea.Enums;

namespace ToDoListArea.Controllers
{
    /// <summary>
    /// 数据一致性检查和修复控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 基础认证要求：所有操作都需要登录用户
    public class DataConsistencyController : ControllerBase
    {
        private readonly DataConsistencyService _dataConsistencyService;
        private readonly ILogger<DataConsistencyController> _logger;

        public DataConsistencyController(
            DataConsistencyService dataConsistencyService,
            ILogger<DataConsistencyController> logger)
        {
            _dataConsistencyService = dataConsistencyService;
            _logger = logger;
        }

        /// <summary>
        /// 检查数据一致性
        /// </summary>
        [HttpGet("check")]
        [Authorize(Policy = "DataConsistencyRead")] // 普通认证用户可查看
        public async Task<IActionResult> CheckDataConsistency()
        {
            try
            {
                var report = await _dataConsistencyService.CheckDataConsistencyAsync();
                
                return Ok(new
                {
                    success = true,
                    message = "数据一致性检查完成",
                    data = report
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据一致性检查失败");
                return StatusCode(500, new
                {
                    success = false,
                    message = "数据一致性检查失败",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 修复数据一致性问题
        /// </summary>
        [HttpPost("fix")]
        [Authorize(Policy = "DataConsistencyWrite")] // 修复操作需要管理员权限
        public async Task<IActionResult> FixDataConsistency()
        {
            try
            {
                var result = await _dataConsistencyService.FixDataConsistencyAsync();
                
                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"数据一致性修复完成，共修复 {result.TotalFixed} 条记录",
                        data = result
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "数据一致性修复失败",
                        error = result.ErrorMessage
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据一致性修复失败");
                return StatusCode(500, new
                {
                    success = false,
                    message = "数据一致性修复失败",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 获取标准的状态和优先级选项
        /// </summary>
        [HttpGet("options")]
        [Authorize(Policy = "DataConsistencyRead")] // 普通认证用户可查看
        public IActionResult GetStandardOptions()
        {
            try
            {
                var options = new
                {
                    statusOptions = EnumExtensions.GetTaskStatusOptions(),
                    priorityOptions = EnumExtensions.GetTaskPriorityOptions()
                };

                return Ok(new
                {
                    success = true,
                    message = "获取标准选项成功",
                    data = options
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取标准选项失败");
                return StatusCode(500, new
                {
                    success = false,
                    message = "获取标准选项失败",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 验证状态值是否有效
        /// </summary>
        [HttpGet("validate/status/{status}")]
        [Authorize(Policy = "DataConsistencyRead")] // 普通认证用户可验证
        public IActionResult ValidateStatus(string status)
        {
            var isValid = Enum.TryParse<TodoTaskStatus>(status, true, out var taskStatus);

            return Ok(new
            {
                success = true,
                data = new
                {
                    isValid = isValid,
                    normalizedValue = isValid ? taskStatus.ToString() : null,
                    description = isValid ? taskStatus.GetDescription() : null
                }
            });
        }

        /// <summary>
        /// 验证优先级值是否有效
        /// </summary>
        [HttpGet("validate/priority/{priority}")]
        [Authorize(Policy = "DataConsistencyRead")] // 普通认证用户可验证
        public IActionResult ValidatePriority(string priority)
        {
            var isValid = Enum.TryParse<TodoTaskPriority>(priority, true, out var taskPriority);

            return Ok(new
            {
                success = true,
                data = new
                {
                    isValid = isValid,
                    normalizedValue = isValid ? taskPriority.ToString() : null,
                    description = isValid ? taskPriority.GetDescription() : null
                }
            });
        }

#if DEBUG
        /// <summary>
        /// 开发环境专用：无认证的数据一致性检查（仅用于调试）
        /// </summary>
        [HttpGet("debug/check")]
        [AllowAnonymous] // 开发环境允许匿名访问
        public async Task<IActionResult> DebugCheckDataConsistency()
        {
            try
            {
                var report = await _dataConsistencyService.CheckDataConsistencyAsync();

                return Ok(new
                {
                    success = true,
                    message = "开发环境数据一致性检查完成",
                    environment = "Development",
                    data = report
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "开发环境数据一致性检查失败");
                return StatusCode(500, new
                {
                    success = false,
                    message = "开发环境数据一致性检查失败",
                    environment = "Development",
                    error = ex.Message
                });
            }
        }
#endif
    }
}
