using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces.ServiceLifetime;

namespace SnapNFix.Domain.Interfaces
{
    public interface INotificationService : ISingleton
    {
        Task SendSnapReportStatusChangedNotificationAsync(Guid reportId, Guid userId ,ImageStatus previousStatus, ImageStatus newStatus);
        // Add other notification types as needed
        Task<bool> SendNotificationToUserAsync(Guid userId, string title, string body, Dictionary<string, string> data = null);
        Task<bool> SendNotificationToTopicAsync(string topic, string title, string body, Dictionary<string, string> data = null);
    }
}