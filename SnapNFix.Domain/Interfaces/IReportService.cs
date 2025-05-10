using SnapNFix.Domain.Entities;

namespace SnapNFix.Domain.Interfaces;

public interface IReportService
{
    public Task AttachReportWithIssue(SnapReport snapReport, CancellationToken cancellationToken);
}