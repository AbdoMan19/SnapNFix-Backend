using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Users.Commands.UnsubscribeFromCityChannel
{
    public class UnsubscribeFromCityChannelCommandHandler : 
        IRequestHandler<UnsubscribeFromCityChannelCommand, GenericResponseModel<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<UnsubscribeFromCityChannelCommandHandler> _logger;

        public UnsubscribeFromCityChannelCommandHandler(
            IUnitOfWork unitOfWork,
            IUserService userService,
            INotificationService notificationService,
            ILogger<UnsubscribeFromCityChannelCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<GenericResponseModel<bool>> Handle(
            UnsubscribeFromCityChannelCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = await _userService.GetCurrentUserIdAsync();
            if (currentUserId == Guid.Empty)
            {
                return GenericResponseModel<bool>.Failure("User not authenticated");
            }

            // Get city info
            var city = await _unitOfWork.Repository<Domain.Entities.CityChannel>()
                .FindBy(cc => cc.Id == request.CityId)
                .FirstOrDefaultAsync(cancellationToken);

            if (city == null)
            {
                return GenericResponseModel<bool>.Failure("City not found");
            }

            // Get subscription
            var subscription = await _unitOfWork.Repository<UserCitySubscription>()
                .GetQuerableData()
                .FirstOrDefaultAsync(s => s.UserId == currentUserId && s.CityChannelId == request.CityId, 
                    cancellationToken);

            if (subscription == null)
            {
                return GenericResponseModel<bool>.Success(true); // Already unsubscribed
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                // Remove subscription
                _unitOfWork.Repository<UserCitySubscription>().Delete(subscription.Id);
                await _unitOfWork.SaveChanges();

                // Unsubscribe from Firebase topic
                var topicName = $"city_{city.Name.Replace(" ", "_").ToLower()}";
                
                // Get user's device tokens
                var deviceManager = _unitOfWork.Repository<UserDevice>().GetQuerableData();
                var deviceTokens = await deviceManager
                    .Where(d => d.UserId == currentUserId && !string.IsNullOrEmpty(d.FCMToken))
                    .Select(d => d.FCMToken)
                    .ToListAsync(cancellationToken);

                if (deviceTokens.Any())
                {
                    await _notificationService.UnsubscribeFromTopicAsync(deviceTokens, topicName);
                }
                
                await transaction.CommitAsync(cancellationToken);
                _logger.LogInformation("User {UserId} unsubscribed from city {CityId}", currentUserId, request.CityId);
                
                return GenericResponseModel<bool>.Success(true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to unsubscribe user {UserId} from city {CityId}", currentUserId, request.CityId);
                return GenericResponseModel<bool>.Failure($"Failed to unsubscribe from city: {ex.Message}");
            }
        }
    }
}