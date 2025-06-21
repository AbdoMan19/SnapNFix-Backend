using MediatR;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;

namespace SnapNFix.Application.Events
{
    public class IssueStatusChanged : INotification
    {
        public Guid IssueId { get; }
        public IssueStatus PreviousStatus { get; }
        public IssueStatus NewStatus { get; }
        
        public IssueStatusChanged(Guid issueId,  IssueStatus previousStatus, IssueStatus newStatus)
        {
            IssueId = issueId;
            PreviousStatus = previousStatus;
            NewStatus = newStatus;
        }
    }
}