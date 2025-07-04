using Application.DTOs;
using Application.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;
using SnapNFix.Domain.Models.Notification;

namespace SnapNFix.Application.EventHandlers
{
    public class IssueStatusChangedHandler : INotificationHandler<IssueStatusChanged>
    {
       private readonly IActivityLogger _activityLogger;
        private readonly ILogger<IssueStatusChangedHandler> _logger;
        private readonly INotificationService _notificationService;
        private readonly IUnitOfWork _unitOfWork;
        
        public IssueStatusChangedHandler(
            IActivityLogger activityLogger,
            INotificationService notificationService,
            ILogger<IssueStatusChangedHandler> logger,
            IUnitOfWork unitOfWork)
        {
            _activityLogger = activityLogger;
            _notificationService = notificationService;
            _logger = logger;
            _unitOfWork = unitOfWork;
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
                        Description =
                            $"Issue status changed from {notification.PreviousStatus} to {notification.NewStatus}",
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

            try
            {
                // Notify users who report on the issue about  status change
                var issue = await _unitOfWork.Repository<Issue>()
                    .FindBy(i => i.Id == notification.IssueId)
                    .Include(i => i.AssociatedSnapReports)
                    .FirstOrDefaultAsync(cancellationToken);

                if (issue != null)
                {
                    foreach (var report in issue.AssociatedSnapReports)
                    {
                        var notificationModel = new NotificationModel
                        {
                            UserId = report.UserId,
                            Title = "Issue Status Updated",
                            Body =
                                $"Issue #{notification.IssueId} status changed from {notification.PreviousStatus} to {notification.NewStatus}",
                            Data = new Dictionary<string, string>
                            {
                                {"issueId" , notification.IssueId.ToString()},
                                { "previousStatus", notification.PreviousStatus.ToString() },
                                { "newStatus", notification.NewStatus.ToString() },
                                { "type" , "ISSUE_STATUS_CHANGED" }
                            }
                        };
                        _logger.LogInformation("Sending notification to user {UserId} for issue {IssueId}",
                            report.UserId, notification.IssueId);

                        await _notificationService.SendNotificationToUserAsync(notificationModel);
                    }
                }
                else
                {
                    _logger.LogWarning("Issue with ID {IssueId} not found or has no reporter.", notification.IssueId);

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling IssueStatusChanged event for issue {IssueId}",
                    notification.IssueId);
            }
        }
    }
}
