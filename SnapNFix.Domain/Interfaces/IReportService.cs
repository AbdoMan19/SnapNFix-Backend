using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces.ServiceLifetime;

namespace SnapNFix.Domain.Interfaces;

public interface IReportService : IScoped
{
    public Task<bool> AttachReportWithNearbyIssue(SnapReport snapReport, CancellationToken cancellationToken);
    public Task<Issue> CreateIssueWithReportAsync(SnapReport report, CancellationToken cancellationToken);
}