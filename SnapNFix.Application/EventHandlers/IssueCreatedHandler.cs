using MediatR;
using SnapNFix.Application.Events;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.EventHandlers
{
    public class IssueCreatedHandler : INotificationHandler<IssueCreated>
    {
        private readonly IActivityLogger _activityLogger;
        
        public IssueCreatedHandler(IActivityLogger activityLogger)
        {
            _activityLogger = activityLogger;
        }
        
        public async Task Handle(IssueCreated notification, CancellationToken cancellationToken)
        {
            await _activityLogger.LogIssueCreatedAsync(notification.Issue);
        }
    }
}