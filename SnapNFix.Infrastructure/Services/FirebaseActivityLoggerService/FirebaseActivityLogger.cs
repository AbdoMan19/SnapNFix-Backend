using Application.DTOs;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Extensions.Configuration;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;

namespace Infrastructure.Services
{
    public class FirebaseActivityLogger : IActivityLogger
    {
        private readonly FirebaseClient _firebaseClient;
        
        public FirebaseActivityLogger(IConfiguration configuration)
        {
            var firebaseUrl = configuration["Firebase:DatabaseUrl"];
            var authSecret = configuration["Firebase:AuthSecret"];
            
            _firebaseClient = new FirebaseClient(
                firebaseUrl,
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(authSecret)
                });
        }
        
        public async Task LogIssueCreatedAsync(Issue issue)
        {
            var activityLog = new ActivityLogDto
            {
                Type = "IssueCreated",
                IssueId = issue.Id,
                Description = $"New issue created: {issue.Id}",
                AdditionalData = new 
                {
                    IssueCategory = issue.Category,
                    Severity = issue.Severity
                }
            };
            
            await LogActivityToFirebaseAsync(activityLog);
        }
        
        public async Task LogIssueStatusChangedAsync(Guid issueId, IssueStatus previousStatus, IssueStatus newStatus)
        {
            var activityLog = new ActivityLogDto
            {
                Type = "IssueStatusChanged",
                IssueId = issueId,
                //add responsible person
                Description = $"Issue status changed from {previousStatus} to {newStatus}",
                AdditionalData = new 
                {
                    PreviousStatus = previousStatus,
                    NewStatus = newStatus
                }
            };
            
            await LogActivityToFirebaseAsync(activityLog);
        }
        
        private async Task LogActivityToFirebaseAsync(ActivityLogDto activityLog)
        {
            await _firebaseClient
                .Child("activities")
                .Child(activityLog.Id.ToString())
                .PutAsync(activityLog);
        }
    }
}