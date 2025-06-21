using SnapNFix.Application.Common.Interfaces.ServiceLifetime;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Features.SnapReport.DTOs;

namespace SnapNFix.Application.Common.Interfaces;

public interface ISnapReportService : IScoped
{
    Task<PagedList<ReportDetailsDto>> GetUserReportsAsync(Guid userId, string? status, string? category, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedList<ReportDetailsDto>> GetSnapReportsByIssueIdAsync(Guid issueId, int pageNumber, CancellationToken cancellationToken = default);
    Task<UserReportsStatisticsDto> GetUserReportsStatisticsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task InvalidateUserReportsCacheAsync(Guid userId, CancellationToken cancellationToken = default);
    Task InvalidateIssueReportsCacheAsync(Guid issueId, CancellationToken cancellationToken = default);
} 