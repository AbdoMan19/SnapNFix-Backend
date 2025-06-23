using SnapNFix.Application.Common.Interfaces.ServiceLifetime;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Features.FastReport.DTOs;

namespace SnapNFix.Application.Interfaces;

public interface IFastReportService : IScoped
{
    Task<PagedList<FastReportDetailsDto>> GetFastReportsByIssueIdAsync(Guid issueId, int pageNumber, CancellationToken cancellationToken = default);
    Task InvalidateIssueFastReportsCacheAsync(Guid issueId, CancellationToken cancellationToken = default);
} 