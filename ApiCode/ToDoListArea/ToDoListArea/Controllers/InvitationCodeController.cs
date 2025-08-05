using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ToDoListArea.DTOs;
using ToDoListArea.Models;
using ToDoListArea.Services;

namespace ToDoListArea.Controllers
{
    /// <summary>
    /// 邀请码管理控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InvitationCodeController : ControllerBase
    {
        private readonly IInvitationCodeService _invitationCodeService;
        private readonly ILogger<InvitationCodeController> _logger;

        public InvitationCodeController(
            IInvitationCodeService invitationCodeService,
            ILogger<InvitationCodeController> logger)
        {
            _invitationCodeService = invitationCodeService;
            _logger = logger;
        }

        /// <summary>
        /// 验证邀请码（公开接口，用于注册时验证）
        /// </summary>
        /// <param name="request">验证请求</param>
        /// <returns>验证结果</returns>
        [HttpPost("validate")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<InvitationCodeValidationDto>>> ValidateInvitationCode([FromBody] ValidateInvitationCodeDto request)
        {
            try
            {
                var result = await _invitationCodeService.ValidateAsync(request.Code);
                
                if (result.IsSuccess)
                {
                    return Ok(ApiResponse<InvitationCodeValidationDto>.SuccessResult(result.Data!, "验证完成"));
                }
                
                return BadRequest(ApiResponse<InvitationCodeValidationDto>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证邀请码异常: {Code}", request.Code);
                return StatusCode(500, ApiResponse<InvitationCodeValidationDto>.ErrorResult("服务器内部错误"));
            }
        }

        /// <summary>
        /// 创建邀请码（仅管理员）
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns>创建结果</returns>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponse<InvitationCodeDto>>> CreateInvitationCode([FromBody] CreateInvitationCodeDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(ApiResponse<InvitationCodeDto>.ErrorResult("用户未登录"));
                }

                var result = await _invitationCodeService.CreateAsync(request, userId.Value);
                
                if (result.IsSuccess)
                {
                    return Ok(ApiResponse<InvitationCodeDto>.SuccessResult(result.Data!, "邀请码创建成功"));
                }
                
                return BadRequest(ApiResponse<InvitationCodeDto>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建邀请码异常");
                return StatusCode(500, ApiResponse<InvitationCodeDto>.ErrorResult("服务器内部错误"));
            }
        }

        /// <summary>
        /// 获取邀请码列表（仅管理员）
        /// </summary>
        /// <param name="query">查询条件</param>
        /// <returns>邀请码列表</returns>
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponse<PagedResultDto<InvitationCodeDto>>>> GetInvitationCodes([FromQuery] InvitationCodeQueryDto query)
        {
            try
            {
                var result = await _invitationCodeService.GetListAsync(query);
                
                if (result.IsSuccess)
                {
                    return Ok(ApiResponse<PagedResultDto<InvitationCodeDto>>.SuccessResult(result.Data!, "获取成功"));
                }
                
                return BadRequest(ApiResponse<PagedResultDto<InvitationCodeDto>>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取邀请码列表异常");
                return StatusCode(500, ApiResponse<PagedResultDto<InvitationCodeDto>>.ErrorResult("服务器内部错误"));
            }
        }

        /// <summary>
        /// 根据ID获取邀请码详情（仅管理员）
        /// </summary>
        /// <param name="id">邀请码ID</param>
        /// <returns>邀请码详情</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponse<InvitationCodeDto>>> GetInvitationCodeById(Guid id)
        {
            try
            {
                var result = await _invitationCodeService.GetByIdAsync(id);
                
                if (result.IsSuccess)
                {
                    return Ok(ApiResponse<InvitationCodeDto>.SuccessResult(result.Data!, "获取成功"));
                }
                
                return NotFound(ApiResponse<InvitationCodeDto>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取邀请码详情异常: {Id}", id);
                return StatusCode(500, ApiResponse<InvitationCodeDto>.ErrorResult("服务器内部错误"));
            }
        }

        /// <summary>
        /// 更新邀请码（仅管理员）
        /// </summary>
        /// <param name="id">邀请码ID</param>
        /// <param name="request">更新请求</param>
        /// <returns>更新结果</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponse<InvitationCodeDto>>> UpdateInvitationCode(Guid id, [FromBody] UpdateInvitationCodeDto request)
        {
            try
            {
                var result = await _invitationCodeService.UpdateAsync(id, request);
                
                if (result.IsSuccess)
                {
                    return Ok(ApiResponse<InvitationCodeDto>.SuccessResult(result.Data!, "更新成功"));
                }
                
                return BadRequest(ApiResponse<InvitationCodeDto>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新邀请码异常: {Id}", id);
                return StatusCode(500, ApiResponse<InvitationCodeDto>.ErrorResult("服务器内部错误"));
            }
        }

        /// <summary>
        /// 删除邀请码（仅管理员）
        /// </summary>
        /// <param name="id">邀请码ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteInvitationCode(Guid id)
        {
            try
            {
                var result = await _invitationCodeService.DeleteAsync(id);
                
                if (result.IsSuccess)
                {
                    return Ok(ApiResponse<bool>.SuccessResult(true, "删除成功"));
                }
                
                return BadRequest(ApiResponse<bool>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除邀请码异常: {Id}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("服务器内部错误"));
            }
        }

        /// <summary>
        /// 启用/禁用邀请码（仅管理员）
        /// </summary>
        /// <param name="id">邀请码ID</param>
        /// <param name="enabled">是否启用</param>
        /// <returns>操作结果</returns>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponse<bool>>> SetInvitationCodeStatus(Guid id, [FromBody] bool enabled)
        {
            try
            {
                var result = await _invitationCodeService.SetStatusAsync(id, enabled);
                
                if (result.IsSuccess)
                {
                    var message = enabled ? "邀请码已启用" : "邀请码已禁用";
                    return Ok(ApiResponse<bool>.SuccessResult(true, message));
                }
                
                return BadRequest(ApiResponse<bool>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置邀请码状态异常: {Id}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("服务器内部错误"));
            }
        }

        /// <summary>
        /// 获取邀请码使用记录（仅管理员）
        /// </summary>
        /// <param name="id">邀请码ID</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <returns>使用记录列表</returns>
        [HttpGet("{id}/usages")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponse<PagedResultDto<InvitationCodeUsageDto>>>> GetInvitationCodeUsages(
            Guid id, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _invitationCodeService.GetUsageHistoryAsync(id, page, pageSize);
                
                if (result.IsSuccess)
                {
                    return Ok(ApiResponse<PagedResultDto<InvitationCodeUsageDto>>.SuccessResult(result.Data!, "获取成功"));
                }
                
                return BadRequest(ApiResponse<PagedResultDto<InvitationCodeUsageDto>>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取邀请码使用记录异常: {Id}", id);
                return StatusCode(500, ApiResponse<PagedResultDto<InvitationCodeUsageDto>>.ErrorResult("服务器内部错误"));
            }
        }

        /// <summary>
        /// 获取邀请码统计信息（仅管理员）
        /// </summary>
        /// <returns>统计信息</returns>
        [HttpGet("stats")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponse<InvitationCodeStatsDto>>> GetInvitationCodeStats()
        {
            try
            {
                var result = await _invitationCodeService.GetStatsAsync();
                
                if (result.IsSuccess)
                {
                    return Ok(ApiResponse<InvitationCodeStatsDto>.SuccessResult(result.Data!, "获取成功"));
                }
                
                return BadRequest(ApiResponse<InvitationCodeStatsDto>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取邀请码统计信息异常");
                return StatusCode(500, ApiResponse<InvitationCodeStatsDto>.ErrorResult("服务器内部错误"));
            }
        }

        /// <summary>
        /// 获取当前用户ID
        /// </summary>
        /// <returns>用户ID</returns>
        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("user_id") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return userId;
            }
            return null;
        }
    }
}
