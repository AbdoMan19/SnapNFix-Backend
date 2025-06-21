using SnapNFix.Application.Common.Interfaces.ServiceLifetime;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Features.Issue.DTOs;

namespace SnapNFix.Application.Common.Interfaces;

public interface IIssueService : IScoped
{
    Task<PagedList<IssueDetailsDto>> GetIssuesAsync(string? status, string? category, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<IssueDetailsDto?> GetIssueByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<NearbyIssueDto>> GetNearbyIssuesAsync(double northEastLat, double northEastLng, double southWestLat, double southWestLng, int maxResults, CancellationToken cancellationToken = default);
    Task InvalidateIssueCacheAsync(Guid issueId, CancellationToken cancellationToken = default);
    Task InvalidateAllIssuesCacheAsync(CancellationToken cancellationToken = default);
} 