using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Utilities;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
            // Validate device ID from current context
            var currentDeviceId = _deviceManager.GetCurrentDeviceId();
            var currentUserId = await _userService.GetCurrentUserIdAsync();
        

            // Start transaction for database operations
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            
            try
            {
                // Find active refresh token for this device
                var sucess = await _deviceManager.DeactivateDeviceAsync(currentUserId, currentDeviceId);

                if (!sucess)
                {
                    // Log the token out by expiring it
                    _logger.LogInformation("Logging out device {DeviceId} by expiring refresh token", currentDeviceId);
                    //return generic response
                    return GenericResponseModel<bool>.Failure("No active refresh token found for this device");
                }
                await _unitOfWork.SaveChanges();
                
                // Commit the transaction
                await transaction.CommitAsync(cancellationToken);
                
                return GenericResponseModel<bool>.Success(true);
            }
            catch (Exception ex)
            {
                // Rollback on database errors
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error during logout process for device {DeviceId}", currentDeviceId);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception during logout");
            return GenericResponseModel<bool>.Failure("An error occurred during logout");
        }
    }
}