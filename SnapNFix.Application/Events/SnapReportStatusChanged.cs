using MediatR;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;

namespace SnapNFix.Application.Events
{
    public class SnapReportStatusChanged : INotification
    {
        public Guid SnapReportId { get; }
        public Guid UserId { get; set; }
        public ImageStatus PreviousStatus { get; }
        public ImageStatus NewStatus { get; }
        
        public SnapReportStatusChanged(SnapReport snapReport, ImageStatus previousStatus, ImageStatus newStatus)
        {
            SnapReportId = snapReport.Id;
            UserId = snapReport.UserId;
            PreviousStatus = previousStatus;
            NewStatus = newStatus;
        }
    }
}