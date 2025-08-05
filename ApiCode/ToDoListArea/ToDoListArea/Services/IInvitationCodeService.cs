using DbContextHelp.Models;
using ToDoListArea.DTOs;

namespace ToDoListArea.Services
{
    /// <summary>
    /// 邀请码服务接口
    /// </summary>
    public interface IInvitationCodeService
    {
        /// <summary>
        /// 创建邀请码
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <param name="createdBy">创建者用户ID</param>
        /// <returns>创建结果</returns>
        Task<ServiceResult<InvitationCodeDto>> CreateAsync(CreateInvitationCodeDto request, Guid createdBy);

        /// <summary>
        /// 验证邀请码
        /// </summary>
        /// <param name="code">邀请码</param>
        /// <returns>验证结果</returns>
        Task<ServiceResult<InvitationCodeValidationDto>> ValidateAsync(string code);

        /// <summary>
        /// 使用邀请码
        /// </summary>
        /// <param name="code">邀请码</param>
        /// <param name="userId">使用者用户ID</param>
        /// <param name="ipAddress">IP地址</param>
        /// <param name="userAgent">用户代理</param>
        /// <returns>使用结果</returns>
        Task<ServiceResult<bool>> UseAsync(string code, Guid userId, string? ipAddress = null, string? userAgent = null);

        /// <summary>
        /// 获取邀请码列表（分页）
        /// </summary>
        /// <param name="query">查询条件</param>
        /// <returns>邀请码列表</returns>
        Task<ServiceResult<PagedResultDto<InvitationCodeDto>>> GetListAsync(InvitationCodeQueryDto query);

        /// <summary>
        /// 根据ID获取邀请码详情
        /// </summary>
        /// <param name="id">邀请码ID</param>
        /// <returns>邀请码详情</returns>
        Task<ServiceResult<InvitationCodeDto>> GetByIdAsync(Guid id);

        /// <summary>
        /// 根据代码获取邀请码详情
        /// </summary>
        /// <param name="code">邀请码</param>
        /// <returns>邀请码详情</returns>
        Task<ServiceResult<InvitationCodeDto>> GetByCodeAsync(string code);

        /// <summary>
        /// 更新邀请码
        /// </summary>
        /// <param name="id">邀请码ID</param>
        /// <param name="request">更新请求</param>
        /// <returns>更新结果</returns>
        Task<ServiceResult<InvitationCodeDto>> UpdateAsync(Guid id, UpdateInvitationCodeDto request);

        /// <summary>
        /// 删除邀请码
        /// </summary>
        /// <param name="id">邀请码ID</param>
        /// <returns>删除结果</returns>
        Task<ServiceResult<bool>> DeleteAsync(Guid id);

        /// <summary>
        /// 启用/禁用邀请码
        /// </summary>
        /// <param name="id">邀请码ID</param>
        /// <param name="enabled">是否启用</param>
        /// <returns>操作结果</returns>
        Task<ServiceResult<bool>> SetStatusAsync(Guid id, bool enabled);

        /// <summary>
        /// 获取邀请码使用记录
        /// </summary>
        /// <param name="invitationCodeId">邀请码ID</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <returns>使用记录列表</returns>
        Task<ServiceResult<PagedResultDto<InvitationCodeUsageDto>>> GetUsageHistoryAsync(Guid invitationCodeId, int page = 1, int pageSize = 20);

        /// <summary>
        /// 获取用户使用的邀请码记录
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>使用记录列表</returns>
        Task<ServiceResult<List<InvitationCodeUsageDto>>> GetUserUsageHistoryAsync(Guid userId);

        /// <summary>
        /// 生成随机邀请码
        /// </summary>
        /// <param name="length">长度</param>
        /// <returns>邀请码字符串</returns>
        string GenerateCode(int length = 8);

        /// <summary>
        /// 检查邀请码是否已存在
        /// </summary>
        /// <param name="code">邀请码</param>
        /// <returns>是否存在</returns>
        Task<bool> CodeExistsAsync(string code);

        /// <summary>
        /// 获取邀请码统计信息
        /// </summary>
        /// <param name="createdBy">创建者ID（可选）</param>
        /// <returns>统计信息</returns>
        Task<ServiceResult<InvitationCodeStatsDto>> GetStatsAsync(Guid? createdBy = null);
    }
}
