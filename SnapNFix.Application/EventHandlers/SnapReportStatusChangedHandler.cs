using Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Models.Notification;

namespace SnapNFix.Application.EventHandlers
{
    public class SnapReportStatusChangedHandler : INotificationHandler<SnapReportStatusChanged>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<SnapReportStatusChangedHandler> _logger;
        
        public SnapReportStatusChangedHandler(
            INotificationService notificationService,
            ILogger<SnapReportStatusChangedHandler> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }
        
        public async Task Handle(SnapReportStatusChanged notification, CancellationToken cancellationToken)
        {

            try
            {
                // Send notification to user
                await _notificationService.SendNotificationToUserAsync(
                    new NotificationModel()
                    {
                        UserId = notification.UserId,
                        Title = "Snap Report Status Changed",
                        Body =
                            $"The status of your snap report {notification.SnapReportId} has changed from {notification.PreviousStatus} to {notification.NewStatus}.",
                        Data = new Dictionary<string, string>
                        {
                            { "reportId", notification.SnapReportId.ToString() },
                            { "previousStatus", notification.PreviousStatus.ToString() },
                            { "newStatus", notification.NewStatus.ToString() }
                        }
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling SnapReportStatusChanged event for report {ReportId}", 
                    notification.SnapReportId);
            }
        }
    }
}
