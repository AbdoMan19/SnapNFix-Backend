
using Application.DTOs;
using Application.Events;
using MediatR;
using SnapNFix.Application.Interfaces;

namespace Application.EventHandlers
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
            await _activityLogger.LogActivityAsync(new ActivityLogDto
            {
                Type = "IssueCreated",
                IssueId = notification.Issue.Id,
                Description = $"New issue created: {notification.Issue.Id}",
                AdditionalData = new 
                {
                    IssueCategory = notification.Issue.Category,
                    Severity = notification.Issue.Severity
                }
            });
        }
    }
}