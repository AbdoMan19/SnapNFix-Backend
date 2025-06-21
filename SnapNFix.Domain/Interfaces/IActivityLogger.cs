using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces.ServiceLifetime;

namespace SnapNFix.Domain.Interfaces
{
    public interface IActivityLogger : ISingleton
    {
        Task LogIssueCreatedAsync(Issue issue);
        Task LogIssueStatusChangedAsync(Guid issueId, IssueStatus previousStatus, IssueStatus newStatus);
        // Add other activity types as needed
    }
}