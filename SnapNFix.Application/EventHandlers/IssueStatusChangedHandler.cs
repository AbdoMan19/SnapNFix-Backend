using Application.DTOs;
using Application.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;
using SnapNFix.Domain.Models.Notification;

namespace Application.EventHandlers
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
            
            // Notify city about the issue status change
            try
            {
                // Get the issue details to determine the city
                var issue = await _unitOfWork.Repository<Issue>()
                    .FindBy(i => i.Id == notification.IssueId)
                    .FirstOrDefaultAsync(cancellationToken);
                    
                if (issue != null && !string.IsNullOrEmpty(issue.City))
                {
                    // Format the city name for topic
                    var cityTopic = $"city_{issue.City.Replace(" ", "_").ToLower()}";
                    
                    // Send notification to the city topic
                    await _notificationService.SendNotificationToTopicAsync(new TopicNotificationModel
                    {
                        Topic = cityTopic,
                        Title = $"Issue Update in {issue.City}",
                        Body = $"An issue status has changed from {notification.PreviousStatus} to {notification.NewStatus}",
                        Data = new Dictionary<string, string>
                        {
                            { "issueId", issue.Id.ToString() },
                            { "type", "IssueStatusChanged" },
                            { "city", issue.City }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send city topic notification for issue {IssueId}", notification.IssueId);
            }
        }
    }
}
