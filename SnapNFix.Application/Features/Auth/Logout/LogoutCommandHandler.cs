using MediatR;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Interfaces;
using SnapNFix.Application.Resources;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Auth.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, GenericResponseModel<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LogoutCommandHandler> _logger;
    private readonly IDeviceManager _deviceManager;
    private readonly IUserService _userService;

    public LogoutCommandHandler(
        IUnitOfWork unitOfWork,
        IDeviceManager deviceManager,
        ILogger<LogoutCommandHandler> logger,
        IUserService userService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _deviceManager = deviceManager;
        _userService = userService;
    }

    public async Task<GenericResponseModel<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var currentDeviceId = _deviceManager.GetCurrentDeviceId();
            var currentUserId = await _userService.GetCurrentUserIdAsync();

            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var success = await _deviceManager.DeactivateDeviceAsync(currentUserId, currentDeviceId);

                if (!success)
                {
                    _logger.LogInformation("Logging out device {DeviceId} by expiring refresh token", currentDeviceId);
                }

                await _unitOfWork.SaveChanges();
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error during logout process for device {DeviceId}", currentDeviceId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception during logout");
        }

        return GenericResponseModel<bool>.Success(true);
    }
}

