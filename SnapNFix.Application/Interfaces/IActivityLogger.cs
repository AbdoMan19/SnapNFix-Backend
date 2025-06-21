using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Application.Common.Interfaces.ServiceLifetime;

namespace Domain.Interfaces
{
    public interface IActivityLogger : ISingleton
    {
        Task LogIssueCreatedAsync(Issue issue);
        Task LogIssueStatusChangedAsync(Guid issueId, IssueStatus previousStatus, IssueStatus newStatus);
        // Add other activity types as needed
    }
}


