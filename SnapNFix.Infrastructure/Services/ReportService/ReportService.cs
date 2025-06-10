using Microsoft.EntityFrameworkCore;
using SnapNFix.Application.Utilities;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Infrastructure.Services.ReportService;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task AttachReportWithIssue(SnapReport report, CancellationToken cancellationToken)
    {
        // Use spatial index for efficient distance query
        var nearbyIssue = await _unitOfWork.Repository<Issue>()
            .FindBy(i => i.Category == report.ReportCategory &&
                            i.Location.IsWithinDistance(report.Location, Constants.NearbyIssueRadiusMeters))
            .OrderBy(i => i.Location.Distance(report.Location))
            .FirstOrDefaultAsync(cancellationToken);

        if (nearbyIssue != null)
        {
            // Connect to existing issue
            report.IssueId = nearbyIssue.Id;
            report.Issue = nearbyIssue;
        }
        else
        {
            // Create new issue
            var newIssue = new Issue
            {
                Location = report.Location,
                Category = report.ReportCategory,
                Status = IssueStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Issue>().Add(newIssue);
            report.IssueId = newIssue.Id;
            report.Issue = newIssue;
        }
    }
}