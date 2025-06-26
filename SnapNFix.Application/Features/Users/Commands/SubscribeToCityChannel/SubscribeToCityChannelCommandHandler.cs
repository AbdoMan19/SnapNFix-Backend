using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;
using SnapNFix.Domain.Models.Notification;

namespace SnapNFix.Application.Features.Users.Commands.SubscribeToCityChannel
{
    public class SubscribeToCityChannelCommandHandler : IRequestHandler<SubscribeToCityChannelCommand, GenericResponseModel<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<SubscribeToCityChannelCommandHandler> _logger;

        public SubscribeToCityChannelCommandHandler(
            IUnitOfWork unitOfWork,
            IUserService userService,
            INotificationService notificationService,
            ILogger<SubscribeToCityChannelCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<GenericResponseModel<bool>> Handle(
            SubscribeToCityChannelCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = await _userService.GetCurrentUserIdAsync();
            if (currentUserId == Guid.Empty)
            {
                return GenericResponseModel<bool>.Failure("User not authenticated",
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create(nameof(currentUserId), "User not authenticated")
                    }
                );
            }

            // Check if city exists and is active
            var cityChannel = await _unitOfWork.Repository<CityChannel>()
                .GetQuerableData()
                .FirstOrDefaultAsync(c => c.Id == request.CityId && c.IsActive, cancellationToken);

            if (cityChannel == null)
            {
                return GenericResponseModel<bool>.Failure(
                    "City not found or not available for subscription",
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create(nameof(request.CityId), "City is not available for subscription")
                    });
            }

            // Check if already subscribed
            var subscriptionExists = await _unitOfWork.Repository<UserCitySubscription>()
                .GetQuerableData()
                .AnyAsync(s => s.UserId == currentUserId && s.CityChannelId == request.CityId, cancellationToken);

            if (subscriptionExists)
            {
                return GenericResponseModel<bool>.Success(true); // Already subscribed
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                // Create new subscription
                var subscription = new UserCitySubscription
                {
                    UserId = currentUserId,
                    CityChannelId = request.CityId,
                    SubscribedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<UserCitySubscription>().Add(subscription);
                await _unitOfWork.SaveChanges();
                
                // Subscribe user to Firebase topic for the city
                var topicName = $"city_{cityChannel.Name.Replace(" ", "_").ToLower()}";
                
                // Get user's device tokens
                var deviceManager = _unitOfWork.Repository<UserDevice>().GetQuerableData();
                var deviceTokens = await deviceManager
                    .Where(d => d.UserId == currentUserId && !string.IsNullOrEmpty(d.FCMToken))
                    .Select(d => d.FCMToken)
                    .ToListAsync(cancellationToken);
                
                if (deviceTokens.Any())
                {
                    await _notificationService.SubscribeToTopicAsync(deviceTokens, topicName);
                }
                
                await transaction.CommitAsync(cancellationToken);
                _logger.LogInformation("User {UserId} subscribed to city {CityId}", currentUserId, request.CityId);
                
                return GenericResponseModel<bool>.Success(true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to subscribe user {UserId} to city {CityId}", currentUserId, request.CityId);
                return GenericResponseModel<bool>.Failure($"Failed to subscribe to city: {ex.Message}",
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create(nameof(request.CityId), $"Failed to subscribe to city: {ex.Message}")
                    });
            }
        }
    }
}