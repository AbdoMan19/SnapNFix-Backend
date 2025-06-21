using System.Threading;
using System.Threading.Tasks;
using Application.Events;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.Interfaces;

namespace Application.EventHandlers
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
                await _notificationService.SendSnapReportStatusChangedNotificationAsync(
                    notification.SnapReportId,
                    notification.UserId,
                    notification.PreviousStatus,
                    notification.NewStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling SnapReportStatusChanged event for report {ReportId}", 
                    notification.SnapReportId);
            }
        }
    }
}
