using MediatR;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Application.Events
{
    public class IssueCreated : INotification
    {
        public Issue Issue { get; }
        
        public IssueCreated(Issue issue)
        {
            Issue = issue;
        }
    }
}