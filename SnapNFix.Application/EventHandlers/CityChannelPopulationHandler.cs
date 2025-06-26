using Application.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.EventHandlers
{
    public class CityChannelPopulationHandler : INotificationHandler<IssueCreated>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CityChannelPopulationHandler> _logger;

        public CityChannelPopulationHandler(
            IUnitOfWork unitOfWork,
            ILogger<CityChannelPopulationHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handle(IssueCreated notification, CancellationToken cancellationToken)
        {
            try
            {
                var issue = notification.Issue;
                
                // Skip if city or state is missing
                if (string.IsNullOrWhiteSpace(issue.City) || string.IsNullOrWhiteSpace(issue.State))
                {
                    _logger.LogInformation("Issue {IssueId} has no city or state information, skipping city creation", 
                        issue.Id);
                    return;
                }
                
                // Check if the city already exists
                var exists = await _unitOfWork.Repository<CityChannel>()
                    .GetQuerableData()
                    .AnyAsync(c => c.Name == issue.City && c.State == issue.State, 
                        cancellationToken);
                
                if (exists)
                {
                    _logger.LogDebug("City {City}, {State} already exists in database", 
                        issue.City, issue.State);
                    return;
                }
                
                // Create the new city
                var newCityChannel = new CityChannel
                {
                    Name = issue.City,
                    State = issue.State,
                    Country = issue.Country ?? "Egypt",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                
                await _unitOfWork.Repository<CityChannel>().Add(newCityChannel);
                await _unitOfWork.SaveChanges();
                
                _logger.LogInformation("Added new city: {City}, {State} with ID {CityId}", 
                    newCityChannel.Name, newCityChannel.State, newCityChannel.Id);
            }
            catch (Exception ex)
            {
                // Just log the error, don't throw - we don't want to interrupt the main flow
                _logger.LogError(ex, "Error while adding new city from issue {IssueId}", 
                    notification.Issue.Id);
            }
        }
    }
}