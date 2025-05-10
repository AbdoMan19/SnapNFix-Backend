using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces.ServiceLifetime;

namespace SnapNFix.Domain.Interfaces;

public interface IReportService : IScoped
{
    public Task AttachReportWithIssue(SnapReport snapReport, CancellationToken cancellationToken);
}