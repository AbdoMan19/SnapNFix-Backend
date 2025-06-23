using Application.DTOs;
using Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Interfaces;

namespace Application.EventHandlers
{
    public class IssueStatusChangedHandler : INotificationHandler<IssueStatusChanged>
    {
       private readonly IActivityLogger _activityLogger;
        private readonly ILogger<IssueStatusChangedHandler> _logger;
        
        public IssueStatusChangedHandler(
            IActivityLogger activityLogger,
            INotificationService notificationService,
            ILogger<IssueStatusChangedHandler> logger)
        {
            _activityLogger = activityLogger;
            _logger = logger;
        }
        
        public async Task Handle(IssueStatusChanged notification, CancellationToken cancellationToken)
        {
            // Log the activity
            try
            {
                // Log the activity
                await _activityLogger.LogActivityAsync(
                    new ActivityLogDto
                    {
                        Type = "IssueStatusChanged",
                        IssueId = notification.IssueId,
                        Description = $"Issue status changed from {notification.PreviousStatus} to {notification.NewStatus}",
                        AdditionalData = new
                        {
                            PreviousStatus = notification.PreviousStatus,
                            NewStatus = notification.NewStatus
                        }
                    });

                // Send notification to the user
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling IssueStatusChanged event for issue {IssueId}", 
                    notification.IssueId);
            }
        }
    }
}
