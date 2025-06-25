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
using SnapNFix.Domain.Models.Notification;

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

        
        public async Task<bool> SendNotificationToUserAsync(NotificationModel notificationModel)
        {
            try
            {
                // Create a scope to resolve the scoped service
                using (var scope = _serviceProvider.CreateScope())
                {
                    var deviceManager = scope.ServiceProvider.GetRequiredService<IDeviceManager>();
                    
                    var fcmTokens = await deviceManager.GetActiveDevicesFCMAsync(notificationModel.UserId);
                    
                    if (fcmTokens.Count == 0)
                    {
                        _logger.LogWarning("No active devices with FCM tokens found for user {UserId}", notificationModel.UserId);
                        return false;
                    }

                    var message = new MulticastMessage
                    {
                        Tokens = fcmTokens,
                        Notification = new Notification
                        {
                            Title = notificationModel.Title,
                            Body = notificationModel.Body
                        },
                        Data = notificationModel.Data,
                        
                    };

                    var response = await _messaging.SendEachForMulticastAsync(message);
                    
                    _logger.LogInformation(
                        "Notification sent to {SuccessCount}/{TotalCount} devices for user {UserId}", 
                        response.SuccessCount, response.FailureCount + response.SuccessCount, notificationModel.UserId);
                    
                    return response.SuccessCount > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to user {UserId}", notificationModel.UserId);
                return false;
            }
        }

        public async Task<bool> SendNotificationToTopicAsync(TopicNotificationModel topicNotificationModel)
        {
            try
            {
                
                var message = new Message
                {
                    Topic = topicNotificationModel.Topic,
                    Notification = new Notification
                    {
                        Title = topicNotificationModel.Title,
                        Body = topicNotificationModel.Body
                    },
                    Data = topicNotificationModel.Data
                };

                var response = await _messaging.SendAsync(message);
                _logger.LogInformation("Notification sent to topic {Topic}, message ID: {MessageId}", topicNotificationModel.Topic, response);
            
                return !string.IsNullOrEmpty(response);   
                
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to topic {Topic}", topicNotificationModel.Topic);
                return false;
            }
        }

        // Add to the FirebaseNotificationService class

        public async Task<bool> SubscribeToTopicAsync(List<string> tokens, string topic)
        {
            try
            {
                await _messaging.SubscribeToTopicAsync(tokens, topic);
                _logger.LogInformation("Devices with tokens {Tokens} subscribed to topic {Topic}", string.Join(", ", tokens), topic);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing devices with tokens {Tokens} to topic {Topic}", string.Join(", ", tokens), topic);
                return false;
            }
        }

        public async Task<bool> UnsubscribeFromTopicAsync(List<string> tokens, string topic)
        {
            try
            {
                await _messaging.UnsubscribeFromTopicAsync(tokens , topic);
                _logger.LogInformation("Devices with tokens {Tokens} unsubscribed from topic {Topic}", string.Join(", ", tokens), topic);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing devices with tokens {Tokens} from topic {Topic}", string.Join(", ", tokens), topic);
                return false;
            }
        }
    }
}

