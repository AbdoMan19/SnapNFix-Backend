using SnapNFix.Domain.Entities;
using SnapNFix.Application.Common.Interfaces.ServiceLifetime;

namespace SnapNFix.Application.Interfaces;

public interface IReportService : IScoped
{
    public Task<bool> AttachReportWithNearbyIssue(SnapReport snapReport, CancellationToken cancellationToken);
    public Task<Issue> CreateIssueWithReportAsync(SnapReport report, CancellationToken cancellationToken);
}


