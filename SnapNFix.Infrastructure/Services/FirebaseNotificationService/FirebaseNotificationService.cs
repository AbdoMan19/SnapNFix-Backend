using Application.DTOs;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SnapNFix.Domain.Enums;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Application.Interfaces;

namespace SnapNFix.Infrastructure.Services.FirebaseNotificationService
{
    public class FirebaseNotificationService : INotificationService
    {
        private readonly FirebaseMessaging _messaging;
        private IConfiguration _configuration;
        private readonly ILogger<FirebaseNotificationService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public FirebaseNotificationService(IConfiguration configuration,
            ILogger<FirebaseNotificationService> logger,
            IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _logger = logger;
            _serviceProvider = serviceProvider;

            if (FirebaseApp.DefaultInstance == null)
            {
                var credentialFilePath = configuration["Firebase:CredentialFilePath"];
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(credentialFilePath)
                });
            }

            _messaging = FirebaseMessaging.DefaultInstance;
        }


        public async Task SendSnapReportStatusChangedNotificationAsync(Guid reportId , Guid userId , ImageStatus previousStatus,
            ImageStatus newStatus)
        {
            await SendNotificationToUserAsync(
                userId,
                "Snap Report Status Changed",
                $"The status of your snap report {reportId} has changed from {previousStatus} to {newStatus}.",
                new Dictionary<string, string>
                {
                    { "reportId", reportId.ToString() },
                    { "previousStatus", previousStatus.ToString() },
                    { "newStatus", newStatus.ToString() }
                }
            );

        }


        public async Task<bool> SendNotificationToUserAsync(Guid userId, string title, string body,
            Dictionary<string, string> data = null)
        {
            try
            {
                // Create a scope to resolve the scoped service
                using (var scope = _serviceProvider.CreateScope())
                {
                    var deviceManager = scope.ServiceProvider.GetRequiredService<IDeviceManager>();
                    
                    var fcmTokens = await deviceManager.GetActiveDevicesFCMAsync(userId);
                    
                    if (fcmTokens.Count == 0)
                    {
                        _logger.LogWarning("No active devices with FCM tokens found for user {UserId}", userId);
                        return false;
                    }

                    var message = new MulticastMessage
                    {
                        Tokens = fcmTokens,
                        Notification = new Notification
                        {
                            Title = title,
                            Body = body
                        },
                        Data = data ?? new Dictionary<string, string>()
                    };

                    var response = await _messaging.SendEachForMulticastAsync(message);
                    
                    _logger.LogInformation(
                        "Notification sent to {SuccessCount}/{TotalCount} devices for user {UserId}", 
                        response.SuccessCount, response.FailureCount + response.SuccessCount, userId);
                    
                    return response.SuccessCount > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> SendNotificationToTopicAsync(string topic, string title, string body,
            Dictionary<string, string> data = null)
        {
            try
            {
                var message = new Message
                {
                    Topic = topic,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data ?? new Dictionary<string, string>()
                };

                var response = await _messaging.SendAsync(message);
                _logger.LogInformation("Notification sent to topic {Topic}, message ID: {MessageId}", topic, response);
                
                return !string.IsNullOrEmpty(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to topic {Topic}", topic);
                return false;
            }
        }
    }
}

