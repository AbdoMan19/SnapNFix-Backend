using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Application.Common.Interfaces.ServiceLifetime;
using SnapNFix.Domain.Models.Notification;

namespace SnapNFix.Application.Interfaces;

public interface INotificationService : ISingleton
{
    Task<bool> SendNotificationToUserAsync(NotificationModel notificationModel);
    Task<bool> SendNotificationToTopicAsync(TopicNotificationModel topicNotificationModel);
}



