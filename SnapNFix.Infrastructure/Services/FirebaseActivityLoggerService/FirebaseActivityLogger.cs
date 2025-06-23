using Application.DTOs;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Interfaces;

namespace Infrastructure.Services
{
    public class FirebaseActivityLogger : IActivityLogger
    {
        private readonly FirebaseClient _firebaseClient;
        //logger
        private readonly ILogger<FirebaseActivityLogger> _logger;
        
        public FirebaseActivityLogger(IConfiguration configuration , ILogger<FirebaseActivityLogger>logger)
        {
            _logger = logger;
            var firebaseUrl = configuration["Firebase:DatabaseUrl"];
            var authSecret = configuration["Firebase:AuthSecret"];
            
            //log
            if (string.IsNullOrEmpty(firebaseUrl))
            {
                _logger.LogError("Firebase Database URL is not configured.");
            }
            
            _firebaseClient = new FirebaseClient(
                firebaseUrl,
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(authSecret)
                });
        }
        
        
        
        public async Task LogActivityAsync(ActivityLogDto activityLog)
        {
           
            _logger.LogInformation("Logging activity: {ActivityType} for Issue ID: {IssueId}", 
                activityLog.Type, activityLog.IssueId);
            await _firebaseClient
                .Child("activities")
                .Child(activityLog.Id.ToString())
                .PutAsync(activityLog);
        }
    }
}