using DbContextHelp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using ToDoListArea.DTOs;

namespace ToDoListArea.Services
{
    /// <summary>
    /// 邀请码服务实现
    /// </summary>
    public class InvitationCodeService : IInvitationCodeService
    {
        private readonly ToDoListAreaDbContext _context;
        private readonly ILogger<InvitationCodeService> _logger;

        public InvitationCodeService(ToDoListAreaDbContext context, ILogger<InvitationCodeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 创建邀请码
        /// </summary>
        public async Task<ServiceResult<InvitationCodeDto>> CreateAsync(CreateInvitationCodeDto request, Guid createdBy)
        {
            try
            {
                // 生成邀请码（如果未提供）
                var code = request.Code ?? GenerateCode();

                // 检查邀请码是否已存在
                if (await CodeExistsAsync(code))
                {
                    return ServiceResult<InvitationCodeDto>.Failure("邀请码已存在", "CODE_EXISTS");
                }

                // 验证创建者是否存在
                var creator = await _context.Users.FindAsync(createdBy);
                if (creator == null)
                {
                    return ServiceResult<InvitationCodeDto>.Failure("创建者不存在", "CREATOR_NOT_FOUND");
                }

                // 创建邀请码实体
                var invitationCode = new InvitationCode
                {
                    Id = Guid.NewGuid(),
                    Code = code,
                    MaxUses = request.MaxUses,
                    UsedCount = 0,
                    ExpiresAt = request.ExpiresAt,
                    Status = "active",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.InvitationCodes.Add(invitationCode);
                await _context.SaveChangesAsync();

                // 转换为DTO
                var dto = new InvitationCodeDto
                {
                    Id = invitationCode.Id,
                    Code = invitationCode.Code,
                    MaxUses = invitationCode.MaxUses,
                    UsedCount = invitationCode.UsedCount,
                    ExpiresAt = invitationCode.ExpiresAt,
                    Status = invitationCode.Status,
                    CreatedBy = invitationCode.CreatedBy,
                    CreatedByName = creator.Name,
                    CreatedAt = invitationCode.CreatedAt,
                    UpdatedAt = invitationCode.UpdatedAt
                };

                // 邀请码创建成功
                return ServiceResult<InvitationCodeDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建邀请码失败: {Error}", ex.Message);
                return ServiceResult<InvitationCodeDto>.Failure($"创建邀请码失败：{ex.Message}", "CREATE_ERROR");
            }
        }

        /// <summary>
        /// 验证邀请码
        /// </summary>
        public async Task<ServiceResult<InvitationCodeValidationDto>> ValidateAsync(string code)
        {
            try
            {
                var invitationCode = await _context.InvitationCodes
                    .Include(ic => ic.CreatedByNavigation)
                    .FirstOrDefaultAsync(ic => ic.Code == code);

                if (invitationCode == null)
                {
                    return ServiceResult<InvitationCodeValidationDto>.Success(new InvitationCodeValidationDto
                    {
                        IsValid = false,
                        Message = "邀请码不存在"
                    });
                }

                // 检查状态
                if (invitationCode.Status != "active")
                {
                    return ServiceResult<InvitationCodeValidationDto>.Success(new InvitationCodeValidationDto
                    {
                        IsValid = false,
                        Message = "邀请码已被禁用"
                    });
                }

                // 检查是否过期
                if (invitationCode.ExpiresAt.HasValue && invitationCode.ExpiresAt.Value < DateTime.UtcNow)
                {
                    return ServiceResult<InvitationCodeValidationDto>.Success(new InvitationCodeValidationDto
                    {
                        IsValid = false,
                        Message = "邀请码已过期"
                    });
                }

                // 检查使用次数
                if (invitationCode.UsedCount >= invitationCode.MaxUses)
                {
                    return ServiceResult<InvitationCodeValidationDto>.Success(new InvitationCodeValidationDto
                    {
                        IsValid = false,
                        Message = "邀请码使用次数已达上限"
                    });
                }

                // 邀请码有效
                var dto = new InvitationCodeDto
                {
                    Id = invitationCode.Id,
                    Code = invitationCode.Code,
                    MaxUses = invitationCode.MaxUses,
                    UsedCount = invitationCode.UsedCount,
                    ExpiresAt = invitationCode.ExpiresAt,
                    Status = invitationCode.Status,
                    CreatedBy = invitationCode.CreatedBy,
                    CreatedByName = invitationCode.CreatedByNavigation.Name,
                    CreatedAt = invitationCode.CreatedAt,
                    UpdatedAt = invitationCode.UpdatedAt
                };

                return ServiceResult<InvitationCodeValidationDto>.Success(new InvitationCodeValidationDto
                {
                    IsValid = true,
                    Message = "邀请码有效",
                    InvitationCode = dto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证邀请码失败: {Code}, {Error}", code, ex.Message);
                return ServiceResult<InvitationCodeValidationDto>.Failure($"验证邀请码失败：{ex.Message}", "VALIDATE_ERROR");
            }
        }

        /// <summary>
        /// 使用邀请码
        /// </summary>
        public async Task<ServiceResult<bool>> UseAsync(string code, Guid userId, string? ipAddress = null, string? userAgent = null)
        {
            try
            {
                // 先验证邀请码
                var validationResult = await ValidateAsync(code);
                if (!validationResult.IsSuccess || !validationResult.Data!.IsValid)
                {
                    return ServiceResult<bool>.Failure(validationResult.Data?.Message ?? "邀请码无效", "INVALID_CODE");
                }

                var invitationCode = await _context.InvitationCodes
                    .FirstOrDefaultAsync(ic => ic.Code == code);

                if (invitationCode == null)
                {
                    return ServiceResult<bool>.Failure("邀请码不存在", "CODE_NOT_FOUND");
                }

                // 检查用户是否已使用过此邀请码
                var existingUsage = await _context.InvitationCodeUsages
                    .FirstOrDefaultAsync(icu => icu.InvitationCodeId == invitationCode.Id && icu.UserId == userId);

                if (existingUsage != null)
                {
                    return ServiceResult<bool>.Failure("您已使用过此邀请码", "ALREADY_USED");
                }

                // 创建使用记录
                var usage = new InvitationCodeUsage
                {
                    Id = Guid.NewGuid(),
                    InvitationCodeId = invitationCode.Id,
                    UserId = userId,
                    UsedAt = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                };

                _context.InvitationCodeUsages.Add(usage);

                // 更新邀请码使用次数
                invitationCode.UsedCount++;
                invitationCode.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // 邀请码使用成功
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "使用邀请码失败: {Code}, 用户: {UserId}, {Error}", code, userId, ex.Message);
                return ServiceResult<bool>.Failure($"使用邀请码失败：{ex.Message}", "USE_ERROR");
            }
        }

        /// <summary>
        /// 生成随机邀请码
        /// </summary>
        public string GenerateCode(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var result = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            return result.ToString();
        }

        /// <summary>
        /// 检查邀请码是否已存在
        /// </summary>
        public async Task<bool> CodeExistsAsync(string code)
        {
            return await _context.InvitationCodes.AnyAsync(ic => ic.Code == code);
        }

        /// <summary>
        /// 获取邀请码列表（分页）
        /// </summary>
        public async Task<ServiceResult<PagedResultDto<InvitationCodeDto>>> GetListAsync(InvitationCodeQueryDto query)
        {
            try
            {
                var queryable = _context.InvitationCodes
                    .Include(ic => ic.CreatedByNavigation)
                    .AsQueryable();

                // 状态筛选
                if (!string.IsNullOrEmpty(query.Status))
                {
                    queryable = queryable.Where(ic => ic.Status == query.Status);
                }

                // 创建者筛选
                if (query.CreatedBy.HasValue)
                {
                    queryable = queryable.Where(ic => ic.CreatedBy == query.CreatedBy.Value);
                }

                // 搜索关键词
                if (!string.IsNullOrEmpty(query.Search))
                {
                    queryable = queryable.Where(ic => ic.Code.Contains(query.Search));
                }

                // 是否包含已过期的
                if (!query.IncludeExpired)
                {
                    var now = DateTime.UtcNow;
                    queryable = queryable.Where(ic => ic.ExpiresAt == null || ic.ExpiresAt > now);
                }

                // 总数量
                var totalCount = await queryable.CountAsync();

                // 分页查询
                var items = await queryable
                    .OrderByDescending(ic => ic.CreatedAt)
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(ic => new InvitationCodeDto
                    {
                        Id = ic.Id,
                        Code = ic.Code,
                        MaxUses = ic.MaxUses,
                        UsedCount = ic.UsedCount,
                        ExpiresAt = ic.ExpiresAt,
                        Status = ic.Status,
                        CreatedBy = ic.CreatedBy,
                        CreatedByName = ic.CreatedByNavigation.Name,
                        CreatedAt = ic.CreatedAt,
                        UpdatedAt = ic.UpdatedAt
                    })
                    .ToListAsync();

                var result = new PagedResultDto<InvitationCodeDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = query.Page,
                    PageSize = query.PageSize
                };

                return ServiceResult<PagedResultDto<InvitationCodeDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取邀请码列表失败: {Error}", ex.Message);
                return ServiceResult<PagedResultDto<InvitationCodeDto>>.Failure($"获取邀请码列表失败：{ex.Message}", "GET_LIST_ERROR");
            }
        }

        /// <summary>
        /// 根据ID获取邀请码详情
        /// </summary>
        public async Task<ServiceResult<InvitationCodeDto>> GetByIdAsync(Guid id)
        {
            try
            {
                var invitationCode = await _context.InvitationCodes
                    .Include(ic => ic.CreatedByNavigation)
                    .FirstOrDefaultAsync(ic => ic.Id == id);

                if (invitationCode == null)
                {
                    return ServiceResult<InvitationCodeDto>.Failure("邀请码不存在", "CODE_NOT_FOUND");
                }

                var dto = new InvitationCodeDto
                {
                    Id = invitationCode.Id,
                    Code = invitationCode.Code,
                    MaxUses = invitationCode.MaxUses,
                    UsedCount = invitationCode.UsedCount,
                    ExpiresAt = invitationCode.ExpiresAt,
                    Status = invitationCode.Status,
                    CreatedBy = invitationCode.CreatedBy,
                    CreatedByName = invitationCode.CreatedByNavigation.Name,
                    CreatedAt = invitationCode.CreatedAt,
                    UpdatedAt = invitationCode.UpdatedAt
                };

                return ServiceResult<InvitationCodeDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取邀请码详情失败: {Id}, {Error}", id, ex.Message);
                return ServiceResult<InvitationCodeDto>.Failure($"获取邀请码详情失败：{ex.Message}", "GET_BY_ID_ERROR");
            }
        }

        /// <summary>
        /// 根据代码获取邀请码详情
        /// </summary>
        public async Task<ServiceResult<InvitationCodeDto>> GetByCodeAsync(string code)
        {
            try
            {
                var invitationCode = await _context.InvitationCodes
                    .Include(ic => ic.CreatedByNavigation)
                    .FirstOrDefaultAsync(ic => ic.Code == code);

                if (invitationCode == null)
                {
                    return ServiceResult<InvitationCodeDto>.Failure("邀请码不存在", "CODE_NOT_FOUND");
                }

                var dto = new InvitationCodeDto
                {
                    Id = invitationCode.Id,
                    Code = invitationCode.Code,
                    MaxUses = invitationCode.MaxUses,
                    UsedCount = invitationCode.UsedCount,
                    ExpiresAt = invitationCode.ExpiresAt,
                    Status = invitationCode.Status,
                    CreatedBy = invitationCode.CreatedBy,
                    CreatedByName = invitationCode.CreatedByNavigation.Name,
                    CreatedAt = invitationCode.CreatedAt,
                    UpdatedAt = invitationCode.UpdatedAt
                };

                return ServiceResult<InvitationCodeDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据代码获取邀请码详情失败: {Code}, {Error}", code, ex.Message);
                return ServiceResult<InvitationCodeDto>.Failure($"获取邀请码详情失败：{ex.Message}", "GET_BY_CODE_ERROR");
            }
        }

        /// <summary>
        /// 更新邀请码
        /// </summary>
        public async Task<ServiceResult<InvitationCodeDto>> UpdateAsync(Guid id, UpdateInvitationCodeDto request)
        {
            try
            {
                var invitationCode = await _context.InvitationCodes
                    .Include(ic => ic.CreatedByNavigation)
                    .FirstOrDefaultAsync(ic => ic.Id == id);

                if (invitationCode == null)
                {
                    return ServiceResult<InvitationCodeDto>.Failure("邀请码不存在", "CODE_NOT_FOUND");
                }

                // 更新字段
                if (request.MaxUses.HasValue)
                {
                    invitationCode.MaxUses = request.MaxUses.Value;
                }

                if (request.ExpiresAt.HasValue)
                {
                    invitationCode.ExpiresAt = request.ExpiresAt.Value;
                }

                if (!string.IsNullOrEmpty(request.Status))
                {
                    invitationCode.Status = request.Status;
                }

                invitationCode.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var dto = new InvitationCodeDto
                {
                    Id = invitationCode.Id,
                    Code = invitationCode.Code,
                    MaxUses = invitationCode.MaxUses,
                    UsedCount = invitationCode.UsedCount,
                    ExpiresAt = invitationCode.ExpiresAt,
                    Status = invitationCode.Status,
                    CreatedBy = invitationCode.CreatedBy,
                    CreatedByName = invitationCode.CreatedByNavigation.Name,
                    CreatedAt = invitationCode.CreatedAt,
                    UpdatedAt = invitationCode.UpdatedAt
                };

                // 邀请码更新成功
                return ServiceResult<InvitationCodeDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新邀请码失败: {Id}, {Error}", id, ex.Message);
                return ServiceResult<InvitationCodeDto>.Failure($"更新邀请码失败：{ex.Message}", "UPDATE_ERROR");
            }
        }

        /// <summary>
        /// 删除邀请码
        /// </summary>
        public async Task<ServiceResult<bool>> DeleteAsync(Guid id)
        {
            try
            {
                var invitationCode = await _context.InvitationCodes.FindAsync(id);
                if (invitationCode == null)
                {
                    return ServiceResult<bool>.Failure("邀请码不存在", "CODE_NOT_FOUND");
                }

                // 检查是否有使用记录
                var hasUsages = await _context.InvitationCodeUsages.AnyAsync(icu => icu.InvitationCodeId == id);
                if (hasUsages)
                {
                    return ServiceResult<bool>.Failure("邀请码已被使用，无法删除", "CODE_IN_USE");
                }

                _context.InvitationCodes.Remove(invitationCode);
                await _context.SaveChangesAsync();

                // 邀请码删除成功
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除邀请码失败: {Id}, {Error}", id, ex.Message);
                return ServiceResult<bool>.Failure($"删除邀请码失败：{ex.Message}", "DELETE_ERROR");
            }
        }

        /// <summary>
        /// 启用/禁用邀请码
        /// </summary>
        public async Task<ServiceResult<bool>> SetStatusAsync(Guid id, bool enabled)
        {
            try
            {
                var invitationCode = await _context.InvitationCodes.FindAsync(id);
                if (invitationCode == null)
                {
                    return ServiceResult<bool>.Failure("邀请码不存在", "CODE_NOT_FOUND");
                }

                invitationCode.Status = enabled ? "active" : "disabled";
                invitationCode.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // 邀请码状态更新成功
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新邀请码状态失败: {Id}, {Error}", id, ex.Message);
                return ServiceResult<bool>.Failure($"更新邀请码状态失败：{ex.Message}", "SET_STATUS_ERROR");
            }
        }

        /// <summary>
        /// 获取邀请码使用记录
        /// </summary>
        public async Task<ServiceResult<PagedResultDto<InvitationCodeUsageDto>>> GetUsageHistoryAsync(Guid invitationCodeId, int page = 1, int pageSize = 20)
        {
            try
            {
                var queryable = _context.InvitationCodeUsages
                    .Include(icu => icu.User)
                    .Include(icu => icu.InvitationCode)
                    .Where(icu => icu.InvitationCodeId == invitationCodeId);

                var totalCount = await queryable.CountAsync();

                var items = await queryable
                    .OrderByDescending(icu => icu.UsedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(icu => new InvitationCodeUsageDto
                    {
                        Id = icu.Id,
                        Code = icu.InvitationCode.Code,
                        UserId = icu.UserId,
                        UserName = icu.User.Name,
                        UserEmail = icu.User.Email,
                        UsedAt = icu.UsedAt,
                        IpAddress = icu.IpAddress,
                        UserAgent = icu.UserAgent
                    })
                    .ToListAsync();

                var result = new PagedResultDto<InvitationCodeUsageDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };

                return ServiceResult<PagedResultDto<InvitationCodeUsageDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取邀请码使用记录失败: {InvitationCodeId}, {Error}", invitationCodeId, ex.Message);
                return ServiceResult<PagedResultDto<InvitationCodeUsageDto>>.Failure($"获取使用记录失败：{ex.Message}", "GET_USAGE_HISTORY_ERROR");
            }
        }

        /// <summary>
        /// 获取用户使用的邀请码记录
        /// </summary>
        public async Task<ServiceResult<List<InvitationCodeUsageDto>>> GetUserUsageHistoryAsync(Guid userId)
        {
            try
            {
                var items = await _context.InvitationCodeUsages
                    .Include(icu => icu.User)
                    .Include(icu => icu.InvitationCode)
                    .Where(icu => icu.UserId == userId)
                    .OrderByDescending(icu => icu.UsedAt)
                    .Select(icu => new InvitationCodeUsageDto
                    {
                        Id = icu.Id,
                        Code = icu.InvitationCode.Code,
                        UserId = icu.UserId,
                        UserName = icu.User.Name,
                        UserEmail = icu.User.Email,
                        UsedAt = icu.UsedAt,
                        IpAddress = icu.IpAddress,
                        UserAgent = icu.UserAgent
                    })
                    .ToListAsync();

                return ServiceResult<List<InvitationCodeUsageDto>>.Success(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户邀请码使用记录失败: {UserId}, {Error}", userId, ex.Message);
                return ServiceResult<List<InvitationCodeUsageDto>>.Failure($"获取用户使用记录失败：{ex.Message}", "GET_USER_USAGE_HISTORY_ERROR");
            }
        }

        /// <summary>
        /// 获取邀请码统计信息
        /// </summary>
        public async Task<ServiceResult<InvitationCodeStatsDto>> GetStatsAsync(Guid? createdBy = null)
        {
            try
            {
                var codesQuery = _context.InvitationCodes.AsQueryable();
                var usagesQuery = _context.InvitationCodeUsages.AsQueryable();

                if (createdBy.HasValue)
                {
                    codesQuery = codesQuery.Where(ic => ic.CreatedBy == createdBy.Value);
                    usagesQuery = usagesQuery.Where(icu => icu.InvitationCode.CreatedBy == createdBy.Value);
                }

                var now = DateTime.UtcNow;
                var today = now.Date;
                var weekStart = today.AddDays(-(int)today.DayOfWeek);
                var monthStart = new DateTime(today.Year, today.Month, 1);

                var stats = new InvitationCodeStatsDto
                {
                    TotalCodes = await codesQuery.CountAsync(),
                    ActiveCodes = await codesQuery.CountAsync(ic => ic.Status == "active"),
                    DisabledCodes = await codesQuery.CountAsync(ic => ic.Status == "disabled"),
                    ExpiredCodes = await codesQuery.CountAsync(ic => ic.ExpiresAt.HasValue && ic.ExpiresAt.Value < now),
                    TotalUsages = await usagesQuery.CountAsync(),
                    TodayUsages = await usagesQuery.CountAsync(icu => icu.UsedAt >= today),
                    WeekUsages = await usagesQuery.CountAsync(icu => icu.UsedAt >= weekStart),
                    MonthUsages = await usagesQuery.CountAsync(icu => icu.UsedAt >= monthStart)
                };

                return ServiceResult<InvitationCodeStatsDto>.Success(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取邀请码统计信息失败: {Error}", ex.Message);
                return ServiceResult<InvitationCodeStatsDto>.Failure($"获取统计信息失败：{ex.Message}", "GET_STATS_ERROR");
            }
        }
    }
}
